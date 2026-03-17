using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiProje.WebUI.Models
{
    public class ChatHub : Hub
    {
        private const string apiKey = "";
        private const string modelGpt = "gpt-40-mini";
        private readonly IHttpClientFactory _httpClientFactory; //Readonly demek:Değer constructor içinde atanabilir,Sonra değiştirilemez

        public ChatHub(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        private static readonly Dictionary<string, List<Dictionary<string, string>>> _history = new(); //Dictionary = key value veri yapısı
        public override Task OnConnectedAsync() //SignalR'da bağlantı gerçekleştirildiği zaman yapılıcak olan işlemleri belirliyor bu method 
        {
            _history[Context.ConnectionId] = new List<Dictionary<string, string>> //Her kullanıcının benzersiz bağlantı ID’sidir.Yeni kullanıcı için konuşma geçmişi başlatılır.
            {
                new Dictionary<string, string>
                {
                     ["role"] = "system",
                    ["content"] = "You are a helpful assistant.Keep answer concise."
                }
            };

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception) //Bağlantı kapandığı zaman ne olucağını söylüyor
        {
            _history.Remove(Context.ConnectionId); //Kullanıcı çıkınca konuşma geçmişi silinir.
            return base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string userMessage)  //Client mesaj gönderince bu method çalışır.
        {
            await Clients.Caller.SendAsync("ReceiveUserEcho", userMessage);  //Caller = mesajı gönderen kullanıcı.Client tarafında şu event çalışır:ReceiveUserEcho ,ReceiveUserEcho aslında client tarafında yazılmış bir event ismidir.Server tarafı sadece bu isimle mesaj gönderir.
            var history = _history[Context.ConnectionId]; //Bu kullanıcıya ait konuşma geçmişini getirir.
            history.Add(new() { ["role"] = "user", ["content"] = userMessage });
            await StreamOpenAI(history, Context.ConnectionAborted); //AI API çağrısı yapılır.ConnectionAborted Bir CancellationToken Yani:Bağlantı kesildi mi? bilgisini verir. Context.ConnectionAborted,Bağlantı kesildiğinde tetiklenen CancellationToken
        }

        public async Task StreamOpenAI(List<Dictionary<string, string>> history, CancellationToken cancellationToken) //OpenAI API’ye request gönderir ve cevabı stream olarak alır.
        {
            var client = _httpClientFactory.CreateClient("openai"); //Program.cs içinde tanımlanan openai client oluşturulur.
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey); //API key header’a eklenir.
            var payload = new  //API’ye gönderilen JSON
            {
                model = modelGpt,
                messages = history,
                stream = true,
                temperature = 0.2
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "v1/chat/completions"); //POST request hazırlanır.
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"); //Payload JSON’a çevrilir.

            using var resp = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cancellationToken); //API çağrılır.ResponseHeadersRead Bu çok önemli Normalde:HTTP response tamamen gelmeden okunamaz Ama burada: header gelir gelmez stream okumaya başla deniyor Bu sayede streaming çalışır.
            resp.EnsureSuccessStatusCode();  //Eğer API hata dönerse exception fırlatır.

            using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken); //API cevabı stream olarak okunur.
            using var reader = new StreamReader(stream); //OpenAI cevabı stream olarak okunur.

            var sb = new StringBuilder(); //AI cevabını biriktirmek için kullanılır.
            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested) //Stream bitene kadar çalışır.
            {
                var line = await reader.ReadLineAsync(); //Her satır okunur. OpenAI stream formatı genelde şöyle olur:data: {JSON}
                if (string.IsNullOrWhiteSpace(line)) continue; //Boş satır varsa atlanır. continue =>Bu döngünün geri kalanını çalıştırma, sonraki döngüye geç
                if (!line.StartsWith("data:")) continue; //Eğer satır "data:" ile başlamıyorsa bu satırı atla ve döngünün başına dön.Sadece şu satırlar işlenir:data: {...}
                var data = line["data:".Length..].Trim(); //Trim String methodudur ve başındaki ve sonundaki boşlukları temizler.Burada data kısmı çıkartılır
                if (data == "[DONE]") break; //Stream bitmiş demektir.Bu gelince döngü biter.

                try
                {
                    var chunk = JsonSerializer.Deserialize<ChatStreamChunk>(data); //JSON → C# nesnesine çevrilir.
                    var delta = chunk?.Choices?.FirstOrDefault()?.Delta?.Content; //OpenAI stream cevabındaki yeni token alınır.
                    if (!string.IsNullOrEmpty(delta))
                    {
                        sb.Append(delta);
                        await Clients.Caller.SendAsync("ReceiveToken", delta, cancellationToken); //Client tarafına anlık token gönderilir.

                    }
                }
                catch
                {
                    //Hata mesajları
                }
            }

            var full = sb.ToString(); //Stream tamamlandıktan sonra tam mesaj elde edilir.Tam cevap elde edilir.
            history.Add(new() { ["role"] = "assistant", ["content"] = full }); //AI cevabı history’ye eklenir.
            await Clients.Caller.SendAsync("CompleteMessage", full, cancellationToken); //Tam mesaj gönderilir.
        }

        //sealed class=Bu sınıftan miras alınamaz
        //sealed override method=bu method daha alt sınıflar tarafından ezilemez
        private sealed class ChatStreamChunk
        {
            [JsonPropertyName("choices")] public List<Choice>? Choices { get; set; }
        }
        private sealed class Choice
        {
            [JsonPropertyName("delta")] public Delta? Delta { get; set; }
            [JsonPropertyName("finish_reason")] public string? FinishReason { get; set; }
        }

        private sealed class Delta
        {
            [JsonPropertyName("content")] public string? Content { get; set; }
            [JsonPropertyName("role")] public string? Role { get; set; }
        }
    } 
}
//“Stream olarak” ifadesi yazılımda özellikle verinin parça parça gönderilmesi veya alınması anlamına gelir.

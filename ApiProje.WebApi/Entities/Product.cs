namespace ApiProje.WebApi.Entities
{
    public class Product
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } 
        public string ProductDescription { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl {  get; set; }
        public int? CategoryID { get; set; } //Her ürünün kategorisi olabilir olmayadabilir
        public Category Category { get; set; } //ürünün kategorisini getirmek istiyoruz önce ürünü sonra onun kategorisinin adı örneğin. Ürün=tiramisü bunun kategorisi=tatlı

    }
}

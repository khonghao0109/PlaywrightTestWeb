namespace MyWebApp.Models
{
    public class ProductInfo
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total 
        { 
            get 
            { 
                return Price * Quantity; 
            } 
        }
        public string Url { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
    }
}

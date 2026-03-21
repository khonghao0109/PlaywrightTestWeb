namespace MyWebApp.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public List<CartItemModel> CartItems { get; set; } = new List<CartItemModel>();
        public decimal GrandTotal { get; set; }
        public OrderAddress Address { get; set; } = new OrderAddress();
    }
}

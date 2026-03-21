using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebApp.Models
{
    public class OrderModel
    {
        public int Id { get; set; }
        public string OrderCode { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public long PayOSOrderCode { get; set; }
        public ICollection<OrderDetails> OrderDetails { get; set; }
        public ICollection<OrderAddress> OrderAddresses { get; set; }
        [NotMapped]
        public decimal? Total { get; set; }
    }
}

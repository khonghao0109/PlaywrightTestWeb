using MyWebApp.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class OrderAddress
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    [ForeignKey("OrderId")]

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    public string FullName { get; set; }


    public string Address1 { get; set; }

    public string Address2 { get; set; }


    public string ZipCode { get; set; }


    public string Country { get; set; }


    public string State { get; set; }

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    public string Phone { get; set; }

    public string Note { get; set; }

    public OrderModel Order { get; set; }

}

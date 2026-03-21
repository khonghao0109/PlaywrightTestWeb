using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Tên người dùng")]
        public string Username { get; set; }
        [DataType(DataType.Password),Required(ErrorMessage="hãy nhập mật khẩu")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập Email")]
        public string Email { get; set; }
    }
}

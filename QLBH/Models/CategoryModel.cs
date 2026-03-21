using System.ComponentModel.DataAnnotations;

namespace MyWebApp.Models
{
    public class CategoryModel
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="Yêu cầu nhập Tên danh mục")]
        public string Name { get; set; }

        public string Description { get; set; }
        public string Slug { get; set; }
        public int Status { get; set; }

    }
}

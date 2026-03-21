using Microsoft.AspNetCore.Identity;

namespace MyWebApp.Models
{
    public class AppUserModel: IdentityUser
    {
        public string Occupation { get; set; }
        public string RoleId { get; set; }

        public virtual IdentityRole? Role { get; set; }
    }
}

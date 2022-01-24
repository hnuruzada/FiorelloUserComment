using Microsoft.AspNetCore.Identity;

namespace FiorelloBack.Models
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public bool IsAdmin { get; set; }
    }
}

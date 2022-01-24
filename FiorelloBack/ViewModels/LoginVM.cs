using System.ComponentModel.DataAnnotations;

namespace FiorelloBack.ViewModels
{
    public class LoginVM
    {
        [StringLength(maximumLength: 50)]
        [Required]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}

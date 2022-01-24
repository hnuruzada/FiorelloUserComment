using System.ComponentModel.DataAnnotations;

namespace FiorelloBack.ViewModels
{
    public class RegisterVM
    {
        [Required]
        [StringLength(maximumLength: 25)]
        public string Username { get; set; }
        [Required]
        [StringLength(maximumLength: 70)]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [Required]
        [StringLength(maximumLength: 30)]
        public string Name { get; set; }
        [Required]
        [StringLength(maximumLength: 30)]
        public string Surname { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
        public bool Terms { get; set; }
    }
}

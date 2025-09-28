using System.ComponentModel.DataAnnotations;

namespace XIROX.Models
{
    public class ContactViewModel
    {
        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(1000, ErrorMessage = "Message is too long.")]
        public string Message { get; set; } = string.Empty;
    }
}

using System.ComponentModel.DataAnnotations;

namespace Core.Application.Payloads.RequestModels.UserRequest
{
    public class Request_Register
    {
        [Required(ErrorMessage ="UserName is required")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "FullName is required")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "PhoneNumber is required")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }
    }
}

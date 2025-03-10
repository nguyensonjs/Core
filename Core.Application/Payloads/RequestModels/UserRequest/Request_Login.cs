using System.ComponentModel.DataAnnotations;

namespace Core.Application.Payloads.RequestModels.UserRequest
{
    public class Request_Login
    {
        [Required(ErrorMessage = "User Name is required")]
        public string UserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}

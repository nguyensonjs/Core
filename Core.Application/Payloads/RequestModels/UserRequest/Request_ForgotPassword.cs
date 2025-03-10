using System.ComponentModel.DataAnnotations;

namespace Core.Application.Payloads.RequestModels.UserRequest
{
    public class Request_ForgotPassword
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;
    }
}

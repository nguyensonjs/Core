using Core.Application.InterfaceServices;
using Core.Application.Payloads.RequestModels.UserRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core.Api.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Request_Register request)
        {
            return Ok(await _authService.Register(request));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] Request_Login request)
        {
            return Ok(await _authService.Login(request));
        }

        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string verifyCode)
        {
            return Ok(await _authService.ConfirmRegisterAccount(verifyCode));
        }

        [HttpPost("resend-verify-code")]
        public async Task<IActionResult> ResendConfirmationCode([FromBody] Request_ResendCode request)
        {
            var result = await _authService.ResendConfirmationCode(request.Email);
            return StatusCode(result.Status, result);
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] Request_ChangePassword request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Dữ liệu không hợp lệ" });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized(new { Message = "Không thể xác thực người dùng." });

            var result = await _authService.ChangePassword(Guid.Parse(userId), request);
            return StatusCode(result.Status, result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] Request_ForgotPassword request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Dữ liệu không hợp lệ" });

            var result = await _authService.ForgotPassword(request);
            return Ok( result);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] Request_ResetPassword request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Message = "Dữ liệu không hợp lệ" });

            var result = await _authService.ResetPassword(request);
            return Ok(result);
        }
    }
}

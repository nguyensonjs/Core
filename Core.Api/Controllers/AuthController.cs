using Core.Application.InterfaceServices;
using Core.Application.Payloads.RequestModels.UserRequest;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

        /// <summary>
        /// Register a new account.
        /// </summary>
        /// <param name="request">Registration details</param>
        /// <returns>Registration result</returns>
        /// <response code="200">Registration successful</response>
        /// <response code="400">Invalid request</response>
        [HttpPost("register")]
        [SwaggerOperation(Summary = "Register an account", Description = "Create a new account with user information.")]
        [SwaggerResponse(200, "Registration successful")]
        [SwaggerResponse(400, "Invalid request")]
        public async Task<IActionResult> Register([FromBody] Request_Register request)
        {
            return Ok(await _authService.Register(request));
        }

        /// <summary>
        /// Verify email after registration.
        /// </summary>
        /// <param name="verifyCode">Verification code sent to the email</param>
        /// <returns>Verification result</returns>
        /// <response code="200">Verification successful</response>
        /// <response code="400">Invalid verification code</response>
        [HttpPost("verify-email")]
        [SwaggerOperation(Summary = "Verify email", Description = "Verify an account using the verification code sent to the email.")]
        [SwaggerResponse(200, "Verification successful")]
        [SwaggerResponse(400, "Invalid verification code")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string verifyCode)
        {
            return Ok(await _authService.ConfirmRegisterAccount(verifyCode));
        }
    }
}

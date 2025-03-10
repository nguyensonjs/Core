using Core.Application.Payloads.RequestModels.UserRequest;
using Core.Application.Payloads.ResponseModels.DataUser;
using Core.Application.Payloads.Responses;
using Core.Domain.Models;

namespace Core.Application.InterfaceServices
{
    public interface IAuthService
    {
        Task<ResponseObject<UserDTO>> Register(Request_Register request);
        Task<string> ConfirmRegisterAccount(string code);
        Task<ResponseObject<UserLoginDTO>> GetJwtTokenAsync (User user);
        Task<ResponseObject<UserLoginDTO>> Login (Request_Login request);

    }
}

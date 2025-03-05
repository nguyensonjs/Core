using Core.Application.Payloads.RequestModels.UserRequest;
using Core.Application.Payloads.ResponseModels.DataUser;
using Core.Application.Payloads.Responses;

namespace Core.Application.InterfaceServices
{
    public interface IAuthService
    {
        Task<ResponseObject<UserDTO>> Register(Request_Register request);
        Task<string> ConfirmRegisterAccount(string code);
    }
}

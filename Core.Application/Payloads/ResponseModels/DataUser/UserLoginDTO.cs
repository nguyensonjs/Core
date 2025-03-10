namespace Core.Application.Payloads.ResponseModels.DataUser
{
    public class UserLoginDTO
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }
}

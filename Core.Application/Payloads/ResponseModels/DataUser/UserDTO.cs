namespace Core.Application.Payloads.ResponseModels.DataUser
{
    public class UserDTO : DataResponseBase
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? AvatarImage { get; set; }
        public string Status { get; set; }
    }
}

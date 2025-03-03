using Core.Domain.Enums;

namespace Core.Domain.Models
{
    public class User : BaseModel
    {
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string? AvatarImage { get; set; }
        public UserStatus Status { get; set; } = UserStatus.Inactive;
        public virtual ICollection<Permission>? Permissions { get; set; }
    }
}

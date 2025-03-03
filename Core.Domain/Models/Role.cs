namespace Core.Domain.Models
{
    public class Role : BaseModel
    {
        public string RoleCode { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public virtual ICollection<Permission>? Permissions { get; set; }
    }
}

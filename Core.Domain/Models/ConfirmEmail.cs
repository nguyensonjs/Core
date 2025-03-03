namespace Core.Domain.Models
{
    public class ConfirmEmail : BaseModel
    {
        public string ConfirmCode { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
        public DateTime ExpiryTime { get; set; }
        public bool IsEmailConfirmed { get; set; }
    }
}

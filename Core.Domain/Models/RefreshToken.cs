namespace Core.Domain.Models
{
    public class RefreshToken : BaseModel
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public virtual User? User { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}

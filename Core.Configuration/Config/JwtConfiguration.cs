namespace Core.Configuration.Config
{
    public class JwtConfiguration
    {
        public string Secret { get; set; } = string.Empty;
        public string ValidIssuer { get; set; } = string.Empty;
        public string ValidAudience { get; set; } = string.Empty;
        public int ExpiryInMinutes { get; set; }
    }
}

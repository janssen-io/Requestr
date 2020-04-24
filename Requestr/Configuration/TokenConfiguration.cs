namespace Requestr.Configuration
{
    public class TokenConfiguration
    {
        public string? SharedSecret { get; set; }
        public string? Issuer { get; set; }
        public int ExpirationInMinutes { get; set; } = 60;
    }
}

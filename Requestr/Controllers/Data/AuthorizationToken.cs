namespace Requestr.Controllers.Data
{
    public class AuthorizationToken
    {
        public AuthorizationToken(string token, bool isInitialized)
        {
            this.Token = token;
            this.IsInitialized = isInitialized;
        }

        public string Token { get; }
        public bool IsInitialized { get; }
    }
}

namespace Requestr.Controllers.Data
{
    public class AuthorizationToken
    {
        public AuthorizationToken(string token)
        {
            this.Token = token;
        }

        public string Token { get; }
    }
}

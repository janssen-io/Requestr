using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Requestr.Configuration;
using Requestr.Data;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Requestr.Services
{
    public class TokenService
    {
        public const string UserIdClaim = "uid";
        public const string UsernameClaim = ClaimTypes.Upn;
        private readonly TokenConfiguration config;

        public TokenService(IOptions<TokenConfiguration> configuration)
        {
            this.config = configuration.Value;
        }

        public string Create(User user, string[] roles)
        {
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role));
            var userClaims = new Claim[]
            {
                new Claim(UserIdClaim, user.Id.ToString()),
                new Claim(UsernameClaim, user.Username),
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims.Concat(roleClaims)),
                Expires = DateTime.UtcNow.AddMinutes(config.ExpirationInMinutes),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config.SharedSecret!)),
                    SecurityAlgorithms.HmacSha256Signature),
                Issuer = config.Issuer,
                Audience = config.Issuer
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }
    }
}

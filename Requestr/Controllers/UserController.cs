using BunqDownloader.Bunq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestr.Controllers.Data;
using Requestr.Data;
using Requestr.Services;
using System;
using System.Linq;
using System.Security.Cryptography;

namespace Requestr.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly RequestrContext dbContext;
        private readonly TokenService tokenService;
        private readonly BunqInitializer initializer;

        public UserController(
            ILogger<UserController> logger,
            RequestrContext dbContext,
            TokenService tokenService,
            BunqInitializer initializer)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.tokenService = tokenService;
            this.initializer = initializer;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public ActionResult<AuthorizationToken> Login([FromBody]LoginRequest loginRequest)
        {
            if (IsPasswordValid(loginRequest.Username, loginRequest.Password))
            {
                var user = dbContext.User.Single(user => user.Username == loginRequest.Username);
                return new AuthorizationToken(
                    tokenService.Create(user),
                    initializer.IsInitialized(user.Id));
            }

            return Unauthorized();
        }

        [HttpPut("apikey")]
        [Authorize]
        public ActionResult Initialize([FromBody] string apiKey)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type == TokenService.UserIdClaim)?.Value;
            if (userId is null)
                return BadRequest();

            this.initializer.CreateApiContext(Guid.Parse(userId), apiKey);

            return NoContent();
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public ActionResult<AuthorizationToken> Register([FromBody]RegistrationRequest registrationRequest)
        {
            var isUnique = !this.dbContext.User.Where(user => user.Username == registrationRequest.Username).Any();
            if (!isUnique)
            {
                return BadRequest();
            }

            if (registrationRequest.Password.Length < 8)
            {
                return BadRequest();
            }

            var user = AddUser(registrationRequest.Username, registrationRequest.Password);
            return new AuthorizationToken(
                tokenService.Create(user),
                initializer.IsInitialized(user.Id));
        }

        private bool IsPasswordValid(string username, string password)
        {
            User? user = this.dbContext.User.Where(user => user.Username == username).FirstOrDefault();
            return user != null && Hash(password, user.Salt) == user.Password;
        }

        public User AddUser(string username, string password)
        {
            (var hashedPassword, var salt) = Hash(password);
            var user = this.dbContext.User.Add(new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                Password = hashedPassword,
                Salt = salt
            });
            this.dbContext.SaveChanges();

            return user.Entity;
        }

        private string Hash(string password, string salt)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10_000,
                numBytesRequested: 256 / 8));
        }

        private (string password, string salt) Hash(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string saltStr = Convert.ToBase64String(salt);

            return (Hash(password, saltStr), saltStr);
        }
    }
}
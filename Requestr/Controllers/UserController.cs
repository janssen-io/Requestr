using BunqDownloader.Bunq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Requestr.Configuration;
using Requestr.Controllers.Data;
using Requestr.Data;
using Requestr.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        private readonly EmailService emailService;
        private readonly MailConfiguration mailConfig;

        public UserController(
            ILogger<UserController> logger,
            RequestrContext dbContext,
            TokenService tokenService,
            BunqInitializer initializer,
            EmailService emailService,
            IOptions<MailConfiguration> mailConfig)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.tokenService = tokenService;
            this.initializer = initializer;
            this.emailService = emailService;
            this.mailConfig = mailConfig.Value;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthorizationToken>> Login([FromBody]LoginRequest loginRequest)
        {
            if (IsPasswordValid(loginRequest.Username, loginRequest.Password))
            {
                var user = dbContext.User.Single(user => user.Username == loginRequest.Username);
                await this.SendOneTimePassword(user);
                return new AuthorizationToken(
                    tokenService.Create(user, Array.Empty<string>()),
                    initializer.IsInitialized(user.Id));
            }

            return Unauthorized();
        }

        [HttpPut("apikey")]
        [Authorize(Roles = Roles.User)]
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
        public async Task<ActionResult<AuthorizationToken>> Register([FromBody]RegistrationRequest registrationRequest)
        {
            var isUnique = !this.dbContext.User.Where(user => user.Username == registrationRequest.Username).Any();
            if (!isUnique)
                return BadRequest("User already exists.");

            if (registrationRequest.Password.Length < 8)
                return BadRequest("Password should be at least 8 characters.");

            if (!IsValidEmail(registrationRequest.Username))
                return BadRequest("Username is not a valid e-mail address.");

            var user = AddUser(registrationRequest.Username, registrationRequest.Password);
            await this.SendOneTimePassword(user);
            return new AuthorizationToken(
                tokenService.Create(user, Array.Empty<string>()),
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
            string salt = RSG.Generate(16);
            return (Hash(password, salt), salt);
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        [HttpPost("token")]
        [Authorize]
        public IActionResult ValidateOneTimePassword(TokenRequest request)
        {
            var userIdClaim = User.Claims.Single(c => c.Type == TokenService.UserIdClaim);
            var otp = dbContext.OneTimePasswordForLogin
                .Include(otp => otp.User)
                .FirstOrDefault(otp => 
                    otp.Password == request.OneTimePassword
                    && otp.User.Id == Guid.Parse(userIdClaim.Value));

            if (otp is null)
                return BadRequest("Invalid password.");

            if (otp.CreatedOn < DateTime.UtcNow.AddMinutes(-10))
                return BadRequest("Password expired.");

            var user = otp.User;
            return this.Ok(new AuthorizationToken(
                    tokenService.Create(user, new[] { Roles.User }),
                    initializer.IsInitialized(user.Id)));
        }

        public async Task SendOneTimePassword(User user)
        {
            string otp = RSG.Generate(8);
            dbContext.OneTimePasswordForLogin.Add(new OneTimePasswordForLogin
            {
                Id = Guid.NewGuid(),
                User = user,
                Password = otp,
                CreatedOn = DateTime.UtcNow,
            });
            dbContext.SaveChanges();

            var email = new MailBuilder(this.mailConfig)
                .AddRecipients(user.Username)
                .WithSubject("New Requestr login")
                .WithBody($"Your login code is: {otp}")
                .Build();
            
            if(!await this.emailService.SendMail(email))
                throw new Exception("One time password could not be sent.");
        }
    }
}
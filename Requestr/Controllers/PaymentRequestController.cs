using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Bunq.Sdk.Http;
using Bunq.Sdk.Model.Generated.Endpoint;
using Bunq.Sdk.Model.Generated.Object;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Requestr.Configuration;
using Requestr.Controllers.Data;
using Requestr.Data;
using Requestr.Services;

namespace Requestr.Controllers
{
    [Route("api/requests")]
    [ApiController]
    public class PaymentRequestController : ControllerBase
    {
        private readonly RequestrContext dbContext;
        private readonly MailConfiguration mailConfig;

        public PaymentRequestController(RequestrContext dbContext, IOptions<MailConfiguration> mailConfig)
        {
            this.dbContext = dbContext;
            this.mailConfig = mailConfig.Value;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<PaymentRequestResponse>> SendRequest(SendPaymentRequest request)
        {
            if (request.Amount <= 0)
                return BadRequest("Amount must be positive.");

            if ((request.WithReceipts || request.WithStatement) && request.Transactions is null)
                return BadRequest("No transactions provided.");

            if (request.Recipients.Length == 0)
                return BadRequest("No recipients provided.");

            if (request.Recipients.Any(email => !IsValidEmail(email)))
                return BadRequest("One or more of the provided e-mail addresses is invalid.");

            var bunqTab = CreatePaymentRequest(
                request.Amount,
                request.Currency,
                request.Description);

            var paymentRequestId = AddToDatabase(request, bunqTab.BunqmeTabShareUrl);

            var username = User.Claims.First(c => c.Type == TokenService.UsernameClaim).Value;
            var isMailSent = await SendMail(username, request, bunqTab.BunqmeTabShareUrl);

            return new PaymentRequestResponse(paymentRequestId, bunqTab.BunqmeTabShareUrl, isMailSent);
        }

        [HttpPost("{id}/code")]
        public async Task<ActionResult> GetOneTimePassword(Guid id, [FromBody] OneTimePasswordRequest request)
        {
            if (!IsValidEmail(request.Email))
                return BadRequest("Invalid e-mail address.");

            var paymentRequest = dbContext.PaymentRequests.Find(id);
            if (paymentRequest is null)
                return NotFound("Payment request not found.");

            if (!paymentRequest.Recipients.Contains(request.Email)
                && paymentRequest.User.Username != request.Email)
                return BadRequest("Unknown e-mail address.");

            if (await SendOneTimePassword(paymentRequest, request.Email))
            {
                return Accepted();
            }
            else
            {
                return BadRequest("Something went wrong. Could not send e-mail at this time.");
            }

        }

        [HttpGet("{id}")]
        public ActionResult<PaymentRequest> GetPaymentRequest(Guid id, [FromQuery] string oneTimePassword)
        {
            var otp = dbContext.OneTimePasswordForPaymentRequests
                .Include(otp => otp.PaymentRequest)
                .FirstOrDefault(otp => otp.Password == oneTimePassword && otp.PaymentRequest.Id == id);

            if (otp is null)
                return NotFound("Payment request not found or password invalid.");

            if (otp.CreatedOn < DateTime.UtcNow.AddDays(-1))
                return BadRequest("Password expired.");

            return this.Ok(otp.PaymentRequest);
        }

        private async Task<bool> SendOneTimePassword(PaymentRequest paymentRequest, string email)
        {
            byte[] salt = new byte[8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string otp = Convert.ToBase64String(salt).Substring(0, 8);
            dbContext.OneTimePasswordForPaymentRequests.Add(new OneTimePasswordForPaymentRequest
            {
                id = Guid.NewGuid(),
                Password = otp,
                PaymentRequest = paymentRequest,
                CreatedOn = DateTime.UtcNow
            });
            dbContext.SaveChanges();

            MailjetClient client = new MailjetClient(mailConfig.PublicKey, mailConfig.PrivateKey)
            {
                Version = ApiVersion.V3_1,
            };
            var recipient = new JArray();
            recipient.Add(new JObject
            {
                { "Email", email },
                { "Name", email }
            });
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.Messages, new JArray {
                new JObject {
                    {"From", new JObject {
                        {"Email", mailConfig.SenderAddress},
                        {"Name", "Requestr"}
                    }},
                    {"To", recipient },
                    {"Subject", "One time password for Payment Request"},
                    {"TextPart", $"Your one time password is: {otp}"},
                }
            });
            MailjetResponse response = await client.PostAsync(request);
            return response.IsSuccessStatusCode;

        }

        private Guid AddToDatabase(SendPaymentRequest request, string bunqmeTabShareUrl)
        {
            var userId = User.Claims.First(c => c.Type == TokenService.UserIdClaim).Value;
            var user = dbContext.User.Find(Guid.Parse(userId));
            var paymentRequestId = Guid.NewGuid();

            dbContext.PaymentRequests.Add(new PaymentRequest
            {
                Id = paymentRequestId,
                Amount = request.Amount,
                Currency = request.Currency,
                Link = new Uri(bunqmeTabShareUrl),
                Description = request.Description,
                Recipients = string.Join(',',request.Recipients),
                User = user
            });
            dbContext.SaveChanges();

            return paymentRequestId;
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

        private BunqMeTab CreatePaymentRequest(decimal amount, string currency, string description)
        {
            BunqResponse<int> requestId = BunqMeTab.Create(new BunqMeTabEntry
            {
                AmountInquired = new Amount(amount.ToString(), currency),
                Description = description
            });

            return BunqMeTab.Get(requestId.Value).Value;
        }

        private async Task<bool> SendMail(string sender, SendPaymentRequest r1, string requestUrl)
        {
            var body = new StringBuilder();
            body.AppendLine("Hello!");
            body.AppendLine($"{sender} requests you to pay {r1.Currency} {r1.Amount}.");
            if (!string.IsNullOrEmpty(r1.Description))
            {
                body.AppendLine();
                body.AppendLine("Description:");
                body.AppendLine(r1.Description);
            }
            body.AppendLine();
            body.AppendLine($"Pay here: {requestUrl}");

            MailjetClient client = new MailjetClient(mailConfig.PublicKey, mailConfig.PrivateKey)
            {
                Version = ApiVersion.V3_1,
            };
            var recipients = new JArray();
            foreach (var recipient in r1.Recipients)
            {
                recipients.Add(new JObject
                {
                    { "Email", recipient },
                    { "Name", recipient }
                });
            }
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            }
            .Property(Send.Messages, new JArray {
                new JObject {
                    {"From", new JObject {
                        {"Email", mailConfig.SenderAddress},
                        {"Name", "Requestr"}
                    }},
                    {"To", new JArray { 
                        new JObject {
                            { "Email", sender },
                            { "Name", sender }
                        }
                    }},
                    {"Bcc", recipients },
                    {"Subject", $"New payment request from {sender}"},
                    {"TextPart", body.ToString()},
                }
            });
            MailjetResponse response = await client.PostAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
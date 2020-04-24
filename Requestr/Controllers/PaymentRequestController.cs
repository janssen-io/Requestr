using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunq.Sdk.Http;
using Bunq.Sdk.Model.Generated.Endpoint;
using Bunq.Sdk.Model.Generated.Object;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<PaymentRequestResponse>> SendRequest(SendPaymentRequest request)
        {
            var bunqTab = CreatePaymentRequest(
                request.Amount,
                request.Currency,
                request.Description);

            AddToDatabase(request, bunqTab.BunqmeTabShareUrl);

            var username = User.Claims.First(c => c.Type == TokenService.UsernameClaim).Value;
            var isMailSent = await SendMail(username, request, bunqTab.BunqmeTabShareUrl);

            return new PaymentRequestResponse(bunqTab.BunqmeTabShareUrl, isMailSent);
        }

        private void AddToDatabase(SendPaymentRequest request, string bunqmeTabShareUrl)
        {
            var userId = User.Claims.First(c => c.Type == TokenService.UserIdClaim).Value;
            var user = dbContext.User.Find(Guid.Parse(userId));
            dbContext.PaymentRequests.Add(new PaymentRequest
            {
                Id = Guid.NewGuid(),
                Amount = request.Amount,
                Currency = request.Currency,
                Link = new Uri(bunqmeTabShareUrl),
                Description = request.Description,
                ToEmail = request.ToEmail,
                ToPhone = request.ToPhone,
                User = user
            });
            dbContext.SaveChanges();
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

        private async Task<bool> SendMail(string senderName, SendPaymentRequest r1, string requestUrl)
        {
            var body = new StringBuilder();
            body.AppendLine("Hello!");
            body.AppendLine($"{senderName} requests you to pay {r1.Currency} {r1.Amount}.");
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
                           {"Email", r1.ToEmail },
                           {"Name", r1.ToEmail }
                        }
                    }},
                    {"Subject", $"New payment request from {senderName}"},
                    {"TextPart", body.ToString()},
                }
            });
            MailjetResponse response = await client.PostAsync(request);
            return response.IsSuccessStatusCode;
        }
    }
}
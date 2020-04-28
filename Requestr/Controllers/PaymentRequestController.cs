using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bunq.Sdk.Http;
using Bunq.Sdk.Model.Generated.Endpoint;
using Bunq.Sdk.Model.Generated.Object;
using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
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
        private readonly EmailService emailService;

        public PaymentRequestController(
            RequestrContext dbContext,
            IOptions<MailConfiguration> mailConfig,
            EmailService emailService)
        {
            this.dbContext = dbContext;
            this.mailConfig = mailConfig.Value;
            this.emailService = emailService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.User)]
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
            var isMailSent = await SendPaymentRequest(username, request, bunqTab.BunqmeTabShareUrl);

            return new PaymentRequestResponse(paymentRequestId, bunqTab.BunqmeTabShareUrl, isMailSent);
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

        private async Task<bool> SendPaymentRequest(string sender, SendPaymentRequest paymentRequest, string requestUrl)
        {
            var body = new StringBuilder();
            body.AppendLine("Hello!");
            body.AppendLine($"{sender} requests you to pay {paymentRequest.Currency} {paymentRequest.Amount}.");
            if (!string.IsNullOrEmpty(paymentRequest.Description))
            {
                body.AppendLine();
                body.AppendLine("Description:");
                body.AppendLine(paymentRequest.Description);
            }
            body.AppendLine();
            body.AppendLine($"Pay here: {requestUrl}");

            var email = new MailBuilder(this.mailConfig)
                .WithBody(body.ToString())
                .WithSubject($"New payment request from {sender}")
                .AddRecipients(sender)
                .AddBccRecipients(paymentRequest.Recipients);

            if (paymentRequest.WithStatement)
            {
                using var file = new MemoryStream();

                using (var fileWriter = new StreamWriter(file))
                using (var csvHelper = new CsvWriter(fileWriter, CultureInfo.InvariantCulture))
                {
                    csvHelper.WriteHeader(typeof(Data.Transaction));
                    csvHelper.Flush();
                    fileWriter.WriteLine();
                    csvHelper.WriteRecords(paymentRequest.Transactions.OrderByDescending(t => t.CreatedOn));
                }

                var fileBytes = file.ToArray();
                email.WithAttachment("payments.csv", "text/csv", Convert.ToBase64String(fileBytes));
            }

            return await this.emailService.SendMail(email.Build());
        }
    }
}
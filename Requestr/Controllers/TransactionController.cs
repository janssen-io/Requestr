using Bunq.Sdk.Model.Generated.Endpoint;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestr.Configuration;
using Requestr.Controllers.Data;
using Requestr.Data;
using Requestr.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Requestr.Controllers
{

    [Route("api/transactions")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ILogger<UserController> logger;
        private readonly RequestrContext dbContext;
        private readonly TransactionReader reader;

        public TransactionController(ILogger<UserController> logger, RequestrContext dbContext, TransactionReader reader)
        {
            this.logger = logger;
            this.dbContext = dbContext;
            this.reader = reader;
        }

        [HttpGet]
        [Authorize(Roles = Roles.User)]
        public ActionResult<List<Transaction>> Index([FromQuery] TransactionFilter filterParameters)
        {
            var paymentsInRange = new List<Payment>();
            foreach (var transaction in this.reader.Read())
            {
                var createdAt = DateTime.Parse(transaction.Created);
                if (createdAt < filterParameters.From.Date)
                    break;

                if (createdAt >= filterParameters.UpTo.Date)
                    continue;

                paymentsInRange.Add(transaction);
            }

            return paymentsInRange.Select(MapToTransaction).ToList();
        }

        private Transaction MapToTransaction(Payment payment) =>
            new Transaction
            {
                Id = (int)payment.Id!,
                Account = payment.Alias.LabelMonetaryAccount.Iban,
                Amount = decimal.Parse(payment.Amount.Value, NumberStyles.Currency),
                CounterParty = payment.CounterpartyAlias.LabelMonetaryAccount.DisplayName,
                CounterPartyIban = payment.CounterpartyAlias.LabelMonetaryAccount.Iban,
                CreatedOn = DateTime.Parse(payment.Created),
                Currency = payment.Amount.Currency,
                Description = payment.Description,
            };
    }
}

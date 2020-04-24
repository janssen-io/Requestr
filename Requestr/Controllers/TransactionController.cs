using Bunq.Sdk.Context;
using Bunq.Sdk.Model.Generated.Endpoint;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Requestr.Controllers.Data;
using Requestr.Data;
using Requestr.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;

namespace Requestr.Controllers
{

    [Route("api/transactions")]
    [ApiController]
    public class TransactionController
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
        [Authorize]
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
                Account = payment.Alias.LabelMonetaryAccount.Iban,
                CounterParty = payment.CounterpartyAlias.LabelMonetaryAccount.DisplayName,
                Description = payment.Description,
                Id = (int)payment.Id!,
                Amount = decimal.Parse(payment.Amount.Value, NumberStyles.Currency),
                Currency = payment.Amount.Currency
            };
    }
}

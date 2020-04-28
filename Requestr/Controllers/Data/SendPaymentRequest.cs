using System;

namespace Requestr.Controllers.Data
{
    public class SendPaymentRequest
    {
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public string[] Recipients { get; set; } = Array.Empty<string>();
        public string Currency { get; set; } = "EUR";
        public bool WithStatement { get; set; }
        public bool WithReceipts { get; set; }
        public Transaction[]? Transactions { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestr.Controllers.Data
{
    public class SendPaymentRequest
    {
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
        public string ToEmail { get; set; }
        public string? ToPhone { get; set; }
        public string Currency { get; set; } = "EUR";
        public bool WithStatement { get; set; }
        public bool WithReceipts { get; set; }
        public int[]? Transactions { get; set; }
    }
}

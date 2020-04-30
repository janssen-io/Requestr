using System;

namespace Requestr.Controllers.Data
{
    public class Transaction
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal Amount { get; set; }
        public string Account { get; set; }
        public string CounterParty { get; set; }
        public string? CounterPartyIban { get; set; }
        public string Currency { get; set; }
        public string Description { get; set; } = "";
    }
}

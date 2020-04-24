using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestr.Controllers.Data
{
    public class Transaction
    {
        public int Id { get; set; }
        public string CounterParty { get; set; }
        public string Account { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}

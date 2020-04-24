using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Requestr.Controllers.Data
{
    public class TransactionFilter
    {
        public DateTime From { get; set; }
        public DateTime UpTo { get; set; }
    }
}

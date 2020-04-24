using Bunq.Sdk.Http;
using Bunq.Sdk.Model.Generated.Endpoint;
using BunqDownloader.Bunq;
using System;
using System.Collections.Generic;

namespace Requestr.Services
{
    public class TransactionReader
    {
        public TransactionReader() { }

        public IEnumerable<Payment> Read()
        {
            // TODO: lock around this to ensure appropriate bunq context
            var pager = new BunqPager(50, PageOrder.Descending);
            Func<IDictionary<string, string>, BunqResponse<List<Payment>>> ListPayments =
                urlParams => Payment.List(urlParams: urlParams);
            return pager.Read(ListPayments);
        }
    }
}

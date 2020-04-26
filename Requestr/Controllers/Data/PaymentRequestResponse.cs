using System;

namespace Requestr.Controllers.Data
{
    public class PaymentRequestResponse
    {
        public PaymentRequestResponse(Guid id, string link, bool isMailSent)
        {
            this.Id = id;
            this.Link = link;
            this.IsMailSent = isMailSent;
        }

        public Guid Id { get; }
        public string Link { get; }
        public bool IsMailSent { get; }
    }
}

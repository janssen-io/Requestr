namespace Requestr.Controllers.Data
{
    public class PaymentRequestResponse
    {
        public PaymentRequestResponse(string link, bool isMailSent)
        {
            this.Link = link;
            this.IsMailSent = isMailSent;
        }

        public string Link { get; }
        public bool IsMailSent { get; }
    }
}

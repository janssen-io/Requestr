﻿using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Requestr.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Requestr.Services
{
    public class EmailService
    {
        private readonly MailConfiguration config;

        public EmailService(IOptions<MailConfiguration> config)
        {
            this.config = config.Value;
        }

        public async Task<bool> SendMail(MailjetRequest mailjetRequest)
        {
            MailjetClient client = new MailjetClient(config.PublicKey, config.PrivateKey)
            {
                Version = ApiVersion.V3_1,
            };

            MailjetResponse response = await client.PostAsync(mailjetRequest);
            return response.IsSuccessStatusCode;
        }
    }

    public class MailBuilder
    {
        private readonly MailConfiguration config;
        private readonly MailjetRequest mail;
        private readonly HashSet<string> toAddresses = new HashSet<string>();
        private readonly HashSet<string> bccAddresses = new HashSet<string>();
        private string subject = "";
        private string body = "";

        public MailBuilder(MailConfiguration config)
        {
            this.config = config;
            this.mail = new MailjetRequest();
        }

        public MailBuilder AddRecipients(params string[] recipients)
        {
            foreach (string recipient in recipients)
            {
                this.toAddresses.Add(recipient);
            }

            return this;
        }

        public MailBuilder AddBccRecipients(params string[] recipients)
        {
            foreach (string recipient in recipients)
            {
                this.bccAddresses.Add(recipient);
            }

            return this;
        }

        public MailBuilder WithSubject(string subject)
        {
            this.subject = subject;
            return this;
        }

        public MailBuilder WithBody(string body)
        {
            this.body = body;
            return this;
        }

        public MailjetRequest Build()
        {
            var to = new JArray();
            foreach(var toAddress in this.toAddresses)
            {
                to.Add(new JObject
                {
                    { "Email", toAddress },
                    { "Name", toAddress }
                });
            }

            var bcc = new JArray();
            foreach(var bccAddress in this.bccAddresses)
            {
                bcc.Add(new JObject
                {
                    { "Email", bccAddress },
                    { "Name", bccAddress }
                });
            }

            return new MailjetRequest { Resource = Send.Resource, }
                .Property(Send.Messages, new JArray {
                    new JObject {
                        {"From", new JObject {
                            { "Email", this.config.SenderAddress },
                            {"Name", "Requestr"}
                        }},
                        {"To", to},
                        {"Bcc", bcc},
                        {"Subject", this.subject},
                        {"TextPart", this.body},
                    }
            });
        }
    }

}

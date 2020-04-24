using Bunq.Sdk.Context;
using Bunq.Sdk.Json;
using Bunq.Sdk.Model.Generated.Endpoint;
using Bunq.Sdk.Model.Generated.Object;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Requestr.Configuration;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BunqDownloader.Bunq
{
    public class BunqInitializer
    {
        private readonly BunqConfiguration config;

        public BunqInitializer(IOptions<BunqConfiguration> configuration)
        {
            this.config = configuration.Value;
        }

        private ApiEnvironmentType Environment =>
            this.config.Environment.ToLower() == "production"
            ? ApiEnvironmentType.PRODUCTION
            : ApiEnvironmentType.SANDBOX;

        private string ComputeConfigPath(Guid userId) =>
            Path.Combine(this.config.ConfigBasePath, $"{userId}.{this.Environment.TypeString}.config");

        public bool IsInitialized(Guid userId) =>
            File.Exists(ComputeConfigPath(userId));

        public void RestoreApiContext(Guid userId)
        {
            string configPath = this.ComputeConfigPath(userId);
            if (!this.IsInitialized(userId))
                throw new ArgumentException($"No configuration exists for user {userId}.");

            var apiContext = ApiContext.Restore(configPath);
            apiContext.EnsureSessionActive();
            apiContext.Save(configPath);

            BunqContext.LoadApiContext(apiContext);

            if (BunqContext.ApiContext.EnvironmentType == ApiEnvironmentType.SANDBOX)
            {
                AddSandboxPayments();
            }
        }

        public void CreateApiContext(Guid userId, string apiKey)
        {
            if (this.Environment == ApiEnvironmentType.SANDBOX)
            {
                var sandboxUser = GenerateNewSandboxUser();
                apiKey = sandboxUser.ApiKey;
            }

            var api = ApiContext.Create(
                this.Environment, apiKey, this.config.Description);

            api.Save(this.ComputeConfigPath(userId));

            BunqContext.LoadApiContext(api);

            if (this.Environment == ApiEnvironmentType.SANDBOX)
                AddSandboxPayments();
        }

        private SandboxUser GenerateNewSandboxUser()
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("X-Bunq-Client-Request-Id", "unique");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no");
            httpClient.DefaultRequestHeaders.Add("X-Bunq-Geolocation", "0 0 0 0 NL");
            httpClient.DefaultRequestHeaders.Add("X-Bunq-Language", "en_US");
            httpClient.DefaultRequestHeaders.Add("X-Bunq-Region", "en_US");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "hoi");

            var requestTask = httpClient.PostAsync(ApiEnvironmentType.SANDBOX.BaseUri + "sandbox-user", null);
            requestTask.Wait();

            var responseString = requestTask.Result.Content.ReadAsStringAsync().Result;
            var responseJson = BunqJsonConvert.DeserializeObject<JObject>(responseString);
            return BunqJsonConvert.DeserializeObject<SandboxUser>(responseJson.First.First.First.First.First
                .ToString());
        }

        private void AddSandboxPayments()
        {
            if (BunqContext.ApiContext.EnvironmentType != ApiEnvironmentType.SANDBOX)
                throw new InvalidOperationException("Payments can only be added in the sandbox.");

            // Usually the BunqContext should be used in a separate task
            // because it is a singleton. However, since this is the sandbox,
            // it does not matter (that much).
            Task.Run(() =>
            {
                RequestInquiry.Create(
                    new Amount("500.00", "EUR"),
                    new Pointer("EMAIL", "sugardaddy@bunq.com"),
                    "Requesting some spending money.",
                    false
                );

                Thread.Sleep(1000);

                var rng = new Random();
                for (int i = 0; i < 10; i++)
                {
                    var euro = rng.Next(10, 50);
                    var cent = rng.Next(0, 100);
                    Payment.Create(
                        new Amount($"{euro}.{cent}", "EUR"),
                        new Pointer("EMAIL", "sugardaddy@bunq.com"),
                        $"Test Payment {i + 1}");
                }
            });
        }
    }
}

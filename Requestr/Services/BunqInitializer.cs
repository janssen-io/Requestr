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

namespace BunqDownloader.Bunq
{
    public class BunqInitializer
    {
        private readonly BunqConfiguration config;

        public BunqInitializer(IOptions<BunqConfiguration> configuration)
        {
            this.config = configuration.Value;
        }

        private string ComputeConfigPath(string username) =>
            Path.Combine( this.config.ConfigBasePath, $"{username}.config");

        public void RestoreApiContext(string username)
        {
            string configPath = this.ComputeConfigPath(username);
            if (!File.Exists(configPath))
                throw new ArgumentException($"No configuration exists for user {username}.");

            var apiContext = ApiContext.Restore(configPath);
            apiContext.EnsureSessionActive();
            apiContext.Save(configPath);

            BunqContext.LoadApiContext(apiContext);

            if (BunqContext.ApiContext.EnvironmentType == ApiEnvironmentType.SANDBOX)
            {
                AddSandboxPayments();
            }
        }

        public void CreateApiContext(string username, string apiKey)
        {
            var useProd = this.config.Environment.ToLower() == "production";
            if (!useProd)
            {
                var sandboxUser = GenerateNewSandboxUser();
                apiKey = sandboxUser.ApiKey;
            }

            var api = ApiContext.Create(
                useProd ? ApiEnvironmentType.PRODUCTION : ApiEnvironmentType.SANDBOX,
                apiKey,
                this.config.Description);
            api.Save(this.ComputeConfigPath(username));

            BunqContext.LoadApiContext(api);
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
            RequestInquiry.Create(
                new Amount("500.00", "EUR"),
                new Pointer("EMAIL", "sugardaddy@bunq.com"),
                "Requesting some spending money.",
                false
            );

            Thread.Sleep(1000);
        }
    }
}

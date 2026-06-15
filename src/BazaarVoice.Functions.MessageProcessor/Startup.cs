using Azure.Identity;
using BazaarVoice.Common.Helpers;
using BazaarVoice.Functions.MessageProcessor.Configuration;
using BazaarVoice.Functions.MessageProcessor.Models;
using BazaarVoice.Functions.MessageProcessor.Services;
using BazaarVoice.Infrastructure.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(BazaarVoice.Functions.MessageProcessor.Startup))]

namespace BazaarVoice.Functions.MessageProcessor
{
    /// <summary>
    /// Configures dependency injection for Function App #2 (Message Processor).
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            // ── Bind typed settings ──────────────────────────────────────────
            var settings = new MessageProcessorSettings();
            configuration.Bind(settings);
            builder.Services.AddSingleton(settings);

            // ── Register Application Insights ────────────────────────────────
            builder.Services.AddApplicationInsightsTelemetryWorkerService();

            // ── Register Memory Cache (for Annex Cloud action caching) ───────
            builder.Services.AddMemoryCache();

            // ── Register Cosmos DB Client ────────────────────────────────────
            builder.Services.AddSingleton(sp =>
            {
                var cfg = sp.GetRequiredService<MessageProcessorSettings>();

                if (cfg.UseManagedIdentity && !string.IsNullOrWhiteSpace(cfg.CosmosDbEndpoint))
                {
                    // Managed Identity (recommended for production)
                    // PLACEHOLDER: e.g., "https://cosmos-customer-dev.documents.azure.com:443/"
                    return new CosmosClient(cfg.CosmosDbEndpoint, new DefaultAzureCredential());
                }

                // Connection String (for local development)
                // PLACEHOLDER: Replace with your Cosmos DB connection string
                string connectionString = cfg.CosmosDbConnectionString
                    ?? throw new InvalidOperationException(
                        "CosmosDbConnectionString is not configured.");
                return new CosmosClient(connectionString);
            });

            // ── Register Cosmos Repository ───────────────────────────────────
            builder.Services.AddSingleton<ICosmosRepository<CustomerRecord>>(sp =>
            {
                var cosmosClient = sp.GetRequiredService<CosmosClient>();
                var cfg = sp.GetRequiredService<MessageProcessorSettings>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CosmosRepository<CustomerRecord>>>();

                return new CosmosRepository<CustomerRecord>(
                    cosmosClient,
                    cfg.CosmosDbDatabaseName ?? "CustomerDatabase",   // PLACEHOLDER: Actual DB name
                    cfg.CosmosDbContainerName ?? "Customers",         // PLACEHOLDER: Actual container name
                    logger);
            });

            // ── Register HttpClient for Common Loyalty API (APIM) ────────────
            builder.Services.AddHttpClient("LoyaltyApi", (sp, client) =>
            {
                var cfg = sp.GetRequiredService<MessageProcessorSettings>();

                if (!string.IsNullOrWhiteSpace(cfg.CommonLoyaltyApiBaseUrl))
                {
                    client.BaseAddress = new Uri(cfg.CommonLoyaltyApiBaseUrl);
                }

                // PLACEHOLDER: Add APIM subscription key header
                if (!string.IsNullOrWhiteSpace(cfg.CommonLoyaltyApiSubscriptionKey))
                {
                    client.DefaultRequestHeaders.Add(
                        "Ocp-Apim-Subscription-Key",
                        cfg.CommonLoyaltyApiSubscriptionKey);
                }

                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(RetryHelper.GetHttpRetryPolicy())
            .AddPolicyHandler(RetryHelper.GetCircuitBreakerPolicy());

            // ── Register HttpClient for Annex Cloud ──────────────────────────
            builder.Services.AddHttpClient("AnnexCloud", (sp, client) =>
            {
                var cfg = sp.GetRequiredService<MessageProcessorSettings>();

                if (!string.IsNullOrWhiteSpace(cfg.AnnexCloudApiBaseUrl))
                {
                    client.BaseAddress = new Uri(cfg.AnnexCloudApiBaseUrl);
                }

                // PLACEHOLDER: Add Annex Cloud API key header if required
                if (!string.IsNullOrWhiteSpace(cfg.AnnexCloudApiKey))
                {
                    client.DefaultRequestHeaders.Add("x-api-key", cfg.AnnexCloudApiKey);
                }

                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(RetryHelper.GetHttpRetryPolicy())
            .AddPolicyHandler(RetryHelper.GetCircuitBreakerPolicy());

            // ── Register Services ────────────────────────────────────────────
            builder.Services.AddSingleton<ILoyaltyApiService, LoyaltyApiService>();
            builder.Services.AddSingleton<ICosmosLookupService, CosmosLookupService>();
            builder.Services.AddSingleton<IAnnexCloudService, AnnexCloudService>();
            builder.Services.AddSingleton<IPointsAssignmentService, PointsAssignmentService>();
        }
    }
}

using Azure.Identity;
using BazaarVoice.Common.Helpers;
using BazaarVoice.Functions.MessageProcessor.Configuration;
using BazaarVoice.Functions.MessageProcessor.Models;
using BazaarVoice.Functions.MessageProcessor.Services;
using BazaarVoice.Infrastructure.Cosmos;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        var configuration = context.Configuration;

        // ── Application Insights ─────────────────────────────────────────
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        // ── Bind typed settings ──────────────────────────────────────────
        var settings = new MessageProcessorSettings();
        configuration.Bind(settings);
        services.AddSingleton(settings);

        // ── Register Memory Cache (for Annex Cloud action caching) ───────
        services.AddMemoryCache();

        // ── Register Cosmos DB Client ────────────────────────────────────
        services.AddSingleton(sp =>
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
        services.AddSingleton<ICosmosRepository<CustomerRecord>>(sp =>
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
        services.AddHttpClient("LoyaltyApi", (sp, client) =>
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
        services.AddHttpClient("AnnexCloud", (sp, client) =>
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
        services.AddSingleton<ILoyaltyApiService, LoyaltyApiService>();
        services.AddSingleton<ICosmosLookupService, CosmosLookupService>();
        services.AddSingleton<IAnnexCloudService, AnnexCloudService>();
        services.AddSingleton<IPointsAssignmentService, PointsAssignmentService>();
    })
    .Build();

host.Run();

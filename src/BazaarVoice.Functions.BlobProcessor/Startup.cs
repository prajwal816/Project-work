using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using BazaarVoice.Functions.BlobProcessor.Configuration;
using BazaarVoice.Functions.BlobProcessor.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(BazaarVoice.Functions.BlobProcessor.Startup))]

namespace BazaarVoice.Functions.BlobProcessor
{
    /// <summary>
    /// Configures dependency injection for Function App #1 (Blob Processor).
    /// </summary>
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;

            // ── Bind typed settings ──────────────────────────────────────────
            var settings = new BlobProcessorSettings();
            configuration.Bind(settings);
            builder.Services.AddSingleton(settings);

            // ── Register Application Insights ────────────────────────────────
            builder.Services.AddApplicationInsightsTelemetryWorkerService();

            // ── Register BlobServiceClient ───────────────────────────────────
            builder.Services.AddSingleton(sp =>
            {
                var cfg = sp.GetRequiredService<BlobProcessorSettings>();

                if (cfg.UseManagedIdentity && !string.IsNullOrWhiteSpace(cfg.BazaarVoiceStorageUri))
                {
                    // Managed Identity (recommended for production)
                    // PLACEHOLDER: e.g., "https://<storage-account-name>.blob.core.windows.net"
                    var storageUri = new Uri(cfg.BazaarVoiceStorageUri);
                    return new BlobServiceClient(storageUri, new DefaultAzureCredential());
                }

                // Connection String (for local development)
                // PLACEHOLDER: Replace with your storage account connection string
                string connectionString = configuration["BazaarVoiceStorageConnection"]
                    ?? throw new InvalidOperationException(
                        "BazaarVoiceStorageConnection is not configured.");
                return new BlobServiceClient(connectionString);
            });

            // ── Register ServiceBusClient ────────────────────────────────────
            builder.Services.AddSingleton(sp =>
            {
                var cfg = sp.GetRequiredService<BlobProcessorSettings>();

                if (cfg.UseManagedIdentity && !string.IsNullOrWhiteSpace(cfg.ServiceBusNamespace))
                {
                    // Managed Identity (recommended for production)
                    // PLACEHOLDER: e.g., "sb-bazaarvoice-loyalty-prod.servicebus.windows.net"
                    return new ServiceBusClient(
                        cfg.ServiceBusNamespace, new DefaultAzureCredential());
                }

                // Connection String (for local development)
                // PLACEHOLDER: Replace with your Service Bus connection string
                string connectionString = configuration["ServiceBusConnection"]
                    ?? throw new InvalidOperationException(
                        "ServiceBusConnection is not configured.");
                return new ServiceBusClient(connectionString);
            });

            // ── Register Services ────────────────────────────────────────────
            builder.Services.AddSingleton<IGzExtractorService, GzExtractorService>();
            builder.Services.AddSingleton<IXmlParserService, XmlParserService>();
            builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
            builder.Services.AddSingleton<IServiceBusPublisherService, ServiceBusPublisherService>();
            builder.Services.AddSingleton<ICheckpointService, CheckpointService>();
        }
    }
}

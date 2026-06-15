using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using BazaarVoice.Functions.BlobProcessor.Configuration;
using BazaarVoice.Functions.BlobProcessor.Services;
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
        var settings = new BlobProcessorSettings();
        configuration.Bind(settings);
        services.AddSingleton(settings);

        // ── Register BlobServiceClient ───────────────────────────────────
        services.AddSingleton(sp =>
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
        services.AddSingleton(sp =>
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
        services.AddSingleton<IGzExtractorService, GzExtractorService>();
        services.AddSingleton<IXmlParserService, XmlParserService>();
        services.AddSingleton<IBlobStorageService, BlobStorageService>();
        services.AddSingleton<IServiceBusPublisherService, ServiceBusPublisherService>();
        services.AddSingleton<ICheckpointService, CheckpointService>();
    })
    .Build();

host.Run();

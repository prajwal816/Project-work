namespace BazaarVoice.Functions.BlobProcessor.Configuration
{
    /// <summary>
    /// Typed configuration settings for the Blob Processor Function App.
    /// Bound from app settings / Azure App Configuration.
    /// </summary>
    public class BlobProcessorSettings
    {
        /// <summary>
        /// Connection string for the BazaarVoice storage account.
        /// </summary>
        // PLACEHOLDER: Replace with your storage account connection string
        //              or use Managed Identity with BazaarVoiceStorageUri.
        public string? BazaarVoiceStorageConnection { get; set; }

        /// <summary>
        /// URI of the BazaarVoice storage account (for Managed Identity auth).
        /// Example: "https://stbazaarvoicedev.blob.core.windows.net"
        /// </summary>
        // PLACEHOLDER: e.g., "https://<storage-account-name>.blob.core.windows.net"
        public string? BazaarVoiceStorageUri { get; set; }

        /// <summary>
        /// Connection string for the Service Bus namespace.
        /// </summary>
        // PLACEHOLDER: Replace with your Service Bus connection string
        //              or use Managed Identity with ServiceBusNamespace.
        public string? ServiceBusConnection { get; set; }

        /// <summary>
        /// Fully-qualified Service Bus namespace (for Managed Identity auth).
        /// Example: "sb-bazaarvoice-loyalty-dev.servicebus.windows.net"
        /// </summary>
        // PLACEHOLDER: e.g., "sb-bazaarvoice-loyalty-{env}.servicebus.windows.net"
        public string? ServiceBusNamespace { get; set; }

        /// <summary>
        /// Whether to use Managed Identity for Azure SDK authentication.
        /// Set to true for production deployments.
        /// </summary>
        public bool UseManagedIdentity { get; set; } = false;
    }
}

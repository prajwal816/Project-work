namespace BazaarVoice.Functions.MessageProcessor.Configuration
{
    /// <summary>
    /// Typed configuration settings for the Message Processor Function App.
    /// Bound from app settings / Azure App Configuration.
    /// </summary>
    public class MessageProcessorSettings
    {
        /// <summary>
        /// Base URL for the Common Loyalty API (APIM gateway).
        /// Used for member search (Step 3) and points assignment (Step 5).
        /// </summary>
        // PLACEHOLDER: e.g., "https://apim-common-loyalty-dev.azure-api.net"
        public string? CommonLoyaltyApiBaseUrl { get; set; }

        /// <summary>
        /// Subscription key for the Common Loyalty API (APIM).
        /// </summary>
        // PLACEHOLDER: APIM subscription key
        public string? CommonLoyaltyApiSubscriptionKey { get; set; }

        /// <summary>
        /// Base URL for direct Annex Cloud API calls.
        /// Used for retrieving Review &amp; Rating action details (Step 4).
        /// </summary>
        // PLACEHOLDER: e.g., "https://s15.socialannex.net" (as shown in architecture diagram)
        public string? AnnexCloudApiBaseUrl { get; set; }

        /// <summary>
        /// API key for Annex Cloud direct API calls, if required.
        /// </summary>
        // PLACEHOLDER: Annex Cloud API key
        public string? AnnexCloudApiKey { get; set; }

        /// <summary>
        /// Cosmos DB connection string for direct access (fallback).
        /// </summary>
        // PLACEHOLDER: Cosmos DB connection string
        public string? CosmosDbConnectionString { get; set; }

        /// <summary>
        /// Cosmos DB endpoint URI (for Managed Identity auth).
        /// </summary>
        // PLACEHOLDER: e.g., "https://cosmos-customer-dev.documents.azure.com:443/"
        public string? CosmosDbEndpoint { get; set; }

        /// <summary>
        /// Cosmos DB database name containing customer records.
        /// </summary>
        // PLACEHOLDER: e.g., "CustomerDatabase"
        public string? CosmosDbDatabaseName { get; set; }

        /// <summary>
        /// Cosmos DB container name for customer records.
        /// </summary>
        // PLACEHOLDER: e.g., "Customers"
        public string? CosmosDbContainerName { get; set; }

        /// <summary>
        /// Service Bus connection string.
        /// </summary>
        // PLACEHOLDER: Service Bus connection string
        public string? ServiceBusConnection { get; set; }

        /// <summary>
        /// Whether to use Managed Identity for Azure SDK authentication.
        /// </summary>
        public bool UseManagedIdentity { get; set; } = false;
    }
}

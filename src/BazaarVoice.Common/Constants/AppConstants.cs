namespace BazaarVoice.Common.Constants
{
    /// <summary>
    /// Centralized application constants used across both Function Apps.
    /// </summary>
    public static class AppConstants
    {
        // ── Blob Storage ──────────────────────────────────────────────────
        /// <summary>
        /// Root container name for all BazaarVoice blob storage.
        /// </summary>
        public const string BlobContainerName = "bazaarvoice";

        /// <summary>
        /// Landing zone path where Axway writes incoming .gz files.
        /// </summary>
        public const string IncomingFolder = "incoming/fromBazaarVoice";

        /// <summary>
        /// Loyalty folder where extracted XML files are written.
        /// </summary>
        public const string LoyaltyFolder = "loyalty";

        /// <summary>
        /// Archive folder for processed loyalty files (7-day retention).
        /// </summary>
        public const string LoyaltyArchiveFolder = "loyalty/archive";

        /// <summary>
        /// Existing eComm folder for HCL Commerce to read processed files.
        /// </summary>
        // PLACEHOLDER: Confirm exact eComm folder path with team.
        public const string EcommFolder = "ecomm";

        /// <summary>
        /// Checkpoint folder for restart state persistence.
        /// </summary>
        public const string CheckpointFolder = "checkpoint";

        // ── Service Bus ───────────────────────────────────────────────────
        /// <summary>
        /// Service Bus topic name for individual BazaarVoice review records.
        /// </summary>
        public const string ServiceBusTopicName = "sbt-bazaarvoice-records";

        /// <summary>
        /// Service Bus subscription name for loyalty processing.
        /// </summary>
        public const string ServiceBusSubscriptionName = "sbs-bazaarvoice-loyalty";

        /// <summary>
        /// Subject label for Service Bus messages containing review records.
        /// </summary>
        public const string ServiceBusMessageSubject = "BazaarVoiceReview";

        // ── Configuration Keys ────────────────────────────────────────────
        public const string StorageConnectionKey = "BazaarVoiceStorageConnection";
        public const string ServiceBusConnectionKey = "ServiceBusConnection";
        public const string AppInsightsConnectionKey = "APPLICATIONINSIGHTS_CONNECTION_STRING";

        // ── File Extensions ───────────────────────────────────────────────
        public const string GzExtension = ".gz";
        public const string XmlExtension = ".xml";

        // ── Annex Cloud ───────────────────────────────────────────────────
        /// <summary>
        /// The action name used to look up Review &amp; Rating action in Annex Cloud.
        /// URL-encoded as "Review%20%26%20Rating" in API calls.
        /// </summary>
        public const string ReviewAndRatingActionName = "Review & Rating";

        /// <summary>
        /// Cache key for the Review &amp; Rating action details.
        /// </summary>
        public const string ReviewRatingCacheKey = "ReviewAndRatingAction";

        /// <summary>
        /// Cache duration in minutes for the Review &amp; Rating action details.
        /// </summary>
        public const int ReviewRatingCacheDurationMinutes = 60;
    }
}

namespace BazaarVoice.Functions.BlobProcessor.Models
{
    /// <summary>
    /// Represents a single review record extracted from the BazaarVoice XML file.
    /// This is the local model used within Function App #1 before serialization
    /// to a Service Bus message.
    /// </summary>
    public class BazaarVoiceRecord
    {
        /// <summary>
        /// Unique identifier for the review from BazaarVoice.
        /// </summary>
        public string? ReviewId { get; set; }

        /// <summary>
        /// Email address of the reviewer — primary key used for Cosmos DB member lookup.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Display name / nickname of the reviewer.
        /// </summary>
        public string? UserNickname { get; set; }

        /// <summary>
        /// Numeric star rating (e.g., 1–5).
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// Full text content of the review.
        /// </summary>
        public string? ReviewText { get; set; }

        /// <summary>
        /// ISO 8601 submission timestamp from BazaarVoice.
        /// </summary>
        public string? SubmissionDate { get; set; }

        /// <summary>
        /// Product identifier associated with the review.
        /// </summary>
        public string? ProductId { get; set; }

        // PLACEHOLDER: Add additional fields from your BazaarVoice XML schema as needed.
    }
}

namespace BazaarVoice.Functions.MessageProcessor.Models
{
    /// <summary>
    /// Represents a customer record from the Cosmos DB customer database.
    /// Used to retrieve the Member Annex ID for loyalty point assignment.
    /// </summary>
    public class CustomerRecord
    {
        /// <summary>
        /// Cosmos DB document ID.
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// Customer email address (used as the lookup key).
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// The Member Annex ID — primary identifier in the Annex Cloud loyalty system.
        /// </summary>
        public string? MemberAnnexId { get; set; }

        /// <summary>
        /// Alternative field name for Annex ID (varies by integration).
        /// </summary>
        // PLACEHOLDER: Confirm which field name is used in your Cosmos DB documents.
        public string? AnnexId { get; set; }

        /// <summary>
        /// Customer first name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Customer last name.
        /// </summary>
        public string? LastName { get; set; }

        // PLACEHOLDER: Add other fields from the actual Cosmos DB customer document as needed.
    }
}

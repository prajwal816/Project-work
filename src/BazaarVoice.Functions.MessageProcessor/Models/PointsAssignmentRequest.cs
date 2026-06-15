namespace BazaarVoice.Functions.MessageProcessor.Models
{
    /// <summary>
    /// Request DTO for assigning manual loyalty points to a member.
    /// Sent to the Common Loyalty API (APIM) → Annex Cloud.
    /// </summary>
    public class PointsAssignmentRequest
    {
        /// <summary>
        /// The member's Annex ID to assign points to.
        /// </summary>
        public string? MemberAnnexId { get; set; }

        /// <summary>
        /// The action code for the Review &amp; Rating action.
        /// </summary>
        public string? ActionCode { get; set; }

        /// <summary>
        /// The BazaarVoice review ID for deduplication and audit.
        /// </summary>
        public string? ReviewId { get; set; }

        /// <summary>
        /// The product ID associated with the review.
        /// </summary>
        public string? ProductId { get; set; }

        /// <summary>
        /// The email address of the reviewer.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Optional: notes or metadata for the points transaction.
        /// </summary>
        public string? Notes { get; set; }

        // PLACEHOLDER: Add other fields required by the points assignment API.
    }

    /// <summary>
    /// Response DTO from the points assignment API call.
    /// </summary>
    public class PointsAssignmentResult
    {
        /// <summary>
        /// Whether the points assignment was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Number of points assigned to the member.
        /// </summary>
        public int PointsAssigned { get; set; }

        /// <summary>
        /// The member's new total points balance after assignment.
        /// </summary>
        public int NewBalance { get; set; }

        /// <summary>
        /// Transaction or reference ID for the points assignment.
        /// </summary>
        public string? TransactionId { get; set; }

        /// <summary>
        /// Error message if assignment failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        // PLACEHOLDER: Add other fields from the actual API response as needed.
    }
}

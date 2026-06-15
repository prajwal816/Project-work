namespace BazaarVoice.Functions.MessageProcessor.Models
{
    /// <summary>
    /// Represents member details returned from Annex Cloud via the Common Loyalty API.
    /// </summary>
    public class AnnexMemberDetails
    {
        /// <summary>
        /// The Member Annex ID in the Annex Cloud system.
        /// </summary>
        public string? MemberAnnexId { get; set; }

        /// <summary>
        /// Alternative field name for member ID.
        /// </summary>
        public string? AnnexId { get; set; }

        /// <summary>
        /// Member's email address.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// Member's first name.
        /// </summary>
        public string? FirstName { get; set; }

        /// <summary>
        /// Member's last name.
        /// </summary>
        public string? LastName { get; set; }

        /// <summary>
        /// Current loyalty points balance.
        /// </summary>
        public int PointsBalance { get; set; }

        /// <summary>
        /// Member's loyalty tier/status.
        /// </summary>
        public string? MemberTier { get; set; }

        // PLACEHOLDER: Add other fields from the actual Annex Cloud member response as needed.
    }
}

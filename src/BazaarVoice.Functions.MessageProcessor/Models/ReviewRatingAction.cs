namespace BazaarVoice.Functions.MessageProcessor.Models
{
    /// <summary>
    /// Represents the "Review &amp; Rating" action configuration from Annex Cloud.
    /// Retrieved via GET /api/3.0/actions?action_name=Review%20%26%20Rating
    /// </summary>
    public class ReviewRatingAction
    {
        /// <summary>
        /// The unique action code used when assigning loyalty points.
        /// </summary>
        public string? ActionCode { get; set; }

        /// <summary>
        /// The action ID in Annex Cloud.
        /// </summary>
        public string? ActionId { get; set; }

        /// <summary>
        /// Display name of the action (e.g., "Review &amp; Rating").
        /// </summary>
        public string? ActionName { get; set; }

        /// <summary>
        /// Number of points awarded for this action.
        /// </summary>
        public int PointsValue { get; set; }

        /// <summary>
        /// Whether this action is currently active/enabled.
        /// </summary>
        public bool IsActive { get; set; }

        // PLACEHOLDER: Add other fields from the actual Annex Cloud actions response as needed.
    }

    /// <summary>
    /// Response wrapper from the Annex Cloud GET /api/3.0/actions endpoint.
    /// </summary>
    public class AnnexCloudActionResponse
    {
        /// <summary>
        /// Whether the API call was successful.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// List of actions returned by the API.
        /// </summary>
        public List<ReviewRatingAction>? Actions { get; set; }

        /// <summary>
        /// Single action data (some API responses use "data" instead of array).
        /// </summary>
        // PLACEHOLDER: Adjust based on actual response structure.
        public ReviewRatingAction? Data { get; set; }
    }
}

using BazaarVoice.Functions.MessageProcessor.Models;

namespace BazaarVoice.Functions.MessageProcessor.Services
{
    /// <summary>
    /// Retrieves action details from Annex Cloud — specifically the
    /// "Review &amp; Rating" action code needed for points assignment.
    /// </summary>
    public interface IAnnexCloudService
    {
        /// <summary>
        /// Gets the Review &amp; Rating action details from Annex Cloud.
        /// Results are cached to reduce API calls.
        /// GET https://s15.socialannex.net/api/3.0/actions?action_name=Review%20%26%20Rating
        /// </summary>
        /// <returns>The <see cref="ReviewRatingAction"/> details, or null if not found.</returns>
        Task<ReviewRatingAction?> GetReviewAndRatingActionAsync();
    }
}

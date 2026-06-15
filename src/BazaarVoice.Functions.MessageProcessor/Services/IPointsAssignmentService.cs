using BazaarVoice.Common.Models;
using BazaarVoice.Functions.MessageProcessor.Models;

namespace BazaarVoice.Functions.MessageProcessor.Services
{
    /// <summary>
    /// Assigns manual loyalty points to a member for a BazaarVoice review
    /// using the Review &amp; Rating Action Code via the Common Loyalty API (APIM).
    /// </summary>
    public interface IPointsAssignmentService
    {
        /// <summary>
        /// Assigns points to a member for a review submission.
        /// </summary>
        /// <param name="memberAnnexId">The member's Annex ID.</param>
        /// <param name="actionCode">The Review &amp; Rating action code.</param>
        /// <param name="record">The original BazaarVoice review record.</param>
        /// <returns>The result of the points assignment.</returns>
        Task<PointsAssignmentResult> AssignPointsAsync(
            string memberAnnexId,
            string actionCode,
            BazaarVoiceXmlRecord record);
    }
}

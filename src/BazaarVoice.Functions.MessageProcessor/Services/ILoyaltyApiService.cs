namespace BazaarVoice.Functions.MessageProcessor.Services
{
    /// <summary>
    /// Searches for a customer member in Cosmos DB via the Common Loyalty API (APIM).
    /// Flow: Common Loyalty API (APIM) → Loyalty Service → SearchCosmos → Cosmos DB
    /// </summary>
    public interface ILoyaltyApiService
    {
        /// <summary>
        /// Looks up a member's Annex ID by their email address via the Common Loyalty API.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>The Member Annex ID, or null if not found.</returns>
        Task<string?> GetMemberAnnexIdByEmailAsync(string email);
    }
}

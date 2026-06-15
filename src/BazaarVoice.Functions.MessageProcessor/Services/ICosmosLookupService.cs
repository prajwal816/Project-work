using BazaarVoice.Functions.MessageProcessor.Models;

namespace BazaarVoice.Functions.MessageProcessor.Services
{
    /// <summary>
    /// Direct Cosmos DB lookup for customer records.
    /// Used as a fallback if the Common Loyalty API is unavailable.
    /// </summary>
    public interface ICosmosLookupService
    {
        /// <summary>
        /// Looks up a customer record by email directly from Cosmos DB.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>The customer record, or null if not found.</returns>
        Task<CustomerRecord?> GetCustomerByEmailAsync(string email);
    }
}

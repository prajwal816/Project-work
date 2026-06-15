using BazaarVoice.Functions.MessageProcessor.Models;
using BazaarVoice.Infrastructure.Cosmos;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.MessageProcessor.Services
{
    /// <summary>
    /// Direct Cosmos DB lookup for customer records using email as the search key.
    /// Used as a fallback if the Common Loyalty API is unavailable.
    /// </summary>
    public class CosmosLookupService : ICosmosLookupService
    {
        private readonly ICosmosRepository<CustomerRecord> _cosmosRepository;
        private readonly ILogger<CosmosLookupService> _logger;

        public CosmosLookupService(
            ICosmosRepository<CustomerRecord> cosmosRepository,
            ILogger<CosmosLookupService> logger)
        {
            _cosmosRepository = cosmosRepository;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<CustomerRecord?> GetCustomerByEmailAsync(string email)
        {
            try
            {
                _logger.LogInformation(
                    "Looking up customer directly in Cosmos DB for email: {Email}", email);

                // PLACEHOLDER: Update the SQL query to match your Cosmos DB
                //      document structure and partition key strategy.
                var query = "SELECT * FROM c WHERE c.email = @email";
                var parameters = new Dictionary<string, object>
                {
                    { "email", email.ToLowerInvariant() }
                };

                var customer = await _cosmosRepository.FindFirstOrDefaultAsync(query, parameters);

                if (customer == null)
                {
                    _logger.LogWarning(
                        "No customer found in Cosmos DB for email: {Email}", email);
                }
                else
                {
                    _logger.LogInformation(
                        "Found customer in Cosmos DB. MemberAnnexId: {MemberAnnexId}",
                        customer.MemberAnnexId ?? customer.AnnexId);
                }

                return customer;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error querying Cosmos DB for email: {Email}", email);
                throw;
            }
        }
    }
}

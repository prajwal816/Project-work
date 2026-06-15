using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Infrastructure.Cosmos
{
    /// <summary>
    /// Generic Cosmos DB repository implementation using the Azure Cosmos DB SDK.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    public class CosmosRepository<T> : ICosmosRepository<T> where T : class
    {
        private readonly Container _container;
        private readonly ILogger<CosmosRepository<T>> _logger;

        public CosmosRepository(
            CosmosClient cosmosClient,
            string databaseName,
            string containerName,
            ILogger<CosmosRepository<T>> logger)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<T?> GetByIdAsync(string id, string partitionKey)
        {
            try
            {
                var response = await _container.ReadItemAsync<T>(
                    id, new PartitionKey(partitionKey));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning(
                    "Document not found. Id: {Id}, PartitionKey: {PartitionKey}",
                    id, partitionKey);
                return null;
            }
            catch (CosmosException ex)
            {
                _logger.LogError(ex,
                    "Cosmos DB error reading document. Id: {Id}, StatusCode: {StatusCode}",
                    id, (int)ex.StatusCode);
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<T>> QueryAsync(
            string query, Dictionary<string, object>? parameters = null)
        {
            var queryDefinition = new QueryDefinition(query);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    queryDefinition = queryDefinition.WithParameter($"@{param.Key}", param.Value);
                }
            }

            var results = new List<T>();
            using var iterator = _container.GetItemQueryIterator<T>(queryDefinition);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response);

                _logger.LogDebug(
                    "Query consumed {RequestCharge} RU/s, returned {Count} items",
                    response.RequestCharge, response.Count);
            }

            return results;
        }

        /// <inheritdoc />
        public async Task<T?> FindFirstOrDefaultAsync(
            string query, Dictionary<string, object>? parameters = null)
        {
            var results = await QueryAsync(query, parameters);
            return results.FirstOrDefault();
        }

        /// <inheritdoc />
        public async Task<T> UpsertAsync(T item, string partitionKey)
        {
            var response = await _container.UpsertItemAsync(
                item, new PartitionKey(partitionKey));

            _logger.LogDebug(
                "Upserted document. StatusCode: {StatusCode}, RU: {RequestCharge}",
                (int)response.StatusCode, response.RequestCharge);

            return response.Resource;
        }
    }
}

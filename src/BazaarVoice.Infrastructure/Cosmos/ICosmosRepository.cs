namespace BazaarVoice.Infrastructure.Cosmos
{
    /// <summary>
    /// Generic repository interface for Cosmos DB operations.
    /// </summary>
    /// <typeparam name="T">The document type.</typeparam>
    public interface ICosmosRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves a document by its ID and partition key.
        /// </summary>
        Task<T?> GetByIdAsync(string id, string partitionKey);

        /// <summary>
        /// Queries documents using a SQL query string.
        /// </summary>
        Task<IEnumerable<T>> QueryAsync(string query, Dictionary<string, object>? parameters = null);

        /// <summary>
        /// Finds a single document matching the query, or null if not found.
        /// </summary>
        Task<T?> FindFirstOrDefaultAsync(string query, Dictionary<string, object>? parameters = null);

        /// <summary>
        /// Creates or updates a document.
        /// </summary>
        Task<T> UpsertAsync(T item, string partitionKey);
    }
}

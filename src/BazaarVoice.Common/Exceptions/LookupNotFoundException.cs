namespace BazaarVoice.Common.Exceptions
{
    /// <summary>
    /// Thrown when a member lookup by email fails to find a matching record
    /// in Cosmos DB via the Common Loyalty API. Messages with this exception
    /// will be dead-lettered after max delivery attempts.
    /// </summary>
    [Serializable]
    public class LookupNotFoundException : Exception
    {
        /// <summary>
        /// The email address that was not found.
        /// </summary>
        public string? Email { get; }

        public LookupNotFoundException()
        {
        }

        public LookupNotFoundException(string message)
            : base(message)
        {
        }

        public LookupNotFoundException(string message, string email)
            : base(message)
        {
            Email = email;
        }

        public LookupNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

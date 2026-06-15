namespace BazaarVoice.Common.Exceptions
{
    /// <summary>
    /// Thrown when communication with an external API fails due to HTTP errors,
    /// timeouts, or network issues. These failures are typically retryable.
    /// </summary>
    [Serializable]
    public class ApiCommunicationException : Exception
    {
        /// <summary>
        /// HTTP status code returned by the API, if available.
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// The API endpoint that was being called when the error occurred.
        /// </summary>
        public string? Endpoint { get; }

        public ApiCommunicationException()
        {
        }

        public ApiCommunicationException(string message)
            : base(message)
        {
        }

        public ApiCommunicationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ApiCommunicationException(string message, int statusCode, string? endpoint = null)
            : base(message)
        {
            StatusCode = statusCode;
            Endpoint = endpoint;
        }
    }
}

using System.Net;
using Polly;
using Polly.Extensions.Http;
using Polly.Retry;

namespace BazaarVoice.Common.Helpers
{
    /// <summary>
    /// Factory for creating Polly retry and circuit-breaker policies for HTTP calls.
    /// </summary>
    public static class RetryHelper
    {
        /// <summary>
        /// Creates an exponential backoff retry policy for HTTP requests.
        /// Retries on transient HTTP errors (5xx, 408) and <see cref="HttpRequestException"/>.
        /// </summary>
        /// <param name="retryCount">Number of retry attempts (default: 3).</param>
        /// <returns>An async retry policy for <see cref="HttpResponseMessage"/>.</returns>
        public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy(int retryCount = 3)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        // Logging is handled by the caller; this is a hook for custom logic.
                    });
        }

        /// <summary>
        /// Creates a circuit breaker policy that opens after consecutive failures
        /// and pauses requests for a cooldown period.
        /// </summary>
        /// <param name="handledEventsBeforeBreaking">Failures before opening circuit (default: 5).</param>
        /// <param name="durationOfBreakSeconds">Cooldown in seconds (default: 30).</param>
        /// <returns>An async circuit breaker policy for <see cref="HttpResponseMessage"/>.</returns>
        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
            int handledEventsBeforeBreaking = 5,
            int durationOfBreakSeconds = 30)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsBeforeBreaking,
                    TimeSpan.FromSeconds(durationOfBreakSeconds));
        }

        /// <summary>
        /// Creates a generic exponential backoff retry policy for any async operation.
        /// </summary>
        /// <param name="retryCount">Number of retry attempts (default: 3).</param>
        /// <returns>An async retry policy.</returns>
        public static AsyncRetryPolicy GetGenericRetryPolicy(int retryCount = 3)
        {
            return Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }
}

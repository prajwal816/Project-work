using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace BazaarVoice.Infrastructure.Http
{
    /// <summary>
    /// A resilient HTTP client with built-in retry and circuit-breaker policies.
    /// Designed for use with external APIs (Common Loyalty API, Annex Cloud).
    /// </summary>
    public class ResilientHttpClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ResilientHttpClient> _logger;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly AsyncCircuitBreakerPolicy<HttpResponseMessage> _circuitBreakerPolicy;

        public ResilientHttpClient(
            HttpClient httpClient,
            ILogger<ResilientHttpClient> logger,
            int retryCount = 3,
            int circuitBreakerThreshold = 5,
            int circuitBreakerDurationSeconds = 30)
        {
            _httpClient = httpClient;
            _logger = logger;

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode &&
                    ((int)r.StatusCode >= 500 || r.StatusCode == System.Net.HttpStatusCode.RequestTimeout))
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryAttempt, context) =>
                    {
                        _logger.LogWarning(
                            "HTTP retry attempt {RetryAttempt} after {Delay}s. " +
                            "StatusCode: {StatusCode}, Exception: {Exception}",
                            retryAttempt,
                            timespan.TotalSeconds,
                            outcome.Result?.StatusCode,
                            outcome.Exception?.Message);
                    });

            _circuitBreakerPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .CircuitBreakerAsync(
                    circuitBreakerThreshold,
                    TimeSpan.FromSeconds(circuitBreakerDurationSeconds),
                    onBreak: (outcome, duration) =>
                    {
                        _logger.LogError(
                            "Circuit breaker opened for {Duration}s. " +
                            "StatusCode: {StatusCode}",
                            duration.TotalSeconds,
                            outcome.Result?.StatusCode);
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit breaker reset.");
                    });
        }

        /// <summary>
        /// Sends a GET request with retry and circuit-breaker policies.
        /// </summary>
        public async Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return await _retryPolicy.WrapAsync(_circuitBreakerPolicy)
                .ExecuteAsync(() => _httpClient.GetAsync(requestUri));
        }

        /// <summary>
        /// Sends a POST request with retry and circuit-breaker policies.
        /// </summary>
        public async Task<HttpResponseMessage> PostAsync(
            string requestUri, HttpContent content)
        {
            return await _retryPolicy.WrapAsync(_circuitBreakerPolicy)
                .ExecuteAsync(() => _httpClient.PostAsync(requestUri, content));
        }

        /// <summary>
        /// Sends a PUT request with retry and circuit-breaker policies.
        /// </summary>
        public async Task<HttpResponseMessage> PutAsync(
            string requestUri, HttpContent content)
        {
            return await _retryPolicy.WrapAsync(_circuitBreakerPolicy)
                .ExecuteAsync(() => _httpClient.PutAsync(requestUri, content));
        }
    }
}

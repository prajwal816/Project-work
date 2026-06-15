using System.Net.Http.Json;
using BazaarVoice.Common.Constants;
using BazaarVoice.Common.Exceptions;
using BazaarVoice.Functions.MessageProcessor.Configuration;
using BazaarVoice.Functions.MessageProcessor.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.MessageProcessor.Services
{
    /// <summary>
    /// Retrieves action details from Annex Cloud.
    /// Caches the Review &amp; Rating action to minimize API calls (as noted in architecture diagram).
    /// </summary>
    public class AnnexCloudService : IAnnexCloudService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AnnexCloudService> _logger;
        private readonly MessageProcessorSettings _settings;

        public AnnexCloudService(
            IHttpClientFactory httpClientFactory,
            IMemoryCache cache,
            MessageProcessorSettings settings,
            ILogger<AnnexCloudService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("AnnexCloud");
            _cache = cache;
            _settings = settings;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<ReviewRatingAction?> GetReviewAndRatingActionAsync()
        {
            // ── Check cache first (as noted in diagram: "Can be cached") ─────
            if (_cache.TryGetValue(AppConstants.ReviewRatingCacheKey, out ReviewRatingAction? cachedAction))
            {
                _logger.LogDebug("Returning cached Review & Rating action");
                return cachedAction;
            }

            try
            {
                // As per architecture diagram:
                // GET https://s15.socialannex.net/api/3.0/actions?action_name=Review%20%26%20Rating

                // PLACEHOLDER: Update base URL if different from what's shown in diagram.
                // This may go through APIM depending on your architecture.
                var actionNameEncoded = Uri.EscapeDataString(AppConstants.ReviewAndRatingActionName);
                var requestUrl = $"{_settings.AnnexCloudApiBaseUrl}/api/3.0/actions?action_name={actionNameEncoded}";

                _logger.LogInformation(
                    "Fetching Review & Rating action from Annex Cloud: {Url}", requestUrl);

                var response = await _httpClient.GetAsync(requestUrl);
                response.EnsureSuccessStatusCode();

                // PLACEHOLDER: Update the response parsing based on actual Annex Cloud API response structure.
                var result = await response.Content.ReadFromJsonAsync<AnnexCloudActionResponse>();

                ReviewRatingAction? action = null;

                // Try to extract action from different possible response structures
                if (result?.Actions != null && result.Actions.Any())
                {
                    action = result.Actions.FirstOrDefault(a =>
                        a.ActionName?.Equals(AppConstants.ReviewAndRatingActionName,
                            StringComparison.OrdinalIgnoreCase) == true);
                }
                else if (result?.Data != null)
                {
                    action = result.Data;
                }

                if (action != null)
                {
                    // Cache the action for configured duration
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(TimeSpan.FromMinutes(
                            AppConstants.ReviewRatingCacheDurationMinutes));

                    _cache.Set(AppConstants.ReviewRatingCacheKey, action, cacheOptions);

                    _logger.LogInformation(
                        "Cached Review & Rating action. ActionCode: {ActionCode}, " +
                        "PointsValue: {PointsValue}",
                        action.ActionCode, action.PointsValue);
                }
                else
                {
                    _logger.LogWarning("Review & Rating action not found in Annex Cloud response");
                }

                return action;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "HTTP error while fetching Review & Rating action from Annex Cloud");
                throw new ApiCommunicationException(
                    $"Failed to reach Annex Cloud API: {ex.Message}", ex);
            }
        }
    }
}

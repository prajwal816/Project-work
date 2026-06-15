using System.Net.Http.Json;
using BazaarVoice.Common.Exceptions;
using BazaarVoice.Functions.MessageProcessor.Configuration;
using BazaarVoice.Functions.MessageProcessor.Models;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.MessageProcessor.Services
{
    /// <summary>
    /// Looks up customer members in Cosmos DB via the Common Loyalty API (APIM).
    /// Flow: Common Loyalty API (APIM) → Loyalty Service SearchCosmos → Customer Database (Cosmos)
    /// </summary>
    public class LoyaltyApiService : ILoyaltyApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LoyaltyApiService> _logger;
        private readonly MessageProcessorSettings _settings;

        public LoyaltyApiService(
            IHttpClientFactory httpClientFactory,
            MessageProcessorSettings settings,
            ILogger<LoyaltyApiService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("LoyaltyApi");
            _settings = settings;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<string?> GetMemberAnnexIdByEmailAsync(string email)
        {
            try
            {
                // PLACEHOLDER: Update the API endpoint URL below to match your
                //      Common Loyalty API (APIM) endpoint for member search.
                // Flow: Common Loyalty API (APIM) → Loyalty Service SearchCosmos → Customer Database (Cosmos)

                var requestUrl = $"{_settings.CommonLoyaltyApiBaseUrl}/api/members/search?email={Uri.EscapeDataString(email)}";
                // PLACEHOLDER: Actual endpoint path for member search by email
                //      e.g., "/api/v1/loyalty/members/search?email={email}"

                _logger.LogInformation(
                    "Searching for member by email via APIM: {Email}", email);

                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        _logger.LogWarning(
                            "Member not found for email: {Email}", email);
                        return null;
                    }

                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Loyalty API returned {StatusCode} for email: {Email}. Error: {Error}",
                        response.StatusCode, email, errorContent);
                    throw new ApiCommunicationException(
                        $"Loyalty API error: {response.StatusCode} - {errorContent}");
                }

                var result = await response.Content.ReadFromJsonAsync<MemberSearchResponse>();

                // PLACEHOLDER: Update property name based on actual API response structure.
                return result?.MemberAnnexId ?? result?.AnnexId;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "HTTP error while searching for member: {Email}", email);
                throw new ApiCommunicationException(
                    $"Failed to reach Loyalty API: {ex.Message}", ex);
            }
        }
    }

    /// <summary>
    /// Response model from the Common Loyalty API member search endpoint.
    /// </summary>
    // PLACEHOLDER: Update this model to match actual API response.
    public class MemberSearchResponse
    {
        public string? MemberAnnexId { get; set; }
        public string? AnnexId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        // PLACEHOLDER: Add other fields from the actual API response.
    }
}

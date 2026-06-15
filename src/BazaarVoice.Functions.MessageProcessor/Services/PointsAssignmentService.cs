using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using BazaarVoice.Common.Exceptions;
using BazaarVoice.Common.Models;
using BazaarVoice.Functions.MessageProcessor.Configuration;
using BazaarVoice.Functions.MessageProcessor.Models;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.MessageProcessor.Services
{
    /// <summary>
    /// Assigns manual loyalty points to a member for a BazaarVoice review
    /// using the Review &amp; Rating Action Code.
    /// Flow: Common Loyalty API (APIM) → Annex Cloud
    /// </summary>
    public class PointsAssignmentService : IPointsAssignmentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PointsAssignmentService> _logger;
        private readonly MessageProcessorSettings _settings;

        public PointsAssignmentService(
            IHttpClientFactory httpClientFactory,
            MessageProcessorSettings settings,
            ILogger<PointsAssignmentService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("LoyaltyApi");
            _settings = settings;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<PointsAssignmentResult> AssignPointsAsync(
            string memberAnnexId,
            string actionCode,
            BazaarVoiceXmlRecord record)
        {
            try
            {
                // PLACEHOLDER: Update the API endpoint URL below to match your
                //      Common Loyalty API (APIM) endpoint for assigning manual points.
                var requestUrl = $"{_settings.CommonLoyaltyApiBaseUrl}/api/members/{memberAnnexId}/points/assign";
                // PLACEHOLDER: Actual endpoint path, e.g.:
                //      "/api/v1/loyalty/members/{memberAnnexId}/actions"
                //      "/api/v1/loyalty/points/manual"

                var request = new PointsAssignmentRequest
                {
                    MemberAnnexId = memberAnnexId,
                    ActionCode = actionCode,
                    ReviewId = record.ReviewId,
                    ProductId = record.ProductId,
                    Email = record.Email,
                    Notes = $"BazaarVoice Review & Rating - ReviewId: {record.ReviewId}, " +
                            $"Product: {record.ProductId}, Rating: {record.Rating}"
                };

                _logger.LogInformation(
                    "Assigning points to Member: {MemberAnnexId} " +
                    "with ActionCode: {ActionCode} for Review: {ReviewId}",
                    memberAnnexId, actionCode, record.ReviewId);

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(request),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync(requestUrl, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError(
                        "Points assignment failed. StatusCode: {StatusCode}, " +
                        "Member: {MemberAnnexId}, Error: {Error}",
                        response.StatusCode, memberAnnexId, errorContent);

                    return new PointsAssignmentResult
                    {
                        Success = false,
                        ErrorMessage = $"API returned {response.StatusCode}: {errorContent}"
                    };
                }

                // PLACEHOLDER: Update response parsing based on actual API response structure.
                var result = await response.Content.ReadFromJsonAsync<PointsAssignmentResult>();

                if (result == null)
                {
                    result = new PointsAssignmentResult
                    {
                        Success = true,
                        PointsAssigned = 0 // PLACEHOLDER: Extract from actual response
                    };
                }
                else
                {
                    result.Success = true;
                }

                _logger.LogInformation(
                    "Points assigned successfully. Member: {MemberAnnexId}, " +
                    "Points: {PointsAssigned}, TransactionId: {TransactionId}",
                    memberAnnexId, result.PointsAssigned, result.TransactionId);

                return result;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex,
                    "HTTP error while assigning points. Member: {MemberAnnexId}",
                    memberAnnexId);
                throw new ApiCommunicationException(
                    $"Failed to assign points: {ex.Message}", ex);
            }
        }
    }
}

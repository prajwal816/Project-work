using System.Text.Json;
using BazaarVoice.Common.Constants;
using BazaarVoice.Common.Exceptions;
using BazaarVoice.Common.Models;
using BazaarVoice.Functions.MessageProcessor.Services;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.MessageProcessor.Functions
{
    /// <summary>
    /// Azure Function triggered by Service Bus messages from the BazaarVoice topic.
    /// Processes individual review records:
    ///   Step 3: Lookup member Annex ID by email via Common Loyalty API → Cosmos DB
    ///   Step 4: Get Review &amp; Rating action details from Annex Cloud (cached)
    ///   Step 5: Assign manual loyalty points using the action code
    /// </summary>
    public class ServiceBusTriggerFunction
    {
        private readonly ILoyaltyApiService _loyaltyApiService;
        private readonly IAnnexCloudService _annexCloudService;
        private readonly IPointsAssignmentService _pointsAssignmentService;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<ServiceBusTriggerFunction> _logger;

        public ServiceBusTriggerFunction(
            ILoyaltyApiService loyaltyApiService,
            IAnnexCloudService annexCloudService,
            IPointsAssignmentService pointsAssignmentService,
            TelemetryClient telemetryClient,
            ILogger<ServiceBusTriggerFunction> logger)
        {
            _loyaltyApiService = loyaltyApiService;
            _annexCloudService = annexCloudService;
            _pointsAssignmentService = pointsAssignmentService;
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        [FunctionName("BazaarVoiceMessageProcessor")]
        public async Task RunAsync(
            [ServiceBusTrigger(
                topicName: AppConstants.ServiceBusTopicName,       // "sbt-bazaarvoice-records"
                subscriptionName: AppConstants.ServiceBusSubscriptionName, // "sbs-bazaarvoice-loyalty"
                Connection = "ServiceBusConnection")]              // PLACEHOLDER: Connection string name
            Azure.Messaging.ServiceBus.ServiceBusReceivedMessage message,
            ILogger log)
        {
            var correlationId = message.CorrelationId ?? Guid.NewGuid().ToString();
            using var operation = _telemetryClient.StartOperation<RequestTelemetry>(
                "MessageProcessor", correlationId);

            _logger.LogInformation(
                "Processing Service Bus message. MessageId: {MessageId}, " +
                "CorrelationId: {CorrelationId}",
                message.MessageId, correlationId);

            BazaarVoiceXmlRecord? record = null;

            try
            {
                // ── Step 1: Deserialize the message ──────────────────────────
                var messageBody = message.Body.ToString();
                record = JsonSerializer.Deserialize<BazaarVoiceXmlRecord>(messageBody);

                if (record == null || string.IsNullOrWhiteSpace(record.Email))
                {
                    _logger.LogError(
                        "Invalid message body or missing email. MessageId: {MessageId}",
                        message.MessageId);
                    throw new ProcessingException("Invalid record: missing email field");
                }

                _logger.LogInformation(
                    "Processing review for Email: {Email}, ReviewId: {ReviewId}",
                    record.Email, record.ReviewId);

                // ══════════════════════════════════════════════════════════════
                // Step 3: Search using email to get Member Annex ID
                //         Flow: Common Loyalty API (APIM) → Loyalty Service
                //               → SearchCosmos → Cosmos DB
                // ══════════════════════════════════════════════════════════════
                string? memberAnnexId = await _loyaltyApiService
                    .GetMemberAnnexIdByEmailAsync(record.Email);

                if (string.IsNullOrWhiteSpace(memberAnnexId))
                {
                    _logger.LogWarning(
                        "No member found in Cosmos for email: {Email}. " +
                        "Message will be dead-lettered.",
                        record.Email);
                    throw new LookupNotFoundException(
                        $"Member not found for email: {record.Email}",
                        record.Email);
                }

                _logger.LogInformation(
                    "Found Member Annex ID: {MemberAnnexId} for Email: {Email}",
                    memberAnnexId, record.Email);

                // ══════════════════════════════════════════════════════════════
                // Step 4: Get 'Review & Ratings' action details from Annex Cloud
                //         GET .../api/3.0/actions?action_name=Review%20%26%20Rating
                //         (Can be cached)
                // ══════════════════════════════════════════════════════════════
                var reviewRatingAction = await _annexCloudService
                    .GetReviewAndRatingActionAsync();

                if (reviewRatingAction == null ||
                    string.IsNullOrWhiteSpace(reviewRatingAction.ActionCode))
                {
                    _logger.LogError(
                        "Failed to retrieve Review & Rating action details " +
                        "from Annex Cloud");
                    throw new ApiCommunicationException(
                        "Review & Rating action not found in Annex Cloud");
                }

                _logger.LogInformation(
                    "Retrieved Review & Rating Action Code: {ActionCode}",
                    reviewRatingAction.ActionCode);

                // ══════════════════════════════════════════════════════════════
                // Step 5: Assign manual points to member using
                //         Review & Rating Action Code
                //         Via Common Loyalty API (APIM) → Annex Cloud
                // ══════════════════════════════════════════════════════════════
                var pointsResult = await _pointsAssignmentService.AssignPointsAsync(
                    memberAnnexId: memberAnnexId,
                    actionCode: reviewRatingAction.ActionCode,
                    record: record);

                if (!pointsResult.Success)
                {
                    _logger.LogError(
                        "Failed to assign points for Member: {MemberAnnexId}, " +
                        "Error: {Error}",
                        memberAnnexId, pointsResult.ErrorMessage);
                    throw new ProcessingException(
                        $"Points assignment failed: {pointsResult.ErrorMessage}");
                }

                _logger.LogInformation(
                    "Successfully assigned points to Member: {MemberAnnexId} " +
                    "for Review: {ReviewId}. Points: {Points}",
                    memberAnnexId, record.ReviewId, pointsResult.PointsAssigned);

                // ── Telemetry ────────────────────────────────────────────────
                _telemetryClient.TrackMetric("PointsAssigned", pointsResult.PointsAssigned);
                _telemetryClient.TrackEvent("ReviewProcessedSuccessfully",
                    new Dictionary<string, string>
                    {
                        ["Email"] = record.Email,
                        ["MemberAnnexId"] = memberAnnexId,
                        ["ReviewId"] = record.ReviewId ?? "UNKNOWN"
                    });
            }
            catch (LookupNotFoundException ex)
            {
                // Member not found — will go to Dead Letter Queue after max retries
                _logger.LogWarning(ex, "Member lookup failed. Will dead-letter.");
                _telemetryClient.TrackException(ex);
                throw; // Let Service Bus handle retry/dead-letter
            }
            catch (ApiCommunicationException ex)
            {
                // API communication issue — retry may help
                _logger.LogError(ex, "API communication error. Will retry.");
                _telemetryClient.TrackException(ex);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error processing message. Email: {Email}, " +
                    "MessageId: {MessageId}",
                    record?.Email ?? "UNKNOWN", message.MessageId);
                _telemetryClient.TrackException(ex);
                operation.Telemetry.Success = false;
                throw; // Dead-letter after max delivery attempts
            }
        }
    }
}

using System.Text.Json;
using Azure.Messaging.ServiceBus;
using BazaarVoice.Common.Constants;
using BazaarVoice.Functions.BlobProcessor.Models;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.BlobProcessor.Services
{
    /// <summary>
    /// Publishes individual BazaarVoice review records as messages to the
    /// Service Bus topic for downstream processing by Function App #2.
    /// </summary>
    public class ServiceBusPublisherService : IServiceBusPublisherService
    {
        private readonly ServiceBusSender _sender;
        private readonly ILogger<ServiceBusPublisherService> _logger;

        public ServiceBusPublisherService(
            ServiceBusClient serviceBusClient,
            ILogger<ServiceBusPublisherService> logger)
        {
            _sender = serviceBusClient.CreateSender(AppConstants.ServiceBusTopicName);
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task PublishRecordAsync(BazaarVoiceRecord record, string operationId)
        {
            var messageBody = JsonSerializer.Serialize(record);

            var message = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
                Subject = AppConstants.ServiceBusMessageSubject,
                CorrelationId = operationId,
                MessageId = $"{record.ReviewId}-{Guid.NewGuid():N}",
                ApplicationProperties =
                {
                    ["Email"] = record.Email ?? string.Empty,
                    ["ReviewId"] = record.ReviewId ?? string.Empty,
                    ["ProcessedAt"] = DateTime.UtcNow.ToString("O")
                }
            };

            // PLACEHOLDER: Configure message TTL if business requirement exists.
            // message.TimeToLive = TimeSpan.FromDays(7);

            await _sender.SendMessageAsync(message);

            _logger.LogInformation(
                "Published message to Service Bus. Email: {Email}, ReviewId: {ReviewId}",
                record.Email, record.ReviewId);
        }
    }
}

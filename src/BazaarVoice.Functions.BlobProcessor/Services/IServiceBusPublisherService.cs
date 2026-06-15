using BazaarVoice.Functions.BlobProcessor.Models;

namespace BazaarVoice.Functions.BlobProcessor.Services
{
    /// <summary>
    /// Publishes individual BazaarVoice review records to the Service Bus topic.
    /// </summary>
    public interface IServiceBusPublisherService
    {
        /// <summary>
        /// Publishes a single record to the Service Bus topic for downstream processing.
        /// </summary>
        /// <param name="record">The review record to publish.</param>
        /// <param name="operationId">Correlation ID for end-to-end tracing.</param>
        Task PublishRecordAsync(BazaarVoiceRecord record, string operationId);
    }
}

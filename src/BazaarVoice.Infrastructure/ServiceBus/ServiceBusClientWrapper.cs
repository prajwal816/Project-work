using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Infrastructure.ServiceBus
{
    /// <summary>
    /// Wraps the Azure <see cref="ServiceBusSender"/> for testability and
    /// adds logging around send operations.
    /// </summary>
    public class ServiceBusClientWrapper : IServiceBusClientWrapper, IAsyncDisposable
    {
        private readonly ServiceBusSender _sender;
        private readonly ILogger<ServiceBusClientWrapper> _logger;

        public ServiceBusClientWrapper(
            ServiceBusClient client,
            string topicOrQueueName,
            ILogger<ServiceBusClientWrapper> logger)
        {
            _sender = client.CreateSender(topicOrQueueName);
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task SendMessageAsync(ServiceBusMessage message)
        {
            _logger.LogDebug(
                "Sending message to Service Bus. MessageId: {MessageId}, Subject: {Subject}",
                message.MessageId, message.Subject);

            await _sender.SendMessageAsync(message);

            _logger.LogInformation(
                "Message sent successfully. MessageId: {MessageId}", message.MessageId);
        }

        /// <inheritdoc />
        public async Task SendMessagesAsync(IEnumerable<ServiceBusMessage> messages)
        {
            using ServiceBusMessageBatch batch = await _sender.CreateMessageBatchAsync();
            int count = 0;

            foreach (var message in messages)
            {
                if (!batch.TryAddMessage(message))
                {
                    // If the batch is full, send it and start a new one
                    await _sender.SendMessagesAsync(batch);
                    _logger.LogInformation("Sent batch of {Count} messages", count);

                    batch.Dispose();
                    var newBatch = await _sender.CreateMessageBatchAsync();

                    if (!newBatch.TryAddMessage(message))
                    {
                        throw new InvalidOperationException(
                            $"Message too large for Service Bus batch. MessageId: {message.MessageId}");
                    }

                    count = 1;
                    continue;
                }

                count++;
            }

            if (count > 0)
            {
                await _sender.SendMessagesAsync(batch);
                _logger.LogInformation("Sent final batch of {Count} messages", count);
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _sender.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}

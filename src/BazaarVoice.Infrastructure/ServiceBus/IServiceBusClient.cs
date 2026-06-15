using Azure.Messaging.ServiceBus;

namespace BazaarVoice.Infrastructure.ServiceBus
{
    /// <summary>
    /// Testable abstraction over the Azure Service Bus client for sending messages.
    /// </summary>
    public interface IServiceBusClientWrapper
    {
        /// <summary>
        /// Sends a single message to the configured topic.
        /// </summary>
        Task SendMessageAsync(ServiceBusMessage message);

        /// <summary>
        /// Sends a batch of messages to the configured topic.
        /// </summary>
        Task SendMessagesAsync(IEnumerable<ServiceBusMessage> messages);
    }
}

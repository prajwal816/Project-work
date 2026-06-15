namespace BazaarVoice.Common.Models
{
    /// <summary>
    /// Envelope DTO that wraps a <see cref="BazaarVoiceXmlRecord"/> with
    /// correlation and processing metadata for Service Bus transport.
    /// </summary>
    public class ServiceBusMessageDto
    {
        /// <summary>
        /// Correlation ID linking this message back to the original blob processing operation.
        /// </summary>
        public string? CorrelationId { get; set; }

        /// <summary>
        /// Original source file name the record was extracted from.
        /// </summary>
        public string? SourceFileName { get; set; }

        /// <summary>
        /// Zero-based index of this record within the source file.
        /// </summary>
        public int RecordIndex { get; set; }

        /// <summary>
        /// Total number of records in the source file.
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// UTC timestamp when this message was created.
        /// </summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// The review record payload.
        /// </summary>
        public BazaarVoiceXmlRecord? Record { get; set; }
    }
}

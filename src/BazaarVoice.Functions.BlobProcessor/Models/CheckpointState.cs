namespace BazaarVoice.Functions.BlobProcessor.Models
{
    /// <summary>
    /// Represents the checkpoint/restart state persisted in blob storage.
    /// Enables resumption of file processing after failure.
    /// </summary>
    public class CheckpointState
    {
        /// <summary>
        /// The original source file name being processed.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Zero-based index of the last successfully processed and published record.
        /// </summary>
        public int LastProcessedIndex { get; set; }

        /// <summary>
        /// Email address of the last successfully processed record (for audit/debugging).
        /// </summary>
        public string? LastProcessedEmail { get; set; }

        /// <summary>
        /// UTC timestamp when this checkpoint was last updated.
        /// </summary>
        public DateTime LastUpdated { get; set; }
    }
}

namespace BazaarVoice.Functions.BlobProcessor.Models
{
    /// <summary>
    /// Wraps the outcome of blob file processing for telemetry and logging.
    /// </summary>
    public class ProcessingResult
    {
        /// <summary>
        /// Whether the entire file was processed successfully.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Total number of records found in the XML file.
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Number of records successfully published to Service Bus.
        /// </summary>
        public int RecordsPublished { get; set; }

        /// <summary>
        /// Number of records skipped (e.g., missing email).
        /// </summary>
        public int RecordsSkipped { get; set; }

        /// <summary>
        /// Error message if processing failed.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// The source file name that was processed.
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Duration of processing in milliseconds.
        /// </summary>
        public long DurationMs { get; set; }
    }
}

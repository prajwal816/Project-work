namespace BazaarVoice.Functions.BlobProcessor.Services
{
    /// <summary>
    /// Extracts XML content from GZip-compressed streams.
    /// </summary>
    public interface IGzExtractorService
    {
        /// <summary>
        /// Decompresses a GZip stream and returns the extracted XML content as a string.
        /// </summary>
        /// <param name="gzStream">The compressed input stream.</param>
        /// <returns>The decompressed XML content.</returns>
        Task<string> ExtractGzToXmlAsync(Stream gzStream);
    }
}

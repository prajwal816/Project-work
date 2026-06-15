using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.BlobProcessor.Services
{
    /// <summary>
    /// Extracts XML content from GZip-compressed (.gz) streams received from BazaarVoice.
    /// </summary>
    public class GzExtractorService : IGzExtractorService
    {
        private readonly ILogger<GzExtractorService> _logger;

        public GzExtractorService(ILogger<GzExtractorService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<string> ExtractGzToXmlAsync(Stream gzStream)
        {
            if (gzStream == null || gzStream.Length == 0)
            {
                throw new ArgumentException("GZ stream is null or empty.");
            }

            using var decompressedStream = new MemoryStream();
            using (var gzipStream = new GZipStream(gzStream, CompressionMode.Decompress))
            {
                await gzipStream.CopyToAsync(decompressedStream);
            }

            decompressedStream.Position = 0;
            using var reader = new StreamReader(decompressedStream, Encoding.UTF8);
            string xmlContent = await reader.ReadToEndAsync();

            _logger.LogInformation(
                "Extracted GZ content. XML length: {Length} characters", xmlContent.Length);

            return xmlContent;
        }
    }
}

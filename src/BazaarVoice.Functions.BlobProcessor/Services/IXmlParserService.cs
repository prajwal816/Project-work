using BazaarVoice.Functions.BlobProcessor.Models;

namespace BazaarVoice.Functions.BlobProcessor.Services
{
    /// <summary>
    /// Parses BazaarVoice XML content into individual review records.
    /// </summary>
    public interface IXmlParserService
    {
        /// <summary>
        /// Parses the full XML content and returns a list of individual review records.
        /// </summary>
        /// <param name="xmlContent">Raw XML string from the extracted .gz file.</param>
        /// <returns>A list of parsed <see cref="BazaarVoiceRecord"/> objects.</returns>
        List<BazaarVoiceRecord> ParseRecords(string xmlContent);
    }
}

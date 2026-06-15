using System.Xml.Linq;
using BazaarVoice.Common.Exceptions;
using BazaarVoice.Common.Extensions;
using BazaarVoice.Functions.BlobProcessor.Models;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.BlobProcessor.Services
{
    /// <summary>
    /// Parses BazaarVoice XML content into individual review records.
    /// </summary>
    public class XmlParserService : IXmlParserService
    {
        private readonly ILogger<XmlParserService> _logger;

        public XmlParserService(ILogger<XmlParserService> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public List<BazaarVoiceRecord> ParseRecords(string xmlContent)
        {
            var records = new List<BazaarVoiceRecord>();

            try
            {
                var xDoc = XDocument.Parse(xmlContent);

                // PLACEHOLDER: Update the XML element/node names below based on
                //      actual BazaarVoice XML schema structure.
                //      Example assumes <Reviews><Review>...</Review></Reviews> structure.
                //      Replace "Review", "EmailAddress", "UserNickname", "Rating",
                //      "ReviewText", "SubmissionTime", "ProductId" with actual field names.

                var reviewElements = xDoc.Descendants("Review"); // PLACEHOLDER: Actual XML element name

                foreach (var element in reviewElements)
                {
                    var record = new BazaarVoiceRecord
                    {
                        Email = element.GetElementValue("EmailAddress"),           // PLACEHOLDER: Actual email field name in XML
                        UserNickname = element.GetElementValue("UserNickname"),     // PLACEHOLDER: Actual field name
                        Rating = element.GetElementIntValue("Rating"),              // PLACEHOLDER: Actual field name
                        ReviewText = element.GetElementValue("ReviewText"),         // PLACEHOLDER: Actual field name
                        SubmissionDate = element.GetElementValue("SubmissionTime"), // PLACEHOLDER: Actual field name
                        ProductId = element.GetElementValue("ProductId"),           // PLACEHOLDER: Actual field name
                        ReviewId = element.GetElementValue("Id")                   // PLACEHOLDER: Actual unique identifier field
                    };

                    if (!string.IsNullOrWhiteSpace(record.Email))
                    {
                        records.Add(record);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Skipping record with missing email. ReviewId: {ReviewId}",
                            record.ReviewId ?? "UNKNOWN");
                    }
                }
            }
            catch (Exception ex) when (ex is not ProcessingException)
            {
                _logger.LogError(ex, "Failed to parse XML content");
                throw new ProcessingException("XML parsing failed", ex);
            }

            _logger.LogInformation("Parsed {RecordCount} valid records from XML", records.Count);
            return records;
        }
    }
}

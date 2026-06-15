using System.Xml.Linq;

namespace BazaarVoice.Common.Extensions
{
    /// <summary>
    /// Extension methods for safe XML element value extraction.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Safely retrieves a trimmed string value from a child element.
        /// Returns null if the element doesn't exist or is empty.
        /// </summary>
        public static string? GetElementValue(this XElement parent, string elementName)
        {
            var value = parent.Element(elementName)?.Value?.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        /// <summary>
        /// Safely retrieves an integer value from a child element.
        /// Returns the default value if the element doesn't exist or is not a valid integer.
        /// </summary>
        public static int GetElementIntValue(this XElement parent, string elementName, int defaultValue = 0)
        {
            var value = parent.Element(elementName)?.Value?.Trim();
            return int.TryParse(value, out int result) ? result : defaultValue;
        }

        /// <summary>
        /// Safely retrieves a DateTime value from a child element.
        /// Returns null if the element doesn't exist or is not a valid date.
        /// </summary>
        public static DateTime? GetElementDateValue(this XElement parent, string elementName)
        {
            var value = parent.Element(elementName)?.Value?.Trim();
            return DateTime.TryParse(value, out DateTime result) ? result : null;
        }

        /// <summary>
        /// Safely retrieves an attribute value from an element.
        /// Returns null if the attribute doesn't exist or is empty.
        /// </summary>
        public static string? GetAttributeValue(this XElement element, string attributeName)
        {
            var value = element.Attribute(attributeName)?.Value?.Trim();
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}

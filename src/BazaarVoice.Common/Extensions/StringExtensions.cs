using System.Text.RegularExpressions;

namespace BazaarVoice.Common.Extensions
{
    /// <summary>
    /// Extension methods for string sanitization and validation.
    /// </summary>
    public static class StringExtensions
    {
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Sanitizes a file name by replacing path separators and removing the .gz extension.
        /// Used for checkpoint file naming.
        /// </summary>
        public static string SanitizeForCheckpoint(this string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            return fileName
                .Replace("/", "_")
                .Replace("\\", "_")
                .Replace(".gz", "")
                .Trim();
        }

        /// <summary>
        /// Returns true if the string is a structurally valid email address.
        /// </summary>
        public static bool IsValidEmail(this string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return EmailRegex.IsMatch(email.Trim());
        }

        /// <summary>
        /// Truncates a string to the specified max length, appending "..." if truncated.
        /// Useful for safe logging of potentially long values.
        /// </summary>
        public static string Truncate(this string? value, int maxLength = 100)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;

            return value.Length <= maxLength
                ? value
                : value[..maxLength] + "...";
        }

        /// <summary>
        /// Masks an email address for safe logging (e.g., "j***@example.com").
        /// </summary>
        public static string MaskEmail(this string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "[empty]";

            var parts = email.Split('@');
            if (parts.Length != 2)
                return "[invalid]";

            var localPart = parts[0];
            var maskedLocal = localPart.Length <= 1
                ? "*"
                : localPart[0] + new string('*', Math.Min(localPart.Length - 1, 3));

            return $"{maskedLocal}@{parts[1]}";
        }
    }
}

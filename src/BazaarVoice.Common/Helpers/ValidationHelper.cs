using BazaarVoice.Common.Constants;
using BazaarVoice.Common.Extensions;
using BazaarVoice.Common.Models;

namespace BazaarVoice.Common.Helpers
{
    /// <summary>
    /// Centralized validation utilities for input data.
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates that the incoming file name is a .gz file.
        /// </summary>
        public static bool IsValidGzFile(string? fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName)
                && fileName.EndsWith(AppConstants.GzExtension, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Validates a <see cref="BazaarVoiceXmlRecord"/> has the minimum required fields.
        /// </summary>
        /// <param name="record">The record to validate.</param>
        /// <param name="errors">Validation error messages, if any.</param>
        /// <returns>True if the record is valid.</returns>
        public static bool ValidateRecord(BazaarVoiceXmlRecord? record, out List<string> errors)
        {
            errors = new List<string>();

            if (record == null)
            {
                errors.Add("Record is null.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(record.Email))
            {
                errors.Add("Email is required.");
            }
            else if (!record.Email.IsValidEmail())
            {
                errors.Add($"Email '{record.Email}' is not a valid email address.");
            }

            if (string.IsNullOrWhiteSpace(record.ReviewId))
            {
                errors.Add("ReviewId is required.");
            }

            return errors.Count == 0;
        }

        /// <summary>
        /// Validates that a member Annex ID is not null or empty.
        /// </summary>
        public static bool IsValidMemberAnnexId(string? memberAnnexId)
        {
            return !string.IsNullOrWhiteSpace(memberAnnexId);
        }

        /// <summary>
        /// Validates that an action code is not null or empty.
        /// </summary>
        public static bool IsValidActionCode(string? actionCode)
        {
            return !string.IsNullOrWhiteSpace(actionCode);
        }
    }
}

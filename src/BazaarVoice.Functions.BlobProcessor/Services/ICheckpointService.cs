using BazaarVoice.Functions.BlobProcessor.Models;

namespace BazaarVoice.Functions.BlobProcessor.Services
{
    /// <summary>
    /// Manages checkpoint/restart state in blob storage to enable
    /// resumption of file processing after failure.
    /// </summary>
    public interface ICheckpointService
    {
        /// <summary>
        /// Retrieves the checkpoint state for a given file, if one exists.
        /// </summary>
        Task<CheckpointState?> GetCheckpointAsync(string fileName);

        /// <summary>
        /// Updates the checkpoint state after successfully processing a record.
        /// </summary>
        Task UpdateCheckpointAsync(string fileName, int lastProcessedIndex, string lastEmail);

        /// <summary>
        /// Clears the checkpoint state after successful completion of all records.
        /// </summary>
        Task ClearCheckpointAsync(string fileName);
    }
}

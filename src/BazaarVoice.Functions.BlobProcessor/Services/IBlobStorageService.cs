namespace BazaarVoice.Functions.BlobProcessor.Services
{
    /// <summary>
    /// Manages blob storage operations for the BazaarVoice container
    /// (upload XML, archive files, existence checks).
    /// </summary>
    public interface IBlobStorageService
    {
        /// <summary>
        /// Uploads XML content to the specified container path.
        /// </summary>
        Task UploadXmlAsync(string containerPath, string fileName, string content);

        /// <summary>
        /// Archives a file from the source path to the archive path with date-based organization.
        /// </summary>
        Task ArchiveFileAsync(string sourceContainer, string destinationContainer, string fileName);

        /// <summary>
        /// Checks whether a blob exists at the specified path.
        /// </summary>
        Task<bool> BlobExistsAsync(string containerPath, string fileName);
    }
}

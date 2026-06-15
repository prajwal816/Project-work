namespace BazaarVoice.Infrastructure.Storage
{
    /// <summary>
    /// Testable abstraction over Azure Blob Storage operations.
    /// </summary>
    public interface IBlobClientWrapper
    {
        /// <summary>
        /// Uploads string content as a blob.
        /// </summary>
        Task UploadAsync(string blobPath, string content, string contentType = "application/xml");

        /// <summary>
        /// Downloads blob content as a string.
        /// </summary>
        Task<string?> DownloadAsStringAsync(string blobPath);

        /// <summary>
        /// Uploads a stream as a blob.
        /// </summary>
        Task UploadStreamAsync(string blobPath, Stream content, string contentType = "application/xml");

        /// <summary>
        /// Copies a blob from source path to destination path.
        /// </summary>
        Task CopyBlobAsync(string sourceBlobPath, string destinationBlobPath);

        /// <summary>
        /// Deletes a blob if it exists.
        /// </summary>
        Task<bool> DeleteIfExistsAsync(string blobPath);

        /// <summary>
        /// Checks whether a blob exists.
        /// </summary>
        Task<bool> ExistsAsync(string blobPath);
    }
}

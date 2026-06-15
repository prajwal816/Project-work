using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Infrastructure.Storage
{
    /// <summary>
    /// Wraps the Azure <see cref="BlobContainerClient"/> for testability and
    /// adds logging around blob operations.
    /// </summary>
    public class BlobClientWrapper : IBlobClientWrapper
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ILogger<BlobClientWrapper> _logger;

        public BlobClientWrapper(
            BlobServiceClient blobServiceClient,
            string containerName,
            ILogger<BlobClientWrapper> logger)
        {
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            _logger = logger;
        }

        /// <summary>
        /// Ensures the blob container exists. Call once during startup.
        /// </summary>
        public async Task EnsureContainerExistsAsync()
        {
            await _containerClient.CreateIfNotExistsAsync();
            _logger.LogInformation(
                "Ensured container exists: {ContainerName}", _containerClient.Name);
        }

        /// <inheritdoc />
        public async Task UploadAsync(
            string blobPath, string content, string contentType = "application/xml")
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));

            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = contentType
            });

            _logger.LogInformation("Uploaded blob: {BlobPath}", blobPath);
        }

        /// <inheritdoc />
        public async Task<string?> DownloadAsStringAsync(string blobPath)
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);

            if (!await blobClient.ExistsAsync())
            {
                _logger.LogWarning("Blob not found: {BlobPath}", blobPath);
                return null;
            }

            var response = await blobClient.DownloadContentAsync();
            return response.Value.Content.ToString();
        }

        /// <inheritdoc />
        public async Task UploadStreamAsync(
            string blobPath, Stream content, string contentType = "application/xml")
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);

            await blobClient.UploadAsync(content, new BlobHttpHeaders
            {
                ContentType = contentType
            });

            _logger.LogInformation("Uploaded blob from stream: {BlobPath}", blobPath);
        }

        /// <inheritdoc />
        public async Task CopyBlobAsync(string sourceBlobPath, string destinationBlobPath)
        {
            var sourceBlob = _containerClient.GetBlobClient(sourceBlobPath);
            var destBlob = _containerClient.GetBlobClient(destinationBlobPath);

            if (!await sourceBlob.ExistsAsync())
            {
                _logger.LogWarning(
                    "Source blob not found for copy: {SourcePath}", sourceBlobPath);
                return;
            }

            await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);

            _logger.LogInformation(
                "Copied blob from {Source} to {Destination}",
                sourceBlobPath, destinationBlobPath);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteIfExistsAsync(string blobPath)
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);
            var response = await blobClient.DeleteIfExistsAsync();

            if (response.Value)
            {
                _logger.LogInformation("Deleted blob: {BlobPath}", blobPath);
            }

            return response.Value;
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string blobPath)
        {
            var blobClient = _containerClient.GetBlobClient(blobPath);
            return await blobClient.ExistsAsync();
        }
    }
}

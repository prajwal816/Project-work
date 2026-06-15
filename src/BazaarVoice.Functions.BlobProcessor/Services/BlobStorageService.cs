using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using BazaarVoice.Common.Constants;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.BlobProcessor.Services
{
    /// <summary>
    /// Manages blob storage operations for the BazaarVoice container.
    /// Handles uploading extracted XML to loyalty and eComm folders, and archiving processed files.
    /// </summary>
    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BlobStorageService> _logger;

        public BlobStorageService(
            BlobServiceClient blobServiceClient,
            ILogger<BlobStorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task UploadXmlAsync(string containerPath, string fileName, string content)
        {
            var containerClient = _blobServiceClient
                .GetBlobContainerClient(AppConstants.BlobContainerName);
            await containerClient.CreateIfNotExistsAsync();

            string blobPath = $"{containerPath}/{fileName}";
            var blobClient = containerClient.GetBlobClient(blobPath);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await blobClient.UploadAsync(stream, new BlobHttpHeaders
            {
                ContentType = "application/xml"
            });

            _logger.LogInformation("Uploaded XML to: {BlobPath}", blobPath);
        }

        /// <inheritdoc />
        public async Task ArchiveFileAsync(
            string sourceContainer, string destinationContainer, string fileName)
        {
            var containerClient = _blobServiceClient
                .GetBlobContainerClient(AppConstants.BlobContainerName);

            string sourcePath = $"{sourceContainer}/{fileName}";
            string destPath = $"{destinationContainer}/{DateTime.UtcNow:yyyy-MM-dd}/{fileName}";

            var sourceBlob = containerClient.GetBlobClient(sourcePath);
            var destBlob = containerClient.GetBlobClient(destPath);

            // Copy to archive
            await destBlob.StartCopyFromUriAsync(sourceBlob.Uri);

            _logger.LogInformation(
                "Archived file from {Source} to {Destination}", sourcePath, destPath);

            // Note: Do NOT delete source — it stays in loyalty folder.
            // Archive has 7-day retention (managed via Lifecycle Management Policy).
        }

        /// <inheritdoc />
        public async Task<bool> BlobExistsAsync(string containerPath, string fileName)
        {
            var containerClient = _blobServiceClient
                .GetBlobContainerClient(AppConstants.BlobContainerName);
            var blobClient = containerClient.GetBlobClient($"{containerPath}/{fileName}");
            return await blobClient.ExistsAsync();
        }
    }
}

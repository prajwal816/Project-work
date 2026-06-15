using System.Text.Json;
using Azure.Storage.Blobs;
using BazaarVoice.Common.Constants;
using BazaarVoice.Common.Extensions;
using BazaarVoice.Functions.BlobProcessor.Models;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.BlobProcessor.Services
{
    /// <summary>
    /// Persists checkpoint/restart state as JSON blobs in the checkpoint folder.
    /// Enables resumption of file processing from the last successfully published record.
    /// </summary>
    public class CheckpointService : ICheckpointService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<CheckpointService> _logger;

        public CheckpointService(
            BlobServiceClient blobServiceClient,
            ILogger<CheckpointService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<CheckpointState?> GetCheckpointAsync(string fileName)
        {
            try
            {
                var containerClient = _blobServiceClient
                    .GetBlobContainerClient(AppConstants.BlobContainerName);
                var blobClient = containerClient.GetBlobClient(
                    $"{AppConstants.CheckpointFolder}/{fileName.SanitizeForCheckpoint()}_checkpoint.json");

                if (!await blobClient.ExistsAsync())
                    return null;

                var response = await blobClient.DownloadContentAsync();
                var content = response.Value.Content.ToString();
                return JsonSerializer.Deserialize<CheckpointState>(content);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read checkpoint for {FileName}", fileName);
                return null;
            }
        }

        /// <inheritdoc />
        public async Task UpdateCheckpointAsync(
            string fileName, int lastProcessedIndex, string lastEmail)
        {
            var checkpoint = new CheckpointState
            {
                FileName = fileName,
                LastProcessedIndex = lastProcessedIndex,
                LastProcessedEmail = lastEmail,
                LastUpdated = DateTime.UtcNow
            };

            var containerClient = _blobServiceClient
                .GetBlobContainerClient(AppConstants.BlobContainerName);
            var blobClient = containerClient.GetBlobClient(
                $"{AppConstants.CheckpointFolder}/{fileName.SanitizeForCheckpoint()}_checkpoint.json");

            var json = JsonSerializer.Serialize(checkpoint);
            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        /// <inheritdoc />
        public async Task ClearCheckpointAsync(string fileName)
        {
            var containerClient = _blobServiceClient
                .GetBlobContainerClient(AppConstants.BlobContainerName);
            var blobClient = containerClient.GetBlobClient(
                $"{AppConstants.CheckpointFolder}/{fileName.SanitizeForCheckpoint()}_checkpoint.json");

            await blobClient.DeleteIfExistsAsync();
            _logger.LogInformation("Cleared checkpoint for {FileName}", fileName);
        }
    }
}

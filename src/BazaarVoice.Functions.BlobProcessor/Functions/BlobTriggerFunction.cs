using System.Diagnostics;
using BazaarVoice.Common.Constants;
using BazaarVoice.Common.Exceptions;
using BazaarVoice.Functions.BlobProcessor.Models;
using BazaarVoice.Functions.BlobProcessor.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;

namespace BazaarVoice.Functions.BlobProcessor.Functions
{
    /// <summary>
    /// Azure Function triggered by new .gz file arrivals in the BazaarVoice
    /// incoming blob container. Extracts XML, writes to loyalty and eComm folders,
    /// parses individual records, and publishes each to Service Bus for downstream processing.
    /// </summary>
    public class BlobTriggerFunction
    {
        private readonly IGzExtractorService _gzExtractorService;
        private readonly IXmlParserService _xmlParserService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IServiceBusPublisherService _serviceBusPublisher;
        private readonly ICheckpointService _checkpointService;
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<BlobTriggerFunction> _logger;

        public BlobTriggerFunction(
            IGzExtractorService gzExtractorService,
            IXmlParserService xmlParserService,
            IBlobStorageService blobStorageService,
            IServiceBusPublisherService serviceBusPublisher,
            ICheckpointService checkpointService,
            TelemetryClient telemetryClient,
            ILogger<BlobTriggerFunction> logger)
        {
            _gzExtractorService = gzExtractorService;
            _xmlParserService = xmlParserService;
            _blobStorageService = blobStorageService;
            _serviceBusPublisher = serviceBusPublisher;
            _checkpointService = checkpointService;
            _telemetryClient = telemetryClient;
            _logger = logger;
        }

        [Function("BazaarVoiceBlobTrigger")]
        public async Task RunAsync(
            [BlobTrigger(
                "bazaarvoice/incoming/fromBazaarVoice/{fileName}",
                Connection = "BazaarVoiceStorageConnection")] // PLACEHOLDER: Connection string name in app settings
            Stream blobStream,
            string fileName)
        {
            var operationId = Guid.NewGuid().ToString();
            var stopwatch = Stopwatch.StartNew();
            using var operation = _telemetryClient.StartOperation<RequestTelemetry>(
                $"BlobProcessor-{fileName}", operationId);

            _logger.LogInformation(
                "Processing blob: {FileName}, OperationId: {OperationId}",
                fileName, operationId);

            try
            {
                // ── Step 1: Validate file is .gz ─────────────────────────────
                if (!fileName.EndsWith(AppConstants.GzExtension, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("File {FileName} is not a .gz file. Skipping.", fileName);
                    return;
                }

                // ── Step 2: Extract .gz file to get XML content ──────────────
                string xmlContent = await _gzExtractorService.ExtractGzToXmlAsync(blobStream);

                if (string.IsNullOrWhiteSpace(xmlContent))
                {
                    _logger.LogError(
                        "Extracted XML content is empty for file: {FileName}", fileName);
                    throw new ProcessingException($"Empty XML content from file: {fileName}");
                }

                _logger.LogInformation("Successfully extracted XML from {FileName}", fileName);

                // ── Step 3: Write extracted XML to TWO folders ───────────────
                string xmlFileName = Path.GetFileNameWithoutExtension(fileName)
                    + AppConstants.XmlExtension;

                // Write to Loyalty folder
                await _blobStorageService.UploadXmlAsync(
                    containerPath: AppConstants.LoyaltyFolder,
                    fileName: xmlFileName,
                    content: xmlContent);

                // Write to Existing eComm folder (for HCL Commerce)
                await _blobStorageService.UploadXmlAsync(
                    containerPath: AppConstants.EcommFolder, // PLACEHOLDER: Confirm exact eComm folder path
                    fileName: xmlFileName,
                    content: xmlContent);

                _logger.LogInformation(
                    "Written XML to loyalty and ecomm folders: {XmlFileName}", xmlFileName);

                // ── Step 4: Parse XML and extract individual records ─────────
                List<BazaarVoiceRecord> records = _xmlParserService.ParseRecords(xmlContent);

                _logger.LogInformation(
                    "Parsed {RecordCount} records from {FileName}",
                    records.Count, fileName);

                // ── Step 5: Check for checkpoint (restart scenario) ──────────
                CheckpointState? checkpoint =
                    await _checkpointService.GetCheckpointAsync(fileName);
                int startIndex = 0;

                if (checkpoint != null && checkpoint.LastProcessedIndex > 0)
                {
                    startIndex = checkpoint.LastProcessedIndex + 1;
                    _logger.LogInformation(
                        "Resuming from checkpoint. Starting at record index: {StartIndex}",
                        startIndex);
                }

                // ── Step 6: Publish records one at a time to Service Bus Topic ──
                int publishedCount = 0;
                int skippedCount = 0;

                for (int i = startIndex; i < records.Count; i++)
                {
                    try
                    {
                        var record = records[i];

                        // Validate record has email field
                        if (string.IsNullOrWhiteSpace(record.Email))
                        {
                            _logger.LogWarning(
                                "Record at index {Index} has no email. Skipping.", i);
                            skippedCount++;
                            continue;
                        }

                        await _serviceBusPublisher.PublishRecordAsync(record, operationId);

                        // Update checkpoint after successful publish
                        await _checkpointService.UpdateCheckpointAsync(
                            fileName, i, record.Email);

                        publishedCount++;

                        _logger.LogInformation(
                            "Published record {Index}/{Total} for email: {Email}",
                            i + 1, records.Count, record.Email);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "Failed to publish record at index {Index} for file {FileName}",
                            i, fileName);

                        // Save checkpoint for restart
                        await _checkpointService.UpdateCheckpointAsync(
                            fileName, i - 1, records[i].Email ?? "UNKNOWN");

                        throw; // Re-throw to trigger retry/failure handling
                    }
                }

                // ── Step 7: Archive the file (7-day retention) ───────────────
                await _blobStorageService.ArchiveFileAsync(
                    sourceContainer: AppConstants.LoyaltyFolder,
                    destinationContainer: AppConstants.LoyaltyArchiveFolder,
                    fileName: xmlFileName);

                // ── Step 8: Clear checkpoint on successful completion ────────
                await _checkpointService.ClearCheckpointAsync(fileName);

                stopwatch.Stop();

                _logger.LogInformation(
                    "Successfully completed processing file: {FileName}. " +
                    "Total records: {TotalRecords}, Published: {Published}, " +
                    "Skipped: {Skipped}, Duration: {DurationMs}ms",
                    fileName, records.Count, publishedCount,
                    skippedCount, stopwatch.ElapsedMilliseconds);

                // ── Telemetry ────────────────────────────────────────────────
                _telemetryClient.TrackMetric("BlobsProcessed", 1);
                _telemetryClient.TrackMetric("RecordsPublished", publishedCount);
                _telemetryClient.TrackMetric("RecordsSkipped", skippedCount);
                _telemetryClient.TrackMetric(
                    "ProcessingDurationMs", stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing blob: {FileName}, OperationId: {OperationId}",
                    fileName, operationId);

                _telemetryClient.TrackException(ex);
                operation.Telemetry.Success = false;
                throw; // Allow Azure Functions retry policy to handle
            }
        }
    }
}

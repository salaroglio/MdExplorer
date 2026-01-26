using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MdExplorer.P2P.Premium.Models;
using MdExplorer.P2P.Premium.Services;

namespace MdExplorer.P2P.Premium.Controllers
{
    /// <summary>
    /// API Controller for P2P file sharing operations.
    /// Acts as a proxy to the Electron P2P plugin.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class P2PController : ControllerBase
    {
        private readonly IP2PService _p2pService;
        private readonly ILogger<P2PController> _logger;

        public P2PController(IP2PService p2pService, ILogger<P2PController> logger)
        {
            _p2pService = p2pService;
            _logger = logger;
        }

        /// <summary>
        /// Get P2P service status
        /// </summary>
        [HttpGet("status")]
        public async Task<ActionResult<P2PStatus>> GetStatus()
        {
            try
            {
                var isAvailable = await _p2pService.IsAvailableAsync();
                var stats = isAvailable ? await _p2pService.GetStatsAsync() : null;

                return Ok(new P2PStatus
                {
                    Enabled = isAvailable,
                    HttpRunning = isAvailable,
                    Stats = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting P2P status");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get P2P service health
        /// </summary>
        [HttpGet("health")]
        public async Task<ActionResult<HealthResponse>> GetHealth()
        {
            try
            {
                var health = await _p2pService.GetHealthAsync();
                if (health == null)
                {
                    return StatusCode(503, new { error = "P2P service not available" });
                }
                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting P2P health");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get P2P statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<P2PStats>> GetStats()
        {
            try
            {
                var stats = await _p2pService.GetStatsAsync();
                if (stats == null)
                {
                    return StatusCode(503, new { error = "P2P service not available" });
                }
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting P2P stats");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all active transfers
        /// </summary>
        [HttpGet("transfers")]
        public async Task<ActionResult<List<TransferInfo>>> GetTransfers()
        {
            try
            {
                var transfers = await _p2pService.GetTransfersAsync();
                return Ok(transfers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting P2P transfers");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get a specific transfer by info hash
        /// </summary>
        [HttpGet("transfers/{infoHash}")]
        public async Task<ActionResult<TransferInfo>> GetTransfer(string infoHash)
        {
            try
            {
                var transfer = await _p2pService.GetTransferAsync(infoHash);
                if (transfer == null)
                {
                    return NotFound(new { error = "Transfer not found" });
                }
                return Ok(transfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting P2P transfer {InfoHash}", infoHash);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Share a file via P2P
        /// </summary>
        [HttpPost("share")]
        public async Task<ActionResult<ShareResult>> ShareFile([FromBody] ShareRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.FilePath))
                {
                    return BadRequest(new { error = "filePath is required" });
                }

                _logger.LogInformation("Sharing file: {FilePath}", request.FilePath);

                var result = await _p2pService.ShareFileAsync(request.FilePath, request.Name);
                if (result == null)
                {
                    return StatusCode(500, new { error = "Failed to share file" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sharing file");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Download from magnet link
        /// </summary>
        [HttpPost("download")]
        public async Task<ActionResult<ShareResult>> Download([FromBody] DownloadRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.MagnetUri))
                {
                    return BadRequest(new { error = "magnetUri is required" });
                }

                _logger.LogInformation("Starting download from magnet");

                var result = await _p2pService.DownloadAsync(request.MagnetUri, request.DestPath);
                if (result == null)
                {
                    return StatusCode(500, new { error = "Failed to start download" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting download");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Pause a transfer
        /// </summary>
        [HttpPost("transfers/{infoHash}/pause")]
        public async Task<ActionResult> PauseTransfer(string infoHash)
        {
            try
            {
                var success = await _p2pService.PauseTransferAsync(infoHash);
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to pause transfer" });
                }
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pausing transfer {InfoHash}", infoHash);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Resume a transfer
        /// </summary>
        [HttpPost("transfers/{infoHash}/resume")]
        public async Task<ActionResult> ResumeTransfer(string infoHash)
        {
            try
            {
                var success = await _p2pService.ResumeTransferAsync(infoHash);
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to resume transfer" });
                }
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resuming transfer {InfoHash}", infoHash);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Stop and remove a transfer
        /// </summary>
        [HttpDelete("transfers/{infoHash}")]
        public async Task<ActionResult> StopTransfer(string infoHash, [FromQuery] bool deleteFiles = false)
        {
            try
            {
                var success = await _p2pService.StopTransferAsync(infoHash, deleteFiles);
                if (!success)
                {
                    return StatusCode(500, new { error = "Failed to stop transfer" });
                }
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping transfer {InfoHash}", infoHash);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Parse a magnet URI
        /// </summary>
        [HttpPost("parse-magnet")]
        public async Task<ActionResult> ParseMagnet([FromBody] DownloadRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.MagnetUri))
                {
                    return BadRequest(new { error = "magnetUri is required" });
                }

                var result = await _p2pService.ParseMagnetAsync(request.MagnetUri);
                if (result == null)
                {
                    return BadRequest(new { error = "Invalid magnet URI" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing magnet URI");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Copy a file to .p2pshare/files/, start seeding it, and append a P2P link to the markdown document.
        /// This is the main endpoint for the "Add file to share via P2P" feature.
        /// </summary>
        [HttpPost("copy-and-share")]
        public async Task<ActionResult<ShareResult>> CopyAndShareFile([FromBody] CopyAndShareRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request?.SourcePath))
                {
                    return BadRequest(new { error = "sourcePath is required" });
                }

                if (string.IsNullOrEmpty(request?.DocumentPath))
                {
                    return BadRequest(new { error = "documentPath is required" });
                }

                // Validate source file exists
                if (!System.IO.File.Exists(request.SourcePath))
                {
                    return BadRequest(new { error = $"Source file not found: {request.SourcePath}" });
                }

                // Validate document exists
                if (!System.IO.File.Exists(request.DocumentPath))
                {
                    return BadRequest(new { error = $"Document not found: {request.DocumentPath}" });
                }

                _logger.LogInformation("CopyAndShare: Source={SourcePath}, Document={DocumentPath}",
                    request.SourcePath, request.DocumentPath);

                // 1. Determine project path from the document
                var projectPath = System.IO.Path.GetDirectoryName(request.DocumentPath);
                if (string.IsNullOrEmpty(projectPath))
                {
                    return BadRequest(new { error = "Could not determine project path from document" });
                }

                // Walk up to find the project root (where .md/ folder exists)
                var currentPath = projectPath;
                while (!string.IsNullOrEmpty(currentPath))
                {
                    if (System.IO.Directory.Exists(System.IO.Path.Combine(currentPath, ".md")))
                    {
                        projectPath = currentPath;
                        break;
                    }
                    currentPath = System.IO.Path.GetDirectoryName(currentPath);
                }

                var p2pSharePath = System.IO.Path.Combine(projectPath, ".p2pshare", "files");

                // 2. Create the .p2pshare/files directory if it doesn't exist
                System.IO.Directory.CreateDirectory(p2pSharePath);

                // 3. Copy the file
                var fileName = System.IO.Path.GetFileName(request.SourcePath);
                var destPath = System.IO.Path.Combine(p2pSharePath, fileName);

                // Handle file name conflicts
                if (System.IO.File.Exists(destPath))
                {
                    var nameWithoutExt = System.IO.Path.GetFileNameWithoutExtension(fileName);
                    var extension = System.IO.Path.GetExtension(fileName);
                    var counter = 1;
                    while (System.IO.File.Exists(destPath))
                    {
                        fileName = $"{nameWithoutExt}_{counter}{extension}";
                        destPath = System.IO.Path.Combine(p2pSharePath, fileName);
                        counter++;
                    }
                }

                System.IO.File.Copy(request.SourcePath, destPath, overwrite: false);
                _logger.LogInformation("File copied to: {DestPath}", destPath);

                // 4. Start seeding via P2P plugin
                var shareResult = await _p2pService.ShareFileAsync(destPath, fileName);
                if (shareResult == null)
                {
                    // Clean up the copied file if sharing failed
                    System.IO.File.Delete(destPath);
                    return StatusCode(500, new { error = "Failed to start P2P sharing for the file" });
                }

                // 5. Save metadata to .p2pshare/metadata.json
                SaveP2PMetadata(projectPath, fileName, shareResult);

                // 6. Append clean P2P link to the markdown document (metadata is in metadata.json)
                var linkMarkdown = $"\n\n[{fileName}](.p2pshare/files/{fileName})\n";
                System.IO.File.AppendAllText(request.DocumentPath, linkMarkdown);
                _logger.LogInformation("P2P link appended to document: {DocumentPath}", request.DocumentPath);

                return Ok(shareResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CopyAndShareFile");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Check if a file exists at the given relative path within the current project
        /// </summary>
        [HttpGet("check-file")]
        public ActionResult CheckFile([FromQuery] string path, [FromQuery] string projectPath)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    return BadRequest(new { error = "path is required" });
                }

                if (string.IsNullOrEmpty(projectPath))
                {
                    return BadRequest(new { error = "projectPath is required" });
                }

                // Resolve relative path against project path
                var fullPath = System.IO.Path.Combine(projectPath, path.TrimStart('.', '/', '\\'));
                var exists = System.IO.File.Exists(fullPath);

                return Ok(new { exists, fullPath });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get peer status for a specific torrent by infoHash.
        /// Returns number of peers, download/upload speeds, and transfer status.
        /// </summary>
        [HttpGet("peer-status/{infoHash}")]
        public async Task<ActionResult> GetPeerStatus(string infoHash)
        {
            try
            {
                if (string.IsNullOrEmpty(infoHash))
                {
                    return BadRequest(new { error = "infoHash is required" });
                }

                var transfer = await _p2pService.GetTransferAsync(infoHash);

                if (transfer == null)
                {
                    // Transfer not found - could be not seeding or not known
                    return Ok(new
                    {
                        found = false,
                        status = "unknown",
                        numPeers = 0,
                        message = "Torrent not active on this node"
                    });
                }

                // Determine status based on transfer type and state
                string status;
                if (transfer.Type == "seeding")
                {
                    status = transfer.NumPeers > 0 ? "seeding" : "seeding_no_peers";
                }
                else if (transfer.Type == "downloading")
                {
                    if (transfer.Progress >= 1.0)
                    {
                        status = "completed";
                    }
                    else if (transfer.NumPeers > 0)
                    {
                        status = "downloading";
                    }
                    else
                    {
                        status = "downloading_no_peers";
                    }
                }
                else
                {
                    status = "unknown";
                }

                return Ok(new
                {
                    found = true,
                    status,
                    numPeers = transfer.NumPeers,
                    downloadSpeed = transfer.DownloadSpeed,
                    uploadSpeed = transfer.UploadSpeed,
                    progress = transfer.Progress,
                    size = transfer.Size,
                    name = transfer.Name,
                    timeRemaining = transfer.TimeRemaining
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting peer status for {InfoHash}", infoHash);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get P2P metadata for a project.
        /// Returns the contents of .p2pshare/metadata.json if it exists.
        /// </summary>
        [HttpGet("metadata")]
        public ActionResult GetMetadata([FromQuery] string projectPath)
        {
            try
            {
                if (string.IsNullOrEmpty(projectPath))
                {
                    return BadRequest(new { error = "projectPath is required" });
                }

                var metadataPath = System.IO.Path.Combine(projectPath, ".p2pshare", "metadata.json");

                if (!System.IO.File.Exists(metadataPath))
                {
                    return Ok(new { files = new Dictionary<string, object>() });
                }

                var content = System.IO.File.ReadAllText(metadataPath);
                var metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(content);

                return Ok(metadata ?? new Dictionary<string, object> { ["files"] = new Dictionary<string, object>() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading P2P metadata for project {ProjectPath}", projectPath);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get P2P info for a specific file by filename.
        /// Returns magnetUri, infoHash, size from metadata.json.
        /// </summary>
        [HttpGet("file-info/{filename}")]
        public ActionResult GetFileInfo(string filename, [FromQuery] string projectPath)
        {
            try
            {
                if (string.IsNullOrEmpty(filename))
                {
                    return BadRequest(new { error = "filename is required" });
                }

                if (string.IsNullOrEmpty(projectPath))
                {
                    return BadRequest(new { error = "projectPath is required" });
                }

                var metadataPath = System.IO.Path.Combine(projectPath, ".p2pshare", "metadata.json");

                if (!System.IO.File.Exists(metadataPath))
                {
                    return Ok(new { found = false, error = "No P2P metadata found for this project" });
                }

                var content = System.IO.File.ReadAllText(metadataPath);
                var metadata = System.Text.Json.JsonDocument.Parse(content);

                if (metadata.RootElement.TryGetProperty("files", out var filesElement) &&
                    filesElement.TryGetProperty(filename, out var fileInfo))
                {
                    return Ok(new
                    {
                        found = true,
                        magnetUri = fileInfo.TryGetProperty("magnetUri", out var m) ? m.GetString() : null,
                        infoHash = fileInfo.TryGetProperty("infoHash", out var h) ? h.GetString() : null,
                        size = fileInfo.TryGetProperty("size", out var s) ? s.GetInt64() : 0,
                        addedAt = fileInfo.TryGetProperty("addedAt", out var a) ? a.GetString() : null
                    });
                }

                return Ok(new { found = false, error = "File not found in P2P metadata" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info for {Filename}", filename);
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Helper method to save P2P metadata to the project's .p2pshare/metadata.json
        /// </summary>
        private void SaveP2PMetadata(string projectPath, string fileName, ShareResult shareResult)
        {
            try
            {
                var metadataPath = System.IO.Path.Combine(projectPath, ".p2pshare", "metadata.json");
                var metadata = new Dictionary<string, object>();

                // Load existing metadata if present
                if (System.IO.File.Exists(metadataPath))
                {
                    var existingContent = System.IO.File.ReadAllText(metadataPath);
                    metadata = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(existingContent)
                        ?? new Dictionary<string, object>();
                }

                // Ensure "files" dictionary exists
                if (!metadata.ContainsKey("files"))
                {
                    metadata["files"] = new Dictionary<string, object>();
                }

                var files = metadata["files"] as System.Text.Json.JsonElement?;
                var filesDict = new Dictionary<string, object>();

                if (files.HasValue && files.Value.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    foreach (var prop in files.Value.EnumerateObject())
                    {
                        filesDict[prop.Name] = prop.Value;
                    }
                }

                // Add new file metadata
                filesDict[fileName] = new
                {
                    magnetUri = shareResult.MagnetUri,
                    infoHash = shareResult.InfoHash,
                    size = shareResult.Size,
                    addedAt = DateTime.UtcNow.ToString("o")
                };

                metadata["files"] = filesDict;

                // Save updated metadata
                var jsonOptions = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                System.IO.File.WriteAllText(metadataPath, System.Text.Json.JsonSerializer.Serialize(metadata, jsonOptions));
                _logger.LogInformation("P2P metadata saved to: {MetadataPath}", metadataPath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to save P2P metadata (non-critical)");
                // Non-critical error - don't fail the whole operation
            }
        }
    }
}

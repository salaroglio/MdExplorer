using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Abstractions.Models;
using MdExplorer.Hubs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Controllers.MdAppStore
{
    [ApiController]
    [Route("api/MdAppStore")]
    public class MdAppStoreController : ControllerBase
    {
        private readonly IUserSettingsDB _session;
        private readonly ILogger<MdAppStoreController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHubContext<MonitorMDHub> _hubContext;

        public MdAppStoreController(
            IUserSettingsDB session,
            ILogger<MdAppStoreController> logger,
            IHttpClientFactory httpClientFactory,
            IHubContext<MonitorMDHub> hubContext)
        {
            _session = session;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
        }

        // ─────────────────────────────────────────────
        //  GET /api/MdAppStore/platform
        // ─────────────────────────────────────────────
        [HttpGet("platform")]
        public IActionResult GetPlatform()
        {
            return Ok(new { platform = GetCurrentPlatform() });
        }

        // ─────────────────────────────────────────────
        //  GET /api/MdAppStore/catalog
        // ─────────────────────────────────────────────
        [HttpGet("catalog")]
        public async Task<IActionResult> GetCatalog()
        {
            try
            {
                var repos = GetRepositories();
                if (repos.Count == 0)
                    return Ok(new { version = "1", apps = new List<object>(), repos = new List<object>() });

                var allApps = new List<object>();
                var repoInfos = new List<object>();
                var fetchTasks = repos.Select(async repo =>
                {
                    try
                    {
                        var catalogUrl = EnsureTrailingSlash(repo.Url) + "catalog.json";
                        var client = _httpClientFactory.CreateClient();
                        ApplyBasicAuth(client, repo);

                        var response = await client.GetAsync(catalogUrl);
                        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                        {
                            _logger.LogInformation("[MdAppStore] catalog.json not found for repo '{Label}' — empty", repo.Label);
                            return (repo, new AppStoreCatalog { Version = "1", Apps = new List<AppStoreCatalogEntry>() });
                        }
                        if (!response.IsSuccessStatusCode)
                        {
                            _logger.LogWarning("[MdAppStore] Catalog fetch failed for repo '{Label}': {StatusCode}", repo.Label, response.StatusCode);
                            return (repo, (AppStoreCatalog)null);
                        }

                        var json = await response.Content.ReadAsStringAsync();
                        var catalog = JsonSerializer.Deserialize<AppStoreCatalog>(json,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        return (repo, catalog);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "[MdAppStore] Error fetching catalog from repo '{Label}'", repo.Label);
                        return (repo, (AppStoreCatalog)null);
                    }
                }).ToList();

                var results = await Task.WhenAll(fetchTasks);

                foreach (var (repo, catalog) in results)
                {
                    repoInfos.Add(new
                    {
                        id = repo.Id,
                        label = repo.Label,
                        repoName = catalog?.RepoName,
                        repoDescription = catalog?.RepoDescription,
                        repoCompany = catalog?.RepoCompany,
                        repoLogo = catalog?.RepoLogo
                    });

                    if (catalog?.Apps != null)
                    {
                        foreach (var app in catalog.Apps)
                        {
                            allApps.Add(new
                            {
                                id = app.Id,
                                name = app.Name,
                                description = app.Description,
                                version = app.Version,
                                icon = app.Icon,
                                downloadUrl = app.DownloadUrl,
                                executableName = app.ExecutableName,
                                defaultArgs = app.DefaultArgs,
                                changelog = app.Changelog,
                                platforms = app.Platforms,
                                repoId = repo.Id,
                                repoLabel = repo.Label
                            });
                        }
                    }
                }

                var failedCount = results.Count(r => r.Item2 == null);
                return Ok(new { version = "1", apps = allApps, repos = repoInfos, failedRepos = failedCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdAppStore] Error fetching catalog");
                return StatusCode(500, new { error = "Failed to fetch catalog." });
            }
        }

        // ─────────────────────────────────────────────
        //  GET /api/MdAppStore/installed
        // ─────────────────────────────────────────────
        [HttpGet("installed")]
        public IActionResult GetInstalled()
        {
            try
            {
                var installed = _session.GetDal<InstalledApp>().GetList();
                return Ok(installed.Select(a => new
                {
                    appId = a.AppId,
                    name = a.Name,
                    version = a.Version,
                    icon = a.Icon,
                    description = a.Description,
                    installedAt = a.InstalledAt,
                    updatedAt = a.UpdatedAt,
                    localPath = a.LocalPath,
                    executableName = a.ExecutableName,
                    platform = a.Platform ?? "windows"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdAppStore] Error fetching installed apps");
                return StatusCode(500, new { error = "Failed to fetch installed apps." });
            }
        }

        // ─────────────────────────────────────────────
        //  POST /api/MdAppStore/install
        // ─────────────────────────────────────────────
        [HttpPost("install")]
        public async Task<IActionResult> InstallApp([FromBody] InstallAppRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.AppId) || string.IsNullOrWhiteSpace(request.DownloadUrl))
                return BadRequest(new { error = "AppId and DownloadUrl are required." });

            var platform = request.Platform ?? GetCurrentPlatform();
            string tempDir = null;
            try
            {
                var repo = ResolveRepo(request.RepoId, request.DownloadUrl);
                var client = _httpClientFactory.CreateClient();
                if (repo != null) ApplyBasicAuth(client, repo);

                _logger.LogInformation("[MdAppStore] Downloading app '{AppId}' ({Platform}) from {Url}",
                    request.AppId, platform, request.DownloadUrl);
                var response = await client.GetAsync(request.DownloadUrl);
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, new { error = $"Download failed: {response.ReasonPhrase}" });

                // Extract ZIP to temp directory
                tempDir = Path.Combine(Path.GetTempPath(), $"MdEInstall_{request.AppId}_{Guid.NewGuid():N}");
                Directory.CreateDirectory(tempDir);

                using var zipStream = await response.Content.ReadAsStreamAsync();
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                archive.ExtractToDirectory(tempDir, overwriteFiles: true);

                // Determine install directory
                var installDir = GetAppInstallDir(request.AppId);
                string executableName;

                if (platform == "linux")
                {
                    // Linux: find .AppImage, copy to installDir, chmod +x
                    var appImagePath = Directory.GetFiles(tempDir, "*.AppImage", SearchOption.AllDirectories).FirstOrDefault();
                    if (appImagePath == null)
                        return BadRequest(new { error = "No .AppImage file found inside the downloaded package." });

                    Directory.CreateDirectory(installDir);
                    executableName = Path.GetFileName(appImagePath);
                    var destPath = Path.Combine(installDir, executableName);
                    System.IO.File.Copy(appImagePath, destPath, overwrite: true);

                    // Make executable
                    var chmod = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "chmod",
                            Arguments = $"+x \"{destPath}\"",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    chmod.Start();
                    await chmod.WaitForExitAsync();

                    _logger.LogInformation("[MdAppStore] Installed Linux app '{AppId}' to {Dir}", request.AppId, installDir);
                }
                else
                {
                    // Windows: NSIS installer
                    var installerPath = Directory.GetFiles(tempDir, "*.exe", SearchOption.AllDirectories).FirstOrDefault();
                    if (installerPath == null)
                        return BadRequest(new { error = "No .exe installer found inside the downloaded package." });

                    _logger.LogInformation("[MdAppStore] Running NSIS installer '{Installer}' with /S /D={Dir}",
                        installerPath, installDir);

                    int exitCode = -1;
                    const int maxAttempts = 2;
                    for (int attempt = 1; attempt <= maxAttempts; attempt++)
                    {
                        var process = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = installerPath,
                                Arguments = $"/S /D={installDir}",
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        process.Start();
                        await process.WaitForExitAsync();
                        exitCode = process.ExitCode;

                        if (exitCode == 0) break;

                        _logger.LogWarning("[MdAppStore] Installer exited with code {Code} (attempt {Attempt}/{Max})",
                            exitCode, attempt, maxAttempts);

                        if (attempt < maxAttempts)
                        {
                            await Task.Delay(2000);
                        }
                    }

                    if (exitCode != 0)
                    {
                        return StatusCode(500, new { error = $"Installer exited with code {exitCode}. Make sure the app is not running." });
                    }

                    // Use executable name from catalog, but VERIFY it exists on disk.
                    // If the catalog name is wrong, fall back to scanning the install directory.
                    if (!string.IsNullOrWhiteSpace(request.ExecutableName))
                    {
                        var candidatePath = Path.Combine(installDir, request.ExecutableName.Trim());
                        if (System.IO.File.Exists(candidatePath))
                        {
                            executableName = request.ExecutableName.Trim();
                        }
                        else
                        {
                            _logger.LogWarning("[MdAppStore] Catalog executableName '{CatalogExe}' not found in install dir for '{AppId}', falling back to scan",
                                request.ExecutableName, request.AppId);
                            executableName = ScanForExecutable(installDir, request.AppId);
                        }
                    }
                    else
                    {
                        executableName = ScanForExecutable(installDir, request.AppId);
                        _logger.LogWarning("[MdAppStore] ExecutableName not in catalog for '{AppId}', resolved by scanning: {Exe}",
                            request.AppId, executableName);
                    }
                    _logger.LogInformation("[MdAppStore] Installed Windows app '{AppId}' to {Dir}, executable: {Exe}",
                        request.AppId, installDir, executableName);
                }

                // Upsert in InstalledApp table
                // Clear session cache first to avoid StaleObjectStateException from previous attempts
                _session.Clear();
                _session.BeginTransaction();
                try
                {
                    var installedDal = _session.GetDal<InstalledApp>();
                    var existing = installedDal.GetList().FirstOrDefault(a => a.AppId == request.AppId);
                    DateTime? originalInstalledAt = existing?.InstalledAt;
                    if (existing != null)
                    {
                        installedDal.Delete(existing);
                        _session.Flush();
                    }

                    var now = DateTime.UtcNow;
                    var newApp = new InstalledApp
                    {
                        AppId = request.AppId,
                        Name = request.Name ?? request.AppId,
                        Description = request.Description,
                        Version = request.Version,
                        LocalPath = installDir,
                        ExecutableName = executableName,
                        Icon = request.Icon,
                        Platform = platform,
                        InstalledAt = originalInstalledAt ?? now,
                        UpdatedAt = originalInstalledAt != null ? now : (DateTime?)null
                    };
                    installedDal.Save(newApp);
                    _session.Commit();
                }
                catch
                {
                    _session.Rollback();
                    throw;
                }

                return Ok(new { success = true, installDir, executableName });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdAppStore] Error installing app '{AppId}'", request?.AppId);
                return StatusCode(500, new { error = $"Installation failed: {ex.Message}" });
            }
            finally
            {
                // Clean up temp directory
                if (tempDir != null && Directory.Exists(tempDir))
                {
                    try { Directory.Delete(tempDir, recursive: true); }
                    catch { /* best effort cleanup */ }
                }
            }
        }

        // ─────────────────────────────────────────────
        //  DELETE /api/MdAppStore/uninstall/{appId}
        // ─────────────────────────────────────────────
        [HttpDelete("uninstall/{appId}")]
        public async Task<IActionResult> UninstallApp(string appId)
        {
            if (string.IsNullOrWhiteSpace(appId))
                return BadRequest(new { error = "appId is required." });

            try
            {
                var installedDal = _session.GetDal<InstalledApp>();
                var app = installedDal.GetList().FirstOrDefault(a => a.AppId == appId);
                if (app == null)
                    return NotFound(new { error = $"App '{appId}' is not installed." });

                // Try NSIS uninstaller first (Windows only), then fallback to directory delete
                if (Directory.Exists(app.LocalPath))
                {
                    var didUninstall = false;

                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var uninstaller = Directory.GetFiles(app.LocalPath, "unins*.exe", SearchOption.TopDirectoryOnly)
                            .FirstOrDefault();

                        if (uninstaller != null)
                        {
                            _logger.LogInformation("[MdAppStore] Running NSIS uninstaller '{Uninstaller}' for '{AppId}'",
                                uninstaller, appId);
                            var process = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = uninstaller,
                                    Arguments = "/S",
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };
                            process.Start();
                            await process.WaitForExitAsync();
                            _logger.LogInformation("[MdAppStore] Uninstaller exited with code {Code} for '{AppId}'",
                                process.ExitCode, appId);
                            didUninstall = true;
                        }
                    }

                    if (!didUninstall)
                    {
                        Directory.Delete(app.LocalPath, recursive: true);
                        _logger.LogInformation("[MdAppStore] Deleted files for '{AppId}' at {Path}", appId, app.LocalPath);
                    }
                }

                // Remove DB record
                _session.BeginTransaction();
                installedDal.Delete(app);
                _session.Commit();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdAppStore] Error uninstalling app '{AppId}'", appId);
                return StatusCode(500, new { error = $"Uninstall failed: {ex.Message}" });
            }
        }

        // ─────────────────────────────────────────────
        //  GET /api/MdAppStore/repositories
        // ─────────────────────────────────────────────
        [HttpGet("repositories")]
        public IActionResult GetRepositoriesList()
        {
            try
            {
                var repos = GetRepositories();
                return Ok(repos.Select(r => new
                {
                    id = r.Id,
                    label = r.Label,
                    url = r.Url,
                    username = r.Username ?? "",
                    passwordConfigured = !string.IsNullOrEmpty(r.Password),
                    sortOrder = r.SortOrder
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdAppStore] Error fetching repositories");
                return StatusCode(500, new { error = "Failed to fetch repositories." });
            }
        }

        // ─────────────────────────────────────────────
        //  POST /api/MdAppStore/repositories
        // ─────────────────────────────────────────────
        [HttpPost("repositories")]
        public IActionResult AddRepository([FromBody] AppStoreRepositoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Label) || string.IsNullOrWhiteSpace(request?.Url))
                return BadRequest(new { error = "Label and Url are required." });

            try
            {
                _session.BeginTransaction();
                var dal = _session.GetDal<AppStoreRepository>();
                var sortOrders = dal.GetList().Select(r => r.SortOrder).ToList();
                var maxSort = sortOrders.Any() ? sortOrders.Max() : -1;

                var repo = new AppStoreRepository
                {
                    Label = request.Label,
                    Url = request.Url,
                    Username = request.Username,
                    Password = request.Password,
                    SortOrder = maxSort + 1
                };
                dal.Save(repo);
                _session.Commit();

                return Ok(new { success = true, id = repo.Id });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[MdAppStore] Error adding repository");
                return StatusCode(500, new { error = "Failed to add repository." });
            }
        }

        // ─────────────────────────────────────────────
        //  PUT /api/MdAppStore/repositories/{id}
        // ─────────────────────────────────────────────
        [HttpPut("repositories/{id}")]
        public IActionResult UpdateRepository(Guid id, [FromBody] AppStoreRepositoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Label) || string.IsNullOrWhiteSpace(request?.Url))
                return BadRequest(new { error = "Label and Url are required." });

            try
            {
                _session.BeginTransaction();
                var dal = _session.GetDal<AppStoreRepository>();
                var repo = dal.GetList().FirstOrDefault(r => r.Id == id);
                if (repo == null)
                {
                    _session.Rollback();
                    return NotFound(new { error = "Repository not found." });
                }

                repo.Label = request.Label;
                repo.Url = request.Url;
                repo.Username = request.Username;
                if (request.Password != null) // null = don't change
                    repo.Password = request.Password;
                dal.Save(repo);
                _session.Commit();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[MdAppStore] Error updating repository");
                return StatusCode(500, new { error = "Failed to update repository." });
            }
        }

        // ─────────────────────────────────────────────
        //  DELETE /api/MdAppStore/repositories/{id}
        // ─────────────────────────────────────────────
        [HttpDelete("repositories/{id}")]
        public IActionResult DeleteRepository(Guid id)
        {
            try
            {
                _session.BeginTransaction();
                var dal = _session.GetDal<AppStoreRepository>();
                var repo = dal.GetList().FirstOrDefault(r => r.Id == id);
                if (repo == null)
                {
                    _session.Rollback();
                    return NotFound(new { error = "Repository not found." });
                }

                dal.Delete(repo);
                _session.Commit();

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _session.Rollback();
                _logger.LogError(ex, "[MdAppStore] Error deleting repository");
                return StatusCode(500, new { error = "Failed to delete repository." });
            }
        }

        // ─────────────────────────────────────────────
        //  POST /api/MdAppStore/publish
        // ─────────────────────────────────────────────
        [HttpPost("publish")]
        [RequestSizeLimit(500 * 1024 * 1024)]
        public async Task<IActionResult> PublishApp([FromForm] PublishAppFormRequest request)
        {
            _logger.LogInformation("[MdAppStore] PublishApp called. AppPackage={HasFile}, Platform={Platform}, RepoId={RepoId}",
                request?.AppPackage != null, request?.Platform, request?.RepoId);

            if (request?.AppPackage == null)
                return BadRequest(new { error = "A package file is required." });

            var platform = request.Platform ?? "windows";
            var fileName = request.AppPackage.FileName;
            _logger.LogInformation("[MdAppStore] FileName='{FileName}', Size={Size}, Platform={Platform}",
                fileName, request.AppPackage.Length, platform);

            var parsed = ParsePackageFilename(fileName, platform);
            if (parsed == null)
            {
                var expectedFormat = platform == "linux"
                    ? "{appId}-{version}.AppImage"
                    : "{appId}-setup-{version}.exe";
                _logger.LogWarning("[MdAppStore] Filename parse failed: '{FileName}' for platform '{Platform}'", fileName, platform);
                return BadRequest(new { error = $"Invalid filename '{fileName}'. Expected format: {expectedFormat}" });
            }

            var (appId, version) = parsed.Value;
            string tempFilePath = null;

            try
            {
                var repo = ResolveRepoById(request.RepoId);
                if (repo == null)
                    return BadRequest(new { error = "No repository configured. Add a repository in Settings first." });

                var repoBase = EnsureTrailingSlash(repo.Url);

                // 1. Save package to temp file
                tempFilePath = Path.Combine(Path.GetTempPath(), $"MdEPublish_{Guid.NewGuid():N}_{fileName}");
                using (var tempStream = new FileStream(tempFilePath, FileMode.Create))
                {
                    await request.AppPackage.CopyToAsync(tempStream);
                }

                // 2. Extract or process icon
                string iconBase64 = null;
                if (request.CustomIcon != null)
                {
                    iconBase64 = await ConvertIconToBase64(request.CustomIcon);
                }
                else if (platform == "windows")
                {
                    // Try to extract icon from exe (Windows only)
                    iconBase64 = TryExtractExeIcon(tempFilePath);
                }

                // 3. Create ZIP from temp file — platform-aware path
                var remoteFileName = $"{appId}-{version}.zip";
                var fileUrl = $"{repoBase}{appId}/{platform}/{remoteFileName}";

                var uploadClient = _httpClientFactory.CreateClient();
                ApplyBasicAuth(uploadClient, repo);

                _logger.LogInformation("[MdAppStore] Auto-zipping '{FileName}' for {Platform} before upload", fileName, platform);
                var ms = new MemoryStream();
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
                {
                    var entry = archive.CreateEntry(fileName, CompressionLevel.Optimal);
                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read);
                    await fileStream.CopyToAsync(entryStream);
                }
                ms.Position = 0;

                // 4. Upload ZIP to Nexus (with progress via SignalR)
                var connectionId = Request.Query["ConnectionId"].ToString();
                try
                {
                    var totalBytes = ms.Length;
                    var progressStream = new ProgressStream(ms, totalBytes, async (percent, phase) =>
                    {
                        if (!string.IsNullOrEmpty(connectionId))
                        {
                            await _hubContext.Clients.Client(connectionId)
                                .SendAsync("publishProgress", new { appId, percent, phase });
                        }
                    });

                    var fileContent = new StreamContent(progressStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    // Notify: upload starting
                    if (!string.IsNullOrEmpty(connectionId))
                        await _hubContext.Clients.Client(connectionId)
                            .SendAsync("publishProgress", new { appId, percent = 0, phase = "uploading" });

                    var uploadResp = await uploadClient.PutAsync(fileUrl, fileContent);
                    if (!uploadResp.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("[MdAppStore] Nexus upload failed: {StatusCode}", uploadResp.StatusCode);
                        return StatusCode(502, new { error = $"Nexus upload failed: {uploadResp.StatusCode}" });
                    }
                    _logger.LogInformation("[MdAppStore] Uploaded '{AppId}' v{Version} ({Platform}) to {Url}", appId, version, platform, fileUrl);
                }
                finally
                {
                    await ms.DisposeAsync();
                }

                // Notify: updating catalog
                if (!string.IsNullOrEmpty(connectionId))
                    await _hubContext.Clients.Client(connectionId)
                        .SendAsync("publishProgress", new { appId, percent = 100, phase = "catalog" });

                // 5. Read current catalog.json
                var catalogUrl = repoBase + "catalog.json";
                var catalogClient = _httpClientFactory.CreateClient();
                ApplyBasicAuth(catalogClient, repo);
                AppStoreCatalog catalog;
                var catalogResp = await catalogClient.GetAsync(catalogUrl);
                if (catalogResp.IsSuccessStatusCode)
                {
                    var json = await catalogResp.Content.ReadAsStringAsync();
                    catalog = JsonSerializer.Deserialize<AppStoreCatalog>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new AppStoreCatalog { Version = "1", Apps = new List<AppStoreCatalogEntry>() };
                }
                else
                {
                    catalog = new AppStoreCatalog { Version = "1", Apps = new List<AppStoreCatalogEntry>() };
                }

                // 6. Add/update catalog entry — merge into Platforms dict
                catalog.Apps ??= new List<AppStoreCatalogEntry>();
                var existingEntry = catalog.Apps.FirstOrDefault(a => a.Id == appId);

                // Executable name: from form field, or fallback for linux (AppImage filename)
                var executableName = !string.IsNullOrWhiteSpace(request.ExecutableName)
                    ? request.ExecutableName.Trim()
                    : (platform == "linux" ? fileName : $"{appId}.exe");

                // Use name from form, or derive from appId
                var derivedName = Regex.Replace(appId, @"(^|-)(\w)", m =>
                    (m.Groups[1].Value == "-" ? " " : "") + char.ToUpper(m.Groups[2].Value[0]) + m.Groups[2].Value[1..]);

                var newBuild = new PlatformBuild
                {
                    DownloadUrl = fileUrl,
                    ExecutableName = executableName,
                    Version = version
                };

                if (existingEntry != null)
                {
                    // Merge: update version, preserve other platform builds
                    existingEntry.Name = !string.IsNullOrWhiteSpace(request.Name) ? request.Name : existingEntry.Name ?? derivedName;
                    existingEntry.Description = request.Description ?? existingEntry.Description;
                    existingEntry.Version = version;
                    existingEntry.Icon = iconBase64 ?? existingEntry.Icon;
                    existingEntry.Platforms ??= new Dictionary<string, PlatformBuild>();

                    // Migrate legacy fields into platforms dict if not already there
                    if (existingEntry.Platforms.Count == 0
                        && !string.IsNullOrEmpty(existingEntry.DownloadUrl))
                    {
                        existingEntry.Platforms["windows"] = new PlatformBuild
                        {
                            DownloadUrl = existingEntry.DownloadUrl,
                            ExecutableName = existingEntry.ExecutableName
                        };
                    }

                    existingEntry.Platforms[platform] = newBuild;

                    // Keep root fields updated (backward compat: prefer windows, or the only build)
                    var rootBuild = existingEntry.Platforms.ContainsKey("windows")
                        ? existingEntry.Platforms["windows"]
                        : existingEntry.Platforms.Values.First();
                    existingEntry.DownloadUrl = rootBuild.DownloadUrl;
                    existingEntry.ExecutableName = rootBuild.ExecutableName;
                }
                else
                {
                    var entry = new AppStoreCatalogEntry
                    {
                        Id = appId,
                        Name = !string.IsNullOrWhiteSpace(request.Name) ? request.Name : derivedName,
                        Description = request.Description,
                        Version = version,
                        DownloadUrl = fileUrl,
                        ExecutableName = executableName,
                        Icon = iconBase64,
                        Platforms = new Dictionary<string, PlatformBuild> { [platform] = newBuild }
                    };
                    catalog.Apps.Add(entry);
                }

                // 7. Rewrite catalog.json on Nexus
                var catalogJson = JsonSerializer.Serialize(catalog,
                    new JsonSerializerOptions { WriteIndented = true });
                var catalogContent = new StringContent(catalogJson, Encoding.UTF8, "application/json");
                var writeClient = _httpClientFactory.CreateClient();
                ApplyBasicAuth(writeClient, repo);
                var writeResp = await writeClient.PutAsync(catalogUrl, catalogContent);
                if (!writeResp.IsSuccessStatusCode)
                {
                    _logger.LogWarning("[MdAppStore] Failed to update catalog.json: {StatusCode}", writeResp.StatusCode);
                    return StatusCode(502, new { error = $"Failed to update catalog.json: {writeResp.StatusCode}" });
                }

                return Ok(new { success = true, downloadUrl = fileUrl, icon = iconBase64 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdAppStore] Error publishing app '{AppId}'", appId);
                return StatusCode(500, new { error = $"Publish failed: {ex.Message}" });
            }
            finally
            {
                // Cleanup temp file
                if (tempFilePath != null && System.IO.File.Exists(tempFilePath))
                {
                    try { System.IO.File.Delete(tempFilePath); }
                    catch { /* best effort cleanup */ }
                }
            }
        }

        // ─────────────────────────────────────────────
        //  PUT /api/MdAppStore/catalog/metadata
        // ─────────────────────────────────────────────
        [HttpPut("catalog/metadata")]
        public async Task<IActionResult> UpdateCatalogMetadata([FromForm] CatalogMetadataRequest request)
        {
            try
            {
                var repo = ResolveRepoById(request.RepoId);
                if (repo == null)
                    return BadRequest(new { error = "No repository configured." });

                var repoBase = EnsureTrailingSlash(repo.Url);
                var catalogUrl = repoBase + "catalog.json";

                var client = _httpClientFactory.CreateClient();
                ApplyBasicAuth(client, repo);

                // Read current catalog
                AppStoreCatalog catalog;
                var resp = await client.GetAsync(catalogUrl);
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    catalog = JsonSerializer.Deserialize<AppStoreCatalog>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new AppStoreCatalog { Version = "1", Apps = new List<AppStoreCatalogEntry>() };
                }
                else
                {
                    catalog = new AppStoreCatalog { Version = "1", Apps = new List<AppStoreCatalogEntry>() };
                }

                // Update metadata fields
                if (request.RepoName != null) catalog.RepoName = request.RepoName;
                if (request.RepoDescription != null) catalog.RepoDescription = request.RepoDescription;
                if (request.RepoCompany != null) catalog.RepoCompany = request.RepoCompany;
                if (request.LogoFile != null) catalog.RepoLogo = await ConvertFileToBase64(request.LogoFile);

                // Rewrite catalog.json
                var catalogJson = JsonSerializer.Serialize(catalog,
                    new JsonSerializerOptions { WriteIndented = true });
                var writeClient = _httpClientFactory.CreateClient();
                ApplyBasicAuth(writeClient, repo);
                var writeResp = await writeClient.PutAsync(catalogUrl,
                    new StringContent(catalogJson, Encoding.UTF8, "application/json"));
                if (!writeResp.IsSuccessStatusCode)
                    return StatusCode(502, new { error = $"Failed to update catalog.json: {writeResp.StatusCode}" });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdAppStore] Error updating catalog metadata");
                return StatusCode(500, new { error = $"Failed to update catalog metadata: {ex.Message}" });
            }
        }

        // ─────────────────────────────────────────────
        //  PUT /api/MdAppStore/catalog/entry
        // ─────────────────────────────────────────────
        [HttpPut("catalog/entry")]
        public async Task<IActionResult> UpdateCatalogEntry([FromForm] UpdateCatalogEntryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Id))
                return BadRequest(new { error = "Id is required." });

            try
            {
                var repo = ResolveRepoById(request.RepoId);
                if (repo == null)
                    return BadRequest(new { error = "No repository configured." });

                var repoBase = EnsureTrailingSlash(repo.Url);
                var catalogUrl = repoBase + "catalog.json";

                var client = _httpClientFactory.CreateClient();
                ApplyBasicAuth(client, repo);

                var resp = await client.GetAsync(catalogUrl);
                if (!resp.IsSuccessStatusCode)
                    return StatusCode(502, new { error = $"Failed to read catalog.json: {resp.StatusCode}" });

                var json = await resp.Content.ReadAsStringAsync();
                var catalog = JsonSerializer.Deserialize<AppStoreCatalog>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (catalog?.Apps == null)
                    return NotFound(new { error = "Catalog is empty." });

                var entry = catalog.Apps.FirstOrDefault(a => a.Id == request.Id);
                if (entry == null)
                    return NotFound(new { error = $"App '{request.Id}' not found in catalog." });

                // Update only provided fields
                if (request.Name != null) entry.Name = request.Name;
                if (request.Description != null) entry.Description = request.Description;
                if (request.Version != null) entry.Version = request.Version;
                if (request.IconFile != null) entry.Icon = await ConvertIconToBase64(request.IconFile);

                // Rewrite catalog.json
                var catalogJson = JsonSerializer.Serialize(catalog,
                    new JsonSerializerOptions { WriteIndented = true });
                var writeClient = _httpClientFactory.CreateClient();
                ApplyBasicAuth(writeClient, repo);
                var writeResp = await writeClient.PutAsync(catalogUrl,
                    new StringContent(catalogJson, Encoding.UTF8, "application/json"));
                if (!writeResp.IsSuccessStatusCode)
                    return StatusCode(502, new { error = $"Failed to update catalog.json: {writeResp.StatusCode}" });

                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MdAppStore] Error updating catalog entry '{Id}'", request?.Id);
                return StatusCode(500, new { error = $"Failed to update catalog entry: {ex.Message}" });
            }
        }

        // ─────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────
        private List<AppStoreRepository> GetRepositories()
        {
            return _session.GetDal<AppStoreRepository>().GetList()
                .OrderBy(r => r.SortOrder).ToList();
        }

        private static void ApplyBasicAuth(HttpClient client, AppStoreRepository repo)
        {
            if (!string.IsNullOrWhiteSpace(repo.Username))
            {
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{repo.Username}:{repo.Password}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            }
        }

        /// <summary>
        /// Resolves a repo by ID, or returns the first repo if only one exists.
        /// </summary>
        private AppStoreRepository ResolveRepoById(string repoId)
        {
            var repos = GetRepositories();
            if (!string.IsNullOrEmpty(repoId) && Guid.TryParse(repoId, out var id))
                return repos.FirstOrDefault(r => r.Id == id);
            return repos.FirstOrDefault();
        }

        /// <summary>
        /// Resolves a repo for install: by explicit RepoId, or by matching the download URL.
        /// </summary>
        private AppStoreRepository ResolveRepo(string repoId, string downloadUrl)
        {
            var repos = GetRepositories();
            if (!string.IsNullOrEmpty(repoId) && Guid.TryParse(repoId, out var id))
                return repos.FirstOrDefault(r => r.Id == id);

            // Try to match the download URL to a repo
            if (!string.IsNullOrEmpty(downloadUrl))
            {
                var match = repos.FirstOrDefault(r =>
                    downloadUrl.StartsWith(EnsureTrailingSlash(r.Url), StringComparison.OrdinalIgnoreCase));
                if (match != null) return match;
            }

            return repos.FirstOrDefault();
        }

        private static string EnsureTrailingSlash(string url) =>
            string.IsNullOrEmpty(url) ? url : url.TrimEnd('/') + "/";

        /// <summary>
        /// Parses a package filename per platform:
        /// Windows: {appId}-setup-{version}.exe
        /// Linux:   {appId}-{version}.AppImage
        /// </summary>
        private static (string appId, string version)? ParsePackageFilename(string fileName, string platform)
        {
            if (platform == "linux")
            {
                var match = Regex.Match(fileName, @"^(.+)-(.+)\.AppImage$", RegexOptions.IgnoreCase);
                if (!match.Success) return null;
                return (match.Groups[1].Value, match.Groups[2].Value);
            }
            else
            {
                var match = Regex.Match(fileName, @"^(.+)-setup-(.+)\.exe$", RegexOptions.IgnoreCase);
                if (!match.Success) return null;
                return (match.Groups[1].Value, match.Groups[2].Value);
            }
        }

        private static string GetCurrentPlatform()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                return "linux";
            return "windows";
        }

        /// <summary>
        /// Scans the install directory for the main executable, excluding uninstallers.
        /// Returns the largest .exe by file size, or {appId}.exe as last resort.
        /// </summary>
        private static string ScanForExecutable(string installDir, string appId)
        {
            var installedExes = Directory.GetFiles(installDir, "*.exe", SearchOption.TopDirectoryOnly)
                .Where(f => !Path.GetFileName(f).StartsWith("Uninstall", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => new FileInfo(f).Length)
                .ToArray();
            return installedExes.Length > 0
                ? Path.GetFileName(installedExes[0])
                : $"{appId}.exe";
        }

        /// <summary>
        /// Cross-platform install directory for MdExplorer apps.
        /// Windows: %LOCALAPPDATA%\MdExplorer-apps\{appId}\
        /// Linux:   ~/.local/share/MdExplorer-apps/{appId}/
        /// </summary>
        private static string GetAppInstallDir(string appId)
        {
            string basePath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            }
            else
            {
                basePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".local", "share");
            }
            return Path.Combine(basePath, "MdExplorer-apps", appId);
        }

        /// <summary>
        /// Tries to extract the associated icon from an exe file (Windows only).
        /// Returns a base64 data URL or null on failure/non-Windows.
        /// </summary>
        private string TryExtractExeIcon(string exePath)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return null;

            try
            {
                using var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
                if (icon == null) return null;

                using var bitmap = new System.Drawing.Bitmap(32, 32);
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawIcon(icon, new System.Drawing.Rectangle(0, 0, 32, 32));
                }

                using var ms = new MemoryStream();
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                var base64 = Convert.ToBase64String(ms.ToArray());
                _logger.LogInformation("[MdAppStore] Extracted icon from exe ({Bytes} bytes base64)", base64.Length);
                return $"data:image/png;base64,{base64}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "[MdAppStore] Failed to extract icon from exe — will use default");
                return null;
            }
        }

        /// <summary>
        /// Converts an uploaded image file to a base64 data URL.
        /// On Windows: resizes to 32x32 PNG. On Linux: returns raw bytes (no resize).
        /// </summary>
        private static async Task<string> ConvertIconToBase64(IFormFile iconFile)
        {
            using var inputStream = new MemoryStream();
            await iconFile.CopyToAsync(inputStream);
            inputStream.Position = 0;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var original = new System.Drawing.Bitmap(inputStream);
                using var resized = new System.Drawing.Bitmap(32, 32);
                using (var g = System.Drawing.Graphics.FromImage(resized))
                {
                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    g.DrawImage(original, 0, 0, 32, 32);
                }

                using var ms = new MemoryStream();
                resized.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                var base64 = Convert.ToBase64String(ms.ToArray());
                return $"data:image/png;base64,{base64}";
            }
            else
            {
                // Linux/macOS: no System.Drawing — return raw bytes as-is
                var mimeType = iconFile.ContentType ?? "image/png";
                var base64 = Convert.ToBase64String(inputStream.ToArray());
                return $"data:{mimeType};base64,{base64}";
            }
        }

        /// <summary>
        /// Converts an uploaded file to a base64 data URL without resizing.
        /// Used for repo logos that must keep their original proportions.
        /// </summary>
        private static async Task<string> ConvertFileToBase64(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var mimeType = file.ContentType ?? "image/png";
            var base64 = Convert.ToBase64String(ms.ToArray());
            return $"data:{mimeType};base64,{base64}";
        }
    }

    public class InstallAppRequest
    {
        public string AppId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
        public string Icon { get; set; }
        public string Platform { get; set; }
        public string RepoId { get; set; }
        public string ExecutableName { get; set; }
    }

    public class PublishAppFormRequest
    {
        public IFormFile AppPackage { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ExecutableName { get; set; }
        public IFormFile? CustomIcon { get; set; }
        public string? Platform { get; set; }
        public string? RepoId { get; set; }
    }

    public class CatalogMetadataRequest
    {
        public string? RepoName { get; set; }
        public string? RepoDescription { get; set; }
        public string? RepoCompany { get; set; }
        public IFormFile? LogoFile { get; set; }
        public string? RepoId { get; set; }
    }

    public class UpdateCatalogEntryRequest
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
        public IFormFile? IconFile { get; set; }
        public string? RepoId { get; set; }
    }

    public class AppStoreRepositoryRequest
    {
        public string Label { get; set; }
        public string Url { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; } // null = don't change on update
    }

    /// <summary>
    /// A Stream wrapper that reports read progress via a callback.
    /// Used to track upload progress when streaming to Nexus.
    /// </summary>
    internal class ProgressStream : Stream
    {
        private readonly Stream _inner;
        private readonly long _totalBytes;
        private readonly Func<int, string, Task> _onProgress;
        private long _bytesRead;
        private int _lastReportedPercent = -1;

        public ProgressStream(Stream inner, long totalBytes, Func<int, string, Task> onProgress)
        {
            _inner = inner;
            _totalBytes = totalBytes;
            _onProgress = onProgress;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = _inner.Read(buffer, offset, count);
            _bytesRead += read;
            ReportProgress();
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = await _inner.ReadAsync(buffer, offset, count, cancellationToken);
            _bytesRead += read;
            ReportProgress();
            return read;
        }

        private void ReportProgress()
        {
            if (_totalBytes <= 0) return;
            var percent = (int)((_bytesRead * 100) / _totalBytes);
            if (percent > 100) percent = 100;
            // Throttle: report only when percent changes
            if (percent != _lastReportedPercent)
            {
                _lastReportedPercent = percent;
                _onProgress(percent, "uploading").ConfigureAwait(false);
            }
        }

        public override bool CanRead => _inner.CanRead;
        public override bool CanSeek => _inner.CanSeek;
        public override bool CanWrite => _inner.CanWrite;
        public override long Length => _inner.Length;
        public override long Position
        {
            get => _inner.Position;
            set => _inner.Position = value;
        }
        public override void Flush() => _inner.Flush();
        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
        public override void SetLength(long value) => _inner.SetLength(value);
        public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
    }
}

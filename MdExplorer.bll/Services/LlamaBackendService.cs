using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services
{
    public class LlamaBackendInfo
    {
        public string Variant { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long DownloadSize { get; set; }
        public bool IsInstalled { get; set; }
        public bool IsRecommended { get; set; }
    }

    public class LlamaBackendStatus
    {
        public bool IsInstalled { get; set; }
        public string InstalledVariant { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
        public bool HasCudaDll { get; set; }
        public bool HasVulkanDll { get; set; }
    }

    public interface ILlamaBackendService
    {
        List<LlamaBackendInfo> GetAvailableBackends();
        LlamaBackendStatus GetInstalledBackend();
        string GetBackendPath();
        bool IsBackendInstalled();
        Task<bool> DownloadBackendAsync(string variant, IProgress<DownloadProgress> progress, CancellationToken ct = default);
        Task<bool> DeleteBackendAsync();
        string GetRecommendedBackend();
        string GetVersion();

        /// <summary>
        /// Il percorso di <c>llama-server</c> nel motore installato, o <c>null</c> se non c'è. Lo usa
        /// chi deve dare al modello anche un'immagine (l'OCR): stesso pacchetto e stessa versione
        /// della chat, ma come processo a parte.
        /// </summary>
        string GetServerPath();

        /// <summary>
        /// Fa in modo che <c>llama-server</c> ci sia. Se il motore è già installato ma senza
        /// l'eseguibile (installazioni fatte prima che venisse tenuto), aggiunge solo quello, senza
        /// toccare le librerie che la chat può avere in uso. Se il motore manca, scarica la variante
        /// consigliata per questo computer.
        /// </summary>
        Task<bool> EnsureServerAsync(IProgress<DownloadProgress> progress, CancellationToken ct = default);
    }

    public class LlamaBackendService : ILlamaBackendService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<LlamaBackendService> _logger;
        private readonly IGpuDetectionService _gpuService;
        private readonly string _version;
        private readonly string _backendsBaseDir;

        // GitHub release URL pattern. L'estensione dipende dal sistema: .zip per Windows, .tar.gz per
        // Linux e macOS (verificato su b8781 il 15/11/2026: il .zip per Linux non esiste, 404).
        private const string GitHubUrlTemplate = "https://github.com/ggml-org/llama.cpp/releases/download/{0}/llama-{0}-bin-{1}{2}";

        private static string ArchiveExtension =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".zip" : ".tar.gz";

        private static string ServerFileName =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "llama-server.exe" : "llama-server";

        // Backend variants per platform (pesi degli archivi di b10221, letti dalla pagina del rilascio il 21/11/2026)
        private static readonly Dictionary<string, (string name, string description, long winSize, long linuxSize)> Variants = new()
        {
            ["cpu"] = ("CPU Only", "Runs on any CPU, no GPU required", 18_000_000, 16_000_000),
            ["cuda-12.4"] = ("NVIDIA CUDA 12.4", "GPU acceleration for NVIDIA GPUs (requires CUDA)", 250_000_000, 0),
            ["vulkan"] = ("Vulkan", "GPU acceleration for AMD, Intel, and NVIDIA GPUs", 34_000_000, 32_000_000),
        };

        public LlamaBackendService(
            IHttpClientFactory httpClientFactory,
            ILogger<LlamaBackendService> logger,
            IGpuDetectionService gpuService,
            IConfiguration configuration)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.Timeout = TimeSpan.FromHours(1);
            _logger = logger;
            _gpuService = gpuService;

            _version = configuration.GetValue<string>("LlamaCpp:Version") ?? "b10221";

            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _backendsBaseDir = Path.Combine(userHome, "MdExplorer-Models", "llama-backends");
            Directory.CreateDirectory(_backendsBaseDir);
        }

        /// <summary>
        /// Resolves the backend path statically (no DI needed).
        /// Used by Startup.cs before services are available.
        /// </summary>
        public static string ResolveBackendPath(string version)
        {
            var userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var backendDir = Path.Combine(userHome, "MdExplorer-Models", "llama-backends", version);
            if (HasLlamaLibrary(backendDir))
            {
                return backendDir;
            }

            // Fallback to LlamaCppOverride in app directory (dev/legacy)
            var overrideDir = Path.Combine(AppContext.BaseDirectory, "LlamaCppOverride");
            if (HasLlamaLibrary(overrideDir))
            {
                return overrideDir;
            }

            return null;
        }

        /// <summary>
        /// Il nome del file di una libreria del motore su questo sistema: <c>llama.dll</c> su Windows,
        /// <c>libllama.so</c> su Linux, <c>libllama.dylib</c> su macOS. Prima si cercava sempre il nome di Windows
        /// e su Linux il motore scaricato non veniva visto (stato «non installato», modello non caricato).
        /// </summary>
        public static string NativeLibraryFile(string name)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return name + ".dll";
            return RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? "lib" + name + ".dylib" : "lib" + name + ".so";
        }

        private static bool HasLlamaLibrary(string directory) =>
            Directory.Exists(directory) && File.Exists(Path.Combine(directory, NativeLibraryFile("llama")));

        public string GetVersion() => _version;

        public string GetBackendPath()
        {
            return ResolveBackendPath(_version);
        }

        public bool IsBackendInstalled()
        {
            return GetBackendPath() != null;
        }

        private string VersionDirectory => Path.Combine(_backendsBaseDir, _version);

        public string GetServerPath()
        {
            var path = Path.Combine(VersionDirectory, ServerFileName);
            return File.Exists(path) ? path : null;
        }

        /// <summary>Le librerie del motore sono nella cartella della versione (non nel ripiego <c>LlamaCppOverride</c>).</summary>
        private bool HasLibraries() => HasLlamaLibrary(VersionDirectory);

        private string InstalledVariantOnDisk()
        {
            if (Directory.EnumerateFiles(VersionDirectory, "*ggml-cuda*").Any()) return "cuda-12.4";
            if (Directory.EnumerateFiles(VersionDirectory, "*ggml-vulkan*").Any()) return "vulkan";
            return "cpu";
        }

        public async Task<bool> EnsureServerAsync(IProgress<DownloadProgress> progress, CancellationToken ct = default)
        {
            if (GetServerPath() != null)
            {
                return true;
            }
            if (!HasLibraries())
            {
                return await DownloadBackendAsync(GetRecommendedBackend(), progress, ct);
            }

            // Motore già installato, manca solo l'eseguibile: stessa variante, e della cartella non si
            // cancella niente (su Windows le dll possono essere caricate dalla chat, non si toccano).
            var variant = InstalledVariantOnDisk();
            var archive = await DownloadArchiveAsync(variant, progress, ct);
            await ExtractAsync(archive, VersionDirectory, name => name == ServerFileName, ct);
            File.Delete(archive);
            if (GetServerPath() == null)
            {
                throw new InvalidOperationException(UiText.Say(
                    $"Nel pacchetto del motore ({variant}, {_version}) non c'è {ServerFileName}.",
                    $"The engine package ({variant}, {_version}) does not contain {ServerFileName}."));
            }
            _logger.LogInformation("Added {Server} to the installed backend {Variant}", ServerFileName, variant);
            progress?.Report(new DownloadProgress { ModelId = $"backend-{variant}", BytesDownloaded = 1, TotalBytes = 1, Status = "Complete" });
            return true;
        }

        /// <summary>Ciò che del pacchetto si tiene: le librerie e <c>llama-server</c>; gli altri strumenti no.</summary>
        private static bool IsEngineFile(string name)
        {
            if (name == ServerFileName) return true;
            var lower = name.ToLowerInvariant();
            // Su Linux le librerie hanno la versione dopo l'estensione (libllama.so.0.0.8781).
            return lower.EndsWith(".dll") || lower.EndsWith(".dylib") || lower.EndsWith(".metal")
                || lower.EndsWith(".so") || lower.Contains(".so.");
        }

        /// <summary>Scarica il pacchetto della variante e restituisce dove l'ha messo. Riprende un download interrotto.</summary>
        private async Task<string> DownloadArchiveAsync(string variant, IProgress<DownloadProgress> progress, CancellationToken ct)
        {
            var platformSuffix = GetPlatformSuffix(variant) ?? throw new PlatformNotSupportedException(UiText.Say(
                $"Il motore llama.cpp «{variant}» non è previsto per questo sistema.",
                $"The llama.cpp engine \"{variant}\" is not available for this system."));
            var url = string.Format(GitHubUrlTemplate, _version, platformSuffix, ArchiveExtension);
            var archive = Path.Combine(_backendsBaseDir, $"{_version}-{variant}{ArchiveExtension}.download");

            _logger.LogInformation("Downloading llama.cpp backend {Variant} from {Url}", variant, url);
            progress?.Report(new DownloadProgress { ModelId = $"backend-{variant}", BytesDownloaded = 0, TotalBytes = 0, Status = "Connecting" });

            long startByte = File.Exists(archive) ? new FileInfo(archive).Length : 0;
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (startByte > 0)
            {
                request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(startByte, null);
            }

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
            if (response.StatusCode == System.Net.HttpStatusCode.RequestedRangeNotSatisfiable && File.Exists(archive))
            {
                _logger.LogInformation("Archive appears complete, proceeding to extract");
                return archive;
            }
            response.EnsureSuccessStatusCode();

            // Chiesto un pezzo ma arrivato tutto (200 invece di 206): si ricomincia, non si accoda.
            var resuming = startByte > 0 && response.StatusCode == System.Net.HttpStatusCode.PartialContent;
            var totalBytes = (response.Content.Headers.ContentLength ?? 0) + (resuming ? startByte : 0);
            long totalBytesRead = resuming ? startByte : 0;
            using (var contentStream = await response.Content.ReadAsStreamAsync(ct))
            using (var fileStream = new FileStream(archive, resuming ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
            {
                var buffer = new byte[81920];
                int bytesRead;
                var lastProgressUpdate = DateTime.UtcNow;
                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                    totalBytesRead += bytesRead;
                    if ((DateTime.UtcNow - lastProgressUpdate).TotalMilliseconds >= 100)
                    {
                        progress?.Report(new DownloadProgress
                        {
                            ModelId = $"backend-{variant}", BytesDownloaded = totalBytesRead, TotalBytes = totalBytes, Status = "Downloading"
                        });
                        lastProgressUpdate = DateTime.UtcNow;
                    }
                }
            }
            return archive;
        }

        /// <summary>
        /// Estrae i file scelti, tutti nella stessa cartella (il pacchetto ha una cartella radice che
        /// qui non serve). Nel .tar.gz di Linux le librerie sono catene di collegamenti
        /// (<c>libllama.so → libllama.so.0 → libllama.so.0.0.8781</c>): vanno ricreati, non copiati.
        /// </summary>
        private static async Task ExtractAsync(string archive, string targetDir, Func<string, bool> keep, CancellationToken ct)
        {
            Directory.CreateDirectory(targetDir);
            if (ArchiveExtension == ".zip")
            {
                using var zip = ZipFile.OpenRead(archive);
                foreach (var entry in zip.Entries)
                {
                    if (string.IsNullOrEmpty(entry.Name) || !keep(entry.Name)) continue;
                    entry.ExtractToFile(Path.Combine(targetDir, entry.Name), true);
                }
                return;
            }

            await using var file = File.OpenRead(archive);
            await using var gzip = new GZipStream(file, CompressionMode.Decompress);
            await using var tar = new TarReader(gzip);
            TarEntry tarEntry;
            while ((tarEntry = await tar.GetNextEntryAsync(cancellationToken: ct)) != null)
            {
                var name = Path.GetFileName(tarEntry.Name);
                if (string.IsNullOrEmpty(name) || !keep(name)) continue;
                var destination = Path.Combine(targetDir, name);
                if (tarEntry.EntryType == TarEntryType.SymbolicLink)
                {
                    if (File.Exists(destination) || new FileInfo(destination).LinkTarget != null) File.Delete(destination);
                    File.CreateSymbolicLink(destination, Path.GetFileName(tarEntry.LinkName));
                }
                else if (tarEntry.EntryType == TarEntryType.RegularFile || tarEntry.EntryType == TarEntryType.V7RegularFile)
                {
                    await tarEntry.ExtractToFileAsync(destination, overwrite: true, ct);
                }
            }
        }

        public LlamaBackendStatus GetInstalledBackend()
        {
            var path = GetBackendPath();
            if (path == null)
            {
                return new LlamaBackendStatus { IsInstalled = false, Version = _version };
            }

            // Detect variant from installed files
            // «*ggml-…*»: su Linux i file hanno il prefisso lib (libggml-vulkan.so)
            var hasCuda = Directory.GetFiles(path, "*ggml-cuda*").Length > 0;
            var hasVulkan = Directory.GetFiles(path, "*ggml-vulkan*").Length > 0;

            string variant = "cpu";
            if (hasCuda) variant = "cuda-12.4";
            else if (hasVulkan) variant = "vulkan";

            return new LlamaBackendStatus
            {
                IsInstalled = true,
                InstalledVariant = variant,
                Version = _version,
                Path = path,
                HasCudaDll = hasCuda,
                HasVulkanDll = hasVulkan
            };
        }

        public List<LlamaBackendInfo> GetAvailableBackends()
        {
            var installed = GetInstalledBackend();
            var recommended = GetRecommendedBackend();
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var backends = new List<LlamaBackendInfo>();
            foreach (var kv in Variants)
            {
                // Skip CUDA on non-Windows (Linux uses different packaging)
                if (kv.Key == "cuda-12.4" && !isWindows) continue;

                backends.Add(new LlamaBackendInfo
                {
                    Variant = kv.Key,
                    Name = kv.Value.name,
                    Description = kv.Value.description,
                    DownloadSize = isWindows ? kv.Value.winSize : kv.Value.linuxSize,
                    IsInstalled = installed.IsInstalled && installed.InstalledVariant == kv.Key,
                    IsRecommended = kv.Key == recommended
                });
            }

            return backends;
        }

        public string GetRecommendedBackend()
        {
            try
            {
                var gpuInfo = _gpuService.DetectGpu();
                if (gpuInfo.IsNvidiaGpu && gpuInfo.IsCudaAvailable &&
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return "cuda-12.4";
                }
                // Vulkan works on AMD, Intel, and NVIDIA
                if (gpuInfo.IsNvidiaGpu || !string.IsNullOrEmpty(gpuInfo.Name))
                {
                    return "vulkan";
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "GPU detection failed for backend recommendation");
            }

            return "cpu";
        }

        public async Task<bool> DownloadBackendAsync(string variant, IProgress<DownloadProgress> progress, CancellationToken ct = default)
        {
            if (!Variants.ContainsKey(variant))
            {
                _logger.LogError("Unknown backend variant: {Variant}", variant);
                return false;
            }

            if (GetPlatformSuffix(variant) == null)
            {
                _logger.LogError("Unsupported platform for variant: {Variant}", variant);
                return false;
            }

            var targetDir = VersionDirectory;
            long totalBytesRead = 0;

            try
            {
                // Clean up target directory if it exists (switching variant)
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                }

                var archive = await DownloadArchiveAsync(variant, progress, ct);
                totalBytesRead = new FileInfo(archive).Length;

                progress?.Report(new DownloadProgress
                {
                    ModelId = $"backend-{variant}",
                    BytesDownloaded = totalBytesRead,
                    TotalBytes = totalBytesRead,
                    Status = "Extracting"
                });

                _logger.LogInformation("Extracting backend to {Dir}", targetDir);
                // Librerie e llama-server: la chat usa le prime, l'OCR il secondo.
                await ExtractAsync(archive, targetDir, IsEngineFile, ct);
                var tempZip = archive;

                // Verify extraction: llama.dll or libllama.so must exist
                var llamaFiles = Directory.GetFiles(targetDir, "llama.*");
                var libLlamaFiles = Directory.GetFiles(targetDir, "libllama.*");
                if (llamaFiles.Length == 0 && libLlamaFiles.Length == 0)
                {
                    _logger.LogError("Extraction failed: llama library not found in {Dir}", targetDir);
                    Directory.Delete(targetDir, true);
                    return false;
                }

                // Clean up zip
                if (File.Exists(tempZip))
                {
                    File.Delete(tempZip);
                }

                _logger.LogInformation("Backend {Variant} installed successfully at {Dir}", variant, targetDir);

                progress?.Report(new DownloadProgress
                {
                    ModelId = $"backend-{variant}",
                    BytesDownloaded = totalBytesRead,
                    TotalBytes = totalBytesRead,
                    Status = "Complete"
                });

                return true;
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Backend download cancelled");
                progress?.Report(new DownloadProgress
                {
                    ModelId = $"backend-{variant}",
                    BytesDownloaded = 0,
                    TotalBytes = 0,
                    Status = "Cancelled"
                });
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading backend {Variant}", variant);
                progress?.Report(new DownloadProgress
                {
                    ModelId = $"backend-{variant}",
                    BytesDownloaded = 0,
                    TotalBytes = 0,
                    Status = $"Error: {ex.Message}"
                });
                return false;
            }
        }

        public async Task<bool> DeleteBackendAsync()
        {
            try
            {
                var targetDir = Path.Combine(_backendsBaseDir, _version);
                if (Directory.Exists(targetDir))
                {
                    Directory.Delete(targetDir, true);
                    _logger.LogInformation("Backend deleted from {Dir}", targetDir);
                }
                return await Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting backend");
                return false;
            }
        }

        private string GetPlatformSuffix(string variant)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return variant switch
                {
                    "cpu" => "win-cpu-x64",
                    "cuda-12.4" => "win-cuda-12.4-x64",
                    "vulkan" => "win-vulkan-x64",
                    _ => null
                };
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return variant switch
                {
                    "cpu" => "ubuntu-x64",
                    "vulkan" => "ubuntu-vulkan-x64",
                    _ => null
                };
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                    ? "macos-arm64"
                    : "macos-x64";
            }
            return null;
        }
    }
}

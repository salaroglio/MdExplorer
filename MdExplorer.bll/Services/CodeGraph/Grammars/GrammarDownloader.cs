using MdExplorer.Features.Services;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Features.Services.CodeGraph.Grammars
{
    /// <summary>Le impostazioni del grafo del codice che arrivano da appsettings (sezione <c>MdExplorer</c>).</summary>
    public sealed class CodeGraphOptions
    {
        /// <summary>Base dell'API degli add-on: <c>{GrammarApiUrl}/grammar-&lt;id&gt;-&lt;rid&gt;/manifest</c>.</summary>
        public string GrammarApiUrl { get; set; } = "https://mdexplorer.net/api/addons";
    }

    /// <summary>Il manifest di un add-on, lo stesso contratto di <c>addon-downloader.js</c> in Electron.</summary>
    public sealed class AddonManifest
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string MinAppVersion { get; set; }
        public string DownloadUrl { get; set; }
        public long Size { get; set; }
        /// <summary>SHA-256 esadecimale dello zip.</summary>
        public string Checksum { get; set; }
        public string ReleaseNotes { get; set; }
    }

    public interface IGrammarDownloader
    {
        Task<InstalledGrammar> DownloadAsync(string grammarId, CancellationToken ct = default);
    }

    /// <summary>
    /// Scarica una grammatica da mdexplorer.net (D8): manifest → zip → checksum → estrazione del solo file atteso →
    /// store. Ogni passo che non torna è un'eccezione con il motivo: una grammatica a metà non si installa.
    /// </summary>
    public sealed class GrammarDownloader : IGrammarDownloader
    {
        private readonly IHttpClientFactory _http;
        private readonly GrammarStore _store;
        private readonly CodeGraphOptions _options;
        private readonly ILogger<GrammarDownloader> _logger;

        public GrammarDownloader(IHttpClientFactory http, GrammarStore store, CodeGraphOptions options, ILogger<GrammarDownloader> logger)
        {
            _http = http; _store = store; _options = options; _logger = logger;
        }

        public async Task<InstalledGrammar> DownloadAsync(string grammarId, CancellationToken ct = default)
        {
            var g = GrammarCatalog.Require(grammarId);
            var addonId = g.AddonId(_store.Rid);
            var baseUrl = (_options.GrammarApiUrl ?? string.Empty).TrimEnd('/');
            if (baseUrl.Length == 0) throw new InvalidOperationException(UiText.Say("MdExplorer:GrammarApiUrl non è configurato: non so da dove scaricare le grammatiche.", "MdExplorer:GrammarApiUrl is not configured: I do not know where to download the grammars from."));
            var manifestUrl = $"{baseUrl}/{addonId}/manifest";

            var client = _http.CreateClient("MdExplorerGrammars");
            client.Timeout = TimeSpan.FromMinutes(5);

            AddonManifest manifest;
            using (var res = await client.GetAsync(manifestUrl, ct))
            {
                if (!res.IsSuccessStatusCode)
                    throw new InvalidOperationException(UiText.Say($"Grammatica '{g.Id}' per {_store.Rid}: manifest non disponibile ({(int)res.StatusCode} {res.ReasonPhrase}) da {manifestUrl}", $"Grammar '{g.Id}' for {_store.Rid}: manifest not available ({(int)res.StatusCode} {res.ReasonPhrase}) from {manifestUrl}"));
                manifest = JsonSerializer.Deserialize<AddonManifest>(await res.Content.ReadAsStringAsync(ct), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                           ?? throw new InvalidOperationException($"Manifest vuoto da {manifestUrl}");
            }
            if (string.IsNullOrWhiteSpace(manifest.DownloadUrl)) throw new InvalidOperationException(UiText.Say($"Il manifest di '{addonId}' non ha downloadUrl.", $"The manifest of '{addonId}' has no downloadUrl."));
            if (string.IsNullOrWhiteSpace(manifest.Checksum)) throw new InvalidOperationException(UiText.Say($"Il manifest di '{addonId}' non ha checksum: senza non si installa.", $"The manifest of '{addonId}' has no checksum: without it nothing is installed."));
            var downloadUrl = Uri.IsWellFormedUriString(manifest.DownloadUrl, UriKind.Absolute) ? manifest.DownloadUrl : $"{baseUrl}/{manifest.DownloadUrl.TrimStart('/')}";

            var tmp = Path.Combine(Path.GetTempPath(), "mde-grammars", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmp);
            try
            {
                var zipPath = Path.Combine(tmp, addonId + ".zip");
                using (var res = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct))
                {
                    if (!res.IsSuccessStatusCode)
                        throw new InvalidOperationException(UiText.Say($"Download di '{addonId}' fallito ({(int)res.StatusCode}) da {downloadUrl}", $"Download of '{addonId}' failed ({(int)res.StatusCode}) from {downloadUrl}"));
                    await using var fs = File.Create(zipPath);
                    await res.Content.CopyToAsync(fs, ct);
                }

                var actual = Sha256File(zipPath);
                if (!string.Equals(actual, manifest.Checksum, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException(UiText.Say($"Checksum di '{addonId}' non corrisponde: atteso {manifest.Checksum}, trovato {actual}. Non installo.", $"Checksum of '{addonId}' does not match: expected {manifest.Checksum}, found {actual}. Not installing."));

                var expectedName = g.FileName(_store.Rid);
                string extracted = null;
                using (var zip = ZipFile.OpenRead(zipPath))
                {
                    var entry = zip.Entries.FirstOrDefault(e => string.Equals(Path.GetFileName(e.FullName), expectedName, StringComparison.OrdinalIgnoreCase));
                    if (entry == null)
                        throw new InvalidOperationException(UiText.Say($"Lo zip di '{addonId}' non contiene '{expectedName}' (contiene: {string.Join(", ", zip.Entries.Select(e => e.FullName))}).", $"The zip of '{addonId}' does not contain '{expectedName}' (it contains: {string.Join(", ", zip.Entries.Select(e => e.FullName))})."));
                    extracted = Path.Combine(tmp, expectedName);
                    entry.ExtractToFile(extracted, overwrite: true);
                }

                var installed = _store.Install(g, extracted, manifest.Version, manifest.Checksum);
                _logger?.LogInformation("[CodeGraph] grammatica '{Id}' {Version} installata in {Path}", g.Id, manifest.Version, installed.Path);
                return installed;
            }
            finally
            {
                try { Directory.Delete(tmp, true); } catch { /* temporanei */ }
            }
        }

        private static string Sha256File(string path)
        {
            using var sha = SHA256.Create();
            using var fs = File.OpenRead(path);
            return Convert.ToHexString(sha.ComputeHash(fs)).ToLowerInvariant();
        }
    }
}

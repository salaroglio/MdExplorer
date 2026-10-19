using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Templates;
using MdExplorer.Utilities;
using MdExplorer.Services.Git;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Templates
{
    /// <summary>Che cosa la vetrina mostra di un template (sprint 2026-10-19-Vetrina-Dei-Template.md).</summary>
    public sealed class TemplateShowcaseEntry
    {
        public string Url { get; init; }
        public string Id { get; init; }
        public string Kind { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public string Version { get; init; }
        public List<string> Languages { get; init; }
        /// <summary>Il template c'è nella lingua scelta: si può spuntare (VD5).</summary>
        public bool Speaks { get; init; }
        /// <summary>La lingua delle slide mostrate: la scelta, o l'inglese (VD7); null = slide di circostanza (VD8).</summary>
        public string SlidesLanguage { get; init; }
        /// <summary>La pagina da mettere nell'iframe della vetrina.</summary>
        public string Page { get; init; }
    }

    public interface ITemplateShowcase
    {
        /// <summary>
        /// Il template all'indirizzo <paramref name="url"/>, guardato in <paramref name="language"/>: il manifesto del ramo di
        /// quella lingua (o della prima che ha) e quale presentazione mostrare. Rete o repository irraggiungibile = eccezione
        /// che dice che cosa non si è raggiunto (VD3).
        /// </summary>
        Task<TemplateShowcaseEntry> DescribeAsync(string url, string language, CancellationToken ct = default);

        /// <summary>Un file di una copia della cache (<paramref name="key"/> come in <see cref="TemplateShowcaseEntry.Page"/>), dentro <c>.mde-template/</c>; null se non c'è.</summary>
        string FileOf(string key, string path);

        /// <summary>La slide di circostanza (VD8) del template copiato in <paramref name="key"/>, in markdown, senza rete.</summary>
        string StandInDeck(string key, string language);

        /// <summary>La presentazione del progetto vuoto (VD4), che viaggia dentro MDE: la lingua, o l'inglese.</summary>
        string EmptyProjectDeck(string language);
    }

    /// <summary>
    /// Legge dal repository di un template ciò che serve alla vetrina, prima di crearci un progetto. Una copia leggera per
    /// commit (<c>clone --depth 1 --filter=blob:none --sparse</c>, solo <c>.mde-template/</c>) nella cartella dei dati di
    /// MDE: git nativo, quindi un repository privato si legge con le credenziali di sempre. La cache serve solo a non
    /// riscaricare un commit già visto: ogni richiesta chiede a git il commit di oggi, e senza rete lo dice invece di
    /// mostrare slide vecchie (VD3).
    /// </summary>
    public sealed class TemplateShowcase : ITemplateShowcase
    {
        public const string Route = "/api/templateshowcase";
        private const string CacheFolder = "template-cache";
        private const string DoneSuffix = ".ok";
        private static readonly Regex LanguageCode = new(@"^[a-z]{2}$", RegexOptions.Compiled);
        private static readonly Regex Key = new(@"^[0-9a-f]{16}/[0-9a-f]{40}$", RegexOptions.Compiled);
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

        private readonly INativeGitRunner _git;
        private readonly ILogger<TemplateShowcase> _logger;

        public TemplateShowcase(INativeGitRunner git, ILogger<TemplateShowcase> logger)
        {
            _git = git;
            _logger = logger;
        }

        /// <summary>Letta a ogni uso: la cartella dei dati può cambiare (i test la isolano per fabbrica).</summary>
        private static string CacheRoot => Path.Combine(CrossPlatformPath.GetAppDataPath(), CacheFolder);

        public async Task<TemplateShowcaseEntry> DescribeAsync(string url, string language, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw Problem("Manca l'indirizzo del template.", "The template address is missing.");
            CheckLanguage(language);
            url = url.Trim();

            var (defaultBranch, defaultTip) = await DefaultBranchAsync(url, ct);
            var key = await CheckoutAsync(url, defaultBranch, defaultTip, ct);
            var manifest = ManifestIn(key, defaultBranch);

            // Il ramo della lingua scelta, se il template la parla; altrimenti quello della sua prima lingua.
            var speaks = manifest.Languages.ContainsKey(language);
            var branch = speaks ? manifest.Languages[language].Structure : manifest.Languages.First().Value.Structure;
            if (branch != defaultBranch)
            {
                key = await CheckoutAsync(url, branch, await TipAsync(url, branch, ct), ct);
                var other = ManifestIn(key, branch);
                if (other.Id != manifest.Id)
                    throw Problem($"Il ramo '{branch}' di {url} porta il template '{other.Id}', il ramo '{defaultBranch}' '{manifest.Id}': un repository porta un template solo.",
                                  $"The branch '{branch}' of {url} carries the template '{other.Id}', the branch '{defaultBranch}' '{manifest.Id}': a repository carries one template only.");
                manifest = other;
            }

            var slides = manifest.SlidesFor(language);
            if (slides != null && FileOf(key, slides.Value.File) == null)
                throw Problem($"Il manifesto di «{manifest.Title}» nomina le slide {slides.Value.File}, che nel ramo '{branch}' non ci sono.",
                              $"The manifest of «{manifest.Title}» names the slides {slides.Value.File}, which are not in the branch '{branch}'.");

            var (title, description) = Texts(url, language, manifest);
            return new TemplateShowcaseEntry
            {
                Url = url,
                Id = manifest.Id,
                Kind = manifest.Kind,
                Title = title,
                Description = description,
                Version = manifest.Version,
                Languages = manifest.Languages.Keys.ToList(),
                Speaks = speaks,
                SlidesLanguage = slides?.Language,
                Page = slides != null
                    ? $"{Route}/deck/{key}/{slides.Value.File}"
                    : $"{Route}/standin/{key}?language={language}",
            };
        }

        public string FileOf(string key, string path)
        {
            if (key == null || !Key.IsMatch(key) || string.IsNullOrWhiteSpace(path)) return null;
            var root = Path.GetFullPath(Path.Combine(CacheRoot, key.Replace('/', Path.DirectorySeparatorChar)));
            if (!File.Exists(root + DoneSuffix)) return null;
            var zone = Path.Combine(root, TemplateManifest.Folder) + Path.DirectorySeparatorChar;
            var full = Path.GetFullPath(Path.Combine(root, path.Replace('/', Path.DirectorySeparatorChar)));
            return full.StartsWith(zone, StringComparison.Ordinal) && File.Exists(full) ? full : null;
        }

        public string StandInDeck(string key, string language)
        {
            CheckLanguage(language);
            if (key == null || !Key.IsMatch(key) || !File.Exists(Path.Combine(CacheRoot, key) + DoneSuffix))
                throw Problem("Questo template non è nella cache della vetrina: riaprila.", "This template is not in the showcase cache: open it again.");
            // Il segno di copia finita dice l'indirizzo (per i testi dell'elenco) e il ramo (per dire dove sta un manifesto guasto).
            var done = File.ReadAllLines(Path.Combine(CacheRoot, key) + DoneSuffix);
            var manifest = ManifestIn(key, done.ElementAtOrDefault(1) ?? key);
            var (title, description) = Texts(done.ElementAtOrDefault(0), language, manifest);
            var it = language == "it";
            var languages = string.Join(", ", manifest.Languages.Keys.Select(l => LanguageName(l, it)));
            var only = manifest.Languages.ContainsKey(language)
                ? (it ? $"Lingue: {languages}" : $"Languages: {languages}")
                : (it ? $"Solo in: {languages}" : $"Only in: {languages}");
            var kind = manifest.IsMethod ? (it ? "Metodo" : "Method") : (it ? "Base di progetto" : "Project base");
            var missing = it ? "Questo template non ha ancora una presentazione." : "This template has no presentation yet.";
            return $"---\ndocument_type: slides\n---\n\n# {title}\n\n{description}\n\n{kind} · {manifest.Version} · {only}\n\n*{missing}*\n";
        }

        public string EmptyProjectDeck(string language)
        {
            CheckLanguage(language);
            var assembly = typeof(TemplateShowcase).Assembly;
            foreach (var l in new[] { language, TemplateManifest.SlidesFallbackLanguage }.Distinct())
            {
                using var stream = assembly.GetManifestResourceStream(EmptyProjectResource(l));
                if (stream == null) continue;
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            throw new InvalidOperationException($"MdExplorer non contiene {EmptyProjectResource(TemplateManifest.SlidesFallbackLanguage)}: la build è incompleta.");
        }

        public static string EmptyProjectResource(string language) => $"MdExplorer.Service.project-templates.empty.{language}.md";

        private static void CheckLanguage(string language)
        {
            if (!LanguageCode.IsMatch(language ?? string.Empty))
                throw Problem($"'{language}' non è un codice di lingua (it, en…).", $"'{language}' is not a language code (it, en…).");
        }

        /// <summary>Titolo e descrizione: quelli dell'elenco nella lingua (o in inglese), sennò quelli del manifesto.</summary>
        private static (string Title, string Description) Texts(string url, string language, TemplateManifest manifest)
        {
            var entry = TemplateCatalogResource.Entries().FirstOrDefault(e => e.Url == url);
            string Pick(Dictionary<string, string> texts, string own)
                => texts?.GetValueOrDefault(language) ?? texts?.GetValueOrDefault(TemplateManifest.SlidesFallbackLanguage) ?? own;
            return (Pick(entry?.Title, manifest.Title), Pick(entry?.Description, manifest.Description));
        }

        private static string LanguageName(string code, bool it) => code switch
        {
            "it" => it ? "italiano" : "Italian",
            "en" => it ? "inglese" : "English",
            _ => code,
        };

        private TemplateManifest ManifestIn(string key, string where)
        {
            var zone = Path.Combine(CacheRoot, key.Replace('/', Path.DirectorySeparatorChar), TemplateManifest.Folder);
            var ids = Directory.Exists(zone)
                ? Directory.GetDirectories(zone).Where(d => File.Exists(Path.Combine(d, TemplateManifest.FileName))).Select(Path.GetFileName).ToList()
                : new List<string>();
            if (ids.Count != 1)
                throw Problem(ids.Count == 0
                        ? $"Il ramo '{where}' non ha il manifesto {TemplateManifest.Folder}/<id>/{TemplateManifest.FileName}: non è un template di MdExplorer."
                        : $"Il ramo '{where}' ha più manifesti ({string.Join(", ", ids)}): un repository porta un template solo.",
                    ids.Count == 0
                        ? $"The branch '{where}' has no manifest {TemplateManifest.Folder}/<id>/{TemplateManifest.FileName}: it is not an MdExplorer template."
                        : $"The branch '{where}' has several manifests ({string.Join(", ", ids)}): a repository carries one template only.");
            return TemplateManifestReader.ReadOrThrow(File.ReadAllText(Path.Combine(zone, ids[0], TemplateManifest.FileName)), ids[0], $"{TemplateManifest.PathOf(ids[0])} ({where})");
        }

        private async Task<(string Branch, string Tip)> DefaultBranchAsync(string url, CancellationToken ct)
        {
            var r = await _git.RunAsync(CacheRootReady(), new[] { "ls-remote", "--symref", url, "HEAD" }, ct);
            if (!r.Ok) throw Unreachable(url, r);
            string branch = null, tip = null;
            foreach (var line in (r.Stdout ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = line.Split('\t');
                if (parts.Length != 2 || parts[1].Trim() != "HEAD") continue;
                if (parts[0].StartsWith("ref: refs/heads/", StringComparison.Ordinal)) branch = parts[0]["ref: refs/heads/".Length..].Trim();
                else tip = parts[0].Trim();
            }
            if (branch == null || tip == null)
                throw Problem($"Il repository {url} non dice qual è il suo ramo principale: è vuoto?", $"The repository {url} does not tell its main branch: is it empty?");
            return (branch, tip);
        }

        private async Task<string> TipAsync(string url, string branch, CancellationToken ct)
        {
            var r = await _git.RunAsync(CacheRootReady(), new[] { "ls-remote", url, "refs/heads/" + branch }, ct);
            if (!r.Ok) throw Unreachable(url, r);
            var tip = (r.Stdout ?? "").Split('\t').FirstOrDefault()?.Trim();
            if (string.IsNullOrEmpty(tip))
                throw Problem($"Il manifesto di {url} dichiara il ramo '{branch}', che nel repository non c'è.",
                              $"The manifest of {url} declares the branch '{branch}', which is not in the repository.");
            return tip;
        }

        /// <summary>
        /// La copia del commit <paramref name="tip"/>, fatta una volta: la chiave è «hash di indirizzo e ramo/commit». Il ramo
        /// entra nella chiave perché la pulizia dei commit vecchi non tolga la copia di un'altra lingua dello stesso template.
        /// </summary>
        private async Task<string> CheckoutAsync(string url, string branch, string tip, CancellationToken ct)
        {
            var source = SourceHash(url, branch);
            var key = $"{source}/{tip}";
            var target = Path.Combine(CacheRoot, source, tip);
            var gate = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync(ct);
            try
            {
                if (File.Exists(target + DoneSuffix)) return key;
                DeleteTree(target);
                Directory.CreateDirectory(Path.GetDirectoryName(target));
                var clone = await _git.RunAsync(CacheRootReady(), new[]
                    { "clone", "-q", "--depth", "1", "--filter=blob:none", "--sparse", "--branch", branch, url, target }, ct);
                if (!clone.Ok) { DeleteTree(target); throw Unreachable(url, clone); }
                var sparse = await _git.RunAsync(target, new[] { "sparse-checkout", "set", TemplateManifest.Folder }, ct);
                if (!sparse.Ok) { DeleteTree(target); throw Unreachable(url, sparse); }
                File.WriteAllText(target + DoneSuffix, url + "\n" + branch + "\n");
                PruneOlder(Path.GetDirectoryName(target), tip);
                return key;
            }
            finally { gate.Release(); }
        }

        /// <summary>Le copie di commit vecchi dello stesso indirizzo non servono più: si tolgono, senza fermare la vetrina se non si può.</summary>
        private void PruneOlder(string folder, string keep)
        {
            foreach (var done in Directory.GetFiles(folder, "*" + DoneSuffix))
            {
                var tip = Path.GetFileNameWithoutExtension(done);
                if (tip == keep || Locks.TryGetValue($"{Path.GetFileName(folder)}/{tip}", out var g) && g.CurrentCount == 0) continue;
                try { File.Delete(done); } catch (IOException) { continue; }
                DeleteTree(Path.Combine(folder, tip));
            }
        }

        private static string SourceHash(string url, string branch)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(url + "\n" + branch))).ToLowerInvariant()[..16];

        private static string CacheRootReady()
        {
            Directory.CreateDirectory(CacheRoot);
            return CacheRoot;
        }

        private static InvalidOperationException Unreachable(string url, GitResult r)
            => Problem($"Non raggiungo il template {url}: {r.Describe()}", $"I cannot reach the template {url}: {r.Describe()}");

        private void DeleteTree(string target)
        {
            try
            {
                if (!Directory.Exists(target)) return;
                // Su Windows gli oggetti di git sono di sola lettura: Directory.Delete li rifiuta.
                foreach (var f in Directory.EnumerateFiles(target, "*", SearchOption.AllDirectories)) File.SetAttributes(f, FileAttributes.Normal);
                Directory.Delete(target, recursive: true);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                _logger.LogWarning(ex, "[Vetrina] non riesco a togliere la copia '{Target}'", target);
            }
        }

        private static InvalidOperationException Problem(string it, string en) => new(UiText.Say(it, en));
    }
}

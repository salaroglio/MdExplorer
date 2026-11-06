using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using MdExplorer.Features.Diagnostics;

namespace MdExplorer.Services.Diagnostics
{
    /// <summary>Un file del pacchetto, già oscurato.</summary>
    public sealed class DiagnosticsFile
    {
        /// <summary>Il nome dentro lo zip.</summary>
        public string Name { get; init; }
        /// <summary>Da dove viene, oscurato anche lui (contiene la cartella dell'utente).</summary>
        public string Source { get; init; }
        /// <summary>Byte presi dal file originale (dalla coda, se è grande).</summary>
        public long OriginalBytes { get; init; }
        /// <summary>true se del file si è presa solo la parte finale.</summary>
        public bool Truncated { get; init; }
        public string Text { get; init; }
        public IReadOnlyDictionary<string, int> Redactions { get; init; }
    }

    /// <summary>
    /// Il pacchetto di «Invia diagnostica» (sprint 2026-11-06-Invia-Diagnostica, D1): i log dell'ultimo giorno, oscurati,
    /// più <c>info.json</c>. Lo stesso pacchetto si vede nell'anteprima, si salva come zip e, dalla D3, si spedisce: chi
    /// invia vede esattamente ciò che parte.
    /// <para>
    /// Non entrano: <c>chat-interactions-*.log</c> (le conversazioni), i DB, i documenti, le configurazioni con chiavi.
    /// I log si leggono con <c>FileShare.ReadWrite</c>: il servizio e Electron li stanno scrivendo.
    /// </para>
    /// </summary>
    public sealed class DiagnosticsPackageService
    {
        /// <summary>Del log del servizio si prende al massimo la coda di 8 MB: un giorno normale ne fa 2-3.</summary>
        private const long ServiceLogTail = 8L * 1024 * 1024;
        /// <summary>I log di Electron non ruotano per giorno: se ne prende la coda.</summary>
        private const long ElectronLogTail = 2L * 1024 * 1024;

        private readonly string _logDirectory;
        private readonly string _electronUserData;
        private readonly Func<RedactionContext> _context;

        public DiagnosticsPackageService(string logDirectory, string electronUserData, Func<RedactionContext> context)
        {
            _logDirectory = logDirectory;
            _electronUserData = electronUserData;
            _context = context;
        }

        /// <summary>La cartella dati di Electron: <c>%APPDATA%\mdexplorer</c>, <c>~/.config/mdexplorer</c>.</summary>
        public static string DefaultElectronUserData()
            => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mdexplorer");

        /// <summary>Chi è la persona: nome utente, dominio, cartella, nome e email di git (globali).</summary>
        public static RedactionContext CurrentContext()
        {
            var names = new List<string>();
            try
            {
                var git = LibGit2Sharp.Configuration.BuildFrom(null);
                var name = git.Get<string>("user.name")?.Value;
                var email = git.Get<string>("user.email")?.Value;
                if (!string.IsNullOrWhiteSpace(name)) names.Add(name);
                if (!string.IsNullOrWhiteSpace(email) && email.Contains('@')) names.Add(email.Substring(0, email.IndexOf('@')));
            }
            catch { /* senza git configurato restano utente e cartella */ }
            return new RedactionContext
            {
                UserName = Environment.UserName,
                Domain = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.UserDomainName : null,
                UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                OtherNames = names,
            };
        }

        public IReadOnlyList<DiagnosticsFile> Collect()
        {
            var context = _context();
            var files = new List<DiagnosticsFile>();

            void Add(string name, string path, long tail)
            {
                if (path == null || !File.Exists(path)) return;
                var (text, bytes, truncated) = ReadTail(path, tail);
                var r = DiagnosticsRedactor.Redact(text, context);
                files.Add(new DiagnosticsFile
                {
                    Name = name, Source = DiagnosticsRedactor.Redact(path, context).Text,
                    OriginalBytes = bytes, Truncated = truncated, Text = r.Text, Redactions = r.Counts,
                });
            }

            if (_logDirectory != null && Directory.Exists(_logDirectory))
            {
                // Solo l'ultimo giorno (decisione dell'utente, 06/11): il file più recente di ogni tipo.
                var service = Newest(_logDirectory, "mdexplorer-2*.log");
                Add("servizio/" + Path.GetFileName(service), service, ServiceLogTail);
                var startup = Newest(_logDirectory, "mdexplorer-startup-*.log");
                Add("servizio/" + Path.GetFileName(startup), startup, ServiceLogTail);
                var crash = Newest(_logDirectory, "mdexplorer-crash-*.log");
                if (crash != null && File.GetLastWriteTime(crash) > DateTime.Now.AddDays(-1))
                    Add("servizio/" + Path.GetFileName(crash), crash, ServiceLogTail);
            }
            if (_electronUserData != null)
            {
                Add("electron/electron_main.log", Path.Combine(_electronUserData, "electron_main.log"), ElectronLogTail);
                Add("electron/main.log", Path.Combine(_electronUserData, "logs", "main.log"), ElectronLogTail);
            }

            var info = DiagnosticsRedactor.Redact(InfoJson(), context);
            files.Insert(0, new DiagnosticsFile { Name = "info.json", Source = "", OriginalBytes = info.Text.Length, Text = info.Text, Redactions = info.Counts });
            return files;
        }

        public byte[] Zip(IReadOnlyList<DiagnosticsFile> files)
        {
            using var ms = new MemoryStream();
            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var f in files)
                {
                    var entry = zip.CreateEntry(f.Name, CompressionLevel.Optimal);
                    using var w = new StreamWriter(entry.Open(), new UTF8Encoding(false));
                    w.Write(f.Text);
                }
            }
            return ms.ToArray();
        }

        private string InfoJson()
        {
            var entry = Assembly.GetEntryAssembly();
            var info = new Dictionary<string, object>
            {
                ["creatoIl"] = DateTime.UtcNow.ToString("o"),
                ["servizio"] = entry?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                               ?? entry?.GetName().Version?.ToString(),
                ["dataDelServizio"] = entry?.Location is string loc && File.Exists(loc) ? File.GetLastWriteTimeUtc(loc).ToString("o") : null,
                ["sistema"] = RuntimeInformation.OSDescription,
                ["architettura"] = RuntimeInformation.OSArchitecture.ToString(),
                ["dotnet"] = RuntimeInformation.FrameworkDescription,
                ["sondeDeiTempi"] = Environment.GetEnvironmentVariable("MDE_PERF_TRACE") == "1",
                ["cartellaLog"] = _logDirectory,
            };
            return JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true });
        }

        private static string Newest(string dir, string pattern)
            => new DirectoryInfo(dir).GetFiles(pattern).OrderByDescending(f => f.LastWriteTimeUtc).FirstOrDefault()?.FullName;

        private static (string Text, long Bytes, bool Truncated) ReadTail(string path, long tail)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            var truncated = fs.Length > tail;
            if (truncated) fs.Seek(-tail, SeekOrigin.End);
            using var reader = new StreamReader(fs, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            var text = reader.ReadToEnd();
            if (truncated)
            {
                // si riparte da una riga intera
                var nl = text.IndexOf('\n');
                if (nl >= 0 && nl < text.Length - 1) text = text.Substring(nl + 1);
            }
            return (text, truncated ? tail : fs.Length, truncated);
        }
    }
}

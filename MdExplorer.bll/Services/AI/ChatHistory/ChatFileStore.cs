using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.AI.ChatHistory
{
    /// <summary>Che cosa c'è nel file di una chat, e che cosa non si è potuto leggere.</summary>
    public sealed class ChatFileContent
    {
        /// <summary>Falso se il file non c'è: la riga della cronologia lo deve dire, non mostrare una chat vuota.</summary>
        public bool Exists { get; init; }
        public IReadOnlyList<ChatRecordEvent> Events { get; init; } = Array.Empty<ChatRecordEvent>();
        /// <summary>Le righe che non si leggono, col numero di riga e il motivo. Vuota se il file è sano.</summary>
        public IReadOnlyList<string> Problems { get; init; } = Array.Empty<string>();
    }

    /// <summary>
    /// Il contenuto delle chat di MarkAgent, un file JSONL per chat fra i dati dell'utente (sprint
    /// 2026-11-24-Chat-Memorizzate, U7). Si scrive solo in coda, una riga intera per evento.
    /// </summary>
    public interface IChatFileStore
    {
        /// <summary>Dove va la chat <paramref name="threadId"/> del progetto, relativo alla cartella delle chat.</summary>
        string RelativePathFor(string projectPath, Guid threadId);
        void Append(string relativePath, IReadOnlyList<ChatRecordEvent> events);
        ChatFileContent Read(string relativePath);
        /// <summary>Vero se il file c'era ed è stato cancellato.</summary>
        bool Delete(string relativePath);
    }

    public sealed class ChatFileStore : IChatFileStore
    {
        private static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            // Una riga per evento: niente a capo dentro il JSON, quelli del contenuto diventano \n.
            WriteIndented = false,
        };

        private static readonly UTF8Encoding Utf8NoBom = new(false);

        private readonly string _root;
        // Un file, un solo scrittore alla volta: due righe scritte insieme non si mescolano.
        private readonly ConcurrentDictionary<string, object> _locks = new(StringComparer.Ordinal);

        /// <param name="root">La cartella delle chat, fra i dati dell'utente (non il progetto: non va in git).</param>
        public ChatFileStore(string root)
        {
            if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("La cartella delle chat è obbligatoria.", nameof(root));
            _root = Path.GetFullPath(root);
        }

        public string RelativePathFor(string projectPath, Guid threadId)
        {
            if (string.IsNullOrWhiteSpace(projectPath)) throw new ArgumentException("Il progetto della chat è obbligatorio.", nameof(projectPath));
            if (threadId == Guid.Empty) throw new ArgumentException("La chat non ha ancora un identificativo.", nameof(threadId));
            // UTF-8 e non ASCII: con ASCII due progetti con lettere accentate diverse avrebbero la stessa cartella.
            var folder = Helper.HGetHashString(Path.GetFullPath(projectPath), Encoding.UTF8);
            return folder + "/" + threadId.ToString("D") + ".jsonl";
        }

        public void Append(string relativePath, IReadOnlyList<ChatRecordEvent> events)
        {
            if (events == null || events.Count == 0) return;
            var full = FullPath(relativePath);

            var text = new StringBuilder();
            foreach (var e in events)
            {
                if (e == null) throw new ArgumentException("Un evento della chat è null.", nameof(events));
                text.Append(JsonSerializer.Serialize(e, Json)).Append('\n');
            }
            var bytes = Utf8NoBom.GetBytes(text.ToString());

            lock (_locks.GetOrAdd(full, _ => new object()))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(full));
                // Una sola scrittura per lotto: chi legge mentre scriviamo vede al massimo un'ultima riga a metà, e lo dice.
                using var stream = new FileStream(full, FileMode.Append, FileAccess.Write, FileShare.Read);
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush(flushToDisk: true);
            }
        }

        public ChatFileContent Read(string relativePath)
        {
            var full = FullPath(relativePath);
            if (!File.Exists(full)) return new ChatFileContent { Exists = false };

            string all;
            using (var stream = new FileStream(full, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(stream, Utf8NoBom))
                all = reader.ReadToEnd();

            var events = new List<ChatRecordEvent>();
            var problems = new List<string>();
            var lines = all.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var isLast = i == lines.Length - 1;
                // Il file finisce con un a capo: l'ultimo pezzo vuoto non è una riga.
                if (isLast && line.Length == 0) break;
                var number = i + 1;
                if (line.Length == 0)
                {
                    problems.Add($"riga {number}: vuota");
                    continue;
                }
                try
                {
                    var e = JsonSerializer.Deserialize<ChatRecordEvent>(line, Json);
                    if (e == null || string.IsNullOrEmpty(e.Kind)) problems.Add($"riga {number}: evento senza tipo");
                    else events.Add(e);
                }
                catch (JsonException ex)
                {
                    // L'ultima riga senza a capo è una scrittura interrotta (un crash a metà): si dice quale.
                    problems.Add(isLast
                        ? $"riga {number}: incompleta, scrittura interrotta ({ex.Message})"
                        : $"riga {number}: non è JSON valido ({ex.Message})");
                }
            }
            return new ChatFileContent { Exists = true, Events = events, Problems = problems };
        }

        public bool Delete(string relativePath)
        {
            var full = FullPath(relativePath);
            lock (_locks.GetOrAdd(full, _ => new object()))
            {
                if (!File.Exists(full)) return false;
                File.Delete(full);
                return true;
            }
        }

        private string FullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentException("Il percorso della chat è obbligatorio.", nameof(relativePath));
            if (Path.IsPathRooted(relativePath))
                throw new ArgumentException($"Il percorso della chat deve essere relativo alla cartella delle chat: «{relativePath}».", nameof(relativePath));
            var full = Path.GetFullPath(Path.Combine(_root, relativePath));
            if (!PathContainment.IsInside(_root, full) || string.Equals(full, _root, PathContainment.Comparison))
                throw new ArgumentException($"Il percorso della chat esce dalla cartella delle chat: «{relativePath}».", nameof(relativePath));
            return full;
        }
    }
}

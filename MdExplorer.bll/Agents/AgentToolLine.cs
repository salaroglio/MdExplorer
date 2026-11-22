using System.Collections.Generic;
using System.Text.Json;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Uno strumento usato da un agente, in una riga per il diario del turno: il nome e ciò che dice SU CHE COSA lavora
    /// (il comando, il file, la cartella, la ricerca). Mai il contenuto che scrive: di un argomento lungo — il testo di
    /// un file — resta solo l'inizio. Vale per i tre motori.
    /// </summary>
    public static class AgentToolLine
    {
        /// <summary>Gli argomenti che dicono su che cosa lavora lo strumento, nell'ordine in cui si preferiscono.</summary>
        private static readonly string[] Subjects = { "command", "file_path", "filePath", "path", "notebook_path", "pattern", "query", "url" };

        public const int MaxArgumentChars = 80;

        public static string Describe(string toolName, JsonElement arguments)
        {
            var name = string.IsNullOrWhiteSpace(toolName) ? "tool" : toolName;
            if (arguments.ValueKind != JsonValueKind.Object) return name;
            foreach (var key in Subjects)
                if (arguments.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()))
                    return name + " · " + v.GetString();
            var parts = new List<string>();
            foreach (var p in arguments.EnumerateObject())
            {
                var value = p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() : p.Value.GetRawText();
                if (value.Length > MaxArgumentChars) value = value.Substring(0, MaxArgumentChars) + "…";
                parts.Add(p.Name + "=" + value);
            }
            return parts.Count == 0 ? name : name + " · " + string.Join(", ", parts);
        }

        /// <summary>
        /// Da «Nome {json degli argomenti}» (così Claude Code annuncia uno strumento) alla riga per il diario. Un testo
        /// che non ha quella forma («↳ ok», un avviso) resta com'è.
        /// </summary>
        public static string FromNameAndJson(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            var space = text.IndexOf(' ');
            if (space <= 0 || space + 1 >= text.Length || text[space + 1] != '{') return text;
            try
            {
                using var doc = JsonDocument.Parse(text.Substring(space + 1));
                return Describe(text.Substring(0, space), doc.RootElement);
            }
            catch (JsonException)
            {
                // Argomenti tagliati a metà (la sessione di Claude Code li accorcia a 160 caratteri): si cerca lo stesso
                // l'argomento che dice su che cosa lavora. Se non c'è, la riga com'è (il diario la taglia se è lunga).
                return FromCutJson(text.Substring(0, space), text.Substring(space + 1)) ?? text;
            }
        }

        private static string FromCutJson(string toolName, string cutJson)
        {
            foreach (var key in Subjects)
            {
                var match = System.Text.RegularExpressions.Regex.Match(cutJson,
                    "\"" + System.Text.RegularExpressions.Regex.Escape(key) + "\"\\s*:\\s*\"((?:[^\"\\\\]|\\\\.)*)(\")?");
                if (!match.Success) continue;
                var raw = match.Groups[1].Value;
                var whole = match.Groups[2].Success;
                string value;
                try { value = JsonSerializer.Deserialize<string>("\"" + raw + "\""); }
                catch (JsonException) { value = raw; }   // tagliato in mezzo a una sequenza: il testo grezzo
                if (string.IsNullOrWhiteSpace(value)) continue;
                return toolName + " · " + value + (whole ? "" : "…");
            }
            return null;
        }
    }
}

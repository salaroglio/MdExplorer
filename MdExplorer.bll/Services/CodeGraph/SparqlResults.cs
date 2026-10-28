using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Le righe di un risultato SPARQL JSON (<c>application/sparql-results+json</c>): una mappa variabile → valore per riga.</summary>
    public static class SparqlResults
    {
        public static List<Dictionary<string, string>> Parse(string json)
        {
            var rows = new List<Dictionary<string, string>>();
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("results", out var results) || !results.TryGetProperty("bindings", out var bindings))
                return rows;
            foreach (var binding in bindings.EnumerateArray())
            {
                var row = new Dictionary<string, string>(System.StringComparer.Ordinal);
                foreach (var prop in binding.EnumerateObject())
                {
                    if (prop.Value.TryGetProperty("value", out var v)) row[prop.Name] = v.GetString();
                }
                rows.Add(row);
            }
            return rows;
        }

        public static string Str(Dictionary<string, string> row, string name) => row.TryGetValue(name, out var v) ? v : null;

        public static int? Int(Dictionary<string, string> row, string name)
            => row.TryGetValue(name, out var v) && int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) ? n : null;

        public static int Count(Dictionary<string, string> row, string name) => Int(row, name) ?? 0;

        public static bool Bool(Dictionary<string, string> row, string name)
            => row.TryGetValue(name, out var v) && (v == "true" || v == "1");
    }
}

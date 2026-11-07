using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using YamlDotNet.RepresentationModel;

namespace MdExplorer.Features.Configuration
{
    /// <summary>
    /// Il testo di <c>.development.yml</c> che i lettori possono usare: senza le voci ritirate e senza i gruppi spenti dal controllo
    /// dello schema (F2 dello sprint docs-internal/Sprints/2026-11-07-Schema-Development-Yml.md). Un gruppo spento qui sparisce, come
    /// se non fosse configurato: chi legge vede la funzione spenta. Chi deve distinguere «assente» da «spento» (prima di scrivere o
    /// di creare qualcosa) guarda <see cref="DevelopmentSchemaCheck.IsOff"/>.
    /// <para>
    /// Le voci non compilate (D6: <c>voce:</c> senza valore) si tolgono anche loro, così il modello C# tiene i suoi predefiniti
    /// (una lista vuota resta una lista, un vero/falso resta falso) invece di ricevere un null.
    /// </para>
    /// </summary>
    public static class DevelopmentConfigText
    {
        public const string FileName = ".development.yml";

        public sealed record Usable(string Text, DevelopmentSchemaCheck Check);

        /// <summary>Il file di un progetto; un file che non c'è dà un testo vuoto e nessun gruppo spento.</summary>
        public static Usable ReadFile(string path)
            => File.Exists(path) ? Prune(File.ReadAllText(path)) : new Usable(string.Empty, DevelopmentSchemaCheck.Run(string.Empty));

        public static Usable Prune(string text) => Prune(text, DevelopmentSchemaSnapshots.All);

        public static Usable Prune(string text, IReadOnlyList<SchemaSnapshot> snapshots)
        {
            string cleaned;
            try
            {
                cleaned = RetiredConfigKeys.Remove(text ?? string.Empty, out _);
            }
            catch (InvalidOperationException ex)
            {
                // La pulizia trova una voce ritirata e quella nuova insieme: il file va sistemato a mano, e fino ad allora è tutto spento.
                return new Usable(string.Empty, DevelopmentSchemaCheck.Broken(text, 0, ex.Message));
            }

            var check = DevelopmentSchemaCheck.Run(cleaned, snapshots);
            if (check.Unreadable != null) return new Usable(string.Empty, check);

            var stream = new YamlStream();
            stream.Load(new StringReader(cleaned));
            if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode root) return new Usable(string.Empty, check);

            if (check.SwitchedOff.Count == 0 && !HasEmpty(root)) return new Usable(cleaned, check);

            Remove(root, check.SwitchedOff.Select(o => o.Path));
            DropEmpty(root);
            return new Usable(Save(root), check);
        }

        private static readonly Regex EmptyValue = new(@"(?m): ~$", RegexOptions.Compiled);

        /// <summary>
        /// Un albero YAML in testo come lo scrive MDE (fine riga LF): una voce vuota resta «voce:» (D6). YamlDotNet la scriverebbe
        /// '' (testo vuoto), che poi non si rilegge come gruppo né come lista: diventa il segnaposto ~ e poi niente. Un testo che vale
        /// davvero «~» è fra apici, quindi non si confonde. Lo usano lo scrittore del servizio e l'unione voce per voce del pull.
        /// </summary>
        public static string Save(YamlMappingNode root)
        {
            if (root == null || root.Children.Count == 0) return string.Empty;
            MarkEmpty(root);
            using var writer = new StringWriter();
            new YamlStream(new YamlDocument(root)).Save(writer, assignAnchors: false);
            var text = writer.ToString().Replace("\r\n", "\n");
            if (text.EndsWith("...\n", StringComparison.Ordinal)) text = text.Substring(0, text.Length - 4);
            return EmptyValue.Replace(text, ":");
        }

        private static void MarkEmpty(YamlNode node)
        {
            switch (node)
            {
                case YamlMappingNode map:
                    foreach (var key in map.Children.Where(c => IsEmpty(c.Value)).Select(c => c.Key).ToList())
                        map.Children[key] = new YamlScalarNode("~");
                    foreach (var child in map.Children.Values) MarkEmpty(child);
                    break;
                case YamlSequenceNode seq:
                    foreach (var child in seq.Children) MarkEmpty(child);
                    break;
            }
        }

        private static readonly Regex Segment = new(@"^(?<name>[^\[\]]+)(?:\[(?<i>\d+)\])?$", RegexOptions.Compiled);

        /// <summary>Toglie le unità spente: un gruppo in cima o un elemento di lista (gli indici più alti per primi).</summary>
        private static void Remove(YamlMappingNode root, IEnumerable<string> paths)
        {
            var elements = new List<(YamlSequenceNode List, int Index)>();
            foreach (var path in paths)
            {
                var parts = path.Split('.');
                YamlNode node = root;
                YamlMappingNode parent = null;
                string lastName = null;
                int? lastIndex = null;
                YamlSequenceNode lastList = null;
                foreach (var part in parts)
                {
                    var m = Segment.Match(part);
                    if (!m.Success || node is not YamlMappingNode map) { node = null; break; }
                    parent = map; lastName = m.Groups["name"].Value;
                    if (!map.Children.TryGetValue(new YamlScalarNode(lastName), out node)) { node = null; break; }
                    lastIndex = null; lastList = null;
                    if (m.Groups["i"].Success)
                    {
                        if (node is not YamlSequenceNode seq) { node = null; break; }
                        var i = int.Parse(m.Groups["i"].Value);
                        if (i >= seq.Children.Count) { node = null; break; }
                        lastList = seq; lastIndex = i; node = seq.Children[i];
                    }
                }
                if (node == null)
                    throw new InvalidOperationException($"DevelopmentConfigText: il gruppo spento «{path}» non si trova nel file: il controllo e la potatura non vanno d'accordo.");
                if (lastList != null) elements.Add((lastList, lastIndex.Value));
                else parent.Children.Remove(new YamlScalarNode(lastName));
            }
            foreach (var (list, index) in elements.OrderByDescending(e => e.Index)) list.Children.RemoveAt(index);
        }

        private static bool IsEmpty(YamlNode n)
            => n is YamlScalarNode s && s.Style == YamlDotNet.Core.ScalarStyle.Plain && (string.IsNullOrEmpty(s.Value) || s.Value == "~" || s.Value == "null");

        private static bool HasEmpty(YamlNode node) => node switch
        {
            YamlMappingNode map => map.Children.Any(c => IsEmpty(c.Value) || HasEmpty(c.Value)),
            YamlSequenceNode seq => seq.Children.Any(HasEmpty),
            _ => false,
        };

        private static void DropEmpty(YamlNode node)
        {
            switch (node)
            {
                case YamlMappingNode map:
                    foreach (var key in map.Children.Where(c => IsEmpty(c.Value)).Select(c => c.Key).ToList()) map.Children.Remove(key);
                    foreach (var child in map.Children.Values) DropEmpty(child);
                    break;
                case YamlSequenceNode seq:
                    foreach (var child in seq.Children) DropEmpty(child);
                    break;
            }
        }
    }
}

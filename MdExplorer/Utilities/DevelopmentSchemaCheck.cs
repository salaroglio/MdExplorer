using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MdExplorer.Features.Configuration;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace MdExplorer.Utilities
{
    public enum SchemaMismatchKind { UnknownGroup, UnknownKey, MissingKey, WrongType, ValueNotAllowed }

    /// <summary>Una differenza fra il file e lo schema: dove, alla riga, che cosa.</summary>
    public sealed record SchemaMismatch(string Path, int Line, SchemaMismatchKind Kind, string Detail);

    /// <summary>
    /// Un gruppo spento: un gruppo in cima al file (<c>harness</c>) o un elemento di lista (<c>codeRoots[2]</c>), con la voce che
    /// lo riconosce e le differenze trovate. È l'unità che si spegne: un sottogruppo che non è un elemento di lista
    /// (<c>compatibility.gitHubOptions</c>) spegne il gruppo che lo contiene, perché la funzione non va a metà.
    /// </summary>
    public sealed record SwitchedOffGroup(string Path, string ItemKey, IReadOnlyList<SchemaMismatch> Mismatches);

    /// <summary>
    /// Il confronto di un <c>.development.yml</c> con lo schema che questo MDE porta dentro di sé (D3 dello sprint
    /// docs-internal/Sprints/2026-11-07-Schema-Development-Yml.md). Puro: non legge la lingua né scrive niente; i messaggi si
    /// compongono con <see cref="Describe"/>, passando <c>UiText.Say</c> quando si è fuori da una transazione (03/11).
    /// </summary>
    public sealed class DevelopmentSchemaCheck
    {
        /// <summary>Il numero scritto nel file (<c># mde-schema: N</c>); 1 se non c'è (file scritti prima dello sprint).</summary>
        public int FileSchema { get; private set; } = 1;

        /// <summary>Il file è stato scritto da un MDE con uno schema più alto di questo: le differenze vogliono dire «aggiorna MDE».</summary>
        public bool WrittenByNewerMde => FileSchema > DevelopmentSchema.Current;

        /// <summary>Non null se il file non si legge come YAML con delle voci: allora è tutto spento.</summary>
        public SchemaMismatch Unreadable { get; private set; }

        public IReadOnlyList<SwitchedOffGroup> SwitchedOff => _off;
        private readonly List<SwitchedOffGroup> _off = new();

        public bool AllGood => Unreadable == null && _off.Count == 0;

        /// <summary>True se il gruppo (<c>harness</c>, <c>codeRoots[2]</c>) è spento, anche perché è spento un gruppo che lo contiene.</summary>
        public bool IsOff(string path)
            => Unreadable != null || _off.Any(o => path == o.Path || path.StartsWith(o.Path + ".", StringComparison.Ordinal) || path.StartsWith(o.Path + "[", StringComparison.Ordinal));

        private static readonly Regex Number = new(@"^#\s*mde-schema:\s*(?<n>\d+)\s*$", RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>Il numero di schema scritto nel file; null se non c'è.</summary>
        public static int? SchemaNumberOf(string text)
        {
            var m = Number.Match(text ?? string.Empty);
            return m.Success && int.TryParse(m.Groups["n"].Value, out var n) ? n : null;
        }

        public static DevelopmentSchemaCheck Run(string text) => Run(text, DevelopmentSchema.Snapshots);

        /// <summary>Con le istantanee date: per i test, che provano uno schema 2 prima che esista.</summary>
        public static DevelopmentSchemaCheck Run(string text, IReadOnlyList<SchemaSnapshot> snapshots)
        {
            var check = new DevelopmentSchemaCheck { FileSchema = SchemaNumberOf(text) ?? 1 };
            YamlNode root;
            try
            {
                var stream = new YamlStream();
                stream.Load(new StringReader(text ?? string.Empty));
                root = stream.Documents.Count == 0 ? new YamlMappingNode() : stream.Documents[0].RootNode;
            }
            catch (YamlException ex)
            {
                check.Unreadable = new SchemaMismatch("", (int)ex.Start.Line, SchemaMismatchKind.WrongType, (ex.InnerException ?? ex).Message);
                return check;
            }
            if (root is YamlScalarNode s && IsEmpty(s)) root = new YamlMappingNode();
            if (root is not YamlMappingNode map)
            {
                check.Unreadable = new SchemaMismatch("", Line(root), SchemaMismatchKind.WrongType, "il file non è un elenco di voci / the file is not a list of entries");
                return check;
            }

            // Le voci ritirate le toglie la pulizia all'apertura (RetiredConfigKeys): non sono differenze, come in ReadStrict.
            var retired = new HashSet<string>(
                RetiredConfigKeys.All.Select(r => r.Section + "." + r.Key).Concat(RetiredConfigKeys.BecameList.Select(r => r.Section + "." + r.Key)),
                StringComparer.Ordinal);

            var newestFirst = snapshots.OrderByDescending(x => x.Version).ToList();
            foreach (var (keyNode, value) in map.Children)
            {
                var key = ((YamlScalarNode)keyNode).Value;
                var candidates = newestFirst.Select(x => (x, x.Root.Keys != null && x.Root.Keys.TryGetValue(key, out var n) ? n : null)).ToList();
                if (candidates.All(c => c.Item2 == null))
                {
                    check._off.Add(new SwitchedOffGroup(key, null, new[] { new SchemaMismatch(key, Line(keyNode), SchemaMismatchKind.UnknownGroup, "") }));
                    continue;
                }
                check.Unit(value, key, candidates, retired);
            }
            return check;
        }

        /// <summary>
        /// Un'unità (gruppo in cima o elemento di lista) combacia se combacia con almeno un'istantanea; se no si spegne con le
        /// differenze rispetto allo schema più nuovo che la conosce. Gli elementi delle sue liste di gruppi sono unità a sé.
        /// </summary>
        private void Unit(YamlNode node, string path, IReadOnlyList<(SchemaSnapshot Snapshot, SchemaNode Node)> candidates, HashSet<string> retired)
        {
            List<SchemaMismatch> first = null;
            (SchemaSnapshot Snapshot, SchemaNode Node)? matched = null;
            foreach (var c in candidates.Where(c => c.Node != null))
            {
                var found = new List<SchemaMismatch>();
                Own(node, c.Node, path, c.Snapshot.MissingKeysAllowed, retired, found);
                first ??= found;
                if (found.Count == 0) { matched = c; break; }
            }
            if (matched == null)
            {
                _off.Add(new SwitchedOffGroup(path, ItemKeyValue(node, candidates.First(c => c.Node != null).Node), first));
                return;
            }

            // Le liste di gruppi dentro questa unità (o l'unità stessa, se è una lista in cima come folders): ogni elemento si
            // controlla da sé, con tutte le istantanee.
            if (node is YamlSequenceNode top && matched.Value.Node.Kind == SchemaKind.List && matched.Value.Node.Item.Kind == SchemaKind.Group)
            {
                var items = candidates.Select(c => (c.Snapshot, c.Node?.Kind == SchemaKind.List ? c.Node.Item : null)).ToList();
                var n = 0;
                foreach (var item in top.Children) Unit(item, $"{path}[{n++}]", items, retired);
                return;
            }
            foreach (var (listPath, sequence, itemCandidates) in GroupLists(node, path, candidates))
            {
                var i = 0;
                foreach (var item in sequence.Children)
                    Unit(item, $"{listPath}[{i++}]", itemCandidates, retired);
            }
        }

        /// <summary>Le differenze dell'unità stessa: si scende nei sottogruppi e nelle liste di valori, non negli elementi di una lista di gruppi.</summary>
        private static void Own(YamlNode node, SchemaNode schema, string path, bool missingAllowed, HashSet<string> retired, List<SchemaMismatch> found)
        {
            if (node is YamlScalarNode empty && IsEmpty(empty)) return;   // non compilata
            switch (schema.Kind)
            {
                case SchemaKind.Group:
                    if (node is not YamlMappingNode map) { found.Add(new(path, Line(node), SchemaMismatchKind.WrongType, "atteso un gruppo di voci / expected a group of entries")); return; }
                    var present = new HashSet<string>(StringComparer.Ordinal);
                    foreach (var (k, v) in map.Children)
                    {
                        var key = ((YamlScalarNode)k).Value;
                        present.Add(key);
                        var where = path + "." + key;
                        if (retired.Contains(where)) continue;
                        if (schema.Keys == null || !schema.Keys.TryGetValue(key, out var child)) { found.Add(new(where, Line(k), SchemaMismatchKind.UnknownKey, "")); continue; }
                        Own(v, child, where, missingAllowed, retired, found);
                    }
                    if (!missingAllowed && schema.Keys != null)
                        foreach (var key in schema.Keys.Keys.Where(k => !present.Contains(k)))
                            found.Add(new(path + "." + key, Line(node), SchemaMismatchKind.MissingKey, ""));
                    return;
                case SchemaKind.List:
                    if (node is not YamlSequenceNode seq) { found.Add(new(path, Line(node), SchemaMismatchKind.WrongType, "attesa una lista / expected a list")); return; }
                    if (schema.Item.Kind == SchemaKind.Group) return;   // elementi = unità a sé (Unit)
                    var i = 0;
                    foreach (var item in seq.Children) Own(item, schema.Item, $"{path}[{i++}]", missingAllowed, retired, found);
                    return;
                default:
                    if (node is not YamlScalarNode scalar) { found.Add(new(path, Line(node), SchemaMismatchKind.WrongType, "atteso un valore / expected a value")); return; }
                    var value = scalar.Value;
                    if (schema.Kind == SchemaKind.Flag && !bool.TryParse(value, out _))
                        found.Add(new(path, Line(node), SchemaMismatchKind.WrongType, $"«{value}»: atteso true o false / expected true or false"));
                    else if (schema.Kind == SchemaKind.Number && !long.TryParse(value, out _))
                        found.Add(new(path, Line(node), SchemaMismatchKind.WrongType, $"«{value}»: atteso un numero / expected a number"));
                    else if (schema.Choices != null && !schema.Choices.Contains(value, StringComparer.OrdinalIgnoreCase))
                        found.Add(new(path, Line(node), SchemaMismatchKind.ValueNotAllowed, $"«{value}» ({string.Join(", ", schema.Choices)})"));
                    return;
            }
        }

        /// <summary>Le liste di gruppi raggiungibili dall'unità senza attraversare un'altra lista di gruppi.</summary>
        private static IEnumerable<(string Path, YamlSequenceNode Sequence, IReadOnlyList<(SchemaSnapshot, SchemaNode)> Items)> GroupLists(
            YamlNode node, string path, IReadOnlyList<(SchemaSnapshot Snapshot, SchemaNode Node)> candidates)
        {
            if (node is not YamlMappingNode map) yield break;
            foreach (var (k, v) in map.Children)
            {
                var key = ((YamlScalarNode)k).Value;
                var children = candidates.Select(c => (c.Snapshot, c.Node?.Kind == SchemaKind.Group && c.Node.Keys != null && c.Node.Keys.TryGetValue(key, out var n) ? n : null)).ToList();
                var shape = children.FirstOrDefault(c => c.Item2 != null).Item2;
                if (shape == null) continue;
                if (shape.Kind == SchemaKind.List && shape.Item.Kind == SchemaKind.Group && v is YamlSequenceNode seq)
                    yield return (path + "." + key, seq, children.Select(c => (c.Snapshot, c.Item2?.Kind == SchemaKind.List ? c.Item2.Item : null)).ToList());
                else if (shape.Kind == SchemaKind.Group)
                    foreach (var inner in GroupLists(v, path + "." + key, children)) yield return inner;
            }
        }

        private static string ItemKeyValue(YamlNode node, SchemaNode schema)
            => schema.ItemKey != null && node is YamlMappingNode map && map.Children.TryGetValue(new YamlScalarNode(schema.ItemKey), out var v) && v is YamlScalarNode s
                ? s.Value : null;

        private static bool IsEmpty(YamlScalarNode s)
            => s.Style == YamlDotNet.Core.ScalarStyle.Plain && (string.IsNullOrEmpty(s.Value) || s.Value == "~" || s.Value == "null");

        private static int Line(YamlNode node) => node == null ? 0 : (int)node.Start.Line;

        /// <summary>
        /// L'avviso per l'utente, nelle due lingue scelte da <paramref name="say"/> (di solito <c>UiText.Say</c>, fuori dalle transazioni).
        /// Vuoto se va tutto bene.
        /// </summary>
        public string Describe(Func<string, string, string> say)
        {
            if (AllGood) return string.Empty;
            if (Unreadable != null)
                return say($".development.yml non si legge (riga {Unreadable.Line}: {Unreadable.Detail}): MDE non usa e non scrive niente del file finché non è corretto.",
                           $".development.yml cannot be read (line {Unreadable.Line}: {Unreadable.Detail}): MDE neither uses nor writes anything in the file until it is fixed.");
            var groups = string.Join(", ", _off.Select(o => o.ItemKey == null ? o.Path : $"{o.Path} ({o.ItemKey})"));
            if (WrittenByNewerMde)
                return say($"Configurato da un MDE più nuovo (schema {FileSchema}, questo MDE ha il {DevelopmentSchema.Current}): {groups} restano spenti finché non aggiorni MDE. Il resto funziona.",
                           $"Configured by a newer MDE (schema {FileSchema}, this MDE has {DevelopmentSchema.Current}): {groups} stay off until you update MDE. The rest works.");
            var first = _off.SelectMany(o => o.Mismatches).First();
            return say($"In .development.yml {groups} non combaciano con lo schema di questo MDE (riga {first.Line}, «{first.Path}»: {Kind(first.Kind, true)}{Detail(first)}): restano spenti. Se un collega usa un MDE più nuovo, aggiorna MDE.",
                       $"In .development.yml {groups} do not match the schema of this MDE (line {first.Line}, «{first.Path}»: {Kind(first.Kind, false)}{Detail(first)}): they stay off. If a colleague uses a newer MDE, update MDE.");
        }

        private static string Detail(SchemaMismatch m) => string.IsNullOrEmpty(m.Detail) ? "" : " " + m.Detail;

        private static string Kind(SchemaMismatchKind kind, bool it) => kind switch
        {
            SchemaMismatchKind.UnknownGroup => it ? "funzione che questo MDE non conosce" : "feature this MDE does not know",
            SchemaMismatchKind.UnknownKey => it ? "voce che questo MDE non conosce" : "entry this MDE does not know",
            SchemaMismatchKind.MissingKey => it ? "voce mancante" : "missing entry",
            SchemaMismatchKind.ValueNotAllowed => it ? "valore che questo MDE non conosce" : "value this MDE does not know",
            _ => it ? "tipo diverso" : "different type",
        };
    }
}

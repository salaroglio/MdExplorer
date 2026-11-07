using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MdExplorer.Features.Configuration;
using MdExplorer.Service.Models;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// La scrittura di <c>.development.yml</c> per gruppo (F3 dello sprint docs-internal/Sprints/2026-11-07-Schema-Development-Yml.md).
    /// Chi scrive passa il modello intero, come prima; qui si confronta gruppo per gruppo con il file e si riscrive solo il gruppo che
    /// cambia. Un gruppo uguale resta byte per byte; un gruppo spento (scritto da un MDE più nuovo o rovinato) resta com'era, anche
    /// quando cambia il gruppo che lo contiene (un elemento di lista spento si rimette al suo posto). Un gruppo riscritto ha tutte le sue
    /// voci, vuote se non compilate (D6, schema 2); i gruppi assenti non si scrivono. In testa il numero di schema, mai abbassato.
    /// <para>
    /// Rifiuta, con <see cref="DevelopmentConfigOffException"/>: scrivere in un gruppo spento, un file che non si legge, e (D5) creare un
    /// gruppo o un elemento nuovo in un file scritto da un MDE più nuovo.
    /// </para>
    /// </summary>
    public static class DevelopmentConfigWriter
    {
        // Senza le voci vuote: quelle le aggiunge Complete dallo schema. YamlDotNet scriverebbe un null come '' (testo vuoto), che
        // poi non si rilegge come gruppo.
        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        public static string Header(int schema) => $"# mde-schema: {schema}";

        private static readonly Regex HeaderLine = new(@"^#\s*mde-schema:\s*\d+[ \t]*\n", RegexOptions.Compiled | RegexOptions.Multiline);

        /// <summary>
        /// N7 (utente 07/11): la città degli agenti lavora solo tra MDE che gestiscono la stessa versione di <c>.development.yml</c>.
        /// Un MDE che ne gestisce una più alta di quella scritta, in un progetto con la città accesa, alza il numero nel file (solo la
        /// riga in testa, il resto resta byte per byte): così chi è rimasto indietro lo scopre con l'avviso «aggiorna MDE», invece di
        /// bloccarsi su un registro dei giri o su messaggi che non capisce. True se ha scritto.
        /// </summary>
        public static bool AlignForCity(string path)
        {
            if (!File.Exists(path)) return false;
            var text = File.ReadAllText(path);
            var current = DevelopmentSchemaSnapshots.Current;
            if ((DevelopmentSchemaCheck.SchemaNumberOf(text) ?? 1) >= current) return false;
            var read = DevelopmentConfigFile.Read(path);
            if (read.IsOff(DevelopmentSchemaCheck.CityGroup) || read.Config.AgentCity?.Enabled != true) return false;
            var lf = text.Replace("\r\n", "\n");
            var aligned = Header(current) + "\n" + HeaderLine.Replace(lf, string.Empty, 1);
            return MdeRepoWriter.WriteIfChanged(path, aligned, new MdeWhy(
                $"portata alla versione {current} la versione di .development.yml: la città degli agenti lavora solo tra MDE che gestiscono la stessa, chi è indietro riceve l'invito ad aggiornare",
                $"raised .development.yml to version {current}: the agent city works only among MDEs handling the same one, whoever is behind is asked to update"));
        }

        private static readonly Regex TopKey = new(@"^(?<key>[A-Za-z_][\w-]*)\s*:", RegexOptions.Compiled);

        /// <summary>Il testo nuovo del file (fine riga LF): <paramref name="original"/> null = il file non c'è ancora.</summary>
        public static string Compose(string original, DevelopmentConfig config)
            => Compose(original, config, DevelopmentSchemaSnapshots.All, DevelopmentSchemaSnapshots.Current);

        /// <summary>Con istantanee e numero dati: per i test.</summary>
        public static string Compose(string original, DevelopmentConfig config, IReadOnlyList<SchemaSnapshot> snapshots, int current)
        {
            var fresh = Blocks(FullText(config)).Blocks;
            if (original == null) return Assemble(new List<string>(), current, fresh.Select(b => b.Text));

            var text = original.Replace("\r\n", "\n");
            var usable = DevelopmentConfigText.Prune(text, snapshots);
            var check = usable.Check;
            if (check.Unreadable != null) throw Refuse(check, "il file non si legge", "the file cannot be read");

            // Il testo originale senza le voci ritirate: le posizioni e gli elementi spenti si cercano qui (il controllo ha visto questo).
            var cleaned = RetiredConfigKeys.Remove(text, out var retired);
            var (header, old) = Blocks(cleaned);
            var oldUsable = Blocks(usable.Text).Blocks.ToDictionary(b => b.Key, b => b.Text, StringComparer.Ordinal);
            var newByKey = fresh.ToDictionary(b => b.Key, b => b.Text, StringComparer.Ordinal);
            var cleanedRoot = Root(cleaned);
            var newerFile = check.WrittenByNewerMde;

            var result = new List<string>();
            var changed = retired.Count > 0;
            foreach (var block in old)
            {
                var offHere = check.SwitchedOff.Where(o => o.Path == block.Key || o.Path.StartsWith(block.Key + "[", StringComparison.Ordinal) || o.Path.StartsWith(block.Key + ".", StringComparison.Ordinal)).ToList();
                var wholeOff = offHere.Any(o => o.Path == block.Key);
                newByKey.TryGetValue(block.Key, out var replacement);
                oldUsable.TryGetValue(block.Key, out var before);

                if (wholeOff)
                {
                    // Il modello non ha questo gruppo (è spento): se chi scrive ne mette uno, vuole scrivere in un gruppo spento.
                    if (replacement != null) throw Refuse(check, $"«{block.Key}» è spento", $"«{block.Key}» is switched off");
                    result.Add(block.Text);
                    continue;
                }
                if (Same(block.Key, before, replacement))
                {
                    result.Add(block.Text);
                    continue;
                }
                changed = true;
                if (replacement == null)
                {
                    // Il gruppo tolto dal modello: se dentro c'è un elemento spento, quello resta e il gruppo non si toglie.
                    if (offHere.Count > 0) throw Refuse(check, $"«{block.Key}» contiene elementi spenti", $"«{block.Key}» holds switched-off items");
                    continue;
                }
                if (newerFile && Grows(Root(before ?? string.Empty), Root(replacement)))
                    throw Refuse(check, $"aggiungere a «{block.Key}» in un file di un MDE più nuovo", $"adding to «{block.Key}» in a file of a newer MDE");
                result.Add(offHere.Count == 0 ? replacement : WithOffItems(block.Key, replacement, cleanedRoot, offHere));
                newByKey.Remove(block.Key);
            }
            foreach (var block in fresh.Where(b => !old.Any(o => o.Key == b.Key) && !Same(b.Key, null, b.Text)))
            {
                if (newerFile) throw Refuse(check, $"creare «{block.Key}» in un file di un MDE più nuovo", $"creating «{block.Key}» in a file of a newer MDE");
                changed = true;
                result.Add(block.Text);
            }

            if (!changed) return original;   // niente da fare: nemmeno il numero in testa cambia (WriteIfChanged non scrive)
            return Assemble(header, Math.Max(check.FileSchema, current), result);
        }

        /// <summary>Il modello in YAML con tutte le voci dei gruppi presenti (vuote se non compilate) e senza i gruppi assenti.</summary>
        private static string FullText(DevelopmentConfig config)
        {
            var root = Root(Serializer.Serialize(config ?? new DevelopmentConfig()));
            // Solo dentro i gruppi presenti: un gruppo assente resta assente (i gruppi sono facoltativi, D6).
            foreach (var p in Properties(typeof(DevelopmentConfig)))
                if (root.Children.TryGetValue(new YamlScalarNode(p.Name), out var v)) Complete(v, p.Type);
            return Save(root);
        }

        /// <summary>Le voci di un tipo del modello, nell'ordine in cui le scrive il serializzatore (quello della dichiarazione).</summary>
        private static IEnumerable<(string Name, Type Type)> Properties(Type type)
            => type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                   .Where(p => p.CanRead && p.CanWrite)
                   .Select(p => (CamelCaseNamingConvention.Instance.Apply(p.Name), p.PropertyType));

        private static bool IsGroup(Type t) => t.IsClass && t != typeof(string) && !typeof(System.Collections.IEnumerable).IsAssignableFrom(t);

        /// <summary>
        /// Le voci mancanti di un gruppo presente, vuote (D6), nell'ordine del modello: lo stesso gruppo dà sempre lo stesso testo, e una
        /// voce che si compila resta al suo posto. Dentro: i sottogruppi e gli elementi delle liste di gruppi.
        /// </summary>
        private static void Complete(YamlNode node, Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var item = type.GetGenericArguments()[0];
                if (IsGroup(item) && node is YamlSequenceNode seq) foreach (var e in seq.Children) Complete(e, item);
                return;
            }
            if (!IsGroup(type) || node is not YamlMappingNode map) return;
            var ordered = new List<KeyValuePair<YamlNode, YamlNode>>();
            foreach (var (name, propType) in Properties(type))
            {
                var key = new YamlScalarNode(name);
                if (map.Children.TryGetValue(key, out var present)) { Complete(present, propType); ordered.Add(new(key, present)); }
                else ordered.Add(new(key, new YamlScalarNode("~")));
            }
            map.Children.Clear();
            foreach (var (k, v) in ordered) map.Children.Add(k, v);
        }

        private static bool IsNull(YamlScalarNode s)
            => s.Style == YamlDotNet.Core.ScalarStyle.Plain && (string.IsNullOrEmpty(s.Value) || s.Value == "~" || s.Value == "null");

        /// <summary>Due versioni di un gruppo dicono la stessa cosa se il modello le legge uguali (predefiniti e voci vuote compresi).</summary>
        private static bool Same(string key, string before, string after)
        {
            // Un gruppo che il modello ha solo come predefinito (compatibility, externalBrowser… di un file che non li ha) non è nuovo.
            if (before == null) return after == null || Canon(after) == EmptyCanon.Value;
            if (after == null) return false;
            return Canon(before) == Canon(after);
        }

        private static readonly Lazy<string> EmptyCanon = new(() => Serializer.Serialize(new DevelopmentConfig()));

        private static string Canon(string block)
        {
            var root = Root(block);
            DropEmpty(root);
            var config = Deserializer.Deserialize<DevelopmentConfig>(Save(root)) ?? new DevelopmentConfig();
            return Serializer.Serialize(config);
        }

        private static void DropEmpty(YamlNode node)
        {
            switch (node)
            {
                case YamlMappingNode map:
                    foreach (var k in map.Children.Where(c => c.Value is YamlScalarNode s && IsNull(s)).Select(c => c.Key).ToList()) map.Children.Remove(k);
                    foreach (var child in map.Children.Values) DropEmpty(child);
                    break;
                case YamlSequenceNode seq:
                    foreach (var child in seq.Children) DropEmpty(child);
                    break;
            }
        }

        /// <summary>True se nel nuovo c'è un gruppo, una voce di gruppo o un elemento di lista che prima non c'era.</summary>
        private static bool Grows(YamlNode before, YamlNode after)
        {
            switch (after)
            {
                case YamlMappingNode a:
                    var b = before as YamlMappingNode;
                    foreach (var (k, v) in a.Children)
                    {
                        if (v is YamlScalarNode s && IsNull(s)) continue;
                        YamlNode old = null;
                        if (b == null || !b.Children.TryGetValue(k, out old)) { if (v is YamlMappingNode || v is YamlSequenceNode seqNew && seqNew.Children.Count > 0) return true; continue; }
                        if (Grows(old, v)) return true;
                    }
                    return false;
                case YamlSequenceNode sa:
                    var sb = before as YamlSequenceNode;
                    var count = sb?.Children.Count ?? 0;
                    if (sa.Children.Count > count) return true;
                    for (var i = 0; i < sa.Children.Count; i++) if (Grows(sb.Children[i], sa.Children[i])) return true;
                    return false;
                default:
                    return false;
            }
        }

        private static readonly Regex Segment = new(@"^(?<name>[^\[\]]+)(?:\[(?<i>\d+)\])?$", RegexOptions.Compiled);

        /// <summary>
        /// Il gruppo nuovo con gli elementi spenti rimessi al loro posto: dal meno profondo, e nella stessa lista dall'indice più basso,
        /// così gli indici del file originale tornano validi anche per gli elementi più in fondo.
        /// </summary>
        private static string WithOffItems(string key, string replacement, YamlMappingNode original, IReadOnlyList<SwitchedOffGroup> off)
        {
            var root = Root(replacement);
            foreach (var item in off.OrderBy(o => o.Path.Count(c => c == '.' || c == '['))
                                    .ThenBy(o => int.Parse(Regex.Match(o.Path, @"\[(\d+)\]$").Groups[1].Value)))
            {
                var source = Find(original, item.Path, out _, out _)
                    ?? throw new InvalidOperationException($"DevelopmentConfigWriter: l'elemento spento «{item.Path}» non si trova nel file.");
                var parentPath = item.Path.Substring(0, item.Path.LastIndexOf('['));
                var index = int.Parse(Regex.Match(item.Path, @"\[(\d+)\]$").Groups[1].Value);
                var list = Find(root, parentPath, out _, out _) as YamlSequenceNode;
                if (list == null)
                {
                    // La lista non c'è più nel modello: si ricrea per tenere l'elemento spento.
                    var parentOfList = parentPath.Contains('.') ? Find(root, parentPath.Substring(0, parentPath.LastIndexOf('.')), out _, out _) as YamlMappingNode : root;
                    if (parentOfList == null) throw new InvalidOperationException($"DevelopmentConfigWriter: non trovo dove rimettere «{item.Path}».");
                    list = new YamlSequenceNode();
                    parentOfList.Children[new YamlScalarNode(parentPath.Split('.').Last())] = list;
                }
                list.Children.Insert(Math.Min(index, list.Children.Count), source);
            }
            return Blocks(Save(root)).Blocks.Single(b => b.Key == key).Text;
        }

        private static YamlNode Find(YamlMappingNode root, string path, out YamlMappingNode parent, out string name)
        {
            YamlNode node = root; parent = null; name = null;
            foreach (var part in path.Split('.'))
            {
                var m = Segment.Match(part);
                if (!m.Success || node is not YamlMappingNode map) return null;
                parent = map; name = m.Groups["name"].Value;
                if (!map.Children.TryGetValue(new YamlScalarNode(name), out node)) return null;
                if (m.Groups["i"].Success)
                {
                    var i = int.Parse(m.Groups["i"].Value);
                    if (node is not YamlSequenceNode seq || i >= seq.Children.Count) return null;
                    node = seq.Children[i];
                }
            }
            return node;
        }

        private sealed record Block(string Key, string Text);

        /// <summary>
        /// Il testo diviso in testa (commenti prima della prima voce) e gruppi in cima: un gruppo va dalla sua riga a colonna 0 alla
        /// riga prima del gruppo dopo. MDE scrive il file in stile a blocchi, quindi basta guardare la colonna 0.
        /// </summary>
        private static (List<string> Header, List<Block> Blocks) Blocks(string text)
        {
            var header = new List<string>();
            var blocks = new List<Block>();
            string key = null;
            var current = new StringBuilder();
            foreach (var line in (text ?? string.Empty).Replace("\r\n", "\n").Split('\n'))
            {
                var m = TopKey.Match(line);
                if (m.Success)
                {
                    if (key != null) blocks.Add(new Block(key, current.ToString()));
                    key = m.Groups["key"].Value; current.Clear();
                }
                if (key == null) { if (line.Length > 0) header.Add(line); continue; }
                current.Append(line).Append('\n');
            }
            if (key != null) blocks.Add(new Block(key, current.ToString().TrimEnd('\n') + "\n"));
            return (header, blocks);
        }

        private static string Assemble(List<string> header, int schema, IEnumerable<string> blocks)
        {
            var sb = new StringBuilder();
            sb.Append(Header(schema)).Append('\n');
            foreach (var line in header.Where(l => DevelopmentSchemaCheck.SchemaNumberOf(l) == null && l.Trim() != "---")) sb.Append(line).Append('\n');
            foreach (var block in blocks) sb.Append(block.TrimEnd('\n')).Append('\n');
            return sb.ToString();
        }

        private static YamlMappingNode Root(string text)
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(text ?? string.Empty));
            return stream.Documents.Count == 0 ? new YamlMappingNode() : stream.Documents[0].RootNode as YamlMappingNode ?? new YamlMappingNode();
        }

        private static string Save(YamlMappingNode root) => DevelopmentConfigText.Save(root);

        private static DevelopmentConfigOffException Refuse(DevelopmentSchemaCheck check, string it, string en)
        {
            var why = check.AllGood
                ? $"Il file è scritto da un MDE più nuovo (schema {check.FileSchema}, questo MDE ha il {DevelopmentSchemaSnapshots.Current}): aggiorna MDE. / " +
                  $"The file is written by a newer MDE (schema {check.FileSchema}, this MDE has {DevelopmentSchemaSnapshots.Current}): update MDE."
                : check.Describe((i, e) => i + " / " + e);
            // Le due lingue insieme, senza UiText: si scrive anche dentro una transazione (03/11).
            return new(why + $" — MDE non scrive: {it}. / MDE does not write: {en}.");
        }
    }
}

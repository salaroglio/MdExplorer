using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MdExplorer.Features.Agents;
using MdExplorer.Features.Yaml;
using MdExplorer.Features.Yaml.Models;
using MdExplorer.Service.Models;
using MdExplorer.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.IntegrationTests.Yaml
{
    /// <summary>
    /// La fotografia di come MDE legge e scrive YAML (21/11/2026, sprint .NET 10, L3: YamlDotNet 11.2.1 → 18.1.0).
    /// MDE vive di YAML: <c>.development.yml</c>, le dichiarazioni degli agenti (e l'impronta della fiducia, che è l'hash dello
    /// YAML scritto), le intestazioni dei documenti. Una versione nuova della libreria non deve leggere né scrivere un valore
    /// in modo diverso senza che ce ne accorgiamo. Il test rifà la fotografia e la confronta, byte per byte, con
    /// <c>Golden/expected.txt</c>. Una differenza NON si «normalizza»: si guarda e si decide.
    /// <para>
    /// Rifare la fotografia (solo dopo aver deciso che le differenze vanno bene):
    /// <c>MDE_YAML_GOLDEN_WRITE=&lt;file&gt; dotnet test --filter FullyQualifiedName~YamlLibraryGolden</c>.
    /// Con <c>MDE_YAML_GOLDEN_EXTRA=cartella1:cartella2</c> la fotografia comprende anche i file YAML veri di quelle cartelle
    /// (per confrontare a mano due versioni su progetti reali: quell'uscita non va nel repository).
    /// </para>
    /// </summary>
    [TestClass]
    public class YamlLibraryGolden_Should
    {
        private static string GoldenDir()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "MdExplorer.IntegrationTests", "Yaml", "Golden");
                if (Directory.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }
            throw new InvalidOperationException("MdExplorer.IntegrationTests/Yaml/Golden non è raggiungibile da " + AppContext.BaseDirectory);
        }

        [TestMethod]
        public void Read_and_write_yaml_exactly_as_photographed()
        {
            var golden = GoldenDir();
            var sb = new StringBuilder();
            foreach (var file in Directory.GetFiles(Path.Combine(golden, "corpus")).OrderBy(f => Path.GetFileName(f), StringComparer.Ordinal))
                DescribeFile(sb, Path.GetFileName(file), File.ReadAllText(file));
            foreach (var (name, yaml) in Snippets) DescribeYaml(sb, "frammento/" + name, yaml, null);
            DescribeTyped(sb);
            DescribeWrites(sb);

            var extra = Environment.GetEnvironmentVariable("MDE_YAML_GOLDEN_EXTRA");
            if (!string.IsNullOrEmpty(extra))
            {
                var n = 0;
                foreach (var root in extra.Split(':', StringSplitOptions.RemoveEmptyEntries))
                    foreach (var file in ExtraFiles(root))
                    {
                        string text; try { text = File.ReadAllText(file); } catch (Exception readError) { sb.Append($"### extra/{n++} illeggibile {readError.GetType().Name}\n"); continue; }
                        DescribeFile(sb, $"extra/{n++}/{Path.GetFileName(file)}", text);
                    }
                sb.Append($"### extra: {n} file\n");
            }

            var actual = sb.ToString();
            var write = Environment.GetEnvironmentVariable("MDE_YAML_GOLDEN_WRITE");
            if (!string.IsNullOrEmpty(write))
            {
                File.WriteAllText(write, actual, new UTF8Encoding(false));
                Assert.Inconclusive($"Fotografia scritta in {write} ({actual.Length} caratteri): nessun confronto fatto.");
            }
            Assert.IsTrue(string.IsNullOrEmpty(extra), "MDE_YAML_GOLDEN_EXTRA serve solo insieme a MDE_YAML_GOLDEN_WRITE");
            var expected = File.ReadAllText(Path.Combine(golden, "expected.txt"));
            if (expected == actual) return;
            var e = expected.Split('\n'); var a = actual.Split('\n');
            var i = 0; while (i < e.Length && i < a.Length && e[i] == a[i]) i++;
            var section = ""; for (var k = Math.Min(i, a.Length - 1); k >= 0; k--) if (a[k].StartsWith("### ", StringComparison.Ordinal)) { section = a[k]; break; }
            var different = 0; for (var k = 0; k < Math.Max(e.Length, a.Length); k++) if (k >= e.Length || k >= a.Length || e[k] != a[k]) different++;
            Assert.Fail($"La libreria YAML legge o scrive in modo diverso dalla fotografia: {different} righe diverse su {e.Length}. Prima alla riga {i + 1}, in «{section}»:\n" +
                        $"  attesa: {(i < e.Length ? e[i] : "(fine)")}\n  ora:    {(i < a.Length ? a[i] : "(fine)")}");
        }

        private static IEnumerable<string> ExtraFiles(string root)
        {
            var stack = new Stack<string>(); stack.Push(root);
            var found = new List<string>();
            while (stack.Count > 0)
            {
                var dir = stack.Pop();
                string[] files, dirs;
                try { files = Directory.GetFiles(dir); dirs = Directory.GetDirectories(dir); } catch { continue; }
                foreach (var f in files)
                {
                    var name = Path.GetFileName(f);
                    if (name.EndsWith(".yml", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || name.EndsWith(".agent.md", StringComparison.OrdinalIgnoreCase))
                        if (new FileInfo(f).Length < 400_000) found.Add(f);
                }
                foreach (var d in dirs)
                {
                    var name = Path.GetFileName(d);
                    if (name is "node_modules" or "bin" or "obj" or ".git" or "dist" or "wwwroot" or ".localhistory" or ".angular" or ".venv" or "venv" or "__pycache__") continue;
                    if ((File.GetAttributes(d) & FileAttributes.ReparsePoint) != 0) continue;
                    stack.Push(d);
                }
            }
            found.Sort(StringComparer.Ordinal);
            return found;
        }

        // ───────────────────────── un file ─────────────────────────

        private static void DescribeFile(StringBuilder sb, string name, string text)
        {
            var isMarkdown = name.EndsWith(".md", StringComparison.OrdinalIgnoreCase);
            string yaml = text;
            if (isMarkdown)
            {
                yaml = FrontMatter(text);
                if (yaml == null) { sb.Append($"### {name}\n  senza intestazione\n"); return; }
            }
            DescribeYaml(sb, name, yaml, isMarkdown ? text : null);
        }

        private static string FrontMatter(string markdown)
        {
            var t = markdown.Replace("\r\n", "\n");
            if (!t.StartsWith("---\n", StringComparison.Ordinal)) return null;
            var end = t.IndexOf("\n---", 4, StringComparison.Ordinal);
            return end < 0 ? null : t.Substring(4, end - 4 + 1);
        }

        private static void DescribeYaml(StringBuilder sb, string name, string yaml, string markdown)
        {
            sb.Append($"### {name}\n");

            // A. il modello a nodi (YamlStream): tipo, stile, etichetta, ancora, posizione, valore
            YamlStream stream = null;
            Section(sb, "nodi", () => { stream = new YamlStream(); stream.Load(new StringReader(yaml)); var o = new StringBuilder(); o.Append($"documenti={stream.Documents.Count}\n"); foreach (var d in stream.Documents) Node(o, d.RootNode, 0); return o.ToString(); });
            // B. riscrittura dal modello a nodi (DevelopmentConfigText, ProjectKeysFile)
            if (stream != null) Section(sb, "nodi riscritti", () => { var w = new StringWriter(); stream.Save(w, assignAnchors: false); return w.ToString(); });
            // C. lettura senza tipo, con i tre modi di nominare
            object plain = null;
            Section(sb, "oggetto", () => { plain = new DeserializerBuilder().Build().Deserialize<object>(yaml); return Dump(plain); });
            Section(sb, "dizionario camelCase", () => Dump(new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build().Deserialize<Dictionary<string, object>>(yaml)));
            Section(sb, "dizionario object", () => Dump(new DeserializerBuilder().Build().Deserialize<Dictionary<object, object>>(yaml)));
            // D. riscrittura dell'oggetto letto, con le configurazioni usate in MDE
            if (plain != null)
            {
                Section(sb, "scritto predefinito", () => new SerializerBuilder().Build().Serialize(plain));
                Section(sb, "scritto camelCase", () => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build().Serialize(plain));
                Section(sb, "scritto camelCase senza null", () => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build().Serialize(plain));
                Section(sb, "scritto snake_case", () => new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build().Serialize(plain));
            }

            // E. i modelli veri di MDE
            var isAgent = name.EndsWith(".agent.md", StringComparison.OrdinalIgnoreCase);
            if (isAgent && markdown != null)
            {
                AgentCardParseResult card = null;
                Section(sb, "agente", () => { card = new YamlAgentCardParser().GetDescriptor(markdown); return Dump(card); });
                if (card?.Card != null)
                {
                    Section(sb, "agente scritto snake_case", () => new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build().Serialize(card.Card));
                    Section(sb, "agente impronta della fiducia", () => AgentTrustHasher.ComputeHash(card.Card, card.Tools));
                }
            }
            else if (markdown != null)
            {
                MdExplorerDocumentDescriptor descriptor = null;
                var parser = new YamlDocumentDescriptorParser();
                Section(sb, "intestazione del documento", () => { descriptor = parser.GetDescriptor(markdown); return Dump(descriptor); });
                if (descriptor != null)
                {
                    Section(sb, "intestazione scritta", () => parser.SerializeDescriptor(descriptor));
                    Section(sb, "documento con l'intestazione riscritta", () => parser.SetDescriptor(descriptor, markdown));
                }
            }
            if (name.Contains("development", StringComparison.OrdinalIgnoreCase))
            {
                DevelopmentConfig config = null;
                Section(sb, "development letto", () => { config = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build().Deserialize<DevelopmentConfig>(yaml); return Dump(config); });
                if (config != null)
                {
                    Section(sb, "development scritto da zero", () => DevelopmentConfigWriter.Compose(null, config));
                    Section(sb, "development scritto sul file", () => DevelopmentConfigWriter.Compose(yaml, config));
                }
            }
        }

        private static void Section(StringBuilder sb, string title, Func<string> produce)
        {
            sb.Append($"== {title}\n");
            string text;
            try { text = produce(); }
            catch (Exception ex)
            {
                var inner = ex; while (inner.InnerException != null) inner = inner.InnerException;
                var at = ex is YamlException y ? $" @{y.Start.Line}:{y.Start.Column}" : "";
                sb.Append($"  ECCEZIONE {ex.GetType().Name}{at}: {Visible(ex.Message)}");
                if (!ReferenceEquals(inner, ex)) sb.Append($" ← {inner.GetType().Name}: {Visible(inner.Message)}");
                sb.Append('\n');
                return;
            }
            if (text == null) { sb.Append("  (null)\n"); return; }
            // una riga per riga, con la fine visibile: gli spazi in coda, i \r e i tab si devono vedere
            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                if (i == lines.Length - 1 && lines[i].Length == 0) break;
                sb.Append("  |").Append(Visible(lines[i])).Append(i < lines.Length - 1 ? "¶" : "(senza a capo)").Append('\n');
            }
        }

        private static string Visible(string s)
        {
            var sb = new StringBuilder(s.Length);
            foreach (var c in s)
            {
                if (c == '\\') sb.Append("\\\\");
                else if (c == '\n') sb.Append("\\n");
                else if (c == '\r') sb.Append("\\r");
                else if (c == '\t') sb.Append("\\t");
                else if (c < ' ' || c == '\u007f' || c == '\u0085' || c == '\u00a0' || c == '\u2028' || c == '\u2029' || c == '\ufeff') sb.Append("\\u").Append(((int)c).ToString("x4"));
                else sb.Append(c);
            }
            return sb.ToString();
        }

        private static void Node(StringBuilder o, YamlNode node, int depth)
        {
            var pad = new string(' ', depth * 2);
            var tag = node.Tag.IsEmpty ? "" : " tag=" + node.Tag.Value;
            var anchor = node.Anchor.IsEmpty ? "" : " ancora=" + node.Anchor.Value;
            var at = $"@{node.Start.Line}:{node.Start.Column}";
            switch (node)
            {
                case YamlScalarNode s:
                    o.Append($"{pad}scalare {s.Style}{tag}{anchor} {at} «{s.Value ?? "(null)"}»\n"); break;
                case YamlSequenceNode q:
                    o.Append($"{pad}lista {q.Style}{tag}{anchor} {at} [{q.Children.Count}]\n");
                    foreach (var c in q.Children) Node(o, c, depth + 1);
                    break;
                case YamlMappingNode m:
                    o.Append($"{pad}mappa {m.Style}{tag}{anchor} {at} [{m.Children.Count}]\n");
                    foreach (var kv in m.Children) { o.Append($"{pad} chiave:\n"); Node(o, kv.Key, depth + 2); o.Append($"{pad} valore:\n"); Node(o, kv.Value, depth + 2); }
                    break;
                default:
                    o.Append($"{pad}{node.GetType().Name}{tag}{anchor} {at} {node}\n"); break;
            }
        }

        /// <summary>Un oggetto con i tipi veri (il tipo .NET di ogni valore è parte della fotografia), cultura invariante.</summary>
        private static string Dump(object value)
        {
            var sb = new StringBuilder();
            Dump(sb, value, 0, new HashSet<object>(ReferenceEqualityComparer.Instance));
            return sb.ToString();
        }

        private static void Dump(StringBuilder sb, object v, int depth, HashSet<object> seen)
        {
            var pad = new string(' ', depth * 2);
            switch (v)
            {
                case null: sb.Append(pad).Append("null\n"); return;
                case string s: sb.Append(pad).Append("String «").Append(s).Append("»\n"); return;
                case bool b: sb.Append(pad).Append("Boolean ").Append(b ? "true" : "false").Append('\n'); return;
                case double d: sb.Append(pad).Append("Double ").Append(d.ToString("R", CultureInfo.InvariantCulture)).Append('\n'); return;
                case float f: sb.Append(pad).Append("Single ").Append(f.ToString("R", CultureInfo.InvariantCulture)).Append('\n'); return;
                case DateTime dt: sb.Append(pad).Append("DateTime ").Append(dt.ToString("o", CultureInfo.InvariantCulture)).Append(' ').Append(dt.Kind).Append('\n'); return;
                case DateTimeOffset dto: sb.Append(pad).Append("DateTimeOffset ").Append(dto.ToString("o", CultureInfo.InvariantCulture)).Append('\n'); return;
                case TimeSpan ts: sb.Append(pad).Append("TimeSpan ").Append(ts.ToString("c", CultureInfo.InvariantCulture)).Append('\n'); return;
                case Enum en: sb.Append(pad).Append(en.GetType().Name).Append(' ').Append(en).Append('\n'); return;
                case IFormattable fo when v.GetType().IsPrimitive || v is decimal || v is Guid:
                    sb.Append(pad).Append(v.GetType().Name).Append(' ').Append(fo.ToString(null, CultureInfo.InvariantCulture)).Append('\n'); return;
                case char c: sb.Append(pad).Append("Char «").Append(c).Append("»\n"); return;
            }
            if (!seen.Add(v)) { sb.Append(pad).Append("(già visto)\n"); return; }
            if (depth > 40) { sb.Append(pad).Append("(troppo profondo)\n"); return; }
            if (v is IDictionary dict)
            {
                sb.Append(pad).Append(TypeName(v.GetType())).Append(" {").Append(dict.Count).Append("}\n");
                foreach (DictionaryEntry e in dict)
                {
                    sb.Append(pad).Append(" chiave:\n"); Dump(sb, e.Key, depth + 2, seen);
                    sb.Append(pad).Append(" valore:\n"); Dump(sb, e.Value, depth + 2, seen);
                }
                return;
            }
            if (v is IEnumerable en2)
            {
                var items = en2.Cast<object>().ToList();
                sb.Append(pad).Append(TypeName(v.GetType())).Append(" [").Append(items.Count).Append("]\n");
                foreach (var i in items) Dump(sb, i, depth + 1, seen);
                return;
            }
            sb.Append(pad).Append(TypeName(v.GetType())).Append('\n');
            foreach (var p in v.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.GetIndexParameters().Length == 0).OrderBy(p => p.Name, StringComparer.Ordinal))
            {
                object pv; try { pv = p.GetValue(v); } catch (Exception ex) { sb.Append(pad).Append(' ').Append(p.Name).Append(": ECCEZIONE ").Append(ex.GetType().Name).Append('\n'); continue; }
                sb.Append(pad).Append(' ').Append(p.Name).Append(":\n");
                Dump(sb, pv, depth + 2, seen);
            }
        }

        private static string TypeName(Type t) => t.IsGenericType ? t.Name.Split('`')[0] + "<" + string.Join(",", t.GetGenericArguments().Select(TypeName)) + ">" : t.Name;

        // ───────────────────────── frammenti: i casi che un file in git non conserva (fine riga, BOM) o che sono errori ─────────────────────────

        private static readonly (string Name, string Yaml)[] Snippets =
        {
            ("vuoto", ""),
            ("solo spazi", "   \n\n"),
            ("solo commento", "# niente\n"),
            ("solo separatore", "---\n"),
            ("scalare nudo", "ciao\n"),
            ("crlf", "a: 1\r\nb:\r\n  - x\r\n  - y\r\ntesto: |\r\n  prima\r\n  seconda\r\n"),
            ("bom", "\ufeffa: 1\nb: due\n"),
            ("senza a capo finale", "a: 1\nb: 2"),
            ("tab nel valore", "a: \"uno\tdue\"\nb: uno\tdue\n"),
            ("spazi in coda", "a: 1   \nb: testo   \n"),
            ("più documenti", "a: 1\n---\nb: 2\n...\n---\n- c\n"),
            ("chiave doppia", "a: 1\nb: 2\na: 3\n"),
            ("chiave doppia annidata", "x:\n  a: 1\n  a: 2\n"),
            ("rientro con tab", "a:\n\tb: 1\n"),
            ("virgolette non chiuse", "a: \"aperta\nb: 2\n"),
            ("rientro sbagliato", "a:\n  b: 1\n c: 2\n"),
            ("lista e mappa mescolate", "a:\n  - x\n  b: y\n"),
            ("alias sconosciuto", "a: *manca\n"),
            // niente ancora ricorsiva (a: &x {b: *x}): il modello a nodi diventa un ciclo e chi lo percorre esaurisce lo stack, che non si può catturare
            ("due punti senza spazio", "a:1\nb: 2\n"),
            ("flusso non chiuso", "a: [1, 2\nb: 3\n"),
            ("carattere di controllo", "a: \"uno\\u0007due\"\nb: \"\\x1b[0m\"\n"),
            ("nbsp e separatori", "a: \"uno\u00a0due\"\nb: uno\u00a0due\nc: \"riga\u2028separata\"\n"),
            ("fuori dal piano base", "a: 𝒳 e 🧑‍🚀\nb: \"\\U0001F680\"\n"),
            ("direttiva", "%YAML 1.1\n---\na: 1\n"),
            ("chiave lunghissima", new string('k', 1100) + ": v\n"),
            ("valore multilinea con due punti", "a: |\n  chiave: valore\n  - non lista\nb: >\n  # non commento\n"),
            ("mappa in lista compatta", "- a: 1\n  b: 2\n- c: 3\n"),
            ("null in lista", "- ~\n- \n- null\n- \"\"\n"),
            ("merge", "base: &b {x: 1, y: 2}\nfiglio:\n  <<: *b\n  y: 3\n  z: 4\n"),
        };

        // ───────────────────────── lettura con tipo: numeri, date, booleani, enum, null, chiavi ─────────────────────────

        public enum Colore { Rosso, VerdeScuro, blu }

        public class Tipi
        {
            public int Intero { get; set; }
            public long Lungo { get; set; }
            public uint SenzaSegno { get; set; }
            public byte Ottetto { get; set; }
            public short Corto { get; set; }
            public double Doppio { get; set; }
            public float Singolo { get; set; }
            public decimal Decimale { get; set; }
            public bool Vero { get; set; }
            public char Carattere { get; set; }
            public string Testo { get; set; }
            public DateTime Data { get; set; }
            public DateTimeOffset DataConFuso { get; set; }
            public TimeSpan Durata { get; set; }
            public Guid Identificativo { get; set; }
            public Colore Colore { get; set; }
            public int? InteroONiente { get; set; }
            public bool? VeroONiente { get; set; }
            public DateTime? DataONiente { get; set; }
            public Colore? ColoreONiente { get; set; }
            public List<int> Interi { get; set; }
            public string[] Testi { get; set; }
            public IList<string> ElencoPredefinito { get; set; } = new List<string> { "predefinito" };
            public Dictionary<string, int> Conteggi { get; set; }
            public Dictionary<string, object> Liberi { get; set; }
            public object Qualunque { get; set; }
            public Tipi Figlio { get; set; }
            public Uri Indirizzo { get; set; }
            [YamlMember(Alias = "altro-nome")] public string ConAlias { get; set; }
            [YamlIgnore] public string Ignorato { get; set; } = "non toccato";
            public string SolaLettura => "fisso";
            public string campoPubblico;
        }

        private static readonly (string Name, string Yaml)[] TypedCases =
        {
            ("interi", "intero: 42\nlungo: 9000000000\nsenzaSegno: 7\nottetto: 255\ncorto: -3\n"),
            ("intero con zeri", "intero: 007\n"),
            ("intero esadecimale", "intero: 0x1F\n"),
            ("intero ottale", "intero: 0o14\n"),
            ("intero ottale vecchio", "intero: 014\n"),
            ("intero con separatore", "intero: 1_000\n"),
            ("intero con più", "intero: +39\n"),
            ("intero tra virgolette", "intero: \"42\"\n"),
            ("intero decimale", "intero: 4.0\n"),
            ("intero troppo grande", "intero: 99999999999\n"),
            ("intero vuoto", "intero: \n"),
            ("intero null", "intero: null\n"),
            ("intero testo", "intero: quarantadue\n"),
            ("decimali", "doppio: 1.5\nsingolo: 0.1\ndecimale: 1234567.891\n"),
            ("decimali con virgola", "doppio: 1,5\n"),
            ("decimali esponente", "doppio: 1e3\ndecimale: 1e3\n"),
            ("decimali speciali", "doppio: .inf\nsingolo: -.inf\n"),
            ("decimale nan", "doppio: .nan\n"),
            ("decimale senza zero", "doppio: .5\n"),
            ("decimale molte cifre", "doppio: 0.1234567890123456789\ndecimale: 0.1234567890123456789012345678\n"),
            ("booleano true", "vero: true\n"), ("booleano True", "vero: True\n"), ("booleano TRUE", "vero: TRUE\n"),
            ("booleano yes", "vero: yes\n"), ("booleano no", "vero: no\n"), ("booleano on", "vero: on\n"), ("booleano off", "vero: off\n"),
            ("booleano y", "vero: y\n"), ("booleano n", "vero: n\n"), ("booleano 1", "vero: 1\n"), ("booleano 0", "vero: 0\n"),
            ("booleano tra virgolette", "vero: \"true\"\n"), ("booleano testo", "vero: forse\n"), ("booleano vuoto", "vero: \n"),
            ("carattere", "carattere: x\n"), ("carattere lungo", "carattere: xy\n"),
            ("testo numero", "testo: 123\n"), ("testo decimale", "testo: 1.10\n"), ("testo booleano", "testo: true\n"), ("testo null", "testo: null\n"),
            ("testo tilde", "testo: ~\n"), ("testo vuoto", "testo: \n"), ("testo virgolette vuote", "testo: \"\"\n"), ("testo data", "testo: 2026-11-21\n"),
            ("data giorno", "data: 2026-11-21\n"), ("data istante utc", "data: 2026-11-21T10:00:00Z\n"), ("data con fuso", "data: 2026-11-21T10:00:00+01:00\n"),
            ("data con spazio", "data: 2026-11-21 10:00:00\n"), ("data frazioni", "data: 2026-11-21T10:00:00.1234567Z\n"), ("data italiana", "data: 21/11/2026\n"),
            ("data americana", "data: 11/21/2026\n"), ("data testo", "data: ieri\n"), ("data con fuso tipo offset", "dataConFuso: 2026-11-21T10:00:00+01:00\n"),
            ("data offset giorno", "dataConFuso: 2026-11-21\n"),
            ("durata", "durata: 01:02:03\n"), ("durata giorni", "durata: 1.02:03:04.5\n"), ("durata numero", "durata: 90\n"),
            ("guid", "identificativo: 3f2504e0-4f89-11d3-9a0c-0305e82c3301\n"), ("guid maiuscolo graffe", "identificativo: \"{3F2504E0-4F89-11D3-9A0C-0305E82C3301}\"\n"), ("guid sbagliato", "identificativo: non-un-guid\n"),
            ("enum esatto", "colore: VerdeScuro\n"), ("enum minuscolo", "colore: verdescuro\n"), ("enum camel", "colore: verdeScuro\n"), ("enum snake", "colore: verde_scuro\n"),
            ("enum numero", "colore: 1\n"), ("enum sconosciuto", "colore: giallo\n"), ("enum già minuscolo", "colore: blu\n"), ("enum Blu", "colore: Blu\n"),
            ("nullabili assenti", "intero: 1\n"), ("nullabili null", "interoONiente: null\nveroONiente: ~\ndataONiente: \ncoloreONiente: null\n"),
            ("nullabili pieni", "interoONiente: 5\nveroONiente: false\ndataONiente: 2026-11-21\ncoloreONiente: blu\n"),
            ("liste", "interi: [1, 2, 3]\ntesti: [a, b]\nelencoPredefinito: [uno, due]\n"), ("lista scalare", "interi: 5\n"), ("lista vuota", "interi: []\ntesti: []\nelencoPredefinito: []\n"),
            ("lista null", "interi: null\nelencoPredefinito: null\n"), ("lista con null", "testi: [a, ~, \"\", null]\n"), ("lista interi sbagliati", "interi: [1, due]\n"),
            ("dizionari", "conteggi: {a: 1, b: 2}\nliberi: {n: 1, t: testo, b: true, d: 1.5, l: [1, a], m: {k: v}, z: ~}\n"),
            ("dizionario chiavi doppie", "conteggi: {a: 1, a: 2}\n"),
            ("qualunque scalari", "qualunque: 42\n"), ("qualunque testo", "qualunque: \"42\"\n"), ("qualunque lista", "qualunque: [1, true, 1.5, a, ~]\n"), ("qualunque mappa", "qualunque: {a: 1, 2: b}\n"),
            ("figlio", "figlio:\n  intero: 1\n  figlio:\n    testo: nipote\n"), ("figlio null", "figlio: ~\n"), ("figlio scalare", "figlio: testo\n"),
            ("indirizzo", "indirizzo: https://example.com/a b?c=d#e\n"),
            ("alias", "altro-nome: valore\n"), ("alias col nome vero", "conAlias: valore\n"),
            ("ignorato", "ignorato: valore\n"), ("sola lettura", "solaLettura: valore\n"), ("campo pubblico", "campoPubblico: valore\n"),
            ("chiave Pascal", "Intero: 1\n"), ("chiave snake", "intero_o_niente: 1\n"), ("chiave maiuscola", "INTERO: 1\n"), ("chiave sconosciuta", "inventata: 1\nintero: 2\n"),
            ("chiave doppia", "intero: 1\nintero: 2\n"), ("chiave doppia con maiuscole", "intero: 1\nIntero: 2\n"),
            ("radice lista", "- a\n- b\n"), ("radice scalare", "ciao\n"), ("radice vuota", ""), ("radice null", "~\n"),
            ("ancora e alias", "figlio: &f {intero: 3}\nqualunque: *f\n"), ("merge", "figlio: &f {intero: 3, testo: a}\nliberi:\n  <<: *f\n  testo: b\n"),
        };

        private static void DescribeTyped(StringBuilder sb)
        {
            var builders = new (string Name, Func<IDeserializer> Make)[]
            {
                ("camelCase", () => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build()),
                ("camelCase + ignora le sconosciute", () => new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).IgnoreUnmatchedProperties().Build()),
                ("snake_case + ignora le sconosciute", () => new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).IgnoreUnmatchedProperties().Build()),
                ("predefinito", () => new DeserializerBuilder().Build()),
            };
            foreach (var (name, yaml) in TypedCases)
            {
                sb.Append($"### tipi/{name}\n");
                foreach (var (bn, make) in builders)
                    Section(sb, bn, () => Dump(make().Deserialize<Tipi>(yaml)));
            }
        }

        // ───────────────────────── scrittura da oggetti: che cosa finisce tra virgolette, come va a capo, i null ─────────────────────────

        private static void DescribeWrites(StringBuilder sb)
        {
            var strings = new[]
            {
                "", " ", "ciao", "due parole", " in testa", "in coda ", "true", "True", "false", "yes", "no", "on", "off", "y", "n", "null", "Null", "~",
                "123", "007", "1.5", "1e3", "0x1F", ".inf", ".nan", "+39", "-", "- voce", "a: b", "a:b", "a :b", "a #b", "a#b", "#b", "@a", "`a`", "!a", "*a", "&a", "%a",
                "{a}", "[a]", "a, b", "? a", "| a", "> a", "=", "'", "\"", "a'b", "a\"b", "\\", "a\\b", "C:\\Users\\mario.rossi", "\\\\server.example.com\\a",
                "2026-11-21", "2026-11-21T10:00:00Z", "10:30", "1.10", "3.15.212", "http://example.com:8080/a?b=c#d", "mario.rossi@example.com",
                "perché così è più giù", "🚀 lancio", "文档", "uno\ndue", "uno\ndue\n", "uno\n\ndue", "\nuno", "uno\r\ndue", "uno\tdue", "\tuno", "uno\u00a0due", "uno\u0007due", "uno\u2028due",
                "  rientrato\nseconda", "riga con spazio in coda \nseconda", "---", "...", "--- a", "a\n---\nb",
                "Questa è una riga molto lunga che supera abbondantemente le ottanta colonne per vedere se chi scrive la spezza oppure la lascia intera, con parole normali e nessun a capo.",
                new string('a', 200),
            };
            var graph = new Dictionary<string, object>
            {
                ["intero"] = 42, ["lungo"] = 9000000000L, ["doppio"] = 1.5, ["doppioIntero"] = 2.0, ["doppioPiccolo"] = 1e-7, ["doppioGrande"] = 1e21, ["singolo"] = 0.1f,
                ["decimale"] = 1234567.891m, ["nan"] = double.NaN, ["inf"] = double.PositiveInfinity, ["ninf"] = double.NegativeInfinity,
                ["vero"] = true, ["falso"] = false, ["carattere"] = 'x', ["null"] = null,
                ["dataUtc"] = new DateTime(2026, 11, 21, 10, 0, 0, DateTimeKind.Utc), ["dataLocaleSenzaTipo"] = new DateTime(2026, 11, 21, 10, 0, 0, DateTimeKind.Unspecified),
                ["dataGiorno"] = new DateTime(2026, 11, 21), ["dataFrazioni"] = new DateTime(2026, 11, 21, 10, 0, 0, DateTimeKind.Utc).AddTicks(1234567),
                ["dataConFuso"] = new DateTimeOffset(2026, 11, 21, 10, 0, 0, TimeSpan.FromHours(1)), ["durata"] = new TimeSpan(1, 2, 3, 4, 5),
                ["guid"] = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"), ["enum"] = Colore.VerdeScuro, ["indirizzo"] = new Uri("https://example.com/a?b=c"),
                ["listaVuota"] = new List<string>(), ["mappaVuota"] = new Dictionary<string, object>(), ["vettore"] = new[] { 1, 2, 3 },
                ["listaDiMappe"] = new List<object> { new Dictionary<string, object> { ["a"] = 1, ["b"] = new List<object> { "x", null } }, new Dictionary<string, object>() },
                ["Chiave Con Spazi"] = 1, ["chiave_snake"] = 2, ["ChiavePascal"] = 3, ["chiave-kebab"] = 4, ["123"] = 5, ["true"] = 6, [""] = 7, ["a: b"] = 8,
            };
            var typed = new Tipi
            {
                Intero = 1, Lungo = 2, Doppio = 3.25, Decimale = 4.5m, Vero = true, Carattere = 'c', Testo = "testo", Data = new DateTime(2026, 11, 21, 10, 0, 0, DateTimeKind.Utc),
                DataConFuso = new DateTimeOffset(2026, 11, 21, 10, 0, 0, TimeSpan.FromHours(1)), Durata = TimeSpan.FromMinutes(90), Identificativo = Guid.Parse("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                Colore = Colore.VerdeScuro, Interi = new List<int> { 1, 2 }, Testi = new[] { "a", "b" }, Conteggi = new Dictionary<string, int> { ["a"] = 1 },
                Liberi = new Dictionary<string, object> { ["n"] = 1, ["z"] = null }, Qualunque = "qualunque", Figlio = new Tipi { Testo = "figlio" }, Indirizzo = new Uri("https://example.com/"),
                ConAlias = "alias", campoPubblico = "campo",
            };
            var card = new AgentCardDescriptor
            {
                Name = "redattore-acme", Role = "Redattore: verbali", Summary = "Legge gli appunti e scrive il verbale.\nSe manca un nome, chiede.", MaxHops = 3,
                AcceptsMessagesFrom = new List<string> { "user", "coordinatore-acme" }, OnApprovalNotify = new List<string> { "mario.rossi" },
                Skills = new List<AgentCardSkill> { new AgentCardSkill { Id = "scrivi", Description = "Scrive il verbale", Replies = new List<string> { "fatto" } } },
                Replies = new List<AgentCardReply> { new AgentCardReply { Id = "fatto", Label = "Fatto", Message = "Verbale scritto." } },
            };
            var serializers = new (string Name, Func<ISerializer> Make)[]
            {
                ("predefinito", () => new SerializerBuilder().Build()),
                ("camelCase", () => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build()),
                ("camelCase senza null", () => new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build()),
                ("snake_case", () => new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build()),
            };
            foreach (var (sn, make) in serializers)
            {
                sb.Append($"### scritture/{sn}\n");
                Section(sb, "testi (uno per voce di lista)", () => make().Serialize(strings));
                Section(sb, "testi come valori", () => make().Serialize(strings.Select((s, i) => (s, i)).ToDictionary(x => "k" + x.i, x => x.s)));
                Section(sb, "testi come chiavi", () => make().Serialize(strings.Where(s => s.Length < 60).Distinct().ToDictionary(s => s, s => 1)));
                Section(sb, "grafo", () => make().Serialize(graph));
                Section(sb, "oggetto con tipo", () => make().Serialize(typed));
                Section(sb, "oggetto vuoto", () => make().Serialize(new Tipi()));
                Section(sb, "null", () => make().Serialize(null));
                Section(sb, "scheda agente", () => make().Serialize(card));
                Section(sb, "development vuoto", () => make().Serialize(new DevelopmentConfig()));
                // letto ciò che si è scritto: i testi devono tornare uguali
                Section(sb, "testi riletti uguali", () =>
                {
                    var back = new DeserializerBuilder().Build().Deserialize<List<string>>(make().Serialize(strings));
                    var wrong = strings.Select((s, i) => (s, i)).Where(x => back.Count <= x.i || back[x.i] != x.s).Select(x => $"{x.i}:«{x.s}»→«{(back.Count > x.i ? back[x.i] ?? "(null)" : "(manca)")}»").ToList();
                    return wrong.Count == 0 ? "tutti" : string.Join("\n", wrong);
                });
            }
            sb.Append("### scritture/mde\n");
            Section(sb, "impronta della fiducia: scheda di prova", () => AgentTrustHasher.ComputeHash(card, new[] { "read", "write" }));
            Section(sb, "impronta della fiducia: scheda vuota", () => AgentTrustHasher.ComputeHash(new AgentCardDescriptor(), null));
            Section(sb, "impronta della fiducia: senza scheda", () => AgentTrustHasher.ComputeHash(null, new[] { "read" }));
            Section(sb, "development da zero", () => DevelopmentConfigWriter.Compose(null, new DevelopmentConfig()));
            Section(sb, "development su file vuoto", () => DevelopmentConfigWriter.Compose("", new DevelopmentConfig()));
            Section(sb, "intestazione di documento vuota", () => new YamlDocumentDescriptorParser().SerializeDescriptor(new MdExplorerDocumentDescriptor()));
        }
    }
}

using MdExplorer.Features.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// L'estrattore dei linguaggi definiti nel progetto (<see cref="DeclarativeLanguage"/>): per i linguaggi a righe, con
    /// blocchi aperti e chiusi da parole chiave (i BASIC aziendali, per esempio). Produce gli stessi script AST
    /// degli altri estrattori.
    /// <para>
    /// Le righe <b>logiche</b>: la continuazione unisce le righe fisiche, il commento di riga si toglie, e nel testo «code»
    /// il contenuto delle stringhe diventa spazi (alle stesse posizioni), così una parola chiave dentro una stringa non apre
    /// un blocco. Il testo «raw» tiene le stringhe: serve ai riferimenti che stanno dentro le virgolette (include, callback).
    /// </para>
    /// <para>
    /// Ogni file è un <c>module</c> (nome qualificato = il percorso logico, estensione compresa: <c>X.toy</c> e <c>X.toh</c>
    /// sono due moduli). I blocchi formano una pila; un'etichetta vale fino alla successiva dello stesso blocco o alla fine
    /// del blocco, e con <c>onlyIfReferenced</c> diventa un simbolo solo se un riferimento <b>dello stesso file</b> la
    /// raggiunge (un salto non è una chiamata). Il codice appartiene al simbolo più interno che lo contiene.
    /// </para>
    /// <para>
    /// La risoluzione dice la confidenza: nel blocco o nel file un solo candidato è <c>extracted</c>; nel perimetro il
    /// candidato dello stesso file vince, uno solo altrove è <c>inferred</c>, fino a
    /// <see cref="MaxAmbiguousCandidates"/> sono <c>ambiguous</c>, oltre un solo nodo esterno <c>?.nome</c>.
    /// </para>
    /// <para>
    /// Frammenti: un blocco di <c>fragments.in</c> più lungo di quanto l'LLM legge in una volta si spezza a ogni
    /// etichetta (<c>fragments.at</c>, e le etichette di <c>labels</c>); ogni pezzo è un simbolo <c>fragment</c> che va fino
    /// al successivo, un'etichetta già dichiarata diventa il suo frammento. I salti (<c>fragments.jump</c>) e il codice che
    /// prosegue nel pezzo dopo diventano archi <c>jumps</c>. Nei blocchi più corti niente cambia.
    /// </para>
    /// </summary>
    public sealed class DeclarativeLanguageExtractor : ICodeExtractor
    {
        /// <summary>La versione del motore: cambia quando cambia il modo di leggere le regole, non le regole.</summary>
        public const string EngineVersion = "2";
        public const int MaxAmbiguousCandidates = 5;

        private readonly DeclarativeLanguage _lang;
        private readonly int _fragmentAbove;

        /// <param name="fragmentAbove">Oltre quanti caratteri un blocco si spezza in frammenti: quanto sorgente porta una domanda all'LLM (i test lo abbassano).</param>
        public DeclarativeLanguageExtractor(DeclarativeLanguage language, int fragmentAbove = CodeDocPlanner.MaxSourceChars)
        {
            _lang = language ?? throw new ArgumentNullException(nameof(language));
            if (fragmentAbove < 1) throw new ArgumentOutOfRangeException(nameof(fragmentAbove));
            _fragmentAbove = fragmentAbove;
        }

        public DeclarativeLanguage Language => _lang;
        public string LanguageId => _lang.Id;
        public string ExtractorName => "declarative";
        /// <summary>Motore + versione dichiarata + impronta del file: una regola cambiata riscrive gli script anche se nessuno alza <c>version</c>.</summary>
        public string ExtractorVersion => $"{EngineVersion}-{_lang.Version}.{_lang.DefinitionHash}";
        public string Availability => "ready";

        public CodeExtractionResult Extract(CodeExtractionRequest request, CancellationToken ct = default)
        {
            var result = new CodeExtractionResult();
            if (request.FilesToEmit.Count == 0) return result;

            var models = new List<FileModel>();
            foreach (var f in request.AllFiles)
            {
                ct.ThrowIfCancellationRequested();
                string text;
                try { text = CodeSourceText.Read(f.AbsolutePath, request.ExtensionEncodings); }
                catch (Exception ex) { result.Warnings.Add(UiText.Say($"{f.LogicalPath}: non leggibile ({ex.Message})", $"{f.LogicalPath}: cannot be read ({ex.Message})")); continue; }
                try { models.Add(new StructureReader(_lang, f, text, _fragmentAbove).Read()); }
                catch (RegexMatchTimeoutException ex)
                {
                    result.Warnings.Add(UiText.Say($"{f.LogicalPath}: una regola di {_lang.DefinitionPath} impiega troppo ({ex.Pattern}); file saltato", $"{f.LogicalPath}: a rule of {_lang.DefinitionPath} takes too long ({ex.Pattern}); file skipped"));
                }
            }
            var index = new PerimeterIndex(_lang, models);

            var toEmit = new HashSet<string>(request.FilesToEmit.Select(f => f.LogicalPath), StringComparer.Ordinal);
            foreach (var model in models)
            {
                ct.ThrowIfCancellationRequested();
                if (!toEmit.Contains(model.File.LogicalPath)) continue;
                try { result.Scripts.Add((model.File, new FileEmitter(this, model, index).Emit(CodeSourceText.Hash(model.Text)))); }
                catch (RegexMatchTimeoutException ex)
                {
                    result.Warnings.Add(UiText.Say($"{model.File.LogicalPath}: una regola di {_lang.DefinitionPath} impiega troppo ({ex.Pattern}); file saltato", $"{model.File.LogicalPath}: a rule of {_lang.DefinitionPath} takes too long ({ex.Pattern}); file skipped"));
                }
                // dopo l'emissione: i salti verso etichette che non ci sono li trova lei
                result.Warnings.AddRange(model.Warnings);
            }
            return result;
        }

        private string StableId(string kind, string qualifiedName) => RoslynCSharpExtractor.StableId(_lang.Id, kind, qualifiedName);

        // ------------------------------------------------------------------ righe logiche

        internal sealed record LogicalLine(int Start, int End, string Code, string Raw);

        /// <summary>Le righe logiche di un testo: continuazione unita, commenti tolti, stringhe svuotate nel testo «code».</summary>
        internal static List<LogicalLine> LogicalLines(DeclarativeLanguage lang, string text)
        {
            var lines = new List<LogicalLine>();
            var physical = text.Split('\n');
            StringBuilder code = null, raw = null;
            var start = 0;
            for (var i = 0; i < physical.Length; i++)
            {
                var (c, r) = Strip(lang, physical[i].TrimEnd('\r'));
                if (code == null) { code = new StringBuilder(); raw = new StringBuilder(); start = i + 1; }
                var cont = lang.Continuation?.Match(c);
                if (cont is { Success: true } && i < physical.Length - 1)
                {
                    code.Append(c, 0, cont.Index).Append(' ');
                    raw.Append(r, 0, cont.Index).Append(' ');
                    continue;
                }
                code.Append(c); raw.Append(r);
                lines.Add(new LogicalLine(start, i + 1, code.ToString(), raw.ToString()));
                code = null; raw = null;
            }
            return lines;
        }

        /// <summary>Una riga fisica: senza il commento (che dentro una stringa non conta) e con le stringhe svuotate nel «code».</summary>
        private static (string Code, string Raw) Strip(DeclarativeLanguage lang, string line)
        {
            var delimiter = lang.StringDelimiter;
            var comment = lang.LineComment;
            var inString = false;
            var cut = line.Length;
            var code = new StringBuilder(line.Length);
            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (delimiter.HasValue && ch == delimiter.Value) { inString = !inString; code.Append(ch); continue; }
                if (!inString && comment != null && string.CompareOrdinal(line, i, comment, 0, comment.Length) == 0) { cut = i; break; }
                code.Append(inString ? ' ' : ch);
            }
            return (code.ToString(), line[..cut]);
        }

        // ------------------------------------------------------------------ struttura

        private sealed class Sym
        {
            public string RuleId;
            public string Kind;
            /// <summary>Il valore del gruppo <c>name</c>: è quello che un riferimento cerca (<c>EXIT</c>, <c>10</c>).</summary>
            public string Key;
            public string Name;
            public string QualifiedName;
            public string Id;
            public int Start;
            public int End;
            public Sym Parent;
            public string Signature;
            public string ReturnType;
            public List<ScriptParameter> Parameters = new();
            public bool OnlyIfReferenced;
            /// <summary>Il nome del programma (<c>module.name</c>) se il file lo dichiara: è quello che <c>resolve: module</c> cerca.</summary>
            public string ProgramName;
            public FileModel File;
            /// <summary>Le etichette sono aperte finché ne comincia un'altra nello stesso blocco o il blocco finisce.</summary>
            public Sym OpenLabel;
            /// <summary>Un frammento: l'etichetta come la scrive un salto (<c>@EXIT</c>, <c>900</c>), il gruppo <c>name</c> di <c>fragments.at</c>.</summary>
            public string FragmentKey;
            /// <summary>Un blocco spezzato: i frammenti delle etichette in ordine di riga (anche nessuno); null se non è spezzato.</summary>
            public List<Sym> Fragments;
            /// <summary>Un blocco spezzato: i frammenti dei rami di una selezione, a qualunque profondità.</summary>
            public List<Sym> Branches;
            /// <summary>Un ramo: il pezzo che contiene la sua selezione, da cui il codice ci arriva.</summary>
            public Sym DispatchedBy;
            /// <summary>Un frammento: l'indice della sua prima riga logica.</summary>
            public int StartIndex;
        }

        private sealed class FileModel
        {
            public CodeSourceFile File;
            public string Text;
            public Sym Module;
            public List<LogicalLine> Lines;
            public List<Sym> Symbols = new();
            /// <summary>Le dichiarazioni con <c>emit: false</c>: solo nell'indice, mai nello script.</summary>
            public List<Sym> Hidden = new();
            /// <summary>Per ogni riga logica: il simbolo più interno (anche un'etichetta) e il blocco più interno.</summary>
            public Sym[] Owner;
            public Sym[] Block;
            public List<string> Warnings = new();
        }

        private sealed class StructureReader
        {
            private readonly DeclarativeLanguage _lang;
            private readonly FileModel _m;
            private readonly HashSet<string> _qualified;
            private readonly int _fragmentAbove;

            public StructureReader(DeclarativeLanguage lang, CodeSourceFile file, string text, int fragmentAbove)
            {
                _lang = lang;
                _m = new FileModel { File = file, Text = text, Lines = LogicalLines(lang, text) };
                _qualified = new HashSet<string>(lang.NameComparer);
                _fragmentAbove = fragmentAbove;
            }

            public FileModel Read()
            {
                var lines = _m.Lines;
                var physicalCount = lines.Count == 0 ? 1 : lines[^1].End;
                string programName = null;
                if (_lang.ModuleName != null)
                    foreach (var l in lines)
                    {
                        var pm = _lang.ModuleName.Match(l.Raw);
                        if (pm.Success) { programName = pm.Groups["name"].Value.Trim(); break; }
                    }
                var module = new Sym
                {
                    RuleId = DeclarativeLanguage.ModuleRule, Kind = "module", Key = _m.File.LogicalPath,
                    Name = string.IsNullOrEmpty(programName) ? Path.GetFileName(_m.File.LogicalPath) : programName,
                    QualifiedName = _m.File.LogicalPath, Start = 1, End = physicalCount, Signature = _m.File.LogicalPath, File = _m,
                    ProgramName = string.IsNullOrEmpty(programName) ? null : programName,
                };
                _m.Module = module;
                _m.Owner = new Sym[lines.Count];
                _m.Block = new Sym[lines.Count];

                var stack = new List<Sym> { module };
                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    var top = stack[^1];
                    if (top != module && BlockRule(top).End.IsMatch(line.Code))
                    {
                        CloseLabel(top, line.Start - 1);
                        top.End = line.End;
                        stack.RemoveAt(stack.Count - 1);
                        _m.Owner[i] = top; _m.Block[i] = top;
                        continue;
                    }
                    var opened = OpenBlock(top, line);
                    if (opened != null)
                    {
                        CloseLabel(top, line.Start - 1);
                        stack.Add(opened);
                        _m.Owner[i] = opened; _m.Block[i] = opened;
                        continue;
                    }
                    var declared = DeclareOnLine(top, line);
                    if (declared != null) { _m.Owner[i] = declared; _m.Block[i] = top; continue; }
                    OpenLabel(top, line);
                    _m.Owner[i] = top.OpenLabel ?? top;
                    _m.Block[i] = top;
                }
                for (var k = stack.Count - 1; k >= 1; k--)
                {
                    var open = stack[k];
                    CloseLabel(open, physicalCount);
                    open.End = physicalCount;
                    _m.Warnings.Add(UiText.Say($"{_m.File.LogicalPath}: il blocco '{open.RuleId}' {open.Name} aperto alla riga {open.Start} non si chiude", $"{_m.File.LogicalPath}: the '{open.RuleId}' block {open.Name} opened at line {open.Start} is never closed"));
                }
                CloseLabel(module, physicalCount);
                if (_lang.Fragments != null)
                {
                    var physical = _m.Text.Split('\n');
                    foreach (var block in _m.Symbols.Where(s => _lang.Fragments.In.Contains(s.RuleId)).ToList())
                        if (Chars(physical, block) > _fragmentAbove) Fragment(block, physical);
                }
                return _m;
            }

            private static long Chars(string[] physical, Sym s)
            {
                long n = 0;
                for (var l = Math.Max(1, s.Start); l <= Math.Min(physical.Length, s.End); l++) n += physical[l - 1].Length + 1;
                return n;
            }

            /// <summary>
            /// Spezza <paramref name="block"/> a ogni etichetta delle sue righe dirette (non quelle dei blocchi annidati): un
            /// pezzo per etichetta, fino alla successiva; il codice prima della prima resta del blocco. Le righe passano al
            /// loro frammento, così le chiamate partono da lì. Poi ogni pezzo ancora troppo lungo si spezza ai rami
            /// (<see cref="SplitBranches"/>): si spezza solo quanto serve.
            /// </summary>
            private void Fragment(Sym block, string[] physical)
            {
                var rule = _lang.Fragments;
                var lines = _m.Lines;
                var fragments = new List<Sym>();
                var labels = _m.Symbols.Where(s => s.Parent == block && _lang.Labels.Any(l => l.Id == s.RuleId))
                    .GroupBy(s => s.Start).ToDictionary(g => g.Key, g => g.First());
                for (var i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    if (_m.Block[i] != block || line.Start == block.Start || line.End == block.End) continue;
                    var m = rule.At.Match(line.Code);
                    labels.TryGetValue(line.Start, out var label);
                    if (!m.Success && label == null) continue;
                    var frag = label ?? Declare(DeclarativeLanguage.FragmentsRule, DeclarativeLanguage.FragmentKind, m.Groups["name"].Value.Trim(), m, line, block);
                    frag.Kind = DeclarativeLanguage.FragmentKind;
                    frag.OnlyIfReferenced = false;
                    frag.FragmentKey = m.Success ? m.Groups["name"].Value.Trim() : label.Name;
                    frag.Signature = Collapse(line.Raw);
                    frag.StartIndex = i;
                    fragments.Add(frag);
                }
                for (var k = 0; k < fragments.Count; k++)
                    fragments[k].End = Math.Max(fragments[k].Start, k + 1 < fragments.Count ? fragments[k + 1].Start - 1 : block.End - 1);
                block.Fragments = fragments;
                block.Branches = new List<Sym>();

                var current = -1;
                for (var i = 0; i < lines.Count && fragments.Count > 0; i++)
                {
                    if (_m.Block[i] != block || lines[i].End == block.End) continue;
                    while (current + 1 < fragments.Count && lines[i].Start >= fragments[current + 1].Start) current++;
                    if (current < 0) continue;
                    var owner = _m.Owner[i];
                    if (owner == block || owner.Parent == block && _lang.Labels.Any(l => l.Id == owner.RuleId)) _m.Owner[i] = fragments[current];
                }
                if (rule.Branches != null)
                    foreach (var piece in fragments.Prepend(block).ToList()) SplitBranches(block, piece, physical);
            }

            /// <summary>I caratteri delle righe che sono proprio di <paramref name="piece"/> (non dei suoi frammenti).</summary>
            private long OwnChars(Sym block, Sym piece, string[] physical)
            {
                long n = 0;
                for (var i = 0; i < _m.Lines.Count; i++)
                    if (_m.Block[i] == block && _m.Owner[i] == piece)
                        for (var l = _m.Lines[i].Start; l <= Math.Min(physical.Length, _m.Lines[i].End); l++) n += physical[l - 1].Length + 1;
                return n;
            }

            /// <summary>
            /// Un pezzo ancora più lungo del limite (codice strutturato senza etichette: una selezione con un ramo per
            /// azione) si spezza ai rami delle sue selezioni più esterne: un frammento per ramo, figlio del
            /// pezzo, fino al ramo dopo o alla chiusura (che resta del pezzo, come il codice che segue). Poi lo stesso sui rami.
            /// </summary>
            private void SplitBranches(Sym block, Sym piece, string[] physical)
            {
                if (OwnChars(block, piece, physical) <= _fragmentAbove) return;
                var rule = _lang.Fragments.Branches;
                var lines = _m.Lines;
                var depth = 0;
                Sym current = null;
                var made = new List<Sym>();
                for (var i = 0; i < lines.Count; i++)
                {
                    if (_m.Block[i] != block || _m.Owner[i] != piece) continue;
                    var code = lines[i].Code;
                    if (depth == 1 && rule.Close.IsMatch(code))
                    {
                        if (current != null) current.End = Math.Max(current.Start, lines[i].Start - 1);
                        current = null;
                        depth = 0;
                        continue;
                    }
                    if (depth == 1 && rule.Case.IsMatch(code))
                    {
                        if (current != null) current.End = Math.Max(current.Start, lines[i].Start - 1);
                        var name = Collapse(lines[i].Raw);
                        current = Declare(DeclarativeLanguage.FragmentsRule, DeclarativeLanguage.FragmentKind, name.Length > 40 ? name[..40] + "…" : name, rule.Case.Match(code), lines[i], piece);
                        current.Signature = name;
                        current.DispatchedBy = piece;
                        current.StartIndex = i;
                        made.Add(current);
                        block.Branches.Add(current);
                        _m.Owner[i] = current;
                        continue;
                    }
                    if (rule.Open.IsMatch(code)) depth++;
                    else if (depth > 1 && rule.Close.IsMatch(code)) depth--;
                    if (current != null) _m.Owner[i] = current;
                }
                if (current != null) current.End = Math.Max(current.Start, piece.End);
                foreach (var b in made) SplitBranches(block, b, physical);
            }

            private DeclarativeLanguage.BlockRule BlockRule(Sym s) => _lang.Blocks.First(b => b.Id == s.RuleId);

            private Sym OpenBlock(Sym parent, LogicalLine line)
            {
                foreach (var rule in _lang.Blocks)
                {
                    if (!rule.In.Contains(parent.RuleId)) continue;
                    var m = rule.Begin.Match(line.Code);
                    if (!m.Success) continue;
                    var sym = Declare(rule.Id, rule.Kind, Template(rule.NameTemplate, m, line), m, line, parent);
                    sym.Signature = rule.SignatureTemplate != null ? Template(rule.SignatureTemplate, m, line) : Collapse(line.Raw);
                    sym.ReturnType = TypeOf(m);
                    var paramsGroup = m.Groups["params"];
                    if (paramsGroup.Success && _lang.Parameters != null) ReadParameters(sym, line.Raw.Substring(paramsGroup.Index, paramsGroup.Length));
                    return sym;
                }
                return null;
            }

            /// <summary>
            /// Le dichiarazioni di una riga. Torna il simbolo che possiede la riga (una macro: le chiamate del suo corpo sono
            /// sue), o l'etichetta/il blocco corrente se la riga dichiara solo variabili, o null se non dichiara niente.
            /// </summary>
            private Sym DeclareOnLine(Sym parent, LogicalLine line)
            {
                foreach (var rule in _lang.Declarations)
                {
                    if (!rule.In.Contains(parent.RuleId)) continue;
                    var m = rule.At.Match(line.Code);
                    if (!m.Success) continue;
                    if (!rule.Emit)
                    {
                        foreach (var name in Names(rule, m, line))
                            _m.Hidden.Add(new Sym { RuleId = rule.Id, Kind = rule.Kind, Key = name, Name = name, QualifiedName = name, Start = line.Start, End = line.End, Parent = parent, File = _m });
                        return parent.OpenLabel ?? parent;
                    }
                    Sym last = null;
                    foreach (var name in Names(rule, m, line))
                    {
                        last = Declare(rule.Id, rule.Kind, rule.NameTemplate.Replace("{name}", name), m, line, parent);
                        last.Key = name;
                        last.Signature = Collapse(line.Raw);
                        last.ReturnType = TypeOf(m);
                        var paramsGroup = m.Groups["params"];
                        if (paramsGroup.Success && _lang.Parameters != null) ReadParameters(last, line.Raw.Substring(paramsGroup.Index, paramsGroup.Length));
                    }
                    return last ?? parent.OpenLabel ?? parent;
                }
                return null;
            }

            private static IEnumerable<string> Names(DeclarativeLanguage.DeclarationRule rule, Match m, LogicalLine line)
            {
                if (rule.Each == null) { yield return m.Groups["name"].Value; yield break; }
                var list = m.Groups["list"];
                foreach (Match each in rule.Each.Matches(line.Code.Substring(list.Index, list.Length)))
                    if (each.Groups["name"].Length > 0) yield return each.Groups["name"].Value;
            }

            private void OpenLabel(Sym parent, LogicalLine line)
            {
                foreach (var rule in _lang.Labels)
                {
                    if (!rule.In.Contains(parent.RuleId)) continue;
                    var m = rule.At.Match(line.Code);
                    if (!m.Success) continue;
                    CloseLabel(parent, line.Start - 1);
                    var sym = Declare(rule.Id, rule.Kind, Template(rule.NameTemplate, m, line), m, line, parent);
                    sym.Signature = Collapse(line.Raw);
                    sym.OnlyIfReferenced = rule.OnlyIfReferenced;
                    parent.OpenLabel = sym;
                    return;
                }
            }

            private static void CloseLabel(Sym parent, int endLine)
            {
                if (parent.OpenLabel == null) return;
                parent.OpenLabel.End = Math.Max(parent.OpenLabel.Start, endLine);
                parent.OpenLabel = null;
            }

            private Sym Declare(string ruleId, string kind, string name, Match m, LogicalLine line, Sym parent)
            {
                var local = parent.RuleId == DeclarativeLanguage.ModuleRule ? name : LocalPath(parent) + "." + name;
                var qualified = _m.Module.QualifiedName + ":" + local;
                if (!_qualified.Add(qualified))
                {
                    // Lo stesso nome due volte nello stesso posto: si tengono entrambi, distinti dalla riga, e lo si dice.
                    _m.Warnings.Add(UiText.Say($"{_m.File.LogicalPath}:{line.Start}: '{name}' è già dichiarato in {(parent == _m.Module ? "questo file" : parent.Name)}", $"{_m.File.LogicalPath}:{line.Start}: '{name}' is already declared in {(parent == _m.Module ? "this file" : parent.Name)}"));
                    qualified += "@" + line.Start;
                    _qualified.Add(qualified);
                }
                var sym = new Sym
                {
                    RuleId = ruleId, Kind = kind, Key = m.Groups["name"].Value, Name = name, QualifiedName = qualified,
                    Start = line.Start, End = line.End, Parent = parent, File = _m,
                };
                _m.Symbols.Add(sym);
                return sym;
            }

            private static string LocalPath(Sym s)
                => s.Parent == null || s.Parent.RuleId == DeclarativeLanguage.ModuleRule ? s.Name : LocalPath(s.Parent) + "." + s.Name;

            /// <summary><c>PART{name}</c> → <c>PART10</c>; i gruppi si prendono dal testo «raw», alle stesse posizioni del «code».</summary>
            private static string Template(string template, Match m, LogicalLine line)
                => Regex.Replace(template, @"\{(\w+)\}", t =>
                {
                    var g = m.Groups[t.Groups[1].Value];
                    return g.Success ? line.Raw.Substring(g.Index, g.Length).Trim() : string.Empty;
                });

            private static string Collapse(string s) => Regex.Replace(s.Trim(), @"\s+", " ");

            /// <summary>Il tipo dai gruppi <c>type</c> (tradotto con <c>types</c>) e <c>length</c>: <c>$</c> + <c>8</c> → <c>string(8)</c>.</summary>
            private string TypeOf(Match m)
            {
                var t = m.Groups["type"];
                if (!t.Success || t.Length == 0) return null;
                var type = _lang.Types.TryGetValue(t.Value, out var mapped) ? mapped : t.Value;
                var len = m.Groups["length"];
                return len.Success && len.Length > 0 ? $"{type}({len.Value.Trim()})" : type;
            }

            private void ReadParameters(Sym sym, string text)
            {
                var rule = _lang.Parameters;
                var position = 0;
                foreach (Match p in rule.Item.Matches(text))
                {
                    var name = p.Groups["name"].Value;
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    var word = p.Groups["direction"];
                    var direction = word.Success && rule.Direction.TryGetValue(word.Value, out var d) ? d
                        : rule.Sticky && sym.Parameters.Count > 0 ? sym.Parameters[^1].Direction : rule.DefaultDirection;
                    sym.Parameters.Add(new ScriptParameter { SymbolId = null, Position = position++, Name = name, ParameterType = TypeOf(p), Direction = direction });
                }
            }
        }

        // ------------------------------------------------------------------ indice del perimetro

        private sealed class PerimeterIndex
        {
            /// <summary>regola → chiave (<c>name</c>) → simboli, in tutti i file.</summary>
            private readonly Dictionary<string, Dictionary<string, List<Sym>>> _byRule = new(StringComparer.Ordinal);
            /// <summary>nome del file (con e senza estensione) → moduli.</summary>
            private readonly Dictionary<string, List<Sym>> _byFileName;
            private readonly Dictionary<string, List<Sym>> _byProgram;

            public PerimeterIndex(DeclarativeLanguage lang, List<FileModel> models)
            {
                _byFileName = new Dictionary<string, List<Sym>>(lang.NameComparer);
                _byProgram = new Dictionary<string, List<Sym>>(lang.NameComparer);
                foreach (var m in models)
                {
                    if (m.Module.ProgramName != null) Add(_byProgram, m.Module.ProgramName, m.Module);
                    var file = Path.GetFileName(m.File.LogicalPath);
                    Add(_byFileName, file, m.Module);
                    var stem = Path.GetFileNameWithoutExtension(file);
                    if (stem != file) Add(_byFileName, stem, m.Module);
                    foreach (var s in m.Symbols.Concat(m.Hidden))
                    {
                        if (!_byRule.TryGetValue(s.RuleId, out var map)) _byRule[s.RuleId] = map = new Dictionary<string, List<Sym>>(lang.NameComparer);
                        Add(map, s.Key, s);
                    }
                }
            }

            public IReadOnlyList<Sym> Symbols(string rule, string key)
                => _byRule.TryGetValue(rule, out var map) && map.TryGetValue(key, out var list) ? list : Array.Empty<Sym>();

            public IReadOnlyList<Sym> Programs(string name) => _byProgram.TryGetValue(name, out var list) ? list : Array.Empty<Sym>();

            public IReadOnlyList<Sym> Files(string name) => _byFileName.TryGetValue(name, out var list) ? list.Distinct().ToList() : Array.Empty<Sym>();

            private static void Add(Dictionary<string, List<Sym>> map, string key, Sym s)
            {
                if (!map.TryGetValue(key, out var list)) map[key] = list = new List<Sym>();
                list.Add(s);
            }
        }

        // ------------------------------------------------------------------ emissione

        private sealed class FileEmitter
        {
            private readonly DeclarativeLanguageExtractor _owner;
            private readonly FileModel _m;
            private readonly PerimeterIndex _index;
            private readonly CodeScript _script = new();
            private readonly HashSet<string> _externals = new(StringComparer.Ordinal);
            private readonly Dictionary<string, ScriptEdge> _edges = new(StringComparer.Ordinal);
            private readonly List<(Sym Source, string Target, string Relation, string Confidence, int Line)> _pending = new();
            private readonly HashSet<Sym> _referenced = new();

            public FileEmitter(DeclarativeLanguageExtractor owner, FileModel model, PerimeterIndex index)
            {
                _owner = owner; _m = model; _index = index;
            }

            private DeclarativeLanguage Lang => _owner._lang;

            public CodeScript Emit(string sourceHash)
            {
                _m.Module.Id = _owner.StableId("module", _m.Module.QualifiedName);
                foreach (var s in _m.Symbols) s.Id = _owner.StableId(s.Kind, s.QualifiedName);

                _script.Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = _m.File.LogicalPath,
                    Language = _owner.LanguageId, SourceHash = sourceHash,
                    Extractor = _owner.ExtractorName, ExtractorVersion = _owner.ExtractorVersion, GeneratedAt = DateTime.UtcNow.ToString("o"),
                };

                CollectReferences();
                var emitted = _m.Symbols.Where(s => !s.OnlyIfReferenced || _referenced.Contains(s)).ToHashSet();
                Sym Visible(Sym s) { while (s != null && s != _m.Module && !emitted.Contains(s)) s = s.Parent; return s ?? _m.Module; }

                AddSymbol(_m.Module, null);
                foreach (var s in _m.Symbols.Where(emitted.Contains))
                {
                    var parent = Visible(s.Parent);
                    AddSymbol(s, parent.Id);
                    Edge(parent.Id, s.Id, "contains", "extracted", s.Start);
                    foreach (var p in s.Parameters) { p.SymbolId = s.Id; _script.Parameters.Add(p); }
                }
                CollectJumps();
                foreach (var (source, target, relation, confidence, line) in _pending)
                    Edge(Visible(source).Id, target, relation, confidence, line);
                return _script;
            }

            private void AddSymbol(Sym s, string parentId)
                => _script.Symbols.Add(new ScriptSymbol
                {
                    Id = s.Id, Kind = s.Kind, Name = s.Name, QualifiedName = s.QualifiedName, LineStart = s.Start, LineEnd = s.End,
                    Signature = s.Signature, ReturnType = s.ReturnType, Visibility = "public", ParentId = parentId,
                });

            private void CollectReferences()
            {
                string previous = null;
                for (var i = 0; i < _m.Lines.Count; i++)
                {
                    var line = _m.Lines[i];
                    if (string.IsNullOrWhiteSpace(line.Code) && string.IsNullOrWhiteSpace(line.Raw)) continue;
                    foreach (var rule in Lang.References)
                    {
                        if (rule.After != null && (previous == null || !rule.After.IsMatch(previous))) continue;
                        if (rule.NotAfter != null && previous != null && rule.NotAfter.IsMatch(previous)) continue;
                        var text = rule.Text == DeclarativeLanguage.TextKind.Code ? line.Code : line.Raw;
                        foreach (Match match in rule.Pattern.Matches(text))
                        {
                            var target = match.Groups["target"].Value;
                            if (rule.Strip != null) target = rule.Strip.Replace(target, string.Empty);
                            target = target.Trim();
                            if (target.Length == 0) continue;
                            foreach (var (id, confidence) in Resolve(rule, target, _m.Block[i]))
                                _pending.Add((_m.Owner[i], id, rule.Relation, Cap(confidence, rule.Confidence), line.Start));
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(line.Code)) previous = line.Code;
                }
            }

            /// <summary>
            /// Gli archi <c>jumps</c> dei blocchi spezzati: ogni salto verso il frammento con quell'etichetta (nello stesso
            /// blocco; un'etichetta che non c'è è un avviso); verso ogni etichetta dal pezzo della riga di codice prima, a meno
            /// che quella chiuda il flusso (<c>fragments.stop</c>); verso ogni ramo dal pezzo che ha la sua selezione. Questi
            /// ultimi due <c>inferred</c>: li dicono le regole, non un salto scritto.
            /// </summary>
            private void CollectJumps()
            {
                var rule = Lang.Fragments;
                if (rule == null) return;
                foreach (var block in _m.Symbols.Where(s => s.Fragments != null))
                {
                    var byKey = new Dictionary<string, Sym>(Lang.NameComparer);
                    foreach (var f in block.Fragments) byKey.TryAdd(f.FragmentKey, f);
                    for (var i = 0; i < _m.Lines.Count; i++)
                    {
                        var line = _m.Lines[i];
                        if (_m.Block[i] != block || line.End == block.End) continue;
                        foreach (Match j in rule.Jump.Matches(line.Code))
                        {
                            var target = j.Groups["target"].Value.Trim();
                            if (byKey.TryGetValue(target, out var to)) _pending.Add((_m.Owner[i], to.Id, DeclarativeLanguage.JumpsRelation, "extracted", line.Start));
                            else _m.Warnings.Add(UiText.Say($"{_m.File.LogicalPath}:{line.Start}: salto verso '{target}', che in {block.Name} non c'è", $"{_m.File.LogicalPath}:{line.Start}: jump to '{target}', which is not in {block.Name}"));
                        }
                    }
                    foreach (var f in block.Fragments)
                        for (var j = f.StartIndex - 1; j >= 0 && _m.Lines[j].Start >= block.Start; j--)
                        {
                            if (_m.Block[j] != block || string.IsNullOrWhiteSpace(_m.Lines[j].Code)) continue;
                            if (rule.Stop == null || !rule.Stop.IsMatch(_m.Lines[j].Code))
                                _pending.Add((_m.Owner[j], f.Id, DeclarativeLanguage.JumpsRelation, "inferred", f.Start));
                            break;
                        }
                    foreach (var b in block.Branches)
                        _pending.Add((b.DispatchedBy, b.Id, DeclarativeLanguage.JumpsRelation, "inferred", b.Start));
                }
            }

            private IEnumerable<(string Id, string Confidence)> Resolve(DeclarativeLanguage.ReferenceRule rule, string target, Sym block)
            {
                if (rule.Ignore.Contains(target)) yield break;
                IReadOnlyList<Sym> found;
                string single;
                if (rule.Resolve == DeclarativeLanguage.ResolveKind.Module)
                {
                    found = _index.Programs(target);
                    single = "extracted";
                }
                else if (rule.Resolve == DeclarativeLanguage.ResolveKind.File)
                {
                    var name = target.Replace('\\', '/');
                    name = name[(name.LastIndexOf('/') + 1)..];
                    found = _index.Files(name);
                    single = "extracted";
                }
                else
                {
                    var all = rule.Rules.SelectMany(r => _index.Symbols(r, target)).ToList();
                    switch (rule.Scope)
                    {
                        case DeclarativeLanguage.ScopeKind.Block:
                            found = all.Where(s => s.Parent == block).ToList(); single = "extracted"; break;
                        case DeclarativeLanguage.ScopeKind.Module:
                            found = all.Where(s => s.File == _m).ToList(); single = "extracted"; break;
                        default:
                            var here = all.Where(s => s.File == _m).ToList();
                            found = here.Count > 0 ? here : all;
                            single = here.Count > 0 ? "extracted" : "inferred";
                            break;
                    }
                }
                if (found.Count == 0)
                {
                    if (rule.Unless.Any(u => _index.Symbols(u, target).Count > 0)) yield break;
                    if (rule.ExternalWhenUnresolved) yield return (External(rule.Resolve == DeclarativeLanguage.ResolveKind.Symbol ? "symbol" : rule.Resolve == DeclarativeLanguage.ResolveKind.File ? "file" : "module", target), rule.Resolve == DeclarativeLanguage.ResolveKind.Symbol ? "inferred" : "extracted");
                    yield break;
                }
                if (found.Count > MaxAmbiguousCandidates)
                {
                    yield return (External("symbol", "?." + target), "ambiguous");
                    yield break;
                }
                foreach (var s in found)
                {
                    if (s.File == _m) _referenced.Add(s);
                    yield return (s.Id ?? _owner.StableId(s.Kind, s.QualifiedName), found.Count == 1 ? single : "ambiguous");
                }
            }

            /// <summary>Con <c>ignoreCase</c> <c>STRLEFT</c> e <c>strleft</c> sono lo stesso nodo: l'id nasce dal nome in maiuscolo, il nome resta il primo visto.</summary>
            private string External(string what, string name)
            {
                var id = _owner.StableId("external", what + ":" + (Lang.IgnoreCase ? name.ToUpperInvariant() : name));
                if (_externals.Add(id)) _script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = name, QualifiedName = name });
                return id;
            }

            private static string Cap(string confidence, string max)
                => max != null && Rank(max) < Rank(confidence) ? max : confidence;

            private void Edge(string source, string target, string relation, string confidence, int? line)
            {
                if (source == null || target == null || source == target) return;
                // Come per gli altri estrattori: un arco per (sorgente, destinazione, relazione); vince la confidenza migliore.
                var key = $"{source}|{target}|{relation}";
                if (_edges.TryGetValue(key, out var existing))
                {
                    if (Rank(confidence) > Rank(existing.Confidence)) { existing.Confidence = confidence; existing.Line = line; }
                    return;
                }
                var edge = new ScriptEdge { SourceId = source, TargetId = target, Relation = relation, Confidence = confidence, Line = line };
                _edges[key] = edge;
                _script.Edges.Add(edge);
            }

            private static int Rank(string confidence) => confidence switch { "extracted" => 2, "inferred" => 1, _ => 0 };
        }
    }
}

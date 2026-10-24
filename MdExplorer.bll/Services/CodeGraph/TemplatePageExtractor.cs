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
    /// L'estrattore delle pagine a direttive (D42 passo 2, 24/10/2026): pagine HTML che il server riempie, con direttive come
    /// <c>$include[x]$</c> o <c>$if[A==S]$…$endif$</c>. Due parti: l'<b>HTML</b> lo legge MdExplorer, uguale per tutti; le
    /// <b>direttive</b> le descrive il dialetto del progetto (<see cref="DeclarativeLanguage.TemplateRule"/>, nel pacchetto
    /// <c>.mde-languages/&lt;id&gt;/</c>, scritto dall'LLM dell'utente). Produce gli stessi script AST degli altri estrattori.
    /// <para>
    /// Che cosa entra: la pagina è un <c>module</c>; un elemento con un evento (<c>onclick=…</c>) o con
    /// <c>href="javascript:…"</c> è un simbolo <c>control</c>; le funzioni degli script incorporati sono <c>function</c>. Il
    /// JS dei controlli e degli script dà i <b>punti di chiamata</b> (quelli dei controlli anche senza stringhe: conta sapere
    /// quale funzione chiamano), e un arco <c>calls</c> verso la funzione della stessa pagina con quel nome; le funzioni di
    /// altri file le collegano le regole (D42). <c>$include[x]$</c> è un arco <c>includes</c> verso l'altra pagina,
    /// <c>&lt;script src&gt;</c> un <c>imports</c> verso il modulo JS. Le condizioni dei blocchi con <c>guard</c> diventano la
    /// <b>guardia</b> dei controlli e degli <c>includes</c> che contengono.
    /// </para>
    /// <para>
    /// Le direttive valgono ovunque (le esegue il server, anche dentro un commento HTML); i controlli dentro un commento
    /// HTML no (non arrivano al browser).
    /// </para>
    /// </summary>
    public sealed class TemplatePageExtractor : ICodeExtractor
    {
        /// <summary>La versione del motore: cambia quando cambia il modo di leggere le pagine, non il dialetto.</summary>
        public const string EngineVersion = "1";
        public const int MaxSignatureLength = 200;

        private readonly DeclarativeLanguage _lang;

        public TemplatePageExtractor(DeclarativeLanguage language)
        {
            _lang = language ?? throw new ArgumentNullException(nameof(language));
            if (language.Template == null) throw new ArgumentException(UiText.Say($"{language.DefinitionPath}: non è un dialetto di pagine (manca template)", $"{language.DefinitionPath}: not a page dialect (template is missing)"), nameof(language));
        }

        public DeclarativeLanguage Language => _lang;
        public string LanguageId => _lang.Id;
        public string ExtractorName => "template";
        /// <summary>Motore + versione dichiarata + impronta del file: un dialetto cambiato riscrive gli script.</summary>
        public string ExtractorVersion => $"{EngineVersion}-{_lang.Version}.{_lang.DefinitionHash}";
        public string Availability => "ready";

        public CodeExtractionResult Extract(CodeExtractionRequest request, CancellationToken ct = default)
        {
            var result = new CodeExtractionResult();
            foreach (var page in Read(request, ct))
            {
                if (page.Script != null) result.Scripts.Add((page.File, page.Script));
                result.Warnings.AddRange(page.Warnings);
            }
            return result;
        }

        /// <summary>Le pagine da emettere con lo script e i numeri di ognuna: li usa anche la prova a secco del dialetto.</summary>
        public IReadOnlyList<TemplatePage> Read(CodeExtractionRequest request, CancellationToken ct = default)
        {
            var pages = new List<TemplatePage>();
            if (request.FilesToEmit.Count == 0) return pages;
            var index = new PageIndex(_lang, request.AllFiles);
            foreach (var f in request.FilesToEmit)
            {
                ct.ThrowIfCancellationRequested();
                var page = new TemplatePage { File = f };
                pages.Add(page);
                string text;
                try { text = CodeSourceText.Read(f.AbsolutePath, request.ExtensionEncodings); }
                catch (Exception ex) { page.Warnings.Add(UiText.Say($"{f.LogicalPath}: non leggibile ({ex.Message})", $"{f.LogicalPath}: cannot be read ({ex.Message})")); continue; }
                try { page.Script = new PageReader(this, f, text, index, page).Read(); }
                catch (RegexMatchTimeoutException ex)
                {
                    page.Warnings.Add(UiText.Say($"{f.LogicalPath}: template.directive di {_lang.DefinitionPath} impiega troppo ({ex.Pattern}); pagina saltata", $"{f.LogicalPath}: template.directive of {_lang.DefinitionPath} takes too long ({ex.Pattern}); page skipped"));
                }
            }
            return pages;
        }

        private string StableId(string kind, string qualifiedName) => RoslynCSharpExtractor.StableId(_lang.Id, kind, qualifiedName);

        // ------------------------------------------------------------------ indice delle pagine

        /// <summary>Le pagine del perimetro per percorso logico, per nome del file e per nome senza estensione.</summary>
        private sealed class PageIndex
        {
            private readonly DeclarativeLanguage _lang;
            private readonly HashSet<string> _paths = new(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, List<string>> _byName = new(StringComparer.OrdinalIgnoreCase);

            public PageIndex(DeclarativeLanguage lang, IEnumerable<CodeSourceFile> files)
            {
                _lang = lang;
                foreach (var f in files)
                {
                    if (!_paths.Add(f.LogicalPath)) continue;
                    var name = Path.GetFileName(f.LogicalPath);
                    Add(name, f.LogicalPath);
                    var stem = Path.GetFileNameWithoutExtension(name);
                    if (stem != name) Add(stem, f.LogicalPath);
                }
            }

            private void Add(string key, string path)
            {
                if (!_byName.TryGetValue(key, out var list)) _byName[key] = list = new List<string>();
                list.Add(path);
            }

            /// <summary>
            /// Le pagine che un <c>include</c> può voler dire, dalla più sicura: il percorso relativo alla pagina, poi un
            /// percorso del perimetro che finisce così, poi il solo nome del file. Con e senza le estensioni del dialetto.
            /// </summary>
            public IReadOnlyList<string> Resolve(string target, string fromLogicalPath)
            {
                var t = target.Replace('\\', '/').Trim();
                var variants = new List<string> { t };
                if (string.IsNullOrEmpty(Path.GetExtension(t))) variants.AddRange(_lang.Extensions.Select(e => t + e));

                var dir = Path.GetDirectoryName(fromLogicalPath)?.Replace('\\', '/') ?? string.Empty;
                var relative = variants.Select(v => Normalize(v.StartsWith('/') ? v.TrimStart('/') : (dir.Length > 0 ? dir + "/" + v : v)))
                    .Where(p => p != null && _paths.Contains(p)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
                if (relative.Count > 0) return relative;

                var suffixes = variants.Select(v => "/" + v.TrimStart('.', '/')).ToList();
                var bySuffix = _paths.Where(p => suffixes.Any(s => ("/" + p).EndsWith(s, StringComparison.OrdinalIgnoreCase))).OrderBy(p => p, StringComparer.Ordinal).ToList();
                if (bySuffix.Count > 0) return bySuffix;

                var name = t[(t.LastIndexOf('/') + 1)..];
                return _byName.TryGetValue(name, out var list) ? list.Distinct(StringComparer.OrdinalIgnoreCase).ToList() : Array.Empty<string>();
            }

            /// <summary><c>a/b/../c/./d</c> → <c>a/c/d</c>; null se esce dalla radice.</summary>
            public static string Normalize(string path)
            {
                var parts = new List<string>();
                foreach (var p in path.Split('/'))
                {
                    if (p.Length == 0 || p == ".") continue;
                    if (p == "..")
                    {
                        if (parts.Count == 0 || parts[^1].StartsWith('@') && parts.Count == 1) return null;
                        parts.RemoveAt(parts.Count - 1);
                        continue;
                    }
                    parts.Add(p);
                }
                return string.Join('/', parts);
            }
        }

        // ------------------------------------------------------------------ lettura di una pagina

        private sealed record Attr(string Name, string Value, int ValueOffset);

        private sealed record Tag(string Name, int Start, int End, List<Attr> Attrs)
        {
            public string Get(string name) => Attrs.FirstOrDefault(a => string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase))?.Value;
        }

        private sealed class InlineFunction
        {
            public string Name;
            public string Id;
            public int Start;
            public int End;
        }

        private sealed class Frame
        {
            public DeclarativeLanguage.TemplateBlock Block;
            public string Arg;
            /// <summary>La condizione del ramo in corso: l'argomento, poi la sua negazione dopo un <c>else</c>, o <c>arg=caso</c>.</summary>
            public string Condition;
            public int Line;
        }

        private sealed class PageReader
        {
            private static readonly HashSet<string> NotCalls = new(StringComparer.Ordinal)
                { "if", "for", "while", "switch", "catch", "function", "return", "typeof", "with", "do", "else", "new", "delete", "void", "in", "of", "instanceof" };

            private readonly TemplatePageExtractor _owner;
            private readonly CodeSourceFile _file;
            private readonly string _text;
            private readonly PageIndex _index;
            private readonly TemplatePage _page;
            private readonly List<int> _lineStarts = new() { 0 };
            private readonly CodeScript _script = new();
            private readonly CodeCallSites _calls;
            private readonly Dictionary<string, ScriptEdge> _edges = new(StringComparer.Ordinal);
            private readonly HashSet<string> _externals = new(StringComparer.Ordinal);
            private readonly HashSet<string> _qualified = new(StringComparer.Ordinal);
            /// <summary>Dove cambia la condizione: (posizione, guardia da lì in poi), in ordine di posizione.</summary>
            private readonly List<(int Offset, string Guard)> _guards = new() { (0, null) };
            private readonly List<(int Offset, string Arg)> _includes = new();
            private readonly List<InlineFunction> _functions = new();
            private string _moduleId;

            public PageReader(TemplatePageExtractor owner, CodeSourceFile file, string text, PageIndex index, TemplatePage page)
            {
                _owner = owner; _file = file; _text = text; _index = index; _page = page;
                for (var i = 0; i < text.Length; i++) if (text[i] == '\n') _lineStarts.Add(i + 1);
                _calls = new CodeCallSites(_script);
            }

            private DeclarativeLanguage.TemplateRule Rule => _owner._lang.Template;
            private StringComparer Words => _owner._lang.NameComparer;

            public CodeScript Read()
            {
                var path = _file.LogicalPath;
                _moduleId = _owner.StableId("module", path);
                _script.Header = new CodeScriptHeader
                {
                    Schema = CodeScriptParser.CurrentSchema, Kind = CodeScriptKind.Ast, Source = path, Language = _owner.LanguageId,
                    SourceHash = CodeSourceText.Hash(_text), Extractor = _owner.ExtractorName, ExtractorVersion = _owner.ExtractorVersion,
                    GeneratedAt = DateTime.UtcNow.ToString("o"),
                };
                _script.Symbols.Add(new ScriptSymbol
                {
                    Id = _moduleId, Kind = "module", Name = Path.GetFileName(path), QualifiedName = path,
                    LineStart = 1, LineEnd = _lineStarts.Count, Signature = path, Visibility = "public",
                });

                ReadDirectives();
                var tags = ReadTags();
                // prima le funzioni di tutti gli script: una chiamata può andare a una funzione scritta più in basso
                var scripts = tags.Where(t => t.Tag.Name == "script").Select(t => Script(t.Tag, t.ContentEnd)).Where(s => s.Start >= 0).ToList();
                foreach (var (start, end, functions) in scripts)
                    JsCalls(start, end, offset => functions.LastOrDefault(f => f.Start <= offset && offset < f.End)?.Id ?? _moduleId, always: false);
                foreach (var (tag, _) in tags.Where(t => t.Tag.Name != "script")) Control(tag);
                foreach (var (offset, arg) in _includes) Include(offset, arg);
                return _script;
            }

            // -------------------------------------------------------------- direttive

            /// <summary>
            /// Le direttive in ordine: gli <c>include</c> si annotano, i blocchi aprono e chiudono la condizione. Una chiusura
            /// senza apertura, o un blocco lasciato aperto, è un avviso: la pagina si legge lo stesso.
            /// </summary>
            private void ReadDirectives()
            {
                var stack = new List<Frame>();
                foreach (Match m in Rule.Directive.Matches(_text))
                {
                    var word = m.Groups["word"].Value;
                    var argGroup = m.Groups["arg"];
                    var arg = argGroup.Success ? argGroup.Value.Trim() : null;
                    if (word.Length == 0) continue;
                    var line = LineOf(m.Index);

                    if (Rule.Include.Contains(word))
                    {
                        Count(_page.Words, word);
                        _includes.Add((m.Index, arg));
                        continue;
                    }
                    var opens = Rule.Blocks.FirstOrDefault(b => Words.Equals(b.Open, word));
                    if (opens != null)
                    {
                        Count(_page.Words, word);
                        stack.Add(new Frame { Block = opens, Arg = arg, Condition = opens.Guard && !string.IsNullOrEmpty(arg) ? arg : null, Line = line });
                        _guards.Add((m.Index + m.Length, Current(stack)));
                        continue;
                    }
                    var closes = Rule.Blocks.FirstOrDefault(b => Words.Equals(b.Close, word));
                    if (closes != null)
                    {
                        Count(_page.Words, word);
                        var at = stack.FindLastIndex(f => f.Block == closes);
                        if (at < 0)
                        {
                            _page.Warnings.Add(UiText.Say($"{_file.LogicalPath}:{line}: '{word}' chiude un '{closes.Open}' che non è aperto", $"{_file.LogicalPath}:{line}: '{word}' closes a '{closes.Open}' that is not open"));
                            continue;
                        }
                        for (var k = stack.Count - 1; k > at; k--) Unclosed(stack[k]);
                        stack.RemoveRange(at, stack.Count - at);
                        _guards.Add((m.Index, Current(stack)));
                        continue;
                    }
                    var branches = Rule.Blocks.FirstOrDefault(b => b.Else.Contains(word, Words) || b.Cases.Contains(word, Words));
                    if (branches != null)
                    {
                        Count(_page.Words, word);
                        var frame = stack.LastOrDefault(f => f.Block == branches);
                        if (frame == null)
                        {
                            _page.Warnings.Add(UiText.Say($"{_file.LogicalPath}:{line}: '{word}' fuori da un '{branches.Open}'", $"{_file.LogicalPath}:{line}: '{word}' outside a '{branches.Open}'"));
                            continue;
                        }
                        frame.Condition = branches.Else.Contains(word, Words)
                            ? (string.IsNullOrEmpty(frame.Arg) ? null : $"!({frame.Arg})")
                            : string.IsNullOrEmpty(arg) ? frame.Arg : string.IsNullOrEmpty(frame.Arg) ? arg : $"{frame.Arg}={arg}";
                        _guards.Add((m.Index + m.Length, Current(stack)));
                        continue;
                    }
                    // Una parola che il dialetto non dichiara: con argomento è quasi certamente una direttiva che manca (la
                    // prova a secco la mostra), senza è un valore che il server scrive nella pagina.
                    if (arg != null)
                    {
                        Count(_page.UnknownWords, word);
                        _page.UnknownWordExamples.TryAdd(word, $"{_file.LogicalPath}:{line}");
                    }
                    else _page.Placeholders++;
                }
                foreach (var f in stack) Unclosed(f);
            }

            private void Unclosed(Frame f)
                => _page.Warnings.Add(UiText.Say($"{_file.LogicalPath}:{f.Line}: '{f.Block.Open}' non si chiude ('{f.Block.Close}')", $"{_file.LogicalPath}:{f.Line}: '{f.Block.Open}' is never closed ('{f.Block.Close}')"));

            private static string Current(List<Frame> stack)
            {
                var conditions = stack.Where(f => f.Condition != null).Select(f => f.Condition).ToList();
                return conditions.Count == 0 ? null : string.Join(" && ", conditions);
            }

            private string GuardAt(int offset)
            {
                int lo = 0, hi = _guards.Count - 1;
                while (lo < hi)
                {
                    var mid = (lo + hi + 1) / 2;
                    if (_guards[mid].Offset <= offset) lo = mid; else hi = mid - 1;
                }
                return _guards[lo].Guard;
            }

            private static void Count(Dictionary<string, int> map, string word) => map[word] = map.TryGetValue(word, out var n) ? n + 1 : 1;

            private bool HasDirective(string s) => s != null && Rule.Directive.IsMatch(s);

            // -------------------------------------------------------------- HTML

            /// <summary>I tag che contano (gli script con la fine del loro contenuto, e gli elementi con attributi), fuori dai commenti.</summary>
            private List<(Tag Tag, int ContentEnd)> ReadTags()
            {
                var tags = new List<(Tag, int)>();
                var i = 0;
                var n = _text.Length;
                while (i < n)
                {
                    if (string.CompareOrdinal(_text, i, "<!--", 0, 4) == 0)
                    {
                        var close = _text.IndexOf("-->", i + 4, StringComparison.Ordinal);
                        i = close < 0 ? n : close + 3;
                        continue;
                    }
                    if (_text[i] != '<' || i + 1 >= n || !char.IsLetter(_text[i + 1])) { i++; continue; }
                    var tag = ReadTag(i);
                    if (tag == null) { i++; continue; }
                    if (tag.Name is "script" or "style")
                    {
                        var close = _text.IndexOf("</" + tag.Name, tag.End, StringComparison.OrdinalIgnoreCase);
                        var contentEnd = close < 0 ? n : close;
                        if (tag.Name == "script") tags.Add((tag, contentEnd));
                        var gt = close < 0 ? -1 : _text.IndexOf('>', close);
                        i = gt < 0 ? n : gt + 1;
                        continue;
                    }
                    if (tag.Attrs.Count > 0) tags.Add((tag, tag.End));
                    i = tag.End;
                }
                return tags;
            }

            /// <summary>Un tag di apertura da <paramref name="start"/> (il <c>&lt;</c>): nome e attributi, o null se non lo è.</summary>
            private Tag ReadTag(int start)
            {
                var n = _text.Length;
                var p = start + 1;
                while (p < n && (char.IsLetterOrDigit(_text[p]) || _text[p] is '-' or ':' or '_')) p++;
                var name = _text[(start + 1)..p].ToLowerInvariant();
                var attrs = new List<Attr>();
                while (p < n)
                {
                    while (p < n && char.IsWhiteSpace(_text[p])) p++;
                    if (p >= n) return null;
                    if (_text[p] == '>') return new Tag(name, start, p + 1, attrs);
                    if (_text[p] == '/' && p + 1 < n && _text[p + 1] == '>') return new Tag(name, start, p + 2, attrs);
                    if (_text[p] == '<') return null; // un tag che non si chiude: il '<' dopo è un altro tag
                    var nameStart = p;
                    while (p < n && !char.IsWhiteSpace(_text[p]) && _text[p] is not ('=' or '>' or '<') && !(_text[p] == '/' && p + 1 < n && _text[p + 1] == '>')) p++;
                    if (p == nameStart) { p++; continue; }
                    var attrName = _text[nameStart..p];
                    var q = p;
                    while (q < n && char.IsWhiteSpace(_text[q])) q++;
                    if (q < n && _text[q] == '=')
                    {
                        q++;
                        while (q < n && char.IsWhiteSpace(_text[q])) q++;
                        if (q >= n) return null;
                        if (_text[q] is '"' or '\'')
                        {
                            var quote = _text[q];
                            var close = _text.IndexOf(quote, q + 1);
                            if (close < 0) return null;
                            attrs.Add(new Attr(attrName, _text[(q + 1)..close], q + 1));
                            p = close + 1;
                        }
                        else
                        {
                            var vs = q;
                            while (q < n && !char.IsWhiteSpace(_text[q]) && _text[q] != '>') q++;
                            attrs.Add(new Attr(attrName, _text[vs..q], vs));
                            p = q;
                        }
                    }
                    else attrs.Add(new Attr(attrName, null, -1));
                }
                return null;
            }

            /// <summary>
            /// <c>&lt;script src&gt;</c> → <c>imports</c>; uno script incorporato → le sue funzioni, e il pezzo di JS di cui
            /// poi si leggono le chiamate (Start = -1: niente da leggere).
            /// </summary>
            private (int Start, int End, List<InlineFunction> Functions) Script(Tag tag, int contentEnd)
            {
                var src = tag.Get("src");
                if (src != null) { ImportScript(src.Trim(), tag.Start); return (-1, -1, null); }
                var type = tag.Get("type");
                if (type != null && !type.Contains("javascript", StringComparison.OrdinalIgnoreCase) && !string.Equals(type.Trim(), "module", StringComparison.OrdinalIgnoreCase)) return (-1, -1, null);
                var functions = InlineFunctions(tag.End, contentEnd);
                _functions.AddRange(functions);
                return (tag.End, contentEnd, functions);
            }

            private static readonly Regex FunctionDeclaration = new(@"\bfunction\s+(?<name>[A-Za-z_$][\w$]*)\s*\(|(?<name>[A-Za-z_$][\w$]*)\s*[:=]\s*function\b\s*[\w$]*\s*\(", RegexOptions.CultureInvariant);

            private List<InlineFunction> InlineFunctions(int start, int end)
            {
                var found = new List<InlineFunction>();
                var code = _text[start..end];
                foreach (Match m in FunctionDeclaration.Matches(code))
                {
                    var open = code.IndexOf('{', m.Index + m.Length);
                    if (open < 0) continue;
                    var close = MatchBrace(code, open);
                    var name = m.Groups["name"].Value;
                    var qualified = Qualify(name, LineOf(start + m.Index));
                    var f = new InlineFunction { Name = name, Id = _owner.StableId("function", qualified), Start = start + m.Index, End = start + (close < 0 ? code.Length : close + 1) };
                    found.Add(f);
                    _script.Symbols.Add(new ScriptSymbol
                    {
                        Id = f.Id, Kind = "function", Name = name, QualifiedName = qualified, LineStart = LineOf(f.Start), LineEnd = LineOf(Math.Max(f.Start, f.End - 1)),
                        Signature = Collapse(code[m.Index..open]), Visibility = "public", ParentId = _moduleId, Guard = GuardAt(f.Start),
                    });
                    Edge(_moduleId, f.Id, "contains", "extracted", LineOf(f.Start));
                    _page.Functions++;
                }
                return found;
            }

            /// <summary>Un elemento con un evento o un <c>href="javascript:"</c>: un simbolo <c>control</c>, con le chiamate del suo JS.</summary>
            private void Control(Tag tag)
            {
                var handlers = tag.Attrs.Where(a => a.Value != null && a.Name.Length > 2 && a.Name.StartsWith("on", StringComparison.OrdinalIgnoreCase) && a.Value.Trim().Length > 0)
                    .Select(a => (a.ValueOffset, a.ValueOffset + a.Value.Length)).ToList();
                foreach (var a in tag.Attrs.Where(a => a.Value != null && a.Name.ToLowerInvariant() is "href" or "action"))
                {
                    var v = a.Value.TrimStart();
                    if (v.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
                    {
                        var skip = a.Value.Length - v.Length + "javascript:".Length;
                        handlers.Add((a.ValueOffset + skip, a.ValueOffset + a.Value.Length));
                    }
                }
                if (handlers.Count == 0) return;

                var line = LineOf(tag.Start);
                var name = new[] { tag.Get("id"), tag.Get("name") }.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim() ?? $"{tag.Name}@{line}";
                var qualified = Qualify(name, line);
                var id = _owner.StableId("control", qualified);
                var guard = GuardAt(tag.Start);
                _script.Symbols.Add(new ScriptSymbol
                {
                    Id = id, Kind = "control", Name = name, QualifiedName = qualified,
                    LineStart = line, LineEnd = LineOf(tag.End - 1), Signature = Truncate(Collapse(_text[tag.Start..tag.End])),
                    Visibility = "public", ParentId = _moduleId, Guard = guard,
                });
                Edge(_moduleId, id, "contains", "extracted", line);
                _page.Controls++;
                if (guard != null) _page.GuardedControls++;
                foreach (var (from, to) in handlers) JsCalls(from, to, _ => id, always: true);
            }

            /// <summary>Il nome qualificato <c>pagina:nome</c>; lo stesso nome due volte nella pagina si distingue con la riga.</summary>
            private string Qualify(string name, int line)
            {
                var qualified = _file.LogicalPath + ":" + name;
                if (!_qualified.Add(qualified))
                {
                    qualified += "@" + line;
                    _qualified.Add(qualified);
                }
                return qualified;
            }

            // -------------------------------------------------------------- JS

            /// <summary>
            /// Le chiamate <c>a.b.f(…)</c> nel JS fra <paramref name="start"/> e <paramref name="end"/>: un punto di chiamata
            /// ciascuna (con le stringhe letterali; con <paramref name="always"/> anche senza), e un arco <c>calls</c> se la
            /// pagina ha una funzione con quel nome. Le stringhe con una direttiva dentro non sono letterali: le scrive il server.
            /// </summary>
            private void JsCalls(int start, int end, Func<int, string> callerAt, bool always)
            {
                var i = start;
                string previousWord = null;
                while (i < end)
                {
                    var ch = _text[i];
                    if (ch == '/' && i + 1 < end && _text[i + 1] == '/') { i = SkipTo(i, end, "\n"); continue; }
                    if (ch == '/' && i + 1 < end && _text[i + 1] == '*') { i = SkipTo(i + 2, end, "*/"); continue; }
                    if (ch is '"' or '\'' or '`') { i = SkipString(i, end); previousWord = null; continue; }
                    if (!IsIdentStart(ch)) { if (!char.IsWhiteSpace(ch)) previousWord = null; i++; continue; }

                    var chainStart = i;
                    var parts = new List<string>();
                    while (true)
                    {
                        var s = i;
                        while (i < end && IsIdentPart(_text[i])) i++;
                        parts.Add(_text[s..i]);
                        var j = i;
                        while (j < end && char.IsWhiteSpace(_text[j])) j++;
                        if (j < end && _text[j] == '.' && j + 1 < end)
                        {
                            var k = j + 1;
                            while (k < end && char.IsWhiteSpace(_text[k])) k++;
                            if (k < end && IsIdentStart(_text[k])) { i = k; continue; }
                        }
                        break;
                    }
                    var p = i;
                    while (p < end && char.IsWhiteSpace(_text[p])) p++;
                    var callee = parts[^1];
                    if (p < end && _text[p] == '(' && !NotCalls.Contains(callee) && previousWord is not ("function" or "new"))
                    {
                        var args = Arguments(p, end);
                        var caller = callerAt(chainStart);
                        var line = LineOf(chainStart);
                        var receiver = parts.Count > 1 ? string.Join('.', parts.Take(parts.Count - 1)) : null;
                        var before = _script.CallSites.Count;
                        _calls.Add(caller, line, callee, receiver, args, always);
                        _page.CallSites += _script.CallSites.Count - before;
                        if (receiver == null)
                            foreach (var f in _functions.Where(f => f.Name == callee && f.Id != caller))
                                Edge(caller, f.Id, "calls", "extracted", line);
                        i = p + 1; // dentro gli argomenti ci sono altre chiamate
                        previousWord = null;
                        continue;
                    }
                    previousWord = parts.Count == 1 ? callee : null;
                }
            }

            /// <summary>Gli argomenti della chiamata che si apre in <paramref name="open"/>: il testo della stringa se è una stringa letterale, se no null.</summary>
            private List<string> Arguments(int open, int end)
            {
                var args = new List<string>();
                var depth = 0;
                var argStart = open + 1;
                var i = open + 1;
                while (i < end)
                {
                    var ch = _text[i];
                    if (ch is '"' or '\'' or '`') { i = SkipString(i, end); continue; }
                    if (ch is '(' or '[' or '{') depth++;
                    else if (ch is ')' or ']' or '}')
                    {
                        if (depth == 0)
                        {
                            if (ch == ')')
                            {
                                var last = _text[argStart..i];
                                if (args.Count > 0 || last.Trim().Length > 0) args.Add(Literal(last));
                            }
                            return args;
                        }
                        depth--;
                    }
                    else if (ch == ',' && depth == 0) { args.Add(Literal(_text[argStart..i])); argStart = i + 1; }
                    i++;
                }
                return args;
            }

            private string Literal(string arg)
            {
                var t = arg.Trim();
                if (t.Length < 2 || t[0] is not ('"' or '\'' or '`') || t[^1] != t[0]) return null;
                var inner = t[1..^1];
                if (inner.Contains(t[0]) && !inner.Contains("\\" + t[0])) return null; // 'a' + 'b'
                if (t[0] == '`' && inner.Contains("${")) return null;
                if (HasDirective(inner)) return null;
                return inner.Replace("\\" + t[0], t[0].ToString());
            }

            private int SkipString(int i, int end)
            {
                var quote = _text[i];
                i++;
                while (i < end)
                {
                    if (_text[i] == '\\') { i += 2; continue; }
                    if (_text[i] == quote) return i + 1;
                    if (_text[i] == '\n' && quote != '`') return i + 1; // una stringa non va a capo: meglio fermarsi
                    i++;
                }
                return end;
            }

            private int SkipTo(int i, int end, string marker)
            {
                var at = _text.IndexOf(marker, i, end - i, StringComparison.Ordinal);
                return at < 0 ? end : at + marker.Length;
            }

            /// <summary>La graffa che chiude quella in <paramref name="open"/>, saltando stringhe e commenti; -1 se non si chiude.</summary>
            private static int MatchBrace(string code, int open)
            {
                var depth = 0;
                for (var i = open; i < code.Length; i++)
                {
                    var ch = code[i];
                    if (ch == '/' && i + 1 < code.Length && code[i + 1] == '/') { var nl = code.IndexOf('\n', i); if (nl < 0) return -1; i = nl; continue; }
                    if (ch == '/' && i + 1 < code.Length && code[i + 1] == '*') { var c = code.IndexOf("*/", i + 2, StringComparison.Ordinal); if (c < 0) return -1; i = c + 1; continue; }
                    if (ch is '"' or '\'' or '`')
                    {
                        var q = ch;
                        i++;
                        while (i < code.Length && code[i] != q) { if (code[i] == '\\') i++; i++; }
                        continue;
                    }
                    if (ch == '{') depth++;
                    else if (ch == '}' && --depth == 0) return i;
                }
                return -1;
            }

            private static bool IsIdentStart(char c) => char.IsLetter(c) || c is '_' or '$';
            private static bool IsIdentPart(char c) => char.IsLetterOrDigit(c) || c is '_' or '$';

            // -------------------------------------------------------------- archi verso fuori

            /// <summary><c>$include[x]$</c> → <c>includes</c> verso la pagina, con la guardia del punto in cui sta.</summary>
            private void Include(int offset, string arg)
            {
                _page.Includes++;
                var line = LineOf(offset);
                if (string.IsNullOrWhiteSpace(arg) || HasDirective(arg)) { _page.DynamicIncludes++; return; }
                var target = arg.Trim().Trim('"', '\'');
                var guard = GuardAt(offset);
                var found = _index.Resolve(target, _file.LogicalPath);
                if (found.Count == 0)
                {
                    _page.UnresolvedIncludes.Add(target);
                    Edge(_moduleId, External("page", target), "includes", "extracted", line, guard);
                    return;
                }
                _page.IncludesResolved++;
                if (found.Count > DeclarativeLanguageExtractor.MaxAmbiguousCandidates)
                {
                    Edge(_moduleId, External("page", "?." + target), "includes", "ambiguous", line, guard);
                    return;
                }
                foreach (var p in found)
                    Edge(_moduleId, _owner.StableId("module", p), "includes", found.Count == 1 ? "extracted" : "ambiguous", line, guard);
            }

            /// <summary>
            /// <c>&lt;script src="x.js"&gt;</c> → <c>imports</c> verso il modulo JS, se il file c'è accanto alla pagina:
            /// l'id è quello che gli dà l'estrattore JS (il percorso logico senza estensione). Se no un nodo esterno.
            /// </summary>
            private void ImportScript(string src, int offset)
            {
                var line = LineOf(offset);
                if (src.Length == 0 || HasDirective(src)) return;
                var clean = src.Split('?', '#')[0].Replace('\\', '/');
                string target = null;
                if (!clean.Contains("://") && !clean.StartsWith("//") && !clean.StartsWith('/'))
                {
                    var dir = Path.GetDirectoryName(_file.LogicalPath)?.Replace('\\', '/') ?? string.Empty;
                    var logical = PageIndex.Normalize(dir.Length > 0 ? dir + "/" + clean : clean);
                    var absolute = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(_file.AbsolutePath) ?? string.Empty, clean));
                    var lang = logical == null ? null : CodeLanguages.ForFile(logical);
                    if (lang != null && lang.ExtractorKind == CodeExtractorKind.TreeSitter && File.Exists(absolute))
                    {
                        var ext = lang.Extensions.FirstOrDefault(e => logical.EndsWith(e, StringComparison.OrdinalIgnoreCase));
                        target = RoslynCSharpExtractor.StableId(lang.Id, "module", ext == null ? logical : logical[..^ext.Length]);
                    }
                }
                Edge(_moduleId, target ?? External("file", clean), "imports", "extracted", line);
            }

            private string External(string what, string name)
            {
                var id = _owner.StableId("external", what + ":" + name);
                if (_externals.Add(id)) _script.Symbols.Add(new ScriptSymbol { Id = id, Kind = "external", Name = name, QualifiedName = name });
                return id;
            }

            /// <summary>Un arco per (sorgente, destinazione, relazione); due guardie diverse sullo stesso arco valgono in «o».</summary>
            private void Edge(string source, string target, string relation, string confidence, int line, string guard = null)
            {
                if (source == null || target == null || source == target) return;
                var key = $"{source}|{target}|{relation}";
                if (_edges.TryGetValue(key, out var existing))
                {
                    existing.Guard = existing.Guard == null || guard == null ? null : existing.Guard == guard ? guard : $"({existing.Guard}) || ({guard})";
                    return;
                }
                var edge = new ScriptEdge { SourceId = source, TargetId = target, Relation = relation, Confidence = confidence, Line = line, Guard = guard };
                _edges[key] = edge;
                _script.Edges.Add(edge);
            }

            private int LineOf(int offset)
            {
                int lo = 0, hi = _lineStarts.Count - 1;
                while (lo < hi)
                {
                    var mid = (lo + hi + 1) / 2;
                    if (_lineStarts[mid] <= offset) lo = mid; else hi = mid - 1;
                }
                return lo + 1;
            }

            private static string Collapse(string s) => Regex.Replace(s.Trim(), @"\s+", " ");
            private static string Truncate(string s) => s.Length <= MaxSignatureLength ? s : s[..MaxSignatureLength] + "…";
        }
    }

    /// <summary>Una pagina letta: lo script e i numeri che la prova a secco del dialetto mostra (D42 passo 2).</summary>
    public sealed class TemplatePage
    {
        public CodeSourceFile File { get; init; }
        /// <summary>Null se la pagina non si è potuta leggere (l'avviso dice perché).</summary>
        public CodeScript Script { get; set; }
        public List<string> Warnings { get; } = new();
        public int Controls { get; set; }
        public int GuardedControls { get; set; }
        public int Functions { get; set; }
        public int CallSites { get; set; }
        public int Includes { get; set; }
        public int IncludesResolved { get; set; }
        /// <summary>Gli <c>include</c> il cui argomento lo scrive il server (una direttiva dentro, o vuoto): non si seguono.</summary>
        public int DynamicIncludes { get; set; }
        public List<string> UnresolvedIncludes { get; } = new();
        /// <summary>Le parole del dialetto trovate, con quante volte.</summary>
        public Dictionary<string, int> Words { get; } = new(StringComparer.Ordinal);
        /// <summary>Le parole con argomento che il dialetto non dichiara: quasi certamente direttive che mancano.</summary>
        public Dictionary<string, int> UnknownWords { get; } = new(StringComparer.Ordinal);
        /// <summary>Per ogni parola non dichiarata, il primo punto in cui compare (<c>percorso:riga</c>).</summary>
        public Dictionary<string, string> UnknownWordExamples { get; } = new(StringComparer.Ordinal);
        /// <summary>Le direttive senza argomento e non dichiarate: valori che il server scrive.</summary>
        public int Placeholders { get; set; }
    }
}

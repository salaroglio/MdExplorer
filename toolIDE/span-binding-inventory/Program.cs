// Inventario delle chiamate legate a System.MemoryExtensions (sprint .NET 10, F2).
//
// Perché: con C# 14 gli array si convertono da soli in Span/ReadOnlySpan («first-class span types») e array.Contains(x)
// non chiama più Enumerable.Contains ma MemoryExtensions.Contains. Fuori da una query è giusto e più veloce; dentro una
// lambda convertita in Expression<T> (una query LINQ sul database) il fornitore trova una forma che non conosce.
// Vale per ogni metodo che esiste in entrambe le classi, non solo Contains.
//
// Che cosa scrive, una riga per ogni punto trovato (separatore TAB):
//   file:riga   metodo   in query: sì|no   progetto   tipo
// dove «tipo» è
//   memext    una chiamata il cui metodo risolto appartiene a System.MemoryExtensions;
//   span      una conversione implicita verso Span<T>/ReadOnlySpan<T> DENTRO un albero di espressioni, verso un metodo
//             di un altro tipo (la stessa trappola, con un altro nome: Microsoft la descrive per tutti i metodi con
//             sovraccarichi a span).
// «in query: sì» = il punto sta dentro una lambda convertita in System.Linq.Expressions.Expression<T>, o dentro una
// clausola della sintassi di query (where, select, orderby…) legata a un metodo che riceve un Expression<T>.
//
// Uso:
//   dotnet run --project toolIDE/span-binding-inventory -- --repo <cartella> [--lang 14|13|default] [--out <file>]
//                                                          [--project <file.csproj>]... [--exclude <file.csproj>]...
// Senza --project apre ogni .csproj registrato in git (git ls-files). Esce con:
//   0  nessuna riga «in query: sì» e nessuna «field»;  1  almeno una delle due (da correggere);
// Tipo «field»: la parola `field` dentro una funzione di accesso di proprietà, che con C# 14 cambia significato.
//   2  lo strumento non ha potuto analizzare TUTTO (progetto non caricato, errori di compilazione, simbolo non risolto):
//      un elenco incompleto non vale come «zero», e lo strumento lo dice invece di tacere.
using System.Diagnostics;
using System.Text;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.Operations;

var repo = Arg("--repo") ?? Directory.GetCurrentDirectory();
repo = Path.GetFullPath(repo);
var lang = Arg("--lang") ?? "14";
var outFile = Arg("--out");
var projects = Args("--project").Select(p => Path.GetFullPath(p, repo)).ToList();
var excluded = Args("--exclude").Select(p => Path.GetFullPath(p, repo)).ToHashSet(StringComparer.Ordinal);
if (projects.Count == 0) projects = GitCsprojs(repo);
projects = projects.Where(p => !excluded.Contains(p)).ToList();

var instance = MSBuildLocator.RegisterDefaults();
Console.Error.WriteLine($"MSBuild: {instance.Name} {instance.Version} ({instance.MSBuildPath})");
Console.Error.WriteLine($"Roslyn dello strumento: {typeof(CSharpCompilation).Assembly.GetName().Version}; LangVersion forzata: {lang}");
foreach (var x in excluded) Console.Error.WriteLine($"ESCLUSO per richiesta: {Path.GetRelativePath(repo, x)}");

var props = new Dictionary<string, string>();
if (lang != "default") props["LangVersion"] = lang;
using var ws = MSBuildWorkspace.Create(props);
var loadFailures = new List<string>();
ws.RegisterWorkspaceFailedHandler(e =>
{
    if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure) loadFailures.Add(e.Diagnostic.Message);
});

var rows = new SortedSet<string>(StringComparer.Ordinal);
var incomplete = new List<string>();
var notes = new List<string>();
var seenDocs = new HashSet<string>(StringComparer.Ordinal);
var loaded = new Dictionary<ProjectId, Project>();

foreach (var csproj in projects)
{
    // Già entrato come dipendenza di un progetto aperto prima: riaprirlo darebbe «already part of the workspace».
    if (ws.CurrentSolution.Projects.Any(x => string.Equals(x.FilePath, csproj, StringComparison.Ordinal)))
    {
        Console.Error.WriteLine($"già caricato come dipendenza: {Rel(csproj)}");
        continue;
    }
    var before = loadFailures.Count;
    Project? p = null;
    try { p = await ws.OpenProjectAsync(csproj); }
    catch (Exception ex)
    {
        incomplete.Add($"{Rel(csproj)}: non caricato: {ex.Message}");
        foreach (var f in loadFailures.Skip(before)) incomplete.Add($"{Rel(csproj)}: {f}");
        continue;
    }
    // Progetto caricato: i messaggi di MSBuild arrivati intanto sono avvisi (NuGet NU1701, vulnerabilità, framework non
    // supportato) che MSBuildWorkspace riporta come «Failure». Il buco vero si vede dagli errori di compilazione, più sotto.
    foreach (var f in loadFailures.Skip(before)) notes.Add($"{Rel(csproj)}: messaggio di MSBuild al caricamento: {f}");
    Console.Error.WriteLine($"caricato {Rel(csproj)} ({p.Documents.Count()} file, LangVersion effettiva {((CSharpParseOptions)p.ParseOptions!).LanguageVersion})");
}

foreach (var p in ws.CurrentSolution.Projects)
{
    if (p.Language != LanguageNames.CSharp) continue;
    var comp = await p.GetCompilationAsync();
    if (comp is null) { incomplete.Add($"{p.Name}: nessuna compilazione"); continue; }
    var errors = comp.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
    if (errors.Count > 0)
    {
        incomplete.Add($"{p.Name}: {errors.Count} errori di compilazione, es. {errors[0]}");
    }
    var memExt = comp.GetTypeByMetadataName("System.MemoryExtensions");
    var exprT = comp.GetTypeByMetadataName("System.Linq.Expressions.Expression`1");
    var span = comp.GetTypeByMetadataName("System.Span`1");
    var roSpan = comp.GetTypeByMetadataName("System.ReadOnlySpan`1");

    foreach (var doc in p.Documents)
    {
        if (doc.FilePath is null || !seenDocs.Add(p.Name + "|" + doc.FilePath)) continue;
        if (doc.FilePath.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}")) continue; // generati
        var tree = await doc.GetSyntaxTreeAsync();
        var model = await doc.GetSemanticModelAsync();
        if (tree is null || model is null) { incomplete.Add($"{doc.FilePath}: nessun modello"); continue; }
        var root = await tree.GetRootAsync();

        // 1. chiamate legate a MemoryExtensions
        foreach (var inv in root.DescendantNodes().OfType<InvocationExpressionSyntax>())
        {
            var info = model.GetSymbolInfo(inv);
            var m = (info.Symbol ?? info.CandidateSymbols.FirstOrDefault()) as IMethodSymbol;
            if (m is null)
            {
                // Un nome che non si risolve sarebbe un buco nell'elenco: lo si conta solo se il file compila
                // (altrimenti è già negli errori di compilazione) e se potrebbe essere un metodo di MemoryExtensions.
                if (info.CandidateReason == CandidateReason.LateBound)
                {
                    // Chiamata su `dynamic`: si risolve a runtime. C# vieta le operazioni dinamiche dentro un albero di
                    // espressioni (CS1963), quindi non può essere un caso «in query: sì». La si nota, non è un buco.
                    if (InQuery(inv, model, exprT)) incomplete.Add($"{Loc(inv)}: chiamata dinamica dentro una query (impossibile: CS1963)");
                    else notes.Add($"{Loc(inv)}: chiamata dinamica (late bound), fuori da ogni query");
                }
                else if (errors.Count == 0 && info.CandidateReason != CandidateReason.None)
                    incomplete.Add($"{Loc(inv)}: chiamata non risolta ({info.CandidateReason})");
                continue;
            }
            var def = (m.ReducedFrom ?? m).ContainingType;
            if (memExt is null || !SymbolEqualityComparer.Default.Equals(def, memExt)) continue;
            rows.Add(Row(inv, m.Name, InQuery(inv, model, exprT), p.Name, "memext"));
        }

        // 3. `field` dentro una funzione di accesso di una proprietà: da C# 14 è il campo nascosto della proprietà, non più
        //    un membro (o una variabile) chiamato `field`. Il parser di C# 14 lo legge come FieldExpressionSyntax: con un
        //    codice scritto per C# 12/13 ogni nodo così è un cambio di significato. «in query» non c'entra: tipo «field».
        foreach (var fe in root.DescendantNodes().OfType<FieldExpressionSyntax>())
            rows.Add(Row(fe, "field (parola chiave di C# 14)", false, p.Name, "field"));

        // 2. conversioni implicite a Span/ReadOnlySpan dentro alberi di espressioni, verso metodi di altri tipi
        foreach (var lambda in root.DescendantNodes().OfType<AnonymousFunctionExpressionSyntax>())
        {
            if (!IsExpressionLambda(lambda, model, exprT)) continue;
            var op = model.GetOperation(lambda);
            if (op is null) continue;
            foreach (var conv in op.DescendantsAndSelf().OfType<IConversionOperation>())
            {
                if (!conv.IsImplicit || conv.Type is not INamedTypeSymbol t) continue;
                var target = t.OriginalDefinition;
                if (!SymbolEqualityComparer.Default.Equals(target, span) && !SymbolEqualityComparer.Default.Equals(target, roSpan)) continue;
                if (conv.Operand.Type is INamedTypeSymbol src && (SymbolEqualityComparer.Default.Equals(src.OriginalDefinition, span) || SymbolEqualityComparer.Default.Equals(src.OriginalDefinition, roSpan))) continue;
                var call = conv.Parent is IArgumentOperation a ? a.Parent as IInvocationOperation : null;
                if (call is not null && memExt is not null && SymbolEqualityComparer.Default.Equals(call.TargetMethod.ContainingType, memExt)) continue; // già nel punto 1
                var name = call is null ? "(conversione)" : $"{call.TargetMethod.ContainingType.Name}.{call.TargetMethod.Name}";
                rows.Add(Row(conv.Syntax, name, true, p.Name, "span"));
            }
        }
    }
}

var sb = new StringBuilder();
sb.AppendLine($"# span-binding-inventory — {DateTime.Now:yyyy-MM-dd HH:mm} — LangVersion {lang} — {projects.Count} progetti, {seenDocs.Count} file");
sb.AppendLine("# file:riga\tmetodo\tin query\tprogetto\ttipo");
foreach (var r in rows) sb.AppendLine(r);
var yes = rows.Count(r => r.Contains("\tin query: sì\t"));
sb.AppendLine($"# righe: {rows.Count}; «in query: sì»: {yes}; «in query: no»: {rows.Count - yes}");
foreach (var x in excluded) sb.AppendLine($"# ESCLUSO per richiesta: {Rel(x)}");
foreach (var n in notes) sb.AppendLine($"# NOTA: {n}");
foreach (var i in incomplete) sb.AppendLine($"# INCOMPLETO: {i}");
if (outFile is not null) File.WriteAllText(Path.GetFullPath(outFile), sb.ToString());
Console.Write(sb.ToString());
var fieldRows = rows.Count(r => r.EndsWith("\tfield", StringComparison.Ordinal));
if (fieldRows > 0) Console.WriteLine($"# righe «field» (cambio di significato con C# 14, da correggere): {fieldRows}");
return incomplete.Count > 0 ? 2 : yes > 0 || fieldRows > 0 ? 1 : 0;

// ---------------------------------------------------------------------------------------------------------------
bool InQuery(SyntaxNode node, SemanticModel model, INamedTypeSymbol? exprT)
{
    foreach (var anc in node.Ancestors())
    {
        if (anc is AnonymousFunctionExpressionSyntax lam && IsExpressionLambda(lam, model, exprT)) return true;
        if (anc is QueryClauseSyntax qc)
        {
            var qi = model.GetQueryClauseInfo(qc);
            if (TakesExpression(qi.OperationInfo.Symbol as IMethodSymbol, exprT)) return true;
        }
        if (anc is SelectOrGroupClauseSyntax sg && TakesExpression(model.GetSymbolInfo(sg).Symbol as IMethodSymbol, exprT)) return true;
        if (anc is OrderingSyntax os && TakesExpression(model.GetSymbolInfo(os).Symbol as IMethodSymbol, exprT)) return true;
    }
    return false;
}

static bool IsExpressionLambda(AnonymousFunctionExpressionSyntax lam, SemanticModel model, INamedTypeSymbol? exprT)
    => exprT is not null && model.GetTypeInfo(lam).ConvertedType is INamedTypeSymbol ct
       && SymbolEqualityComparer.Default.Equals(ct.OriginalDefinition, exprT);

static bool TakesExpression(IMethodSymbol? m, INamedTypeSymbol? exprT)
    => m is not null && exprT is not null
       && (m.ReducedFrom ?? m).Parameters.Any(par => par.Type is INamedTypeSymbol t && SymbolEqualityComparer.Default.Equals(t.OriginalDefinition, exprT));

string Row(SyntaxNode n, string method, bool inQuery, string project, string kind)
    => $"{Loc(n)}\t{method}\tin query: {(inQuery ? "sì" : "no")}\t{project}\t{kind}";

string Loc(SyntaxNode n)
{
    var span = n.GetLocation().GetLineSpan();
    return $"{Rel(span.Path)}:{span.StartLinePosition.Line + 1}";
}

string Rel(string path) => Path.GetRelativePath(repo, path).Replace('\\', '/');

string? Arg(string name) { var i = Array.IndexOf(args, name); return i >= 0 && i + 1 < args.Length ? args[i + 1] : null; }
IEnumerable<string> Args(string name) { for (var i = 0; i < args.Length - 1; i++) if (args[i] == name) yield return args[i + 1]; }

static List<string> GitCsprojs(string repo)
{
    var psi = new ProcessStartInfo("git", "ls-files *.csproj") { WorkingDirectory = repo, RedirectStandardOutput = true };
    using var proc = Process.Start(psi)!;
    var list = proc.StandardOutput.ReadToEnd().Split('\n', StringSplitOptions.RemoveEmptyEntries)
        .Select(l => l.Trim())
        // Lo strumento stesso e il suo progetto di controllo (che contiene APPOSTA i casi positivi) non sono codice di MDE.
        .Where(l => !l.StartsWith("toolIDE/span-binding-inventory/", StringComparison.Ordinal))
        .Select(l => Path.GetFullPath(Path.Combine(repo, l))).ToList();
    Console.Error.WriteLine("escluso sempre: toolIDE/span-binding-inventory/** (lo strumento e i suoi controlli)");
    proc.WaitForExit();
    if (proc.ExitCode != 0 || list.Count == 0) throw new InvalidOperationException("git ls-files *.csproj non ha dato progetti: lancia lo strumento con --repo sulla radice del repository");
    return list;
}

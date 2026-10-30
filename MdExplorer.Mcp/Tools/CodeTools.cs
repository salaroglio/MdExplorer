using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace MdExplorer.Mcp;

/// <summary>
/// Gruppo <c>code</c>: il grafo del codice (sprint 2026-10-09). I tool leggono SOLO il database di MdExplorer
/// (D15), mai i sorgenti né gli script: rispondono in millisecondi e portano <c>path:line</c> per aprire il punto
/// giusto quando serve davvero il codice. Scritture: <c>code_set_description</c> (D34), che passa dal backend,
/// il quale aggiorna lo script <c>.doc.ttl</c> e il grafo; <c>code_rule_propose</c> (D42), che PROPONE una regola di
/// arricchimento all'utente (la applica il «Sì» del wizard, mai Mark). Acceso di default nei progetti con una cartella «ambiente di sviluppo»
/// che ha dei linguaggi scelti.
/// </summary>
[McpServerToolType]
public sealed class CodeTools : McpToolsBase
{
    public CodeTools(IHttpClientFactory httpClientFactory) : base(httpClientFactory) { }

    private async Task<(HttpClient Client, string Path, string Error)> ProjectAsync(string project)
    {
        var client = _httpClientFactory.CreateClient("MdExplorer");
        var path = await ResolveProjectPathAsync(client, project);
        return path == null ? (client, null, $"Project '{project}' not found. Use GetProjects to list the available names.") : (client, path, null);
    }

    private static string Q(string projectPath, string rest) => $"/api/codegraph/{rest}{(rest.Contains('?') ? "&" : "?")}projectPath={Uri.EscapeDataString(projectPath)}";

    private async Task<string> GetAsync(HttpClient client, string url, string toolName, string project, string request)
    {
        try
        {
            var resp = await client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) return $"Error: MdExplorer answered {(int)resp.StatusCode}: {body}";
            await LogToolCall(toolName, project, request, body);
            return body;
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    [McpServerTool, Description(
        "Find a code symbol (class, interface, method, function, property, Angular component/service…) in the project's code graph. " +
        "Pass an exact qualified name (e.g. 'Demo.Foo.Bar(string, int)' or 'src/app/x.component:XComponent'), a simple name ('Bar'), " +
        "or a free-text fragment: names AND the functional descriptions are searched, so 'writes to Neo4j' works too. " +
        "Returns up to `limit` candidates with id, kind, file path, line, signature, parameters and the description if one was written. " +
        "Every result has `path` (logical: project-relative, or '@alias/…' for code outside the project) and `absolutePath` (where the file " +
        "is ON THIS MACHINE: open that one, at `lineStart`); `pathUnavailable` says why a file cannot be opened here. " +
        "Use the returned `id` with the other code_* tools. If the result is empty, the knowledge may not be generated yet: " +
        "the user generates it from the folder's «Development environment» window.")]
    public async Task<string> code_find_symbol(
        [Description("Project name (see GetProjects).")] string project,
        [Description("Qualified name, simple name, or free-text fragment (min 3 chars for free text).")] string query,
        [Description("Max candidates (default 10).")] int? limit = null)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        var n = limit ?? 10;
        // Prima l'esatto (id, nome qualificato, nome); se niente, il testo libero su nomi e descrizioni.
        var exact = await GetAsync(client, Q(path, $"symbols/resolve?q={Uri.EscapeDataString(query ?? string.Empty)}&limit={n}"), "code_find_symbol", project, query);
        if (exact.TrimStart().StartsWith("[") && exact.Trim() != "[]") return exact;
        if ((query ?? string.Empty).Trim().Length < 3) return exact;
        return await GetAsync(client, Q(path, $"symbols/search?q={Uri.EscapeDataString(query)}&limit={n}"), "code_find_symbol", project, query);
    }

    [McpServerTool, Description(
        "Describe a code symbol in full: signature, return type, every parameter with type/direction/default, and the functional " +
        "descriptions written by the project's AI (summary, what it returns, what each parameter means, remarks). " +
        "`descriptionStale` = true means the description was written for an older signature or body: trust the code, double-check the prose. " +
        "Answers 'what does X do, what do I pass it, what does it return' without opening the file.")]
    public async Task<string> code_describe(
        [Description("Project name.")] string project,
        [Description("Symbol id from code_find_symbol (or an exact qualified name).")] string symbol)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        var id = await ResolveSingleIdAsync(client, path, symbol);
        if (id.Error != null) return id.Error;
        return await GetAsync(client, Q(path, $"symbols/{Uri.EscapeDataString(id.Id)}"), "code_describe", project, symbol);
    }

    [McpServerTool, Description(
        "Outline of one source file: `summary` = what the file is for (1-2 sentences written when the code was described; null if not described yet), " +
        "then `symbols` = every symbol declared in it (classes, members, functions) in line order, with kind, signature, " +
        "parentId (to rebuild the tree) and description. Cheaper than reading the file when you only need its purpose and shape.")]
    public async Task<string> code_outline(
        [Description("Project name.")] string project,
        [Description("The file's logical path, with '/': relative to the project root (e.g. 'MdExplorer.bll/Services/X.cs') or, for code checked out outside the project, '@alias/…' (e.g. '@backend/Api/Foo.cs'). The `path` field of any code_* result works as is.")] string path)
    {
        var (client, projectPath, error) = await ProjectAsync(project);
        if (error != null) return error;
        return await GetAsync(client, Q(projectPath, $"outline?path={Uri.EscapeDataString(path ?? string.Empty)}"), "code_outline", project, path);
    }

    [McpServerTool, Description(
        "Neighbors of a symbol in the code graph. direction='out' = what the symbol uses (calls, instantiates, references, inherits, implements, imports, contains, jumps); " +
        "direction='in' = who uses it (its callers, implementers, subclasses). Optional `relation` filters one relation. " +
        "`depth` follows the edges up to N hops (1-5). Each edge carries `confidence`: extracted (certain), inferred (resolved by name, " +
        "e.g. an interface call to its implementations), llm-resolved (several candidates, the LLM that described the code chose this one: a deduction, not the parser), " +
        "ambiguous (several candidates), rule (added by an approved enrichment rule: dispatches, resolvesTo). An edge of a rule with a `describe` " +
        "question may carry `description`: the answer for that point (e.g. qualifier and scope of a binding). " +
        "Answers 'who calls X', 'which classes implement I', 'which implementation is registered for I', 'what does X depend on'.")]
    public async Task<string> code_neighbors(
        [Description("Project name.")] string project,
        [Description("Symbol id (or exact qualified name).")] string symbol,
        [Description("'out' (default) or 'in'.")] string direction = "out",
        [Description("One of: contains, imports, inherits, implements, references, calls, instantiates, jumps (between the fragments of a long block), dispatches (enrichment rules), resolvesTo (interface → implementation registered in META-INF/services). Empty = all.")] string relation = null,
        [Description("Hops, 1-5 (default 1).")] int? depth = null,
        [Description("Max edges (default 50).")] int? limit = null)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        var id = await ResolveSingleIdAsync(client, path, symbol);
        if (id.Error != null) return id.Error;
        var url = Q(path, $"neighbors?id={Uri.EscapeDataString(id.Id)}&direction={Uri.EscapeDataString(direction ?? "out")}&depth={depth ?? 1}&limit={limit ?? 50}" +
                           (string.IsNullOrWhiteSpace(relation) ? string.Empty : $"&relation={Uri.EscapeDataString(relation)}"));
        return await GetAsync(client, url, "code_neighbors", project, $"{symbol} {direction} {relation} d={depth}");
    }

    [McpServerTool, Description(
        "Impact analysis: everything that depends on a symbol, directly or indirectly (callers of callers, implementers, subclasses, " +
        "files importing its module), up to `depth` hops. Use it before changing a signature or deleting something: " +
        "'what breaks if I change X'. Each row says the relation, the confidence and the file:line of the dependency.")]
    public async Task<string> code_impact(
        [Description("Project name.")] string project,
        [Description("Symbol id (or exact qualified name).")] string symbol,
        [Description("Hops, 1-5 (default 3).")] int? depth = null,
        [Description("Max rows (default 200).")] int? limit = null)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        var id = await ResolveSingleIdAsync(client, path, symbol);
        if (id.Error != null) return id.Error;
        return await GetAsync(client, Q(path, $"impact?id={Uri.EscapeDataString(id.Id)}&depth={depth ?? 3}&limit={limit ?? 200}"), "code_impact", project, symbol);
    }

    [McpServerTool, Description(
        "Shortest path between two symbols through the code graph, following edges in either direction: 'how does A reach B', " +
        "'is the controller connected to that repository'. Empty result = no path within 8 hops.")]
    public async Task<string> code_path(
        [Description("Project name.")] string project,
        [Description("Start symbol id (or exact qualified name).")] string from,
        [Description("End symbol id (or exact qualified name).")] string to)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        var a = await ResolveSingleIdAsync(client, path, from); if (a.Error != null) return a.Error;
        var b = await ResolveSingleIdAsync(client, path, to); if (b.Error != null) return b.Error;
        return await GetAsync(client, Q(path, $"path?from={Uri.EscapeDataString(a.Id)}&to={Uri.EscapeDataString(b.Id)}"), "code_path", project, $"{from} -> {to}");
    }

    [McpServerTool, Description(
        "What is in the project's code graph, per program folder: languages, whether the folder is linked on this machine, how many files " +
        "and symbols were scanned and how many already have a description, when it was last scanned, the programs it USES according to its build " +
        "files (`uses`: pom.xml / Gradle / .csproj, with the line that says so; `linked` = calls into it reach that program's real symbols), the last description run (`interrupted` = " +
        "MdExplorer was closed while describing, or it failed: the user is asked at project open whether to continue); plus the description jobs " +
        "running now (file reached out of the total). Use it for 'what programs are in the graph?', 'how far did the description get?', 'is it done?'. " +
        "The code graph is MdExplorer's own: answer ONLY from code_* tools, never with SPARQL or an HTTP call to a Fuseki server.")]
    public async Task<string> code_inventory(
        [Description("Project name.")] string project)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        return await GetAsync(client, Q(path, "inventory"), "code_inventory", project, "");
    }

    [McpServerTool, Description(
        "The MODEL of the project's code graph, explained: what each symbol kind, relation and confidence means, with this project's counts; " +
        "the facts enrichment rules can read (calls with X.class arguments, calls with string arguments, annotations — most frequent first — chains, " +
        "resolved return types); the active rules with how many edges they make; the catalog of ready-made questions for code_query. " +
        "Call it before writing a rule, before answering «what would you have needed in the graph?», and when you are unsure what a relation means.")]
    public async Task<string> code_schema(
        [Description("Project name.")] string project)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        return await GetAsync(client, Q(path, "schema"), "code_schema", project, "");
    }

    [McpServerTool, Description(
        "Overview of the project's code graph: files, symbols by language and kind, edges by relation, how many symbols and files have a " +
        "description, the most connected symbols (the 'god nodes') and the largest files with what each is for (`summary`). Start here to get the shape of an unknown codebase.")]
    public async Task<string> code_overview(
        [Description("Project name.")] string project)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        return await GetAsync(client, Q(path, "overview"), "code_overview", project, "");
    }

    [McpServerTool, Description(
        "The project's code roots: where the analyzed source code lives ON THIS MACHINE. Folders inside the project have paths " +
        "relative to it; external roots (code checked out elsewhere) have logical paths '@alias/…' — the same on every machine — " +
        "and here you get, for each alias, the absolute folder on this machine, its git repository and whether it is linked. " +
        "Every code_* tool already returns `absolutePath` next to `path`; use this to know which folders you may read and edit, " +
        "and why one is not available (not linked here, folder moved, different repository, server unreachable). A root with `remote` " +
        "set (ssh://host/path) lives on an SSH server: it has no absolute path here, use the remote_* tools for its files. " +
        "`encodings` lists the file encoding of each extension (e.g. ibm850 = CP-850 for .cbl): files with those extensions " +
        "are NOT UTF-8 — keep their encoding when you change them; an extension not listed is UTF-8. A huge root may be analysed " +
        "in parts: then it is not a program as a whole (no 'program' in `tags`) and `programs` lists its analysed subfolders " +
        "('@alias/<path>'), each with its `languages`; the graph has only the code inside them.")]
    public async Task<string> code_roots(
        [Description("Project name.")] string project)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        return await GetAsync(client, $"/api/coderoots?projectPath={Uri.EscapeDataString(path)}", "code_roots", project, "");
    }

    [McpServerTool, Description(
        "Fix or complete a description in the code graph (WRITE) — the only code_* tool that writes. Use it when the user says a " +
        "description is wrong or missing, or asks you to describe something: read the source first, then write what it really does. " +
        "Target EITHER `file` (what the whole file is for, 1-2 sentences in `summary`: its purpose, NOT a list of its functions) OR `symbol` (a class/method/function: `summary` = " +
        "what it does, `returnsDescription`, `remarks`, and `parameters` = {parameterName: description} for its call parameters). " +
        "A script's own node (kind `module`, e.g. a PowerShell script with a param() block) IS the file: target it as `symbol` to write the " +
        "`parameters` the script is launched with; its `summary` becomes the file's description, and it has no returnsDescription/remarks. " +
        "Omitted fields stay as they are; an empty string removes `returnsDescription`, `remarks` or one parameter's description. " +
        "`summary` is required the first time and when the existing description was written for an older signature. " +
        "MdExplorer updates the versioned description script (.mde-code/…/*.doc.ttl) and the graph: never edit those files by hand. " +
        "Write in the language of the file's comments, Italian if there are none.")]
    public async Task<string> code_set_description(
        [Description("Project name.")] string project,
        [Description("Symbol id from code_find_symbol (or an exact qualified name). Leave empty when describing a file.")] string symbol = null,
        [Description("The file's logical path, as `path` in any code_* result. Leave empty when describing a symbol.")] string file = null,
        [Description("File: what it is for. Symbol: what it does, when to use it, its effects.")] string summary = null,
        [Description("Symbol only: what it returns and what that means, edge cases included.")] string returnsDescription = null,
        [Description("Symbol only: a warning for callers (threads, performance, preconditions).")] string remarks = null,
        [Description("Symbol only: {parameterName: description} for the parameters to write; names exactly as in code_describe.")] Dictionary<string, string> parameters = null)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        if (string.IsNullOrWhiteSpace(symbol) == string.IsNullOrWhiteSpace(file)) return "Give either `symbol` or `file`, exactly one.";
        string symbolId = null;
        if (!string.IsNullOrWhiteSpace(symbol))
        {
            var id = await ResolveSingleIdAsync(client, path, symbol);
            if (id.Error != null) return id.Error;
            symbolId = id.Id;
        }
        try
        {
            var payload = new { projectPath = path, path = file, symbolId, summary, returnsDescription, remarks, parameters, writtenBy = "code_set_description" };
            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("/api/codegraph/set-description", content);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall("code_set_description", project, symbol ?? file, body);
            return resp.IsSuccessStatusCode ? body : $"Error: MdExplorer answered {(int)resp.StatusCode}: {body}";
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    private async Task<string> PostAsync(HttpClient client, string url, object payload, string toolName, string project, string request)
    {
        try
        {
            var content = new StringContent(JsonSerializer.Serialize(payload), System.Text.Encoding.UTF8, "application/json");
            var resp = await client.PostAsync(url, content);
            var body = await resp.Content.ReadAsStringAsync();
            await LogToolCall(toolName, project, request, body);
            return resp.IsSuccessStatusCode ? body : $"Error: MdExplorer answered {(int)resp.StatusCode}: {body}";
        }
        catch (HttpRequestException ex)
        {
            return $"Error connecting to MdExplorer: {ex.Message}";
        }
    }

    private const string RuleShape =
        "Rule JSON: {\"id\": \"lowercase-dashes\", \"description\": \"what it does, plain words for the user\", " +
        "\"call\": {\"callee\": \"methodName\", \"argument\": 0}, " +
        "\"split\": {\"separator\": \".\", \"at\": \"last\"|\"first\"} (optional: the string is owner+separator+method), " +
        "\"owner\": {\"from\": \"string\" (the part before the separator) | \"pairedCall\" (the string of another call in the same method, " +
        "written before it or in its receiver: {\"pairedCall\": {\"callee\": \"x\", \"argument\": 0}}) | \"class\" ({\"class\": \"qualified.Name\"}) | " +
        "\"page\" (the string names a PAGE of the project's page dialect, not a method: e.g. the page shown after the call; matched by file name or path end), " +
        "\"aliasMap\": \"project-relative/path.properties\" (optional: alias=qualified.ClassName)}, " +
        "\"excludePrefixes\": [\"redirect:\"] (optional)}. " +
        "REGISTRATION rule (an abstraction resolved elsewhere: DI binding, @ImplementedBy, @Provides, a registry with X.class): " +
        "{\"id\": \"…\", \"kind\": \"registration\", \"description\": \"…\", \"anchor\": {\"call\": \"to\"} | {\"annotation\": \"ImplementedBy\"}, " +
        "\"abstraction\": {\"from\": …}, \"implementation\": {\"from\": …}} with from = \"argument\" ({\"argument\": 0}: an X.class of the anchor call) | " +
        "\"chain\" ({\"callee\": \"bind\", \"argument\": 0}: an X.class of a call further down the anchor's chain) | \"annotated\" (the annotated symbol) | " +
        "\"returnType\" (of the annotated method) | \"annotationArgument\" ({\"element\": \"value\"}); edges `resolvesTo`. " +
        "ANY rule may add \"describe\": \"one specific question\" that MdExplorer asks the LLM for each linked point during «Describe with Mark»; " +
        "the answer becomes the edge's description. Unknown fields are an error.";

    [McpServerTool, Description(
        "Dry-run an ENRICHMENT RULE for the code graph. Use it when, answering the user (or when the user asks what the graph lacked), a call chain " +
        "stops at an abstraction whose implementation is registered elsewhere (registration rule), or at a STRING that names a method " +
        "(e.g. setX(\"methodName\"), submit('alias.method'), a dispatcher that picks the method by name): the parser cannot see that link. " +
        "A rule declares the convention; MdExplorer applies it to every recorded fact (calls with string or X.class arguments, chains, annotations) " +
        "and adds `dispatches` or `resolvesTo` edges. " +
        "Nothing is written. Returns: callSites, notLiteral, excluded, resolved, unresolved, outsideGraph (registration: an end is a library class, not counted against the rule), missCounts (why points were not linked), " +
        "missExamples, contexts (folders by number of calls), edgeCount, accepted + rejectReasons (MdExplorer rejects rules that link under 60%), " +
        "and `sample`: up to 5 linked calls spread over the list. Fix the rule and retry until accepted; then OPEN EACH SAMPLE CALL in the source " +
        "(path:line) and check the target is really the method executed, before code_rule_propose. " + RuleShape)]
    public async Task<string> code_rule_try(
        [Description("Project name.")] string project,
        [Description("The rule, as a JSON object.")] JsonElement rule)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        return await PostAsync(client, "/api/codegraph/rules/try", new { projectPath = path, rule }, "code_rule_try", project, rule.GetRawText());
    }

    [McpServerTool, Description(
        "Propose an enrichment rule (already accepted by code_rule_try) to the USER: they see «Improve the graph» in the MarkAgent tab, a wizard " +
        "with the problem, where to apply it, the checks and a yes/no. You never apply it yourself and never write files under .mde-code. " +
        "Refused unless every call in code_rule_try's `sample` is in `sampleChecks` with ok=true (you opened it in the source). " +
        "`problem`: the gap in the graph and what the user could not get because of it, in plain words (the user may not be technical). " +
        "`furtherAnalyses`: other analyses this problem suggests (e.g. page → servlet → controller, profile conditions), kept in a shared list " +
        "for later; they do not start by themselves. Tell the user the proposal is waiting in the tab. " + RuleShape)]
    public async Task<string> code_rule_propose(
        [Description("Project name.")] string project,
        [Description("The rule, as a JSON object (the same accepted by code_rule_try).")] JsonElement rule,
        [Description("The problem the rule solves, in plain words for the user.")] string problem,
        [Description("One per sample call: {\"path\": \"…\", \"line\": 12, \"ok\": true, \"note\": \"what you saw\"}.")] List<Dictionary<string, JsonElement>> sampleChecks,
        [Description("Optional: [{\"title\": \"…\", \"description\": \"…\"}] further analyses suggested by this problem.")] List<Dictionary<string, string>> furtherAnalyses = null)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        var checks = (sampleChecks ?? new()).Select(c => new
        {
            path = c.TryGetValue("path", out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null,
            line = c.TryGetValue("line", out var l) && l.ValueKind == JsonValueKind.Number ? l.GetInt32() : 0,
            ok = c.TryGetValue("ok", out var o) && o.ValueKind == JsonValueKind.True,
            note = c.TryGetValue("note", out var n) && n.ValueKind == JsonValueKind.String ? n.GetString() : null,
        }).ToList();
        var analyses = (furtherAnalyses ?? new()).Select(a => new { title = a.GetValueOrDefault("title"), description = a.GetValueOrDefault("description") }).ToList();
        return await PostAsync(client, "/api/codegraph/rules/propose",
            new { projectPath = path, rule, problem, sampleChecks = checks, furtherAnalyses = analyses, proposedBy = "MarkAgent (code_rule_propose)" },
            "code_rule_propose", project, rule.GetRawText());
    }

    [McpServerTool, Description(
        "The project's active enrichment rules (approved by the user, shared with Git) and the list of further analyses proposed so far. " +
        "Edges made by rules have relation `dispatches` (dispatch rules) or `resolvesTo` (registration rules) and confidence `rule`.")]
    public async Task<string> code_rules([Description("Project name.")] string project)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        return await GetAsync(client, Q(path, "rules"), "code_rules", project, "");
    }

    private const string DialectShape =
        "A PAGE DIALECT is a language.yml (YAML) for server-side HTML pages with directives: " +
        "schema: 1 / id: lowercase-dashes / name: … / version: 1 / extensions: [.template] / template: { " +
        "directive: a regex for ONE directive with the named group (?<word>…) and optionally (?<arg>…), e.g. '\\$(?<word>[A-Za-z_]+)(?:\\[(?<arg>[^\\]]*)\\])?\\$'; " +
        "include: [words whose argument is another page]; " +
        "blocks: [{open: if, close: endif, else: [else], guard: true}, {open: switch, close: endswitch, cases: [case], guard: true}, {open: loop, close: endloop}] }. " +
        "guard: true = the argument is a CONDITION on what the block contains (controls and includes get code:guard). " +
        "MdExplorer reads the HTML itself: elements with on…= or href=\"javascript:\" become `control` symbols, inline <script> functions become " +
        "`function`, their JS calls become call sites (rules can link them), <script src> becomes `imports`. Unknown fields are an error. " +
        "RECOGNIZER FORM, for pages where server code and HTML mix (Razor .cshtml, JSP, Thymeleaf…), alone or with the directive form: " +
        "template: { dynamic: regex that makes a string server-written (e.g. '@[A-Za-z(]'), events: [attribute-name regexes, default '^on.+$'; " +
        "Angular '^\\(.+\\)$'], recognizers: [ {role: include, match: '…(?<arg>page)…'}, {role: open, match: '@if\\s*\\((?<arg>.+?)\\)\\s*\\{', " +
        "close: brace, guard: true}, {role: else, match: '\\belse\\s*\\{', close: brace}, {id: endif, role: close, match: …} with " +
        "{role: open, …, close: endif}, {role: case, match: '…(?<arg>…)…'}, {role: call, match: 'asp-controller=\"(?<a0>[^\"]+)\"\\s+asp-action=\"(?<a1>[^\"]+)\"', " +
        "callee: asp-action} ] }. close: brace = the block ends at the brace matching the first { of the match; close: element = the block is " +
        "the element whose tag contains the match, up to its end tag (Angular {role: open, match: '\\*ngIf=\"(?<arg>[^\";]+)(?:;\\s*then\\s+(?<then>\\w+))?(?:;?\\s*else\\s+(?<else>\\w+))?\\s*\"', close: element, guard: true}). " +
        "role: named = a block with a name (group name, close: brace or element) that holds for whoever names it: an open with guard and the " +
        "group else gives it the negated condition, the group then the condition itself (Angular {role: named, match: '<ng-template\\b[^>]*?#(?<name>\\w+)', close: element}). " +
        "A call recognized inside a tag makes that element a control; its strings (groups a0…a9) become a call site that an enrichment rule can link. " +
        "ANGULAR: MdExplorer links by itself the component to its templateUrl page (renders), a template call like (click)=\"save()\" to the " +
        "component's method, and a call recognizer with callee: selector (e.g. match: '<(?<a0>app-[\\w-]+)') to the component with that " +
        "selector (instantiates). " +
        "Inline templates (Angular template: `…` in the .ts): template.inline = {extensions: [.ts], match: 'template\\s*:\\s*`(?<body>[^`]*)`'}: " +
        "the group body is a page of the dialect, path '<file>.ts#template', with the lines of the .ts; the .ts stays TypeScript. " +
        "A line-based language (blocks/labels/references instead of template) uses the same tools.";

    [McpServerTool, Description(
        "Dry-run a LANGUAGE for the code graph, usually a PAGE DIALECT. Use it when, answering the user, you need the pages (server-side HTML " +
        "templates with directives like $include[x]$ or $if[X]$) and the graph has no node for them (MdExplorer skips extensions it does not know). " +
        "Read a few pages first and write the dialect from what you see. Nothing is written. Returns: files and folders found, symbolsByKind, " +
        "edgesByRelation, pages (controls, guardedControls, functions, callSites, includes/includesResolved/dynamicIncludes, unresolvedIncludes, " +
        "words, unknownWords with an example place), warnings, notes, accepted + rejectReasons (MdExplorer rejects a language that finds nothing, " +
        "resolves under 60% of includes, or warns on over 10% of files), and `sample`: up to 5 files with the facts found. Fix and retry until " +
        "accepted and the notes are explained; then OPEN EACH SAMPLE FILE and check the facts, before code_language_propose. " + DialectShape)]
    public async Task<string> code_language_try(
        [Description("Project name.")] string project,
        [Description("The full text of language.yml.")] string definition)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        return await PostAsync(client, "/api/codegraph/languages/try", new { projectPath = path, definition }, "code_language_try", project, definition);
    }

    [McpServerTool, Description(
        "Propose a language (already accepted by code_language_try) to the USER: they see «Improve the graph» in the MarkAgent tab, a wizard with " +
        "the problem, the numbers, your checks and a yes/no; on yes MdExplorer writes .mde-languages/<id>/ (shared with Git) and re-analyzes the " +
        "folders. You never write that folder yourself. Refused unless every file in code_language_try's `sample` is in `sampleChecks` with ok=true. " +
        "`problem`: what the graph could not tell the user about these pages, in plain words (the user may not be technical). " +
        "`furtherAnalyses`: what this makes possible next (e.g. a rule from a page action to the controller method), kept in a shared list. " +
        "Tell the user the proposal is waiting in the tab. " + DialectShape)]
    public async Task<string> code_language_propose(
        [Description("Project name.")] string project,
        [Description("The full text of language.yml (the same accepted by code_language_try).")] string definition,
        [Description("The problem the language solves, in plain words for the user.")] string problem,
        [Description("One per sample file: {\"path\": \"…\", \"ok\": true, \"note\": \"what you saw\"}.")] List<Dictionary<string, JsonElement>> sampleChecks,
        [Description("Optional: [{\"title\": \"…\", \"description\": \"…\"}] further analyses suggested by this problem.")] List<Dictionary<string, string>> furtherAnalyses = null)
    {
        var (client, path, error) = await ProjectAsync(project);
        if (error != null) return error;
        var checks = (sampleChecks ?? new()).Select(c => new
        {
            path = c.TryGetValue("path", out var p) && p.ValueKind == JsonValueKind.String ? p.GetString() : null,
            ok = c.TryGetValue("ok", out var o) && o.ValueKind == JsonValueKind.True,
            note = c.TryGetValue("note", out var n) && n.ValueKind == JsonValueKind.String ? n.GetString() : null,
        }).ToList();
        var analyses = (furtherAnalyses ?? new()).Select(a => new { title = a.GetValueOrDefault("title"), description = a.GetValueOrDefault("description") }).ToList();
        return await PostAsync(client, "/api/codegraph/languages/propose",
            new { projectPath = path, definition, problem, sampleChecks = checks, furtherAnalyses = analyses, proposedBy = "MarkAgent (code_language_propose)" },
            "code_language_propose", project, definition);
    }

    /// <summary>Un id già dato passa com'è; un nome si risolve, e se i candidati sono più d'uno si chiede di scegliere.</summary>
    private async Task<(string Id, string Error)> ResolveSingleIdAsync(HttpClient client, string projectPath, string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol)) return (null, "symbol is required.");
        var body = await GetAsync(client, Q(projectPath, $"symbols/resolve?q={Uri.EscapeDataString(symbol.Trim())}&limit=10"), "code_resolve", "", symbol);
        if (!body.TrimStart().StartsWith("[")) return (null, body);
        try
        {
            using var doc = JsonDocument.Parse(body);
            var items = doc.RootElement.EnumerateArray().ToList();
            if (items.Count == 0) return (null, $"No symbol matches '{symbol}'. Try code_find_symbol with a fragment.");
            if (items.Count == 1) return (items[0].GetProperty("id").GetString(), null);
            var exact = items.Where(i => string.Equals(i.GetProperty("qualifiedName").GetString(), symbol.Trim(), StringComparison.Ordinal)).ToList();
            if (exact.Count == 1) return (exact[0].GetProperty("id").GetString(), null);
            var names = string.Join("\n", items.Select(i => $"  - {i.GetProperty("id").GetString()}  {i.GetProperty("kind").GetString()}  {i.GetProperty("qualifiedName").GetString()}  ({i.GetProperty("path").GetString()}:{i.GetProperty("lineStart")})"));
            return (null, $"'{symbol}' is ambiguous ({items.Count} candidates). Call again with one of these ids:\n{names}");
        }
        catch (JsonException) { return (null, "Unexpected answer from MdExplorer: " + body); }
    }
}

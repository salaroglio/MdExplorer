using System;
using MdExplorer.Features.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Services.CodeGraph;
using MdExplorer.Services.RemoteFolders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.CodeGraph
{
    /// <summary>
    /// Le radici di codice esterne (sprint 2026-10-12-Radici-Di-Codice-Esterne, F1): elenco con lo stato su questa
    /// macchina, collegare, scollegare, risolvere un percorso logico. Gli errori arrivano come messaggio (400), mai 500.
    /// </summary>
    [ApiController]
    [Route("api/coderoots")]
    public class CodeRootsController : ControllerBase
    {
        private readonly ICodeRootService _roots;
        private readonly RemoteCodeRootFiles _remoteFiles;
        private readonly IRemoteSessions _sessions;
        private readonly IRemoteServerSettings _servers;

        public CodeRootsController(ICodeRootService roots, RemoteCodeRootFiles remoteFiles, IRemoteSessions sessions, IRemoteServerSettings servers)
        {
            _roots = roots; _remoteFiles = remoteFiles; _sessions = sessions; _servers = servers;
        }

        public class LinkRequest { public string ProjectPath { get; set; } public string Alias { get; set; } public string LocalPath { get; set; } }
        public class UnlinkRequest { public string ProjectPath { get; set; } public string Alias { get; set; } }
        public class DismissRequest { public string ProjectPath { get; set; } public List<string> Aliases { get; set; } }
        // string? = facoltativo: un string non nullabile diventa [Required] implicito e il binding rifiuta il vuoto
        public class EncodingsRequest { public string ProjectPath { get; set; } public string Alias { get; set; } public List<MdExplorer.Service.Models.CodeRootEncodingRule> Rules { get; set; } }
        public class ServerSettingsRequest { public string Host { get; set; } public string? AgentFolder { get; set; } public bool AllowUpload { get; set; } }
        public class FileRequest
        {
            public string ProjectPath { get; set; }
            public string Op { get; set; }
            /// <summary>Percorso logico <c>@alias/…</c>.</summary>
            public string Path { get; set; }
            /// <summary><c>move</c>: la destinazione, percorso logico nella stessa radice.</summary>
            public string? To { get; set; }
            public string? Text { get; set; }
            public string? Stamp { get; set; }
            public bool Recursive { get; set; }
            public string? Pattern { get; set; }
            /// <summary><c>read</c>: rileggere in questa codifica (anche <c>mixed-ibm850</c>); <c>write</c>: scrivere in questa.</summary>
            public string? Encoding { get; set; }
            public bool IgnoreCase { get; set; }
        }

        /// <summary>GET /api/coderoots?projectPath — le radici dichiarate e il loro stato qui.</summary>
        [HttpGet]
        public Task<IActionResult> List([FromQuery] string projectPath, CancellationToken ct) => Guarded(async () =>
            Ok((await _roots.ListAsync(projectPath, ct)).Select(View)), projectPath);

        /// <summary>POST /api/coderoots/link {projectPath, alias, localPath} — collega (e, se nuova, dichiara) una radice.</summary>
        [HttpPost("link")]
        public Task<IActionResult> Link([FromBody] LinkRequest request, CancellationToken ct) => Guarded(async () =>
        {
            var state = await _roots.LinkAsync(request.ProjectPath, request.Alias, request.LocalPath, ct);
            // una radice remota porta gli strumenti remote_* a MarkAgent subito (F4): come «Salva e analizza» per code_*
            if (state.IsRemote) TurnOnRemoteTools(request.ProjectPath);
            return Ok(View(state));
        }, request?.ProjectPath);

        private void TurnOnRemoteTools(string projectPath)
        {
            try
            {
                var db = HttpContext.RequestServices.GetRequiredService<MdExplorer.Abstractions.DB.IUserSettingsDB>();
                var argument = MdExplorer.Service.ProjectsManager.TurnOnMcpGroup(db, projectPath, "remote");
                if (argument != null) MdExplorer.Service.ProjectsManager.RefreshMcpRegistration(projectPath, argument);
            }
            catch (InvalidOperationException ex)
            {
                // il catalogo dei gruppi non si legge (MdExplorer.Mcp mancante): la radice è collegata, gli strumenti no — nel log
                HttpContext.RequestServices.GetRequiredService<Microsoft.Extensions.Logging.ILogger<CodeRootsController>>()
                    .LogWarning(ex, "[CodeRoots] gruppo MCP 'remote' non acceso per {Project}", projectPath);
            }
        }

        /// <summary>POST /api/coderoots/unlink {projectPath, alias} — dimentica la posizione su questa macchina.</summary>
        [HttpPost("unlink")]
        public Task<IActionResult> Unlink([FromBody] UnlinkRequest request) => Guarded(() =>
        {
            _roots.Unlink(request.ProjectPath, request.Alias);
            return Task.FromResult<IActionResult>(Ok(new { alias = request.Alias, linked = false }));
        }, request?.ProjectPath);

        /// <summary>GET /api/coderoots/resolve?projectPath&amp;path — il percorso assoluto di un percorso logico (o relativo al progetto).</summary>
        [HttpGet("resolve")]
        public Task<IActionResult> Resolve([FromQuery] string projectPath, [FromQuery] string path, CancellationToken ct) => Guarded(async () =>
        {
            var resolver = await _roots.CreateResolverAsync(projectPath, ct);
            if (resolver.RemoteOf(path) is { } remote)
                return Ok(new { path, absolutePath = (string)null, remote = remote.Root.Remote.Combine(remote.Relative).ToString() });
            return Ok(new { path, absolutePath = resolver.Resolve(path) });
        }, projectPath);

        /// <summary>
        /// GET /api/coderoots/browse?projectPath&amp;path — i figli diretti di una cartella di una radice (<c>@alias/…</c>),
        /// per l'albero in sola lettura (R9): cartelle prima, poi file, con il percorso logico e se si aprono come testo.
        /// </summary>
        [HttpGet("browse")]
        public Task<IActionResult> Browse([FromQuery] string projectPath, [FromQuery] string path, CancellationToken ct) => Guarded(async () =>
        {
            var resolver = await _roots.CreateResolverAsync(projectPath, ct);
            // una radice remota (sprint 2026-10-21-Cartella-Remota-Via-SSH, F2): stessi elementi, da MDE Remote
            var entries = resolver.RemoteOf(path) is { } remote
                ? await _remoteFiles.ListAsync(remote.Root, remote.Relative, ct)
                : CodeRootBrowser.List(resolver, path);
            return Ok(entries.Select(e => new
            {
                name = e.Name, path = e.LogicalPath, fullPath = e.AbsolutePath, isFolder = e.IsFolder, isText = e.IsText, unreadable = e.Unreadable,
            }));
        }, projectPath);

        /// <summary>GET /api/coderoots/changes?projectPath&amp;alias — ramo e file modificati del repository della radice (sola lettura).</summary>
        [HttpGet("changes")]
        public Task<IActionResult> Changes([FromQuery] string projectPath, [FromQuery] string alias, CancellationToken ct) => Guarded(async () =>
        {
            var c = await _roots.ChangesAsync(projectPath, alias, ct);
            return Ok(new { c.Alias, c.IsGit, c.Branch, files = c.Files.Select(f => new { status = f.Status, path = f.Path }) });
        }, projectPath);

        /// <summary>GET /api/coderoots/reminder?projectPath — le radici non utilizzabili qui da ricordare all'apertura.</summary>
        [HttpGet("reminder")]
        public Task<IActionResult> Reminder([FromQuery] string projectPath, CancellationToken ct) => Guarded(async () =>
            Ok((await _roots.PendingReminderAsync(projectPath, ct)).Select(View)), projectPath);

        /// <summary>POST /api/coderoots/reminder/dismiss {projectPath, aliases} — «non ricordarmelo più» per questi alias.</summary>
        [HttpPost("reminder/dismiss")]
        public Task<IActionResult> DismissReminder([FromBody] DismissRequest request) => Guarded(() =>
        {
            _roots.DismissReminder(request.ProjectPath, request.Aliases);
            return Task.FromResult<IActionResult>(Ok(new { dismissed = request.Aliases }));
        }, request?.ProjectPath);

        /// <summary>
        /// POST /api/coderoots/encodings {projectPath, alias, rules:[{extensions:[".cbl"], encoding:"ibm850"}]} — le codifiche
        /// dei file per estensione della radice (nel .development.yml, su Git).
        /// </summary>
        [HttpPost("encodings")]
        public Task<IActionResult> SetEncodings([FromBody] EncodingsRequest request, CancellationToken ct) => Guarded(async () =>
            Ok(View(await _roots.SetEncodingsAsync(request.ProjectPath, request.Alias, request.Rules ?? new List<MdExplorer.Service.Models.CodeRootEncodingRule>(), ct))), request?.ProjectPath);

        // ------------------------------------------------------------------ cartelle remote (sprint 2026-10-21, F3)

        /// <summary>GET /api/coderoots/ssh-hosts — gli alias di ~/.ssh/config, da proporre (si può scrivere qualunque host).</summary>
        [HttpGet("ssh-hosts")]
        public IActionResult SshHosts() => Ok(SshConfigHosts.Read(SshConfigHosts.DefaultConfigPath));

        /// <summary>
        /// GET /api/coderoots/server-browse?host&amp;path — le sottocartelle di una cartella del server (path vuoto = la home),
        /// per scegliere che cosa collegare. 409 se MDE Remote va caricato e l'utente non ha ancora detto sì (P13).
        /// </summary>
        [HttpGet("server-browse")]
        public Task<IActionResult> ServerBrowse([FromQuery] string host, [FromQuery] string? path, CancellationToken ct) => GuardedRemote(async () =>
        {
            var b = await _sessions.BrowseServerAsync(host, path, ct);
            var at = MdExplorer.Features.Services.RemoteFolders.RemoteLocation.Of(b.Host, b.Path);
            return Ok(new
            {
                host = b.Host, path = b.Path, home = b.Home, location = at.ToString(),
                parent = b.Path == "/" ? null : b.Path.Substring(0, Math.Max(1, b.Path.LastIndexOf('/'))),
                folders = b.Folders.Select(f => new { name = f.Name, path = at.Combine(f.Path).Path }),
            });
        });

        /// <summary>GET /api/coderoots/server-settings?host — cartella di MDE Remote e consenso a caricarlo per questo server.</summary>
        [HttpGet("server-settings")]
        public Task<IActionResult> ServerSettings([FromQuery] string host) => GuardedRemote(() =>
        {
            var destination = DestinationOf(host);
            return Task.FromResult<IActionResult>(Ok(new
            {
                host, destination,
                agentFolder = _servers.AgentFolder(destination),
                defaultAgentFolder = "~/" + RemoteAgentFolders.DefaultAgentFolder,
                uploadAllowed = _servers.UploadAllowed(destination),
            }));
        });

        /// <summary>POST /api/coderoots/server-settings {host, agentFolder, allowUpload} — salva le scelte per il server.</summary>
        [HttpPost("server-settings")]
        public Task<IActionResult> SaveServerSettings([FromBody] ServerSettingsRequest request) => GuardedRemote(() =>
        {
            var destination = DestinationOf(request?.Host);
            _servers.SetAgentFolder(destination, request.AgentFolder);
            if (request.AllowUpload) _servers.AllowUpload(destination);
            return Task.FromResult<IActionResult>(Ok(new { host = request.Host, destination, agentFolder = _servers.AgentFolder(destination), uploadAllowed = _servers.UploadAllowed(destination) }));
        });

        /// <summary>
        /// POST /api/coderoots/files {projectPath, op, path, …} — un'operazione su un file o una cartella di una radice
        /// remota (D7): read, write (con stamp), create, mkdir, delete (recursive), move (to), grep (pattern), list.
        /// </summary>
        [HttpPost("files")]
        public Task<IActionResult> Files([FromBody] FileRequest request, CancellationToken ct) => Guarded(async () =>
        {
            var resolver = await _roots.CreateResolverAsync(request.ProjectPath, ct);
            // remota → MDE Remote sul server; locale → le stesse regole sulla cartella del disco (FL)
            var target = resolver.RemoteOf(request.Path) ?? resolver.LocalOf(request.Path)
                ?? throw new InvalidOperationException(UiText.Say($"'{request.Path}' non è in una radice di codice esterna (@alias/…).", $"'{request.Path}' is not in an external code root (@alias/…)."));
            string to = null;
            if (!string.IsNullOrEmpty(request.To))
            {
                var dest = resolver.RemoteOf(request.To) ?? resolver.LocalOf(request.To);
                if (dest == null || dest.Value.Root.Alias != target.Root.Alias)
                    throw new InvalidOperationException(UiText.Say($"'{request.To}' deve stare nella stessa radice di '{request.Path}'.", $"'{request.To}' must be in the same root as '{request.Path}'."));
                to = dest.Value.Relative;
            }
            var operation = new MdExplorer.RemoteProtocol.RemoteRequest
            {
                Op = request.Op, Path = target.Relative, To = to, Text = request.Text, Stamp = request.Stamp,
                Recursive = request.Recursive, Pattern = request.Pattern, IgnoreCase = request.IgnoreCase, Encoding = request.Encoding,
                Exclude = request.Op == MdExplorer.RemoteProtocol.RemoteOps.Grep ? new List<string> { ".git" } : null,
            };
            var response = target.Root.IsRemote
                ? await _remoteFiles.OperateAsync(target.Root, operation, ct)
                : RemoteCodeRootFiles.OperateLocal(target.Root, operation);
            var alias = target.Root.Alias;
            return Ok(new
            {
                path = request.Path, text = response.Text, stamp = response.Stamp, bom = response.Bom, truncated = response.Truncated,
                encoding = response.Encoding,
                hits = response.Hits?.Select(h => new { path = CodeRootPaths.Format(alias, h.Path), line = h.Line, text = h.Text }),
            });
        }, request?.ProjectPath);

        private static string DestinationOf(string host)
        {
            if (string.IsNullOrWhiteSpace(host)) throw new ArgumentException(UiText.Say("Manca il server.", "The server is missing."));
            return MdExplorer.Features.Services.RemoteFolders.RemoteLocation.Of(host.Trim(), "/").Destination;
        }

        /// <summary>Come <see cref="Guarded"/>, senza progetto: per le chiamate che parlano solo con un server.</summary>
        private async Task<IActionResult> GuardedRemote(Func<Task<IActionResult>> action)
        {
            try { return await action(); }
            catch (RemoteUploadConsentRequiredException ex) { return Consent(ex); }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or FormatException)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>409: MDE Remote va caricato e serve il sì dell'utente; la UI mostra che cosa, dove e quanto (P13).</summary>
        private IActionResult Consent(RemoteUploadConsentRequiredException ex) => StatusCode(409, new
        {
            error = ex.Message,
            consent = new { destination = ex.Destination, size = ex.Size, folder = ex.Folder },
        });

        private static object View(CodeRootState s) => new
        {
            s.Alias, s.Repository, s.Subpath, s.Tags, s.Languages, s.LocalPath, s.RootPath,
            status = s.Status.ToString(), s.Reason, s.ActualRemotes,
            remote = s.Remote?.ToString(),
            encodings = s.ExtensionEncodings.GroupBy(p => p.Value).Select(g => new { encoding = g.Key, extensions = g.Select(p => p.Key).OrderBy(e => e).ToList() }),
        };

        private async Task<IActionResult> Guarded(Func<Task<IActionResult>> action, string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath))
                return BadRequest(new { error = UiText.Say($"projectPath non esiste: {projectPath}", $"projectPath does not exist: {projectPath}") });
            try { return await action(); }
            catch (RemoteUploadConsentRequiredException ex) { return Consent(ex); }
            // il codice di MDE Remote (not_found, changed, exists…) serve a chi chiama: MarkAgent decide se creare o rileggere
            catch (RemoteOperationException ex) { return BadRequest(new { error = ex.Message, code = ex.Code }); }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or FormatException)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Services.Git;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Services.Telegram
{
    /// <summary>Il bot di progetto come lo vede la sezione Telegram.</summary>
    public sealed record ProjectBotStatus(
        string State,            // none | setup | configured | error
        string BotUsername,
        string ChannelTitle,
        string Subscription,     // subscribed | not-subscribed | unknown (nessun bot personale: non si sa chi sei su Telegram)
        string SetupState,       // waiting-channel | failed (durante la creazione)
        string ChannelLink,      // durante la creazione: il link che aggiunge il bot a un canale come amministratore
        bool NotPublished,       // .mde-keys.yml ha un commit non ancora pubblicato
        string Error);

    /// <summary>Che cosa sapere prima di creare il bot di progetto: il repository è pubblico?</summary>
    public sealed record ProjectBotPreflight(bool PublicRepository, string Origin);

    /// <summary>
    /// Il bot di progetto (D5-D8): scrive in un canale Telegram privato gli avvisi del progetto (commit pubblicati, scaricamenti
    /// con conflitti). Solo invia, quindi tanti MDE lo usano insieme. Token e canale stanno in <see cref="ProjectKeysFile"/>.
    /// <para>
    /// Creazione: il token arriva dal gestore (<see cref="TelegramPairingService"/>, azione <c>project</c>), poi l'utente aggiunge
    /// il bot a un canale privato come amministratore (link <c>?startchannel</c>): qui si ascolta <c>my_chat_member</c> per sapere
    /// quale canale. Solo allora si scrive il file, dopo aver riguardato l'origin (D7: mai due bot per lo stesso progetto), e si
    /// crea il commit di quel solo file. Pubblicarlo resta un gesto dell'utente.
    /// </para>
    /// </summary>
    public sealed class ProjectBotService
    {
        private const string CommitMessage = "chore(mde): bot Telegram del progetto in .mde-keys.yml";
        private static readonly TimeSpan ChannelWait = TimeSpan.FromMinutes(30);

        private readonly TelegramBotApi _api;
        private readonly INativeGitRunner _git;
        private readonly IServiceScopeFactory _scopes;
        private readonly IHttpClientFactory _http;
        private readonly TelegramRemoteService _remote;
        private readonly ILogger<ProjectBotService> _logger;
        private readonly object _lock = new();

        private Setup _setup;

        private sealed class Setup
        {
            public string Root;
            public string Token;
            public string BotUsername;
            public string State;
            public string Error;
            public CancellationTokenSource Cts;
        }

        public ProjectBotService(TelegramBotApi api, INativeGitRunner git, IServiceScopeFactory scopes, IHttpClientFactory http,
            TelegramRemoteService remote, ILogger<ProjectBotService> logger)
        {
            _api = api;
            _git = git;
            _scopes = scopes;
            _http = http;
            _remote = remote;
            _logger = logger;
        }

        public async Task<ProjectBotStatus> StatusAsync(string projectRoot, CancellationToken ct)
        {
            var root = RequireRoot(projectRoot);
            Setup setup;
            lock (_lock) setup = _setup != null && SamePath(_setup.Root, root) ? _setup : null;
            if (setup != null)
                return new ProjectBotStatus("setup", setup.BotUsername, null, null, setup.State, ChannelLink(setup.BotUsername), false, setup.Error);

            ProjectBotKeys keys;
            try { keys = ProjectKeysFile.ReadProjectBot(root); }
            catch (InvalidOperationException ex) { return new ProjectBotStatus("error", null, null, null, null, null, false, ex.Message); }
            if (keys == null) return new ProjectBotStatus("none", null, null, null, null, null, false, null);

            string subscription = "unknown", error = null;
            var owner = _remote.OwnerId;
            if (owner != 0)
            {
                try
                {
                    var member = await _api.CallAsync(keys.Token, "getChatMember", new { chat_id = keys.ChannelId, user_id = owner }, ct);
                    var status = member?["status"]?.GetValue<string>();
                    subscription = status is "member" or "administrator" or "creator" ? "subscribed" : "not-subscribed";
                }
                catch (TelegramApiException ex) when (ex.ErrorCode == 400)
                {
                    subscription = "not-subscribed";   // «user not found» per chi non è mai entrato
                }
                catch (TelegramApiException ex) when (ex.ErrorCode == 401)
                {
                    error = TokenChanged();
                }
                catch (Exception ex) when (ex is TelegramApiException or HttpRequestException or TaskCanceledException)
                {
                    error = UiText.Say($"Telegram non risponde: {ex.Message}", $"Telegram is not answering: {ex.Message}");
                }
            }
            var notPublished = await NotPublishedAsync(root, ct);
            return new ProjectBotStatus("configured", keys.BotUsername, keys.ChannelTitle, subscription, null, null, notPublished, error);
        }

        /// <summary>
        /// Prima del «Crea il bot di progetto»: il progetto non deve averne già uno, né qui né sull'origin (D7, regola 1). Dice
        /// se il repository è pubblico: in quel caso chi ha il repo controlla il bot, e l'utente deve confermare (regola 6).
        /// </summary>
        public async Task<ProjectBotPreflight> PreflightAsync(string projectRoot, CancellationToken ct)
        {
            var root = RequireRoot(projectRoot);
            if (ProjectKeysFile.ReadProjectBot(root) != null)
                throw new InvalidOperationException(UiText.Say("Il progetto ha già un bot Telegram.", "The project already has a Telegram bot."));
            await RequireNoneOnOriginAsync(root, ct);
            var origin = await OriginUrlAsync(root, ct);
            return new ProjectBotPreflight(await IsPublicGitHubAsync(origin, ct), origin);
        }

        /// <summary>Il token del bot appena creato (confermato dall'utente): si aspetta che lo aggiunga a un canale.</summary>
        public async Task AcceptTokenAsync(string projectRoot, string token, string botUsername, CancellationToken ct)
        {
            var root = RequireRoot(projectRoot);
            // Il bot appena nato non ha webhook, ma un deleteWebhook rende getUpdates sicuro comunque.
            await _api.CallAsync(token, "deleteWebhook", new { drop_pending_updates = true }, ct);
            var setup = new Setup { Root = root, Token = token, BotUsername = botUsername, State = "waiting-channel", Cts = new CancellationTokenSource(ChannelWait) };
            lock (_lock)
            {
                _setup?.Cts.Cancel();
                _setup = setup;
            }
            _ = Task.Run(() => WaitForChannelAsync(setup));
        }

        public void CancelSetup(string projectRoot)
        {
            lock (_lock)
            {
                if (_setup == null || !SamePath(_setup.Root, projectRoot)) return;
                _setup.Cts.Cancel();
                _setup = null;
            }
        }

        /// <summary>Un link d'invito al canale per una sola persona (D5: iscrizione volontaria), valido un giorno.</summary>
        public async Task<string> InviteAsync(string projectRoot, CancellationToken ct)
        {
            var keys = RequireKeys(projectRoot);
            var name = "MDE " + (_remote.Status.OwnerName ?? _remote.InstallationName);   // Telegram: al massimo 32 caratteri
            try
            {
                var link = await _api.CallAsync(keys.Token, "createChatInviteLink", new
                {
                    chat_id = keys.ChannelId,
                    member_limit = 1,
                    expire_date = DateTimeOffset.UtcNow.AddDays(1).ToUnixTimeSeconds(),
                    name = name.Length > 32 ? name.Substring(0, 32) : name,
                }, ct);
                return link!["invite_link"]!.GetValue<string>();
            }
            catch (TelegramApiException ex) when (ex.ErrorCode == 401) { throw new InvalidOperationException(TokenChanged()); }
        }

        /// <summary>Un messaggio di prova nel canale.</summary>
        public async Task SendTestAsync(string projectRoot, CancellationToken ct)
        {
            var keys = RequireKeys(projectRoot);
            try
            {
                await _api.SendMessageAsync(keys.Token, keys.ChannelId, UiText.Say(
                    $"Prova da «{_remote.InstallationName}»: gli avvisi del progetto arrivano qui.",
                    $"Test from «{_remote.InstallationName}»: project notices arrive here."), ct);
            }
            catch (TelegramApiException ex) when (ex.ErrorCode == 401) { throw new InvalidOperationException(TokenChanged()); }
        }

        /// <summary>Un avviso nel canale del progetto. Non deve mai disturbare chi l'ha causato: un fallimento finisce nel log.</summary>
        public async Task NotifyAsync(string projectRoot, string text, CancellationToken ct)
        {
            ProjectBotKeys keys;
            try { keys = ProjectKeysFile.ReadProjectBot(projectRoot); }
            catch (InvalidOperationException ex) { _logger.LogWarning("[Telegram] avviso di progetto non mandato: {Why}", ex.Message); return; }
            if (keys == null) return;
            try { await _api.SendMessageAsync(keys.Token, keys.ChannelId, text, ct); }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning("[Telegram] avviso di progetto non consegnato a @{Bot}: {Why}", keys.BotUsername,
                    ex is TelegramApiException { ErrorCode: 401 } ? TokenChanged() : ex.Message);
            }
        }

        // ------------------------------------------------------------------ creazione: il canale

        private async Task WaitForChannelAsync(Setup setup)
        {
            var ct = setup.Cts.Token;
            long offset = 0;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    var updates = await _api.CallAsync(setup.Token, "getUpdates",
                        new { offset, timeout = 25, allowed_updates = new[] { "my_chat_member" } }, ct);
                    foreach (var update in updates.AsArray())
                    {
                        offset = update["update_id"]!.GetValue<long>() + 1;
                        var change = update["my_chat_member"];
                        var chat = change?["chat"];
                        var member = change?["new_chat_member"];
                        if (chat?["type"]?.GetValue<string>() != "channel" || member?["status"]?.GetValue<string>() != "administrator") continue;
                        if (member["can_post_messages"]?.GetValue<bool>() != true || member["can_invite_users"]?.GetValue<bool>() != true)
                        {
                            Fail(setup, UiText.Say(
                                "Il bot è amministratore del canale ma senza i permessi «Pubblica messaggi» e «Invita utenti»: dàglieli e riprova.",
                                "The bot is a channel administrator without the «Post messages» and «Invite users» rights: grant them and try again."), keepWaiting: true);
                            continue;
                        }
                        var keys = new ProjectBotKeys(setup.Token, setup.BotUsername, chat["id"]!.GetValue<long>(), chat["title"]?.GetValue<string>());
                        await SaveAsync(setup, keys, ct);
                        return;
                    }
                }
            }
            catch (OperationCanceledException) { if (!IsCurrent(setup)) return; Fail(setup, UiText.Say("Tempo scaduto: nessun canale ricevuto. Ricomincia da «Crea il bot di progetto».", "Time is up: no channel received. Start again from «Create the project bot»."), false); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Telegram] creazione del bot di progetto");
                Fail(setup, ex.Message, false);
            }
        }

        private async Task SaveAsync(Setup setup, ProjectBotKeys keys, CancellationToken ct)
        {
            // L'origin di nuovo: qualcuno potrebbe aver pubblicato un bot nel frattempo.
            await RequireNoneOnOriginAsync(setup.Root, ct);
            ProjectKeysFile.WriteProjectBot(setup.Root, keys);
            if (await IsRepositoryAsync(setup.Root, ct))
            {
                var add = await _git.RunAsync(setup.Root, new[] { "add", "--", ProjectKeysFile.FileName }, ct);
                var commit = add.Ok ? await _git.RunAsync(setup.Root, new[] { "commit", "-m", CommitMessage, "--", ProjectKeysFile.FileName }, ct) : add;
                if (!commit.Ok)
                    _logger.LogWarning("[Telegram] {File} scritto ma non committato: {Why}", ProjectKeysFile.FileName, commit.Describe());
            }
            await _api.SendMessageAsync(keys.Token, keys.ChannelId, UiText.Say(
                $"Questo canale riceve gli avvisi del progetto «{Path.GetFileName(setup.Root)}»: commit pubblicati e scaricamenti con conflitti.",
                $"This channel receives the notices of project «{Path.GetFileName(setup.Root)}»: pushed commits and pulls with conflicts."), ct);
            lock (_lock) if (_setup == setup) _setup = null;
            _logger.LogInformation("[Telegram] bot di progetto @{Bot} nel canale «{Channel}»", keys.BotUsername, keys.ChannelTitle);
        }

        private void Fail(Setup setup, string error, bool keepWaiting)
        {
            lock (_lock)
            {
                if (_setup != setup) return;
                setup.Error = error;
                if (!keepWaiting) setup.State = "failed";
            }
        }

        private bool IsCurrent(Setup setup) { lock (_lock) return _setup == setup; }

        private static string ChannelLink(string botUsername)
            => $"https://t.me/{botUsername}?startchannel&admin=post_messages+invite_users";

        // ------------------------------------------------------------------ git

        /// <summary>D7, regola 1: se l'origin ha già il bot si usa quello (si scarica), non se ne crea un altro. Fetch fallito = errore.</summary>
        private async Task RequireNoneOnOriginAsync(string root, CancellationToken ct)
        {
            if (!await IsRepositoryAsync(root, ct) || await OriginUrlAsync(root, ct) == null) return;
            using (var scope = _scopes.CreateScope())
            {
                var fetch = await scope.ServiceProvider.GetRequiredService<INativeGitTransport>().FetchAsync(root, "origin", ct);
                if (!fetch.Ok)
                    throw new InvalidOperationException(UiText.Say(
                        $"Non riesco a leggere l'origin per controllare che il progetto non abbia già un bot: {fetch.Error}",
                        $"I cannot read the origin to check that the project has no bot yet: {fetch.Error}"));
            }
            var upstream = await _git.RunAsync(root, new[] { "rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{u}" }, ct);
            var reference = upstream.Ok ? upstream.Stdout.Trim() : "origin/HEAD";
            var show = await _git.RunAsync(root, new[] { "show", $"{reference}:{ProjectKeysFile.FileName}" }, ct);
            if (!show.Ok) return;   // il file non c'è sull'origin (o l'origin è vuoto)
            if (ProjectKeysFile.ParseProjectBot(show.Stdout, $"{reference}:{ProjectKeysFile.FileName}") != null)
                throw new InvalidOperationException(UiText.Say(
                    "Sull'origin il progetto ha già un bot Telegram: scarica gli aggiornamenti e usa quello.",
                    "On the origin the project already has a Telegram bot: pull the updates and use that one."));
        }

        private async Task<bool> NotPublishedAsync(string root, CancellationToken ct)
        {
            if (!await IsRepositoryAsync(root, ct)) return false;
            var tracked = await _git.RunAsync(root, new[] { "ls-files", "--error-unmatch", "--", ProjectKeysFile.FileName }, ct);
            if (!tracked.Ok) return true;
            var ahead = await _git.RunAsync(root, new[] { "log", "--oneline", "@{u}..HEAD", "--", ProjectKeysFile.FileName }, ct);
            return ahead.Ok && !string.IsNullOrWhiteSpace(ahead.Stdout);
        }

        private async Task<bool> IsRepositoryAsync(string root, CancellationToken ct)
            => (await _git.RunAsync(root, new[] { "rev-parse", "--is-inside-work-tree" }, ct)).Ok;

        private async Task<string> OriginUrlAsync(string root, CancellationToken ct)
        {
            var r = await _git.RunAsync(root, new[] { "remote", "get-url", "origin" }, ct);
            return r.Ok ? r.Stdout.Trim() : null;
        }

        private static readonly Regex GitHubRepo = new(@"github\.com[:/]([^/]+)/([^/]+?)(?:\.git)?/?$", RegexOptions.IgnoreCase);

        /// <summary>Solo GitHub dice senza credenziali se un repository è pubblico: per gli altri host vale l'avviso generico.</summary>
        private async Task<bool> IsPublicGitHubAsync(string origin, CancellationToken ct)
        {
            var m = origin == null ? null : GitHubRepo.Match(origin);
            if (m == null || !m.Success) return false;
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/repos/{m.Groups[1].Value}/{m.Groups[2].Value}");
                request.Headers.UserAgent.ParseAdd("MdExplorer");
                using var response = await _http.CreateClient(TelegramBotApi.HttpClientName).SendAsync(request, ct);
                if (!response.IsSuccessStatusCode) return false;   // 404 = privato (o inesistente)
                var body = JsonNode.Parse(await response.Content.ReadAsStringAsync(ct));
                return body?["private"]?.GetValue<bool>() == false;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogWarning("[Telegram] visibilità del repository non verificata: {Why}", ex.Message);
                return false;
            }
        }

        // ------------------------------------------------------------------ varie

        private static ProjectBotKeys RequireKeys(string projectRoot)
            => ProjectKeysFile.ReadProjectBot(RequireRoot(projectRoot))
               ?? throw new InvalidOperationException(UiText.Say("Il progetto non ha un bot Telegram.", "The project has no Telegram bot."));

        private static string RequireRoot(string projectRoot)
        {
            if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
                throw new ArgumentException(UiText.Say("Cartella del progetto inesistente.", "Project folder does not exist."));
            return Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        private static bool SamePath(string a, string b)
            => string.Equals(Path.GetFullPath(a).TrimEnd('/', '\\'), Path.GetFullPath(b).TrimEnd('/', '\\'),
                OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);

        private static string TokenChanged() => UiText.Say(
            "Telegram rifiuta il token del bot di progetto: probabilmente è stato rigenerato. Scarica gli aggiornamenti del progetto.",
            "Telegram rejects the project bot token: it was probably regenerated. Pull the project updates.");
    }
}

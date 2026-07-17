using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.EngineDB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Agents;
using MdExplorer.Services.AgentRegistry;
using MdExplorer.Services.DatabaseManager;
using Microsoft.Extensions.DependencyInjection;

namespace MdExplorer.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Contesto di un test A2A: ospita il Service (<see cref="AgentCityFactory"/>), espone un
    /// client HTTP per esercitare gli endpoint come farebbe un agente, e helper white-box per
    /// il setup (seed di un progetto, trust di un cittadino, conio di un RunToken) e per le
    /// asserzioni sullo stato della mailbox (poll dello stato dei messaggi, che il dispatcher
    /// aggiorna in modo asincrono).
    /// </summary>
    public sealed class AgentCityContext : IDisposable
    {
        public AgentCityFactory Factory { get; }
        public HttpClient Client { get; }
        public FakeAgentTurnRunner Runner => Factory.Runner;
        public FakeAgentRunGate Gate => Factory.Gate;
        public FakeFederationSender FederationSender => Factory.FederationSender;

        public AgentCityContext()
        {
            Factory = new AgentCityFactory();
            Client = Factory.CreateClient();
        }

        // ---- setup white-box ----

        /// <summary>Crea una directory di progetto temp e la registra nella UserDB; ritorna (Id, path).</summary>
        public (Guid Id, string Path) SeedProject(string name)
        {
            var path = Path.Combine(Factory.DataDir, "projects", name);
            Directory.CreateDirectory(path);

            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var project = new Project { Name = name, Path = path, LastUpdate = DateTime.UtcNow };
            db.GetDal<Project>().Save(project);   // Id GuidComb assegnato al Save
            db.Commit();
            return (project.Id, path);
        }

        /// <summary>
        /// Scrive un <c>&lt;name&gt;.agent.md</c> con blocco a2a: su disco (autorità del
        /// contenuto). Da solo NON rende l'agente un cittadino: chiama poi
        /// <see cref="IndexAgentFiles"/> per popolare l'indice Engine DB da cui il registry
        /// scopre gli agenti.
        /// </summary>
        public void WriteLlmCitizen(string projectPath, string name, string role,
            IEnumerable<string> acceptsMessagesFrom, int? maxHops = null)
        {
            // Ogni voce quotata: in YAML un '*' nudo è un alias e romperebbe il blocco a2a:.
            var accepts = string.Join(", ",
                (acceptsMessagesFrom ?? Enumerable.Empty<string>()).Select(a => $"\"{a}\""));
            var hops = maxHops.HasValue ? $"\n  max_hops: {maxHops.Value}" : string.Empty;
            var md = $@"---
a2a:
  name: {name}
  role: {role}
  accepts_messages_from: [{accepts}]{hops}
---
# {name}

Sei l'agente {name}.";
            File.WriteAllText(Path.Combine(projectPath, $"{name}.agent.md"), md);
        }

        /// <summary>
        /// Indicizza nell'Engine DB tutti gli <c>.agent.md</c> presenti su disco nel progetto,
        /// in <b>una sola sessione</b> (aperta e chiusa) — è così che il registry li scopre
        /// (indice → rilettura da disco). Idempotente: non duplica righe già presenti.
        /// </summary>
        public void IndexAgentFiles(string projectPath)
        {
            var dbm = Factory.Services.GetRequiredService<IDatabaseManager>();
            using var engine = dbm.CreateIsolatedEngineDBForProjectPath(projectPath);
            engine.BeginTransaction();
            var dal = engine.GetDal<MarkdownFile>();
            var existing = dal.GetList().Select(m => m.Path).ToList();
            // Niente pattern "*.agent.md": il doppio punto non matcha in modo affidabile.
            var agentFiles = Directory.EnumerateFiles(projectPath)
                .Where(f => f.EndsWith(".agent.md", System.StringComparison.OrdinalIgnoreCase));
            foreach (var file in agentFiles)
            {
                if (existing.Any(p => string.Equals(p, file, System.StringComparison.OrdinalIgnoreCase)))
                    continue;
                dal.Save(new MarkdownFile
                {
                    FileName = Path.GetFileName(file),
                    Path = file,
                    FileType = ".md",
                });
            }
            engine.Commit();
        }

        /// <summary>Conferma il trust di un cittadino (ri-discovery + trust via registry).</summary>
        public void Trust(string projectPath, string agentName)
        {
            using var scope = Factory.Services.CreateScope();
            var registry = scope.ServiceProvider.GetRequiredService<IAgentRegistryService>();
            registry.RefreshCatalog(projectPath);
            registry.TrustAgent(projectPath, agentName);
        }

        /// <summary>Abilita Fuseki per un progetto puntando a un dataset dato (memoria, Fase 5).</summary>
        public void EnableFuseki(Guid projectId, string uri, string dataset)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var project = db.GetDal<Project>().GetList().First(p => p.Id == projectId);
            var dal = db.GetDal<ProjectFusekiSettings>();
            var settings = dal.GetList().FirstOrDefault(s => s.Project.Id == projectId)
                ?? new ProjectFusekiSettings { Project = project };
            settings.Enabled = true;
            settings.Uri = uri;
            settings.Dataset = dataset;
            settings.Username = string.Empty;
            dal.Save(settings);
            db.Commit();
        }

        /// <summary>Abilita Fuseki GESTITO per un progetto (il Service avvia l'istanza; Uri ignorato).</summary>
        public void EnableManagedFuseki(Guid projectId, string dataset)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var project = db.GetDal<Project>().GetList().First(p => p.Id == projectId);
            var dal = db.GetDal<ProjectFusekiSettings>();
            var settings = dal.GetList().FirstOrDefault(s => s.Project.Id == projectId)
                ?? new ProjectFusekiSettings { Project = project };
            settings.Enabled = true;
            settings.Managed = true;
            settings.Dataset = dataset;
            settings.Username = string.Empty;
            dal.Save(settings);
            db.Commit();
        }

        /// <summary>Conia un RunToken legato a queste claim (per esercitare il canale autenticato).</summary>
        public string MintRunToken(string agentName, string projectPath, string conversationId)
        {
            var store = Factory.Services.GetRequiredService<IRunTokenStore>();
            return store.Mint(new RunTokenClaims
            {
                RunId = Guid.NewGuid(),
                AgentName = agentName,
                ProjectPath = projectPath,
                ConversationId = conversationId,
            });
        }

        /// <summary>
        /// POST autenticato a <c>/api/A2A/messages/send</c> col RunToken nell'header
        /// <c>X-MDE-Run-Token</c> (il mittente è certificato dal token, non dal body).
        /// </summary>
        public async Task<(System.Net.HttpStatusCode Status, string Body)> SendAuthenticated(
            string runToken, string toAgent, string message, IEnumerable<string> topics = null)
        {
            var payload = new Dictionary<string, object> { ["toAgent"] = toAgent, ["message"] = message };
            if (topics != null) payload["topics"] = topics;

            var content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var req = new HttpRequestMessage(HttpMethod.Post, "/api/A2A/messages/send") { Content = content };
            if (runToken != null) req.Headers.Add("X-MDE-Run-Token", runToken);

            var resp = await Client.SendAsync(req);
            return (resp.StatusCode, await resp.Content.ReadAsStringAsync());
        }

        // ---- porta dell'umano sulla mailbox (Fase 4a, /api/A2A/mailbox) ----

        /// <summary>GET della inbox dell'umano (messaggi to:user). Ritorna il JSON grezzo.</summary>
        public async Task<(System.Net.HttpStatusCode Status, System.Text.Json.JsonDocument Json)> GetInbox(
            string projectPath, bool includeRead = false)
        {
            var url = $"/api/A2A/mailbox/inbox?projectPath={Uri.EscapeDataString(projectPath)}&includeRead={includeRead}";
            var resp = await Client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            return (resp.StatusCode, System.Text.Json.JsonDocument.Parse(body));
        }

        /// <summary>Marca letto un messaggio to:user.</summary>
        public async Task<System.Net.HttpStatusCode> MarkRead(Guid messageId)
        {
            var resp = await Client.PostAsync($"/api/A2A/mailbox/inbox/{messageId}/read", null);
            return resp.StatusCode;
        }

        /// <summary>L'umano risponde in un thread: POST /api/A2A/mailbox/reply.</summary>
        public async Task<(System.Net.HttpStatusCode Status, string Body)> Reply(string conversationId, string body)
        {
            var payload = System.Text.Json.JsonSerializer.Serialize(
                new { conversationId, body });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            var resp = await Client.PostAsync("/api/A2A/mailbox/reply", content);
            return (resp.StatusCode, await resp.Content.ReadAsStringAsync());
        }

        // ---- helper HTTP generici ----

        public async Task<(System.Net.HttpStatusCode Status, System.Text.Json.JsonDocument Json)> GetJson(string url)
        {
            var resp = await Client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            return (resp.StatusCode, System.Text.Json.JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body));
        }

        public async Task<(System.Net.HttpStatusCode Status, string Body)> PostJson(string url, string json)
        {
            HttpContent content = json == null ? null
                : new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await Client.PostAsync(url, content);
            return (resp.StatusCode, await resp.Content.ReadAsStringAsync());
        }

        // ---- coda differita: pausa utente locale (Fase 6c) ----

        /// <summary>Mette in pausa un agente su questa "macchina" (riga AgentPause in UserDB).</summary>
        public void PauseAgent(string projectPath, string agentName, string reason = null)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            db.GetDal<AgentPause>().Save(new AgentPause
            {
                ProjectPath = projectPath,
                AgentName = agentName,
                Reason = reason,
                CreatedAt = DateTime.UtcNow,
            });
            db.Commit();
        }

        /// <summary>Toglie la pausa (elimina le righe AgentPause del progetto+agente).</summary>
        public void ResumeAgent(string projectPath, string agentName)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<AgentPause>();
            var rows = dal.GetList().ToList()
                .Where(p => string.Equals(p.AgentName, agentName, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(p.ProjectPath, projectPath, StringComparison.OrdinalIgnoreCase))
                .ToList();
            foreach (var r in rows) dal.Delete(r);
            db.Commit();
        }

        // ---- governance dei thread (Fase 4b) ----

        /// <summary>White-box: crea una conversazione (per esercitare gli hop d'origine) e ne ritorna l'Id.</summary>
        public Guid SeedConversation(string projectPath, string startedBy = "external", int hopLimit = 8)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var conv = new AgentConversation
            {
                ProjectPath = projectPath,
                StartedBy = startedBy,
                Status = AgentConversation.StatusEnum.Active,
                HopCount = 0,
                HopLimit = hopLimit,
                StartedAt = DateTime.UtcNow,
                LastActivityAt = DateTime.UtcNow,
            };
            db.GetDal<AgentConversation>().Save(conv);
            db.Commit();
            return conv.Id;
        }

        /// <summary>White-box: forza lo stato (ed eventualmente l'hopCount) di una conversazione.</summary>
        public void SetConversationStatus(Guid id, string status, int? hopCount = null)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<AgentConversation>();
            var conv = dal.GetList().First(c => c.Id == id);
            conv.Status = status;
            if (hopCount.HasValue) conv.HopCount = hopCount.Value;
            dal.Save(conv);
            db.Commit();
        }

        /// <summary>White-box: marca una conversazione come federata (correlazione §12.6).</summary>
        public void MarkConversationFederated(Guid id, Guid federationId, string remoteOwner, string remoteAgent)
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var dal = db.GetDal<AgentConversation>();
            var conv = dal.GetList().First(c => c.Id == id);
            conv.FederationId = federationId;
            conv.RemoteOwner = remoteOwner;
            conv.RemoteAgent = remoteAgent;
            dal.Save(conv);
            db.Commit();
        }

        public async Task<(System.Net.HttpStatusCode Status, System.Text.Json.JsonDocument Json)> GetConversations(string projectPath)
        {
            var url = $"/api/A2A/mailbox/conversations?projectPath={Uri.EscapeDataString(projectPath)}";
            var resp = await Client.GetAsync(url);
            var body = await resp.Content.ReadAsStringAsync();
            return (resp.StatusCode, System.Text.Json.JsonDocument.Parse(body));
        }

        public async Task<(System.Net.HttpStatusCode Status, System.Text.Json.JsonDocument Json)> GetConversationMessages(Guid id)
        {
            var resp = await Client.GetAsync($"/api/A2A/mailbox/conversations/{id}/messages");
            var body = await resp.Content.ReadAsStringAsync();
            return (resp.StatusCode, System.Text.Json.JsonDocument.Parse(body));
        }

        public async Task<System.Net.HttpStatusCode> Kill(Guid id)
            => (await Client.PostAsync($"/api/A2A/mailbox/conversations/{id}/kill", null)).StatusCode;

        public async Task<(System.Net.HttpStatusCode Status, string Body)> Reopen(Guid id)
        {
            var resp = await Client.PostAsync($"/api/A2A/mailbox/conversations/{id}/reopen", null);
            return (resp.StatusCode, await resp.Content.ReadAsStringAsync());
        }

        // ---- asserzioni sulla mailbox ----

        public List<AgentMessage> Messages()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var list = db.GetDal<AgentMessage>().GetList().ToList();
            db.Commit();
            return list;
        }

        public List<AgentConversation> Conversations()
        {
            using var scope = Factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IUserSettingsDB>();
            db.BeginTransaction();
            var list = db.GetDal<AgentConversation>().GetList().ToList();
            db.Commit();
            return list;
        }

        /// <summary>
        /// Attende (poll ogni 250ms) che una condizione sui messaggi sia vera, entro il timeout.
        /// Il dispatcher lavora in background: le transizioni di stato sono asincrone.
        /// </summary>
        public async Task<List<AgentMessage>> WaitForMessages(
            Func<List<AgentMessage>, bool> predicate, int timeoutMs = 15000)
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
            List<AgentMessage> last = Messages();
            while (DateTime.UtcNow < deadline)
            {
                last = Messages();
                if (predicate(last)) return last;
                await Task.Delay(250);
            }
            return last;
        }

        public void Dispose()
        {
            Client.Dispose();
            Factory.Dispose();
        }
    }
}

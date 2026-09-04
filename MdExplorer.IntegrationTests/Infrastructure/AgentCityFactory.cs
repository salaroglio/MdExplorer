using System;
using System.IO;
using System.Linq;
using MdExplorer.Features.Agents;
using MdExplorer.Service.HostedServices;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace MdExplorer.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Ospita il Service reale in-process per i test della "città degli agenti", con tre
    /// accorgimenti che rendono il flusso deterministico e senza dipendenze esterne:
    /// <list type="bullet">
    /// <item><b>UserDB isolata</b>: <c>XDG_DATA_HOME</c> punta a una temp dir per-factory, così
    /// ogni run parte da un DB pulito (schema creato dalle migrazioni all'avvio) senza toccare
    /// i dati reali;</item>
    /// <item><b>niente Copilot</b>: <see cref="IAgentTurnRunner"/> è sostituito dalla
    /// <see cref="FakeAgentTurnRunner"/> (il risveglio LLM diventa esercitabile);</item>
    /// <item><b>niente effetti collaterali d'avvio</b>: rimossi gli hosted service che aprono il
    /// browser / avviano i watcher (Monitor, ApplicationInitialization); resta il dispatcher
    /// della mailbox, che è ciò che vogliamo testare.</item>
    /// </list>
    /// </summary>
    public sealed class AgentCityFactory : WebApplicationFactory<Program>
    {
        public string DataDir { get; }
        public FakeAgentTurnRunner Runner { get; } = new FakeAgentTurnRunner();
        public FakeAgentRunGate Gate { get; } = new FakeAgentRunGate();
        public FakeFederationSender FederationSender { get; } = new FakeFederationSender();

        public AgentCityFactory()
        {
            DataDir = Path.Combine(Path.GetTempPath(), "mde-inttests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(DataDir);
            // Letta da CrossPlatformPath.GetAppDataDirectory() su Linux: isola tutta la UserDB.
            Environment.SetEnvironmentVariable("XDG_DATA_HOME", DataDir);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Harness ermetico anche sulla config: se la macchina di chi esegue i test ha una
            // API key vera del relay (env MdChat__ApiKey o appsettings.Development.json), la
            // catena di risoluzione ricadrebbe su quella e i test su "nessuna chiave" fallirebbero
            // solo su quella macchina. Qui la azzeriamo, come già facciamo con XDG_DATA_HOME.
            builder.ConfigureAppConfiguration(cfg => cfg.AddInMemoryCollection(
                new System.Collections.Generic.Dictionary<string, string> { ["MdChat:ApiKey"] = string.Empty }));

            builder.ConfigureTestServices(services =>
            {
                RemoveHostedService<MonitorMDHostedService>(services);
                RemoveHostedService<ApplicationInitializationService>(services);

                services.RemoveAll<IAgentTurnRunner>();
                services.AddSingleton<IAgentTurnRunner>(Runner);

                // Gate del run sostituibile: la coda differita diventa esercitabile senza
                // forzare concorrenza reale sul tetto istanze Copilot.
                services.RemoveAll<MdExplorer.Services.AgentRun.IAgentRunGate>();
                services.AddSingleton<MdExplorer.Services.AgentRun.IAgentRunGate>(Gate);

                // Sender federato sostituibile: RequestIntervention testabile senza relay vivo.
                services.RemoveAll<MdExplorer.Services.Federation.IFederationSender>();
                services.AddSingleton<MdExplorer.Services.Federation.IFederationSender>(FederationSender);
            });
        }

        private static void RemoveHostedService<T>(IServiceCollection services)
        {
            var toRemove = services
                .Where(d => d.ServiceType == typeof(IHostedService) && d.ImplementationType == typeof(T))
                .ToList();
            foreach (var d in toRemove)
                services.Remove(d);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                try { if (Directory.Exists(DataDir)) Directory.Delete(DataDir, recursive: true); }
                catch { /* best effort: temp dir */ }
            }
        }
    }
}

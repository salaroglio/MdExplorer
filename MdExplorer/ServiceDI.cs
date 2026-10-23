using MdExplorer.Service.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MdExplorer.Abstractions.Services;
using MdExplorer.Features.Services;

namespace MdExplorer.Service
{
   
    public static class ServiceDI
    {
        public static IServiceCollection AddServiceFeatures(this IServiceCollection services)
        {
            services.AddSingleton<ProcessUtil>();
            services.AddSingleton<MdExplorer.Features.Services.GitIgnore.IGitIgnoreRules, MdExplorer.Features.Services.GitIgnore.GitIgnoreRules>();
            services.AddSingleton<IMdIgnoreService, MdIgnoreService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddSingleton<IMarkdownFtsService>(sp => new MarkdownFtsService(
                MdExplorer.Utilities.CrossPlatformPath.GetAppDataPath(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MarkdownFtsService>>()));
            services.AddSingleton<ITextFtsService>(sp => new TextFtsService(
                MdExplorer.Utilities.CrossPlatformPath.GetAppDataPath(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TextFtsService>>()));
            // Grafo del codice (sprint 2026-10-09, D19): vive nel Fuseki del pacchetto, un dataset `code` e un grafo
            // nominato per progetto; lo store carica gli script di .mde-code/ ed è il modello di lettura per API e tool.
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeGraphStore, MdExplorer.Features.Services.CodeGraph.CodeGraphStore>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.CodeRuleService>();
            services.AddSingleton<MdExplorer.Services.CodeGraph.CodeRulePending>();
            services.AddSingleton<MdExplorer.Services.UserSettingsUiLanguage>();
            services.AddSingleton<MdExplorer.Features.Services.IUiLanguage>(sp => sp.GetRequiredService<MdExplorer.Services.UserSettingsUiLanguage>());
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.CodeGraphReportWriter>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.Grammars.GrammarStore>(sp =>
                new MdExplorer.Features.Services.CodeGraph.Grammars.GrammarStore(MdExplorer.Utilities.CrossPlatformPath.GetAppDataPath()));
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.Grammars.CodeGraphOptions>(sp =>
            {
                var cfg = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>();
                var o = new MdExplorer.Features.Services.CodeGraph.Grammars.CodeGraphOptions();
                var url = cfg["MdExplorer:GrammarApiUrl"];
                if (!string.IsNullOrWhiteSpace(url)) o.GrammarApiUrl = url;
                return o;
            });
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.Grammars.IGrammarDownloader, MdExplorer.Features.Services.CodeGraph.Grammars.GrammarDownloader>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeExtractor, MdExplorer.Features.Services.CodeGraph.RoslynCSharpExtractor>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeExtractor, MdExplorer.Features.Services.CodeGraph.TreeSitterTypeScriptExtractor>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeExtractor, MdExplorer.Features.Services.CodeGraph.TreeSitterPythonExtractor>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeExtractor, MdExplorer.Features.Services.CodeGraph.TreeSitterJavaExtractor>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeExtractor, MdExplorer.Features.Services.CodeGraph.TreeSitterJavaScriptExtractor>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeExtractor, MdExplorer.Features.Services.CodeGraph.PowerShellExtractor>();
            // Radici di codice esterne (sprint 2026-10-12): identità nel .development.yml, posizione nello UserDB
            services.AddSingleton<MdExplorer.Services.CodeGraph.ICodeRootBindingStore, MdExplorer.Services.CodeGraph.UserDbCodeRootBindingStore>();
            services.AddSingleton<MdExplorer.Services.CodeGraph.ICodeRootService, MdExplorer.Services.CodeGraph.CodeRootService>();
            // Cartelle remote via SSH (sprint 2026-10-21-Cartella-Remota-Via-SSH, F2): ssh di sistema, MDE Remote per cartella.
            services.AddSingleton<MdExplorer.Services.RemoteFolders.IRemoteServerSettings, MdExplorer.Services.RemoteFolders.UserDbRemoteServerSettings>();
            services.AddSingleton<MdExplorer.Services.RemoteFolders.IRemoteTransport>(_ => new MdExplorer.Services.RemoteFolders.SshTransport());
            services.AddSingleton<MdExplorer.Services.RemoteFolders.IRemoteSessions>(sp => new MdExplorer.Services.RemoteFolders.RemoteSessions(
                sp.GetRequiredService<MdExplorer.Services.RemoteFolders.IRemoteTransport>(),
                () => MdExplorer.Services.RemoteFolders.RemoteAgentBinary.Load(MdExplorer.Services.RemoteFolders.RemoteAgentBinary.DefaultLocalPath),
                sp.GetRequiredService<MdExplorer.Services.RemoteFolders.IRemoteServerSettings>(),
                sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<MdExplorer.Services.RemoteFolders.RemoteSessions>>()));
            services.AddSingleton<MdExplorer.Services.RemoteFolders.RemoteCodeRootFiles>();
            services.AddSingleton<MdExplorer.Services.CodeGraph.ICodeGraphTurnRefresher, MdExplorer.Services.CodeGraph.CodeGraphTurnRefresher>();
            services.AddSingleton<MdExplorer.Features.Services.CodeGraph.ICodeExtractorRegistry, MdExplorer.Features.Services.CodeGraph.CodeExtractorRegistry>();
            services.AddSingleton<MdExplorer.Services.CodeGraph.ICodeChangeScanner, MdExplorer.Services.CodeGraph.CodeChangeScanner>();
            services.AddSingleton<MdExplorer.Services.CodeGraph.ICodeAnalysisService, MdExplorer.Services.CodeGraph.CodeAnalysisService>();
            services.AddSingleton<MdExplorer.Services.CodeGraph.ICodeDocJobService, MdExplorer.Services.CodeGraph.CodeDocJobService>();
            return services;
        }
    }
}

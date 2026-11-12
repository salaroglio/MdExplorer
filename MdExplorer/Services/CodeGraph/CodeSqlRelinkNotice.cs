using System;
using System.Linq;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.CodeGraph;
using MdExplorer.Hubs;
using MdExplorer.Services.Git;
using Microsoft.AspNetCore.SignalR;

namespace MdExplorer.Services.CodeGraph
{
    /// <summary>
    /// L'avviso del ricollegamento dell'SQL (12/11/2026) quando non lo chiede l'utente — all'apertura del progetto, dopo un pull:
    /// MDE ha riscritto degli script del grafo, che restano da salvare su Git. Stesso evento dei file di sistema di MDE
    /// (<see cref="MdeAutoCommit.NoticeEvent"/>): lo mostra la finestra del progetto. Dice che cosa è cambiato, non il motivo.
    /// </summary>
    public static class CodeSqlRelinkNotice
    {
        public static async Task SendAsync(IHubContext<MonitorMDHub> hub, string projectPath, CodeScriptLoadOutcome outcome)
        {
            if (hub == null || outcome == null || outcome.SqlRelinked == 0) return;
            var scripts = outcome.SqlRelinkedScripts.Count;
            var text = UiText.Say(
                $"MDE ha aggiornato {scripts} script del grafo del codice in .mde-code: {outcome.SqlRelinked} collegamenti dell'SQL agli oggetti del database. Da salvare su Git.",
                $"MDE updated {scripts} code-graph scripts in .mde-code: {outcome.SqlRelinked} links from SQL to database objects. To be committed.");
            try
            {
                await hub.Clients.All.SendAsync(MdeAutoCommit.NoticeEvent, new
                {
                    repositoryRoot = projectPath, files = outcome.SqlRelinkedScripts.ToList(), committed = false, pushed = false, pending = "None", notices = new[] { text },
                });
            }
            catch (Exception) { /* l'avviso è un di più: il log del caricamento dice già tutto */ }
        }
    }
}

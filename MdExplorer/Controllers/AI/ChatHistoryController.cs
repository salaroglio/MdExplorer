using System;
using System.Linq;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI.ChatHistory;
using MdExplorer.Services.ChatHistory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Controllers.AI
{
    /// <summary>
    /// La cronologia delle chat di MarkAgent (sprint 2026-11-24-Chat-Memorizzate, F4): l'elenco delle chat di un progetto
    /// (righe del DB utente), il contenuto di una chat (dal suo file) e l'eliminazione (riga e file).
    /// </summary>
    [Route("api/chats")]
    [ApiController]
    public class ChatHistoryController : ControllerBase
    {
        private readonly IUserSettingsDB _userSettingsDB;
        private readonly IChatFileStore _files;
        private readonly ChatHistoryRecorder _recorder;
        private readonly ILogger<ChatHistoryController> _logger;

        public ChatHistoryController(IUserSettingsDB userSettingsDB, IChatFileStore files, ChatHistoryRecorder recorder,
            ILogger<ChatHistoryController> logger)
        {
            _userSettingsDB = userSettingsDB;
            _files = files;
            _recorder = recorder;
            _logger = logger;
        }

        /// <summary>Le chat del progetto, le più recenti prima, con titolo e dettaglio; <c>fileExists</c> dice se il contenuto c'è ancora.</summary>
        [HttpGet]
        public IActionResult List([FromQuery] string projectPath)
        {
            if (string.IsNullOrWhiteSpace(projectPath))
                return BadRequest(new { message = UiText.Say("Manca il progetto delle chat (projectPath).", "The chats' project is missing (projectPath).") });

            var rows = _userSettingsDB.GetDal<ChatThread>().GetList()
                .Where(t => t.ProjectPath == projectPath)
                .OrderByDescending(t => t.LastActivityAt)
                .ToList();
            return Ok(rows.Select(t => new
            {
                id = t.Id,
                title = t.Title,
                detail = t.Detail,
                engine = t.Engine,
                modelId = t.ModelId,
                startedAt = t.StartedAt,
                lastActivityAt = t.LastActivityAt,
                fileExists = _files.Read(t.FilePath).Exists,
            }));
        }

        /// <summary>Quanto testo restituisce al massimo una lettura per intervallo (<c>chat_read</c>), se non lo si dice.</summary>
        public const int DefaultMaxChars = 60_000;

        /// <summary>
        /// Quello che c'è nel file della chat, e le righe che non si leggono. Senza parametri, tutto (la cronologia del tab).
        /// Con <paramref name="from"/>/<paramref name="to"/> (progressivi, inclusi) un intervallo; con <paramref name="maxChars"/>
        /// un tetto al testo restituito: un evento non si taglia mai, e se il tetto ferma la lettura <c>nextFrom</c> dice da
        /// dove continuare (strumento MCP <c>chat_read</c>, sprint skill che migliorano F1). Con <paramref name="projectPath"/> la
        /// chat deve essere di quel progetto.
        /// </summary>
        [HttpGet("{id:guid}/events")]
        public IActionResult Events(Guid id, [FromQuery] long? from = null, [FromQuery] long? to = null,
            [FromQuery] int? maxChars = null, [FromQuery] string projectPath = null)
        {
            var row = Find(id);
            if (row == null) return NotFound(new { message = NotInHistory(id) });
            if (!string.IsNullOrWhiteSpace(projectPath) && !string.Equals(row.ProjectPath, projectPath, StringComparison.Ordinal))
                return BadRequest(new { message = UiText.Say($"La chat {id} è di un altro progetto ({row.ProjectPath}).", $"The chat {id} belongs to another project ({row.ProjectPath}).") });
            if (from.HasValue && to.HasValue && from > to)
                return BadRequest(new { message = UiText.Say($"Intervallo vuoto: from {from} è dopo to {to}.", $"Empty range: from {from} is after to {to}.") });
            var content = _files.Read(row.FilePath);
            if (!content.Exists)
                return NotFound(new { message = UiText.Say($"Il file della chat non c'è più: {row.FilePath}.", $"The chat file is gone: {row.FilePath}.") });

            var inRange = content.Events.Where(e => (!from.HasValue || e.Seq >= from) && (!to.HasValue || e.Seq <= to)).ToList();
            var lastSeq = content.Events.Count == 0 ? 0 : content.Events.Max(e => e.Seq);
            if (!maxChars.HasValue && !from.HasValue && !to.HasValue)
                return Ok(new { id = row.Id, title = row.Title, detail = row.Detail, events = inRange, problems = content.Problems, lastSeq, nextFrom = (long?)null });

            var budget = maxChars ?? DefaultMaxChars;
            var taken = new System.Collections.Generic.List<ChatRecordEvent>();
            var used = 0;
            long? nextFrom = null;
            foreach (var e in inRange)
            {
                var size = System.Text.Json.JsonSerializer.Serialize(e).Length;
                // Il primo evento entra sempre, anche se da solo supera il tetto: un evento non si taglia.
                if (taken.Count > 0 && used + size > budget) { nextFrom = e.Seq; break; }
                taken.Add(e);
                used += size;
            }
            return Ok(new { id = row.Id, title = row.Title, detail = row.Detail, events = taken, problems = content.Problems, lastSeq, nextFrom });
        }

        /// <summary>Elimina la chat: il file e la riga. Una connessione che ci stava scrivendo apre una chat nuova al turno dopo.</summary>
        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            var row = Find(id);
            if (row == null) return NotFound(new { message = NotInHistory(id) });

            _recorder.ForgetThread(id);
            var fileDeleted = _files.Delete(row.FilePath);
            _userSettingsDB.BeginTransaction();
            _userSettingsDB.GetDal<ChatThread>().Delete(row);
            _userSettingsDB.Commit();
            _logger.LogInformation("[ChatHistory] chat {ThreadId} eliminata (file {State})", id, fileDeleted ? "cancellato" : "già assente");
            return Ok(new { deleted = true, fileDeleted });
        }

        private ChatThread Find(Guid id) => _userSettingsDB.GetDal<ChatThread>().GetList().SingleOrDefault(t => t.Id == id);

        private static string NotInHistory(Guid id) => UiText.Say($"La chat {id} non è nella cronologia.", $"The chat {id} is not in the history.");
    }
}

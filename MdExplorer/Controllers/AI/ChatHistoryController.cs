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

        /// <summary>Tutto quello che c'è nel file della chat, e le righe che non si leggono.</summary>
        [HttpGet("{id:guid}/events")]
        public IActionResult Events(Guid id)
        {
            var row = Find(id);
            if (row == null) return NotFound(new { message = NotInHistory(id) });
            var content = _files.Read(row.FilePath);
            if (!content.Exists)
                return NotFound(new { message = UiText.Say($"Il file della chat non c'è più: {row.FilePath}.", $"The chat file is gone: {row.FilePath}.") });
            return Ok(new { id = row.Id, title = row.Title, detail = row.Detail, events = content.Events, problems = content.Problems });
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

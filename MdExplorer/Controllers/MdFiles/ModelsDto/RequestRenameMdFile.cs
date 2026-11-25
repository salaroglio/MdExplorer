namespace MdExplorer.Service.Controllers.MdFiles.ModelsDto
{
    /// <summary><c>POST api/mdfiles/RenameMdFile</c> (sprint 2026-11-25-Spostamento-File-E-Link).</summary>
    public class RequestRenameMdFile
    {
        /// <summary>Il documento: relativo al progetto come nell'albero, o completo.</summary>
        public string SourcePath { get; set; }
        /// <summary>Il nome nuovo, con o senza <c>.md</c>.</summary>
        public string NewName { get; set; }
        /// <summary>Il livello nell'albero, restituito com'è (lo usa la finestra delle regole).</summary>
        public int Level { get; set; }
    }
}

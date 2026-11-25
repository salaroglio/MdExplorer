using System.Collections.Generic;

namespace MdExplorer.Service.Controllers.MdFiles.ModelsDto
{
    /// <summary>
    /// <c>POST api/mdfiles/MoveMdFiles</c>: i documenti scelti nell'albero (Ctrl/Shift) e la cartella in cui portarli
    /// (sprint 2026-11-25-Spostamento-File-E-Link).
    /// </summary>
    public class RequestMoveMdFiles
    {
        /// <summary>Percorsi relativi al progetto, come li ha l'albero (<c>/docs/a.md</c> o <c>\docs\a.md</c>).</summary>
        public List<string> SourceRelativePaths { get; set; }
        /// <summary>La cartella di arrivo, completa (come la manda il trascinamento) o relativa al progetto.</summary>
        public string DestinationPath { get; set; }
    }
}

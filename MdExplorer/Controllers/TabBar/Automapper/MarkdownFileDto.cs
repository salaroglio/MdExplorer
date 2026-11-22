using MdExplorer.Abstractions.Entities.EngineDB;
using System.Collections.Generic;
using System;

namespace MdExplorer.Service.Controllers.TabBar.Automapper
{
    public class MarkdownFileDto
    {
        public virtual Guid Id { get; set; }
        public virtual string FileName { get; set; }
        public virtual string Path { get; set; }
        public virtual string FileType { get; set; }

        /// <summary>Dal file del database: i quattro campi che la scheda mostra (prima li copiava una libreria di mappatura, tolta con .NET 10).</summary>
        public static MarkdownFileDto From(MarkdownFile file)
        {
            if (file == null) return null;
            return new MarkdownFileDto
            {
                Id = file.Id,
                FileName = file.FileName,
                Path = file.Path,
                FileType = file.FileType,
            };
        }
    }
}

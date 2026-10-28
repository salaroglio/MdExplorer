using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Dove sta, su QUESTA macchina, una radice di codice esterna dichiarata nel <c>.development.yml</c> del progetto
    /// (sprint 2026-10-12-Radici-Di-Codice-Esterne, R1). Non va su Git: lo stesso alias è in <c>C:\src\backend</c> per un
    /// collega e in <c>~/work/backend</c> per un altro.
    /// </summary>
    public class CodeRootBinding
    {
        public virtual Guid Id { get; set; }

        /// <summary>Il progetto di documentazione.</summary>
        public virtual string ProjectPath { get; set; }

        /// <summary>L'alias della radice in <c>.development.yml</c> (<c>codeRoots[].alias</c>).</summary>
        public virtual string Alias { get; set; }

        /// <summary>
        /// La cartella su questa macchina: la radice del repository git (la sottocartella dichiarata si aggiunge dopo), o
        /// la cartella scelta se non è un repository.
        /// </summary>
        public virtual string LocalPath { get; set; }

        public virtual DateTime BoundAt { get; set; }
    }
}

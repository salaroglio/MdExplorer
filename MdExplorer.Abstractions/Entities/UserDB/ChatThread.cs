using System;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    /// <summary>
    /// Una chat di MarkAgent nella cronologia (sprint 2026-11-24-Chat-Memorizzate, U7). Qui c'è solo quello che serve
    /// alla lista e a riprendere la chat: il contenuto, cioè tutto quello che è tornato dal motore, sta nel file JSONL
    /// indicato da <see cref="FilePath"/>, fra i dati dell'utente. Il nome è diverso da <c>AgentConversation</c>, che è
    /// della città degli agenti.
    /// </summary>
    public class ChatThread
    {
        /// <summary>GuidComb, mai assegnato a mano: è anche il nome del file della chat.</summary>
        public virtual Guid Id { get; set; }

        public virtual string ProjectPath { get; set; }

        /// <summary>Lo dà il modello a fine turno; null finché non arriva.</summary>
        public virtual string Title { get; set; }

        /// <summary>Markdown, lo dà il modello. Per ora un elenco puntato di al massimo 5 punti (skill mde-mark-thread).</summary>
        public virtual string Detail { get; set; }

        /// <summary>Il file JSONL della chat, relativo alla cartella delle chat (<c>&lt;hash progetto&gt;/&lt;Id&gt;.jsonl</c>).</summary>
        public virtual string FilePath { get; set; }

        /// <summary>Motore e modello dell'ultimo turno.</summary>
        public virtual string Engine { get; set; }
        public virtual string ModelId { get; set; }

        /// <summary>Per far ripartire il motore da dove era; null finché il motore non l'ha data.</summary>
        public virtual string EngineSessionId { get; set; }

        public virtual DateTime StartedAt { get; set; }
        public virtual DateTime LastActivityAt { get; set; }
    }
}

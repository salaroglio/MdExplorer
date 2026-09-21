using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Abstractions.Entities.UserDB
{
    public class Project
    {
        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Path { get; set; }
        public virtual DateTime LastUpdate { get; set; }
        public virtual IList<Bookmark> Bookmarks { get; set; }
        public virtual int? SidenavWidth {  get; set; }
        public virtual string SelectedIde { get; set; }
        public virtual bool LinkIndexingEnabled { get; set; } = true;
        public virtual bool PlantUmlKeepOriginalColorsInDarkMode { get; set; } = false;
        /// <summary>
        /// Il CLI agentico con cui MarkAgent parla in questo progetto: <c>copilot</c>,
        /// <c>opencode</c>, <c>claude</c> oppure <c>none</c>.
        /// <para>
        /// <c>null</c> = <b>collegato all'ambiente agentico</b>: vale il motore dell'harness
        /// dichiarato dal repository (<c>.development.yml</c>). È lo stato normale, ed è il motivo
        /// per cui la colonna è nullable: un progetto ha UNA scelta, l'ambiente, e il motore la segue.
        /// Un valore scritto qui vuol dire «su questa macchina no, uso quest'altro CLI» — la modalità
        /// avanzata delle impostazioni.
        /// </para>
        /// <para>
        /// Sostituisce i due booleani <c>UseCopilotCliAsDefault</c>/<c>UseClaudeCodeAsDefault</c> e la
        /// loro precedenza («se sono accesi entrambi vince Claude»): due interruttori indipendenti per
        /// una scelta esclusiva. Le colonne vecchie restano nel DB perché su SQLite non si cancellano
        /// (vedi la migrazione M2026_09_21_001), ma non le legge più nessuno.
        /// </para>
        /// <para>Si risolve con <c>MarkAgentEngines.Resolve</c>, mai a mano.</para>
        /// </summary>
        public virtual string MarkAgentEngine { get; set; }

        /// <summary>
        /// Il modello con cui la chat parla a Copilot in questo progetto. <c>null</c> = lo sceglie
        /// il CLI (<c>auto</c>).
        /// <para>
        /// Per progetto, come il motore qui sopra: su un progetto grosso si tiene il modello
        /// forte, su uno piccolo quello economico. Nessun default scritto nel codice: quali
        /// modelli esistano è una proprietà dell'installazione, e un nome che l'installazione
        /// non ha non dà errore — il CLI lo rimpiazza in silenzio con un altro.
        /// </para>
        /// </summary>
        public virtual string CopilotChatModel { get; set; }

        /// <summary>
        /// Il modello con cui MarkAgent parla a Claude Code in questo progetto: un <c>value</c> dell'elenco che
        /// il CLI dichiara (<c>sonnet</c>, <c>opus[1m]</c>, <c>default</c>…). <c>null</c> = mai scelto, e la chat
        /// usa <c>sonnet</c> come faceva prima che la scelta esistesse — non un modello più caro in silenzio.
        /// </summary>
        public virtual string ClaudeCodeChatModel { get; set; }
        public virtual bool ExecutionTrusted { get; set; } = false;

        /// <summary>
        /// Isolamento worktree per-agente su QUESTA macchina. <c>null</c> = non deciso → default
        /// (acceso se il progetto è git con remoto <c>origin</c>). Vive qui e non nel
        /// <c>.development.yml</c> perché costa spazio disco locale: è una scelta della macchina,
        /// non una regola del repo.
        /// </summary>
        public virtual bool? UseAgentWorktrees { get; set; }

        /// <summary>
        /// Posti di lavoro (worktree) che gli agenti possono occupare contemporaneamente su
        /// questa macchina. <c>null</c> = non deciso → <b>2</b>. È anche il tetto fisico di
        /// quanti agenti girano insieme: il pool sostituisce il semaforo separato, così non
        /// esistono due limiti che possono dire cose diverse.
        /// </summary>
        public virtual int? AgentWorktreeSlots { get; set; }

        /// <summary>
        /// When ON, a SEPARATE background index (side-car FTS + TextFile table) is
        /// built and maintained for non-markdown text files. Completely additive:
        /// the markdown world (MarkdownFile / MdEngineFts) is never touched.
        /// Default OFF so existing projects behave exactly as before.
        /// </summary>
        public virtual bool IndexAllTextFiles { get; set; } = false;

        /// <summary>
        /// Per-project allow-list of extensions considered "text" for the text
        /// index, as a comma-separated list (e.g. ".txt,.csv,.json"). When null or
        /// empty the central default (<see cref="Services.TextFileClassifier"/>)
        /// applies. Deterministic by design: no binary sniffing.
        /// </summary>
        public virtual string TextFileExtensions { get; set; }

    }
}

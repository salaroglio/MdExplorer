using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace MdExplorer.Features.Yaml.Models
{
    /// <summary>
    /// Fonte di verità della Agent Card A2A: mappa il blocco <c>a2a:</c> del
    /// frontmatter di un <c>.agent.md</c> (§5 del design doc Agent-Harness-A2A).
    /// Chiavi in snake_case, coerenti con la <c>UnderscoredNamingConvention</c>
    /// già usata da <see cref="YamlDocumentDescriptorParser"/>.
    /// </summary>
    public class AgentCardDescriptor
    {
        /// <summary>Identità stabile, kebab-case, unica nel progetto (<c>a2a.name</c>).</summary>
        public string Name { get; set; }

        /// <summary>Ruolo umano-leggibile del cittadino (<c>a2a.role</c>).</summary>
        public string Role { get; set; }

        /// <summary>Le "skills" della Agent Card A2A.</summary>
        public IList<AgentCardSkill> Skills { get; set; } = new List<AgentCardSkill>();

        /// <summary>
        /// Whitelist dei mittenti ammessi (<c>a2a.accepts_messages_from</c>).
        /// <c>["*"]</c> = chiunque nel progetto.
        /// </summary>
        public IList<string> AcceptsMessagesFrom { get; set; } = new List<string>();

        /// <summary>
        /// Override del limite conversazione (<c>a2a.max_hops</c>).
        /// Null = default harness. Il cap (16) è applicato a valle, dal registry.
        /// </summary>
        public int? MaxHops { get; set; }

        /// <summary>
        /// A chi passa il lavoro quando la persona approva una consegna di questo agente
        /// (<c>a2a.on_approval_notify</c>): i nomi degli agenti che il pulsante «Approva» può
        /// avvisare. Uno solo = va a quello; più di uno = la persona sceglie. Null = nessun avviso
        /// (comportamento di sempre).
        /// <para>
        /// Sta nel blocco <c>a2a:</c>, quindi cambiarlo fa decadere la fiducia (R3): non si cambia la
        /// destinazione del lavoro senza che la persona se ne accorga. <b>OmitNull</b>: se non è
        /// dichiarato non compare nell'impronta, e le schede già fidate restano fidate.
        /// </para>
        /// </summary>
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public IList<string> OnApprovalNotify { get; set; }

        /// <summary>Lunghezza massima del riassunto: è testo che la persona legge prima di fidarsi, deve restare breve.</summary>
        public const int SummaryMaxLength = 500;

        /// <summary>
        /// Che cosa fa l'agente, in poche righe (<c>a2a.summary</c>): è ciò che la persona legge nella finestra di fiducia.
        /// <para>
        /// È una <b>dichiarazione dell'autore</b>, non una garanzia: la UI la mostra distinta da ciò che l'app calcola e
        /// fa rispettare (gli strumenti). Sta nel blocco <c>a2a:</c>, quindi cambiarlo fa decadere la fiducia: la persona
        /// non può aver letto una descrizione e trovarsene un'altra. <b>OmitNull</b>: una scheda senza riassunto ha la
        /// stessa impronta di prima.
        /// </para>
        /// </summary>
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public string Summary { get; set; }

        /// <summary>
        /// Le risposte che la persona può dare a questo agente (<c>a2a.replies</c>). Stanno nel blocco coperto dalla
        /// fiducia: un pulsante può inviare solo ciò che la scheda dichiara. null = nessuna dichiarata (e l'impronta
        /// delle schede che non le usano non cambia).
        /// </summary>
        [YamlMember(DefaultValuesHandling = DefaultValuesHandling.OmitNull)]
        public IList<AgentCardReply> Replies { get; set; }
    }

    /// <summary>Una skill dichiarata nella Agent Card (<c>a2a.skills[]</c>).</summary>
    /// <summary>
    /// Una risposta che la persona può dare all'agente (<c>a2a.replies</c>): diventa un pulsante sotto il messaggio.
    /// I testi possono contenere segnaposto <c>{nome}</c>, che l'agente riempie quando propone la risposta.
    /// </summary>
    public class AgentCardReply
    {
        public string Id { get; set; }
        /// <summary>Il testo del pulsante: un verbo e il suo oggetto.</summary>
        public string Label { get; set; }
        /// <summary>Che cosa succede premendo: chi viene contattato e per ottenere cosa.</summary>
        public string Description { get; set; }
        /// <summary>Il messaggio che l'agente riceve: lo stesso che la sua scheda sa trattare.</summary>
        public string Message { get; set; }
    }

    public class AgentCardSkill
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
}

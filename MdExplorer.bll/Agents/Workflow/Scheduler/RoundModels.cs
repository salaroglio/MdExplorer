using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MdExplorer.Features.Agents.Workflow.Scheduler
{
    /// <summary>
    /// Che cosa è successo a un passo di un giro. Gli eventi si aggiungono soltanto: lo stato di un passo si ricava
    /// rileggendoli, e un computer che li legge in ritardo arriva alla stessa conclusione degli altri.
    /// </summary>
    public static class RoundEventType
    {
        /// <summary>Il passo aspetta che il responsabile del suo agente lo avvii (<c>ask-owner</c>).</summary>
        public const string Held = "held";
        /// <summary>Il passo è partito (dal responsabile, da solo, o da «Fai ripartire»).</summary>
        public const string Started = "started";
        /// <summary>L'agente ha consegnato un artefatto da approvare.</summary>
        public const string Delivered = "delivered";
        public const string Approved = "approved";
        public const string Rejected = "rejected";
        /// <summary>L'agente ha finito un passo che non produce artefatti.</summary>
        public const string Done = "done";
        /// <summary>Il responsabile non ha avviato il passo, e ha detto perché.</summary>
        public const string Declined = "declined";
        public const string Failed = "failed";
        /// <summary>La persona ha premuto un pulsante sotto il messaggio di questo passo, con i suoi valori.</summary>
        public const string Replied = "replied";
        /// <summary>
        /// Chi ne risponde passa il passo a un altro responsabile dello stesso agente (W22: un collega del team, per esempio
        /// perché va in ferie). Lo scrive solo chi ne risponde adesso; da lì il passo è del nuovo.
        /// </summary>
        public const string Assigned = "assigned";

        public static readonly string[] All = { Held, Started, Delivered, Approved, Rejected, Done, Declined, Failed, Replied, Assigned };
    }

    public class RoundEvent
    {
        [JsonPropertyName("type")] public string Type { get; set; }
        [JsonPropertyName("at")] public DateTime At { get; set; }
        /// <summary>Chi: l'email git della persona sul cui computer è successo.</summary>
        [JsonPropertyName("by")] public string By { get; set; }
        /// <summary>Il giro del passo (1, 2, 3 per un ciclo «for»).</summary>
        [JsonPropertyName("round")] public int Round { get; set; } = 1;
        /// <summary>Il tentativo dentro il giro (2 e oltre: rifatto dopo un rifiuto).</summary>
        [JsonPropertyName("attempt")] public int Attempt { get; set; } = 1;
        /// <summary>Motivo di un rifiuto o di un «non lo avvio», indicazioni del responsabile all'avvio, errore.</summary>
        [JsonPropertyName("note")] public string Note { get; set; }
        /// <summary><see cref="RoundEventType.Replied"/>: l'id della risposta.</summary>
        [JsonPropertyName("reply")] public string Reply { get; set; }
        /// <summary><see cref="RoundEventType.Replied"/>: i valori che porta (le variabili del giro).</summary>
        [JsonPropertyName("values")] public Dictionary<string, string> Values { get; set; }
        /// <summary>
        /// <see cref="RoundEventType.Replied"/>: a chi va ogni passo che il pulsante fa partire (id del passo → email), quando
        /// il suo agente ha più responsabili e chi preme ha scelto (W22).
        /// </summary>
        [JsonPropertyName("assign")] public Dictionary<string, string> Assign { get; set; }
        /// <summary><see cref="RoundEventType.Assigned"/>: l'email della persona che ora ne risponde.</summary>
        [JsonPropertyName("owner")] public string Owner { get; set; }
        /// <summary><see cref="RoundEventType.Delivered"/>: il ramo pubblicato dell'agente.</summary>
        [JsonPropertyName("branch")] public string Branch { get; set; }
        /// <summary><see cref="RoundEventType.Delivered"/>: i file consegnati, dalla radice del progetto.</summary>
        [JsonPropertyName("files")] public List<string> Files { get; set; }
        /// <summary>Il turno dell'agente (RunId) che ha prodotto l'evento: lega consegna e approvazione al passo.</summary>
        [JsonPropertyName("run")] public string Run { get; set; }
        [JsonPropertyName("engine")] public string Engine { get; set; }
        [JsonPropertyName("model")] public string Model { get; set; }
    }

    /// <summary>Gli eventi di un passo in un giro. Un file per passo, scritto solo dal computer del suo responsabile.</summary>
    public class StepRecord
    {
        [JsonPropertyName("mde_round_step")] public int Format { get; set; } = 1;
        [JsonPropertyName("step")] public string Step { get; set; }
        [JsonPropertyName("agent")] public string Agent { get; set; }
        [JsonPropertyName("events")] public List<RoundEvent> Events { get; set; } = new();
    }

    /// <summary>L'apertura di un giro: scritta una volta, da chi lancia.</summary>
    public class RoundHeader
    {
        [JsonPropertyName("mde_round")] public int Format { get; set; } = 1;
        [JsonPropertyName("id")] public string Id { get; set; }
        /// <summary>Il workflow, dalla radice del progetto.</summary>
        [JsonPropertyName("workflow")] public string Workflow { get; set; }
        [JsonPropertyName("started_at")] public DateTime StartedAt { get; set; }
        [JsonPropertyName("started_by")] public string StartedBy { get; set; }
    }

    /// <summary>Un giro: l'apertura e i passi che hanno già un evento.</summary>
    public class RoundState
    {
        public RoundHeader Header { get; set; }
        public Dictionary<string, StepRecord> Steps { get; } = new();

        public StepRecord Record(string stepId) => Steps.TryGetValue(stepId, out var r) ? r : null;

        public IEnumerable<RoundEvent> EventsOf(string stepId) => Record(stepId)?.Events ?? Enumerable.Empty<RoundEvent>();

        /// <summary>
        /// Le variabili del giro: i valori dei pulsanti premuti. Se lo stesso nome arriva due volte vince il primo: un giro
        /// è su un bando, e un secondo pulsante apre un altro giro (lo decide l'esecutore), non cambia questo.
        /// </summary>
        public IReadOnlyDictionary<string, string> Variables
        {
            get
            {
                var values = new Dictionary<string, string>(StringComparer.Ordinal);
                foreach (var e in Steps.Values.SelectMany(s => s.Events)
                                              .Where(e => e.Type == RoundEventType.Replied && e.Values != null)
                                              .OrderBy(e => e.At))
                    foreach (var kv in e.Values)
                        values.TryAdd(kv.Key, kv.Value);
                return values;
            }
        }
    }
}

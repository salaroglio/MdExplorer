using System;
using System.Collections.Generic;
using System.Linq;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Una cosa che l'agente può fare sul computer di chi lo autorizza, in forma che la UI traduce in una frase.
    /// <see cref="Id"/> è una chiave stabile (non testo): le frasi stanno nei file di traduzione.
    /// </summary>
    public sealed class AgentEffect
    {
        public string Id { get; init; }

        /// <summary>True = lo può fare; false = esplicitamente no (la finestra lo dice, non lo tace).</summary>
        public bool Granted { get; init; }

        /// <summary>Azione che esce dalla sola lettura: la UI la evidenzia.</summary>
        public bool Danger { get; init; }
    }

    /// <summary>
    /// «Cosa può fare sul tuo computer», <b>calcolato dall'app</b> e non scritto dall'autore dell'agente.
    /// <para>
    /// Sta sul manifesto <c>tools:</c> perché è l'unica dichiarazione che l'app fa <b>rispettare</b> davvero
    /// (<see cref="AgentToolCatalog.NativeToolsToDeny"/> toglie al motore la shell e la scrittura che non sono dichiarate).
    /// Un testo libero dell'autore («scrive solo in gara/schede») non è verificabile e non entra qui: la UI lo mostra a
    /// parte, come dichiarazione.
    /// </para>
    /// <para>Pura, senza I/O: le stesse regole del catalogo, lette al contrario.</para>
    /// </summary>
    public static class AgentEffects
    {
        public const string ReadFiles = "read-files";
        public const string SearchDocuments = "search-documents";
        public const string WriteFiles = "write-files";
        public const string RunCommands = "run-commands";
        public const string MessageOthers = "message-others";

        public static IList<AgentEffect> For(IEnumerable<string> tools)
        {
            var declared = new HashSet<string>(
                (tools ?? Enumerable.Empty<string>()).Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()),
                StringComparer.OrdinalIgnoreCase);

            bool writes = declared.Contains(AgentToolCatalog.ManifestWrite) || declared.Contains(AgentToolCatalog.ManifestEdit);
            bool shell = declared.Contains(AgentToolCatalog.ManifestShell) || declared.Contains("execute");

            return new List<AgentEffect>
            {
                // Leggere non si nega a nessun cittadino (AgentToolCatalog): c'è sempre.
                new AgentEffect { Id = ReadFiles, Granted = true },
                new AgentEffect { Id = SearchDocuments, Granted = declared.Contains(AgentToolCatalog.ManifestSearch) },
                new AgentEffect { Id = WriteFiles, Granted = writes, Danger = writes },
                new AgentEffect { Id = RunCommands, Granted = shell, Danger = shell },
                // Il cancello è la fiducia stessa (AgentToolCatalog.OutboundTools): chi è fidato può scrivere ai colleghi.
                new AgentEffect { Id = MessageOthers, Granted = true },
            };
        }
    }
}

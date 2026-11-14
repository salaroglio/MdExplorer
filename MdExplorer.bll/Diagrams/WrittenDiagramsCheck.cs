using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.AI.CopilotSdk;

namespace MdExplorer.Features.Diagrams
{
    /// <summary>
    /// La verifica dei diagrammi di un turno per i motori senza gli hook di Copilot (Claude Code, opencode). C4 della
    /// valutazione del 07/11: con Copilot MdExplorer controllava i diagrammi dei file appena scritti e rimandava gli
    /// errori al modello; con gli altri due no, e un diagramma rotto passava in silenzio.
    /// <para>
    /// Durante il turno la sessione passa qui gli argomenti delle chiamate degli strumenti e il testo della risposta; a
    /// turno finito <see cref="TakeAsync"/> legge i file .md scritti (con lo stesso lettore di Copilot,
    /// <see cref="AgentWrittenMarkdown"/>: un file nuovo esiste solo DOPO la chiamata) e li verifica. Chi la usa fa al
    /// massimo un turno di correzione: dopo, gli errori rimasti li legge l'utente.
    /// </para>
    /// </summary>
    public sealed class WrittenDiagramsCheck
    {
        private readonly PlantumlBlockVerifier _verifier;
        private readonly string _workingDirectory;
        private readonly string _engine;
        private readonly object _gate = new();
        private readonly List<JsonElement> _toolInputs = new();
        private readonly StringBuilder _answer = new();

        public WrittenDiagramsCheck(PlantumlBlockVerifier verifier, string workingDirectory, string engine)
        {
            _verifier = verifier ?? throw new ArgumentNullException(nameof(verifier));
            _workingDirectory = workingDirectory;
            _engine = engine;
        }

        /// <summary>Gli argomenti di una chiamata di uno strumento (qualsiasi: i percorsi .md li riconosce chi legge).</summary>
        public void ToolInput(JsonElement input)
        {
            if (input.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null) return;
            lock (_gate) _toolInputs.Add(input.Clone());
        }

        /// <summary>Un pezzo del testo della risposta: anche i diagrammi scritti in chat si vedono nella pagina.</summary>
        public void AnswerText(string delta)
        {
            if (string.IsNullOrEmpty(delta)) return;
            lock (_gate) _answer.Append(delta);
        }

        public void Reset()
        {
            lock (_gate) { _toolInputs.Clear(); _answer.Clear(); }
        }

        /// <summary>
        /// Verifica quello che il turno ha scritto e ricomincia da zero. <c>Notice</c>: la riga per la chat (null se non
        /// c'erano diagrammi); <c>ForModel</c>: la spiegazione da rimandare al modello (null se non c'è niente da
        /// correggere, o se <paramref name="lastRound"/>).
        /// </summary>
        public async Task<(string Notice, string ForModel)> TakeAsync(bool lastRound, CancellationToken ct)
        {
            List<JsonElement> inputs;
            string answer;
            lock (_gate)
            {
                inputs = _toolInputs.ToList();
                answer = _answer.ToString();
                _toolInputs.Clear();
                _answer.Clear();
            }

            var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            var files = inputs.SelectMany(i => AgentWrittenMarkdown.Paths(i, _workingDirectory)).Distinct(comparer).Where(File.Exists).ToList();
            var texts = files.Select(f => (Where: Path.GetRelativePath(_workingDirectory, f), Markdown: ReadShared(f))).ToList();
            if (answer.Length > 0) texts.Add((UiText.Say("la tua risposta", "your answer"), answer));
            if (texts.Count == 0) return (null, null);

            PlantumlVerification result;
            try { result = await _verifier.VerifyAsync(texts, ct).ConfigureAwait(false); }
            catch (OperationCanceledException) { throw; }
            catch (Exception ex)
            {
                return (UiText.Say("Verifica dei diagrammi non riuscita: ", "Checking the diagrams failed: ") + ex.Message, null);
            }
            if (result.Diagrams == 0) return (null, null);

            var names = files.Count == 0 ? UiText.Say("la risposta", "the answer") : string.Join(", ", files.Select(f => Path.GetRelativePath(_workingDirectory, f)));
            if (result.ToolUnavailable != null)
                return (UiText.Say("Diagrammi di " + names + " NON verificati: ", "Diagrams of " + names + " NOT checked: ") + result.ToolUnavailable, null);
            if (!result.HasProblems)
                return (UiText.Say($"MdExplorer: diagrammi di {names} verificati ✓", $"MdExplorer: diagrams of {names} checked ✓"), null);

            var problems = result.Problems.Count == 1 ? UiText.Say("1 problema", "1 problem") : result.Problems.Count + UiText.Say(" problemi", " problems");
            if (lastRound)
                return (UiText.Say($"MdExplorer: restano {problems} nei diagrammi di {names}", $"MdExplorer: {problems} still in the diagrams of {names}"), null);
            return (UiText.Say($"MdExplorer: {problems} nei diagrammi di {names}, rimandati a {_engine}", $"MdExplorer: {problems} in the diagrams of {names}, handed back to {_engine}"),
                    result.ForModel());
        }

        // L'agente può tenere ancora il file, e lo legge anche il watcher di MdExplorer.
        private static string ReadShared(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}

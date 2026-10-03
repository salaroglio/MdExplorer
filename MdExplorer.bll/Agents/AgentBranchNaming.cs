using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MdExplorer.Features.Agents
{
    /// <summary>
    /// Il nome con cui il lavoro di un agente viene <b>pubblicato</b> su origin.
    /// <para>
    /// Il problema: un nome dovrebbe raccontare il lavoro fatto, ma il branch si crea
    /// <i>prima</i> di lavorare, e gli input dell'agente descrivono la <i>richiesta</i>, non
    /// l'esito. La via d'uscita è che il push usa una <b>refspec</b>: il nome locale e quello
    /// pubblicato sono indipendenti, e il secondo si decide al momento del push — quando l'esito
    /// è noto e non è troppo tardi.
    /// </para>
    /// <para>
    /// L'etichetta si ricava dai <b>file effettivamente toccati</b>: un dato fattuale, non
    /// un'interpretazione del testo né una domanda a un modello.
    /// </para>
    /// <code>
    /// agent/salaroglio/wiki-curator/2026-08-02-llm-wiki-a1b2c3
    ///       └ padrone  └ agente     └ data  └ etichetta └ id breve
    /// </code>
    /// </summary>
    public static class AgentBranchNaming
    {
        /// <summary>Etichetta di ripiego quando il lavoro è sparso e non c'è altro da dire.</summary>
        public const string FallbackLabel = "misc";

        private const int MaxLabelLength = 40;
        private const int ShortIdLength = 6;

        /// <summary>
        /// Compone il nome pubblicato.
        /// </summary>
        /// <param name="ownerEmail">Email git del padrone: se ne usa la parte locale, sanificata.</param>
        /// <param name="agentName">Nome A2A dell'agente.</param>
        /// <param name="changedFiles">Percorsi (relativi alla radice) toccati dal lavoro.</param>
        /// <param name="activityId">Id dell'attività: i primi caratteri fanno da discriminante.</param>
        /// <param name="whenUtc">Data del lavoro.</param>
        /// <param name="fallbackLabel">Ambito di ownership o primo topic, se i file non bastano.</param>
        public static string ComposePublishedBranch(
            string ownerEmail,
            string agentName,
            IEnumerable<string> changedFiles,
            string activityId,
            DateTime whenUtc,
            string fallbackLabel = null)
        {
            var owner = Sanitize(LocalPart(ownerEmail));
            if (string.IsNullOrEmpty(owner)) owner = "sconosciuto";

            var agent = Sanitize(agentName);
            if (string.IsNullOrEmpty(agent)) agent = "agente";

            var label = LabelFrom(changedFiles) ?? Sanitize(fallbackLabel);
            if (string.IsNullOrEmpty(label)) label = FallbackLabel;

            // L'id breve NON è un residuo del Guid: con padrone, agente, data ed etichetta
            // uguali — lo stesso agente sullo stesso ambito nello stesso giorno, che capiterà —
            // senza discriminante il secondo branch collide col primo. È derivato
            // dall'activityId, quindi ripubblicare la STESSA attività aggiorna lo stesso ramo
            // invece di crearne uno nuovo a ogni push.
            var shortId = ShortId(activityId);

            return $"agent/{owner}/{agent}/{whenUtc:yyyy-MM-dd}-{label}-{shortId}";
        }

        /// <summary>
        /// Etichetta dai file toccati: prefisso comune delle cartelle (caso più frequente e più
        /// parlante), oppure il nome del file quando è uno solo. <c>null</c> se non si ricava
        /// nulla di utile — il chiamante ripiegherà sull'ambito.
        /// </summary>
        public static string LabelFrom(IEnumerable<string> changedFiles)
        {
            var files = (changedFiles ?? Enumerable.Empty<string>())
                .Where(f => !string.IsNullOrWhiteSpace(f))
                .Select(f => f.Replace('\\', '/').Trim().TrimStart('/'))
                .Where(f => f.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (files.Count == 0) return null;

            if (files.Count == 1)
            {
                var only = files[0];
                var dir = DirectoryOf(only);
                // Un file in una cartella: la cartella dice più del nome ("llm-wiki" batte "log").
                var candidate = !string.IsNullOrEmpty(dir) ? LastSegment(dir) : NameWithoutExtension(only);
                return Sanitize(candidate);
            }

            var common = CommonDirectoryPrefix(files);
            if (!string.IsNullOrEmpty(common)) return Sanitize(LastSegment(common));

            // Modifiche sparse su cartelle diverse: non c'è una parola onesta da dare.
            return null;
        }

        private static string CommonDirectoryPrefix(List<string> files)
        {
            var split = files.Select(f => DirectoryOf(f).Split('/', StringSplitOptions.RemoveEmptyEntries)).ToList();
            if (split.Any(s => s.Length == 0)) return null;   // qualcosa sta nella radice

            var common = new List<string>();
            for (var i = 0; ; i++)
            {
                if (split.Any(s => s.Length <= i)) break;
                var seg = split[0][i];
                if (split.Any(s => !string.Equals(s[i], seg, StringComparison.OrdinalIgnoreCase))) break;
                common.Add(seg);
            }
            return common.Count == 0 ? null : string.Join('/', common);
        }

        private static string DirectoryOf(string path)
        {
            var i = path.LastIndexOf('/');
            return i <= 0 ? string.Empty : path.Substring(0, i);
        }

        private static string LastSegment(string path)
        {
            var i = path.LastIndexOf('/');
            return i < 0 ? path : path.Substring(i + 1);
        }

        private static string NameWithoutExtension(string path)
        {
            var name = LastSegment(path);
            var dot = name.LastIndexOf('.');
            return dot <= 0 ? name : name.Substring(0, dot);
        }

        private static string LocalPart(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var at = email.IndexOf('@');
            return at <= 0 ? email : email.Substring(0, at);
        }

        private static string ShortId(string activityId)
        {
            var clean = new string((activityId ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
            if (clean.Length == 0) return "000000";
            return clean.Length <= ShortIdLength ? clean.ToLowerInvariant()
                                                 : clean.Substring(0, ShortIdLength).ToLowerInvariant();
        }

        /// <summary>
        /// Riduce a ciò che un ref git accetta senza sorprese: minuscole, solo lettere/cifre/
        /// trattino, niente doppioni di trattino, niente trattini ai bordi, lunghezza limitata.
        /// Le regole dei ref vietano spazi, <c>..</c>, <c>~^:?*[</c>, il trattino iniziale e il
        /// suffisso <c>.lock</c>: restringendo a questo alfabeto sono escluse tutte.
        /// </summary>
        public static string Sanitize(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var sb = new StringBuilder(value.Length);
            foreach (var c in value.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(c) && c < 128) sb.Append(c);
                else if (sb.Length > 0 && sb[^1] != '-') sb.Append('-');
            }

            var result = sb.ToString().Trim('-');
            if (result.Length > MaxLabelLength) result = result.Substring(0, MaxLabelLength).Trim('-');
            return result;
        }
    }
}

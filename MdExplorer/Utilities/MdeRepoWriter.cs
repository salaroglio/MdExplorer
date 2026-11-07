using System;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Il motivo di una scrittura di MDE, nelle due lingue. La lingua si sceglie DOPO, nel servizio che notifica (S3), e mai dentro
    /// lo scrittore: chi scrive può essere in una transazione, e leggere lì la lingua dell'interfaccia ha già bloccato il DB (03/11).
    /// L'italiano va anche nel messaggio di commit (contenuto del repository).
    /// </summary>
    public sealed record MdeWhy(string It, string En)
    {
        public static readonly MdeWhy ConfigUpdated = new("configurazione del progetto aggiornata", "project configuration updated");
        public static readonly MdeWhy Generic = new("aggiornato da MDE", "updated by MDE");
    }

    /// <summary>
    /// Il punto unico da cui MDE scrive i suoi file del repository (S1 dello sprint «File di sistema di MDE», 07/11/2026).
    /// <para>
    /// <b>Scrive solo se il contenuto cambia.</b> Prima ogni salvataggio riscriveva il file anche identico, e due scrittori non
    /// usavano OmitNull: in progetto B <c>.development.yml</c> si era riempito di <c>description:</c>, <c>knowledgeGraph:</c>,
    /// <c>codeGraph:</c> vuoti, modifiche senza significato che bloccavano il pull. Il confronto ignora CRLF/LF: con
    /// <c>core.autocrlf</c> su Windows la copia su disco ha CRLF e il serializzatore scrive LF, e riscrivere lo stesso testo con
    /// fine riga diversi cambierebbe il file senza cambiarne il contenuto.
    /// </para>
    /// </summary>
    public static class MdeRepoWriter
    {
        private static readonly ISerializer DevelopmentSerializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        /// <summary>
        /// L'unico serializzatore di <c>.development.yml</c>: camelCase, senza le voci vuote. Tutti gli scrittori lo usano, così lo
        /// stesso contenuto produce sempre lo stesso testo, chiunque lo salvi.
        /// </summary>
        public static string SerializeDevelopmentConfig(object config) => DevelopmentSerializer.Serialize(config);

        /// <summary>
        /// Avvisato dopo ogni scrittura EFFETTIVA di un file del repository (non quando il testo era già uguale), con il motivo:
        /// lo ascolta il commit automatico (S2) e, dopo, le notifiche (S3). Statico perché gli scrittori sono statici e sparsi.
        /// </summary>
        public static event Action<string, MdeWhy> Written;

        /// <summary>Per le scritture che non passano da <see cref="WriteIfChanged"/> (istruzioni, pulizia delle voci ritirate).</summary>
        public static void NotifyWritten(string fullPath, MdeWhy why)
        {
            try { Written?.Invoke(fullPath, why); }
            catch (Exception ex) { Console.WriteLine($"[MdeRepoWriter] avviso di scrittura fallito per {fullPath}: {ex.Message}"); }
        }

        /// <summary>
        /// Salva <c>.development.yml</c> se cambia. True = il file è stato scritto. Se il file ha un gruppo spento dal controllo dello
        /// schema non scrive e lancia, con il motivo: il modello non ha quel gruppo e riscrivendo lo cancellerebbe
        /// (<see cref="DevelopmentConfigFile.WhyNotWritable"/>; F2, fino alla scrittura per gruppo di F3).
        /// </summary>
        public static bool WriteDevelopmentConfig(string filePath, object config, MdeWhy why = null)
        {
            var refused = DevelopmentConfigFile.WhyNotWritable(filePath);
            if (refused != null) throw new DevelopmentConfigOffException(refused);
            return WriteIfChanged(filePath, SerializeDevelopmentConfig(config), why ?? MdeWhy.ConfigUpdated);
        }

        /// <summary>
        /// Scrive <paramref name="content"/> in <paramref name="fullPath"/> solo se è diverso da quello che c'è (fine riga a parte).
        /// Conserva i fine riga e il BOM del file esistente, così un file CRLF resta CRLF. True = il file è stato scritto.
        /// </summary>
        public static bool WriteIfChanged(string fullPath, string content, MdeWhy why = null)
        {
            var written = WriteIfChangedCore(fullPath, content);
            if (written) NotifyWritten(fullPath, why ?? MdeWhy.Generic);
            return written;
        }

        private static bool WriteIfChangedCore(string fullPath, string content)
        {
            if (string.IsNullOrWhiteSpace(fullPath)) throw new ArgumentException("Percorso mancante.", nameof(fullPath));
            content ??= string.Empty;

            if (File.Exists(fullPath))
            {
                var bytes = File.ReadAllBytes(fullPath);
                var bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
                var existing = new UTF8Encoding(false).GetString(bytes, bom ? 3 : 0, bytes.Length - (bom ? 3 : 0));
                if (Normalize(existing) == Normalize(content)) return false;

                var crlf = existing.Contains("\r\n");
                var text = Normalize(content);
                if (crlf) text = text.Replace("\n", "\r\n");
                File.WriteAllText(fullPath, text, new UTF8Encoding(bom));
                return true;
            }

            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(fullPath, content, new UTF8Encoding(false));
            return true;
        }

        private static string Normalize(string text) => (text ?? string.Empty).Replace("\r\n", "\n");
    }

    /// <summary>Scrittura di <c>.development.yml</c> rifiutata perché il file ha gruppi spenti (configurati da un MDE più nuovo o rovinati).</summary>
    public sealed class DevelopmentConfigOffException : InvalidOperationException
    {
        public DevelopmentConfigOffException(string message) : base(message) { }
    }
}

using System;
using System.IO;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MdExplorer.Utilities
{
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

        /// <summary>Salva <c>.development.yml</c> se cambia. True = il file è stato scritto.</summary>
        public static bool WriteDevelopmentConfig(string filePath, object config)
            => WriteIfChanged(filePath, SerializeDevelopmentConfig(config));

        /// <summary>
        /// Scrive <paramref name="content"/> in <paramref name="fullPath"/> solo se è diverso da quello che c'è (fine riga a parte).
        /// Conserva i fine riga e il BOM del file esistente, così un file CRLF resta CRLF. True = il file è stato scritto.
        /// </summary>
        public static bool WriteIfChanged(string fullPath, string content)
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
}

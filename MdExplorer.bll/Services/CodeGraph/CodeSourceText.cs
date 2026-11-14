using MdExplorer.Features.Services;
using MdExplorer.RemoteProtocol;
using System.Collections.Generic;
using System.IO;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>
    /// Il testo di un sorgente del grafo del codice, con la codifica giusta: le regole per estensione della radice
    /// (<c>codeRoots[].encodings</c>) se ci sono, altrimenti il riconoscimento dal contenuto di <see cref="TextDecoding"/>
    /// (UTF-8, CP-850, CP-1252, UTF-16 — i sorgenti di molti linguaggi aziendali sono in CP-850). Un unico lettore per l'hash del sorgente,
    /// l'impronta dei corpi e il testo che va all'LLM: letto in UTF-8, un file CP-850 trasforma ogni accentata nello stesso
    /// U+FFFD, e «è» cambiato in «à» non cambierebbe l'hash. Per un file UTF-8 il testo
    /// è quello di <see cref="File.ReadAllText(string)"/>, quindi gli hash già scritti restano validi.
    /// </summary>
    public static class CodeSourceText
    {
        /// <summary>Il testo del file; un file binario (byte NUL fuori da UTF-16) è un errore con il percorso.</summary>
        public static string Read(string path, IReadOnlyDictionary<string, string> extensionEncodings = null)
        {
            var bytes = File.ReadAllBytes(path);
            var assigned = TextDecoding.ForExtension(path, bytes, extensionEncodings);
            var decoded = assigned != null ? TextDecoding.DecodeAs(bytes, assigned) : TextDecoding.Decode(bytes);
            return decoded?.Text ?? throw new InvalidDataException(UiText.Say($"{path}: non è un file di testo (contiene byte NUL)", $"{path}: not a text file (it contains NUL bytes)"));
        }

        /// <summary>L'hash che va nell'intestazione degli script AST (D14) e che conferma un candidato.</summary>
        public static string Hash(string text) => CodeGraphStore.Sha256(text);
    }
}

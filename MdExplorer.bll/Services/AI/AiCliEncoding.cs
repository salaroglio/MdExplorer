using System.Text;

namespace MdExplorer.Features.Services.AI
{
    /// <summary>
    /// Codifica con cui si scrive un prompt sullo <c>stdin</c> di un CLI di AI.
    /// <para>
    /// Serve un valore esplicito perche' <see cref="System.Diagnostics.ProcessStartInfo"/>,
    /// se non gliela si dice, apre StandardInput con <c>Console.InputEncoding</c>: su Linux
    /// e' UTF-8 e non si nota nulla, su Windows e' il code page della console (1252/850).
    /// La "e'" di "perche'" parte allora come singolo byte <c>0xE9</c>, il CLI (Node) la
    /// rilegge come UTF-8, non e' una sequenza valida e diventa U+FFFD: il modello riceve
    /// <c>perch�</c> e ripete lo stesso rombo nella risposta. Verificato il 07/09/2026.
    /// </para>
    /// <para>
    /// <b>Non sostituire con <see cref="Encoding.UTF8"/></b>: quella istanza ha il preambolo,
    /// e StreamWriter lo scrive in testa alla pipe — il prompt arriverebbe preceduto da
    /// <c>EF BB BF</c>. Verificato: con <c>Encoding.UTF8</c> i byte su stdin sono
    /// <c>efbbbf 7065726368c3a9</c>, con questa istanza <c>7065726368c3a9</c>.
    /// </para>
    /// </summary>
    public static class AiCliEncoding
    {
        /// <summary>UTF-8 senza BOM, per lo stdin dei CLI di AI.</summary>
        public static readonly Encoding PromptStdin = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
    }
}

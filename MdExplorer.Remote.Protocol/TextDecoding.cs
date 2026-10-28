using System;
using System.Collections.Generic;
using System.Text;

namespace MdExplorer.RemoteProtocol
{
    /// <summary>Un file di testo decodificato: il testo, la codifica in cui va riscritto, se aveva il BOM.</summary>
    public sealed class DecodedText
    {
        public DecodedText(string text, string encoding, bool bom) { Text = text; Encoding = encoding; Bom = bom; }
        public string Text { get; }
        /// <summary>Nome web: <c>utf-8</c>, <c>ibm850</c>, <c>windows-1252</c>, <c>utf-16</c> (LE), <c>utf-16BE</c>.</summary>
        public string Encoding { get; }
        public bool Bom { get; }
    }

    /// <summary>
    /// La codifica dei file di una radice, riconosciuta dal contenuto, senza configurazioni (sprint
    /// 2026-10-21-Cartella-Remota-Via-SSH, dopo FL — l'utente: radici con molti file CP-850 e pochi UTF-8, «in automatico,
    /// SENZA configurazioni precotte»).
    /// <list type="number">
    /// <item>BOM UTF-8 / UTF-16 → quella.</item>
    /// <item>UTF-8 valido (stretto) → UTF-8: un testo CP-850 con accentate non forma mai sequenze UTF-8 valide.</item>
    /// <item>Altrimenti 8 bit: CP-850 o CP-1252, scelto dal contenuto — vince la lettura in cui i caratteri non ASCII sono
    /// lettere coerenti con la parola (CP-850 letto come 1252 dà «perch‚», «citt…»; 1252 letto come 850 dà «perchÚ»).
    /// CP-1252 è escluso se compaiono i suoi byte non definiti. Pari → CP-850.</item>
    /// </list>
    /// Si riscrive SEMPRE nella codifica in cui si è letto: CP-850 copre tutti i 256 byte, quindi la parte non toccata resta
    /// identica byte per byte anche se la stima fosse sbagliata; un carattere che la codifica non ha è rifiutato.
    /// </summary>
    public static class TextDecoding
    {
        public const string Utf8 = "utf-8";
        public const string Ibm850 = "ibm850";
        public const string Windows1252 = "windows-1252";

        private static readonly byte[] Utf8Bom = { 0xEF, 0xBB, 0xBF };
        private static readonly UTF8Encoding StrictUtf8 = new(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

        static TextDecoding() => System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        /// <summary>Testo, dai primi byte: niente NUL (o un BOM UTF-16, che di NUL è pieno).</summary>
        public static bool LooksLikeText(ReadOnlySpan<byte> head) => Utf16Bom(head) != null || head.IndexOf((byte)0) < 0;

        /// <summary>Il testo e la sua codifica; null se è binario.</summary>
        public static DecodedText? Decode(byte[] bytes)
        {
            var span = bytes.AsSpan();
            if (Utf16Bom(span) is { } utf16)
                return new DecodedText(utf16.GetString(bytes, 2, bytes.Length - 2), utf16.WebName, bom: true);
            if (span.IndexOf((byte)0) >= 0) return null;
            var bom = span.StartsWith(Utf8Bom);
            var offset = bom ? 3 : 0;
            try { return new DecodedText(StrictUtf8.GetString(bytes, offset, bytes.Length - offset), Utf8, bom); }
            catch (DecoderFallbackException) when (!bom) { /* non è UTF-8: una codifica a 8 bit */ }
            var name = Guess8Bit(bytes);
            return new DecodedText(Get(name).GetString(bytes), name, bom: false);
        }

        /// <summary>Le codifiche che si possono assegnare a un'estensione (e scegliere nell'editor).</summary>
        public static readonly string[] Assignable = { Utf8, Ibm850, Windows1252, "utf-16", "utf-16BE" };

        /// <summary>
        /// La codifica di un file secondo le regole della radice (l'utente, 21/10: «in base all'estensione del file viene
        /// scelto il formato»): il BOM, se c'è, la dichiara il file stesso e vince; poi l'estensione; altrimenti null =
        /// UTF-8, e se non lo è la stima (<see cref="Decode"/>).
        /// </summary>
        public static string? ForExtension(string path, ReadOnlySpan<byte> head, IReadOnlyDictionary<string, string>? rules)
        {
            if (rules == null || rules.Count == 0) return null;
            if (Utf16Bom(head) != null || head.StartsWith(Utf8Bom)) return null;
            var ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
            return ext.Length > 0 && rules.TryGetValue(ext, out var encoding) && encoding != Utf8 ? encoding : null;
        }

        /// <summary>Prefisso di una lettura «mista»: <c>mixed-ibm850</c> = sequenze UTF-8 valide come UTF-8, gli altri byte alti in CP-850.</summary>
        public const string MixedPrefix = "mixed-";

        /// <summary>
        /// Rileggere in una codifica SCELTA dall'utente (l'editor, «Rileggi come» / «Ripara codifica mista»): niente stima.
        /// UTF-8 stretto (un byte non valido → <see cref="RemoteErrorCodes.NotUtf8"/>); 8 bit e UTF-16 come sono;
        /// <c>mixed-…</c> per i file in cui un LLM o uno script ha messo testo UTF-8 dentro un file CP-850.
        /// </summary>
        public static DecodedText DecodeAs(byte[] bytes, string encoding)
        {
            if (encoding.StartsWith(MixedPrefix, StringComparison.Ordinal))
                return new DecodedText(DecodeMixed(bytes, encoding.Substring(MixedPrefix.Length)), encoding, bom: false);
            var span = bytes.AsSpan();
            if (encoding == Utf8)
            {
                var bom = span.StartsWith(Utf8Bom);
                var offset = bom ? 3 : 0;
                try { return new DecodedText(StrictUtf8.GetString(bytes, offset, bytes.Length - offset), Utf8, bom); }
                catch (DecoderFallbackException ex) { throw new RemoteFailure(RemoteErrorCodes.NotUtf8, $"not valid UTF-8 (byte at {ex.Index + offset}): choose another encoding"); }
            }
            if (encoding.StartsWith("utf-16", StringComparison.Ordinal))
            {
                var enc = Get(encoding);
                var preamble = enc.GetPreamble();
                var hasBom = preamble.Length > 0 && span.StartsWith(preamble);
                var start = hasBom ? preamble.Length : 0;
                return new DecodedText(enc.GetString(bytes, start, bytes.Length - start), encoding, bom: true);
            }
            return new DecodedText(Get(encoding).GetString(bytes), encoding, bom: false);
        }

        /// <summary>
        /// Un file con due codifiche: una sequenza UTF-8 valida di 2-4 byte si legge come UTF-8 SE dà un carattere plausibile
        /// in un testo aggiunto (lettere latine accentate, €, virgolette e trattini tipografici, riquadri); ogni altro byte
        /// alto nella codifica a 8 bit data. Il filtro serve perché i riquadri DOS in CP-850 formano UTF-8 valido per caso:
        /// «═╗» = <c>CD BB</c> = «ͻ» greco (visto dal vivo il 21/10 su una maschera COBOL). Si vede nell'editor prima di salvare.
        /// </summary>
        public static string DecodeMixed(byte[] bytes, string eightBit)
        {
            var single = Get(eightBit);
            var text = new StringBuilder(bytes.Length);
            var i = 0;
            while (i < bytes.Length)
            {
                var b = bytes[i];
                if (b < 0x80) { text.Append((char)b); i++; continue; }
                var length = b >= 0xC2 && b <= 0xDF ? 2 : b >= 0xE0 && b <= 0xEF ? 3 : b >= 0xF0 && b <= 0xF4 ? 4 : 0;
                if (length > 0 && i + length <= bytes.Length)
                {
                    try
                    {
                        var candidate = StrictUtf8.GetString(bytes, i, length);
                        if (PlausibleAdded(candidate))
                        {
                            text.Append(candidate);
                            i += length;
                            continue;
                        }
                    }
                    catch (DecoderFallbackException) { /* non è UTF-8: il byte è della codifica a 8 bit */ }
                }
                text.Append(single.GetString(bytes, i, 1));
                i++;
            }
            return text.ToString();
        }

        /// <summary>Un carattere che qualcuno scrive davvero in un commento o in una stringa, non un incidente di byte.</summary>
        private static bool PlausibleAdded(string candidate)
        {
            var c = candidate[0];
            if (char.IsSurrogate(c)) return true;                       // emoji e simili: nessun CP-850 li forma
            return (c >= '\u00C0' && c <= '\u024F' && char.IsLetter(c))  // lettere latine accentate (À…ÿ, estese)
                   || (c >= '\u00A0' && c <= '\u00BF')                    // « » ° ± ¿ ¡ § £ …
                   || c == '\u20AC'                                       // €
                   || (c >= '\u2010' && c <= '\u2027')                    // trattini, virgolette, puntini
                   || (c >= '\u2500' && c <= '\u259F');                   // riquadri scritti in UTF-8
        }

        /// <summary>Il testo nella codifica data (e il BOM se c'era); un carattere che la codifica non ha → eccezione con il motivo.</summary>
        public static byte[] Encode(string text, string encoding, bool bom)
        {
            if (encoding.StartsWith(MixedPrefix, StringComparison.Ordinal))
                throw new RemoteFailure(RemoteErrorCodes.BadRequest, "a file is written in ONE encoding: choose UTF-8, CP-850 or CP-1252");
            var enc = Get(encoding);
            byte[] body;
            try { body = enc.GetBytes(text); }
            catch (EncoderFallbackException ex)
            {
                // un carattere fuori dal piano base (un'emoji) arriva come coppia High/Low, e CharUnknown è '\0'
                var character = ex.CharUnknownHigh != '\0' ? new string(new[] { ex.CharUnknownHigh, ex.CharUnknownLow }) : ex.CharUnknown.ToString();
                throw new RemoteFailure(RemoteErrorCodes.NotRepresentable,
                    $"the file is {DisplayName(encoding)}: the character «{character}» cannot be written in it");
            }
            if (!bom) return body;
            var preamble = encoding == Utf8 ? Utf8Bom : enc.GetPreamble();
            var all = new byte[preamble.Length + body.Length];
            preamble.CopyTo(all, 0);
            body.CopyTo(all, preamble.Length);
            return all;
        }

        /// <summary>Il nome da mostrare: «UTF-8», «CP-850», «CP-1252», «UTF-16».</summary>
        public static string DisplayName(string encoding) => encoding switch
        {
            Utf8 => "UTF-8",
            Ibm850 => "CP-850",
            Windows1252 => "CP-1252",
            "utf-16" => "UTF-16 LE",
            "utf-16BE" => "UTF-16 BE",
            _ when encoding.StartsWith(MixedPrefix, StringComparison.Ordinal) => "UTF-8 + " + DisplayName(encoding.Substring(MixedPrefix.Length)),
            _ => encoding,
        };

        /// <summary>CP-850 o CP-1252: vince la lettura più plausibile; pari → CP-850.</summary>
        public static string Guess8Bit(byte[] bytes)
        {
            foreach (var b in bytes)
                if (b == 0x81 || b == 0x8D || b == 0x8F || b == 0x90 || b == 0x9D) return Ibm850;   // non definiti in 1252
            var as1252 = Score(Get(Windows1252).GetString(bytes));
            var as850 = Score(Get(Ibm850).GetString(bytes));
            return as1252 > as850 ? Windows1252 : Ibm850;
        }

        /// <summary>
        /// Quanto è plausibile un testo, guardando solo i caratteri non ASCII: lettera dentro una parola +3, lettera da sola
        /// +1, maiuscola subito dopo una minuscola −3 («perchÚ»), la stessa lettera ripetuta −2 (CP-850 «═══» letto come
        /// 1252 è «ÍÍÍ»), segni grafici di riquadro 0 (legittimi nelle maschere DOS), ogni altro simbolo −2 («citt…»).
        /// </summary>
        internal static int Score(string s)
        {
            var score = 0;
            for (var i = 0; i < s.Length; i++)
            {
                var c = s[i];
                if (c < 0x80) continue;
                var prev = i > 0 ? s[i - 1] : ' ';
                var next = i + 1 < s.Length ? s[i + 1] : ' ';
                if (char.IsLetter(c))
                {
                    if (c == prev || c == next) score -= 2;
                    else if (char.IsUpper(c) && char.IsLower(prev)) score -= 3;
                    else score += char.IsLetter(prev) || char.IsLetter(next) ? 3 : 1;
                }
                else if (c >= '─' && c <= '▟') { /* riquadri e blocchi: neutri */ }
                else score -= 2;
            }
            return score;
        }

        private static Encoding Get(string name) => name switch
        {
            Utf8 => StrictUtf8,
            _ => System.Text.Encoding.GetEncoding(name, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback),
        };

        private static Encoding? Utf16Bom(ReadOnlySpan<byte> head)
        {
            if (head.Length >= 2 && head[0] == 0xFF && head[1] == 0xFE) return new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true);
            if (head.Length >= 2 && head[0] == 0xFE && head[1] == 0xFF) return new UnicodeEncoding(bigEndian: true, byteOrderMark: true, throwOnInvalidBytes: true);
            return null;
        }
    }
}

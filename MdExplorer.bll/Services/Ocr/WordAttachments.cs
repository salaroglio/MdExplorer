using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Gli allegati «annegati» in un Word (un PDF, un Excel, un altro Word trascinati nella pagina: oggetti OLE
    /// incorporati). Nel documento si vede solo la loro icona, e convertendo restava quella: l'allegato andava perso.
    /// Qui ogni allegato esce come file accanto alle immagini e al posto dell'icona va un collegamento.
    /// <para>
    /// Si lavora su una copia del docx: l'oggetto (<c>w:object</c>) diventa un gettone di testo che attraversa la
    /// conversione e alla fine (<see cref="FinishMarkdown"/>) diventa il collegamento. Un allegato che non si riesce
    /// a estrarre resta com'era, con la sua icona, e lo si dice fra gli avvisi.
    /// </para>
    /// </summary>
    public sealed class WordAttachments
    {
        private static readonly XNamespace W = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private static readonly XNamespace O = "urn:schemas-microsoft-com:office:office";
        private static readonly XNamespace V = "urn:schemas-microsoft-com:vml";
        private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private static readonly XNamespace Rel = "http://schemas.openxmlformats.org/package/2006/relationships";
        private static readonly Regex Token = new Regex(@"MDEATTACH(\d{4})X", RegexOptions.Compiled);

        private readonly List<(string Text, string Link)> _links = new List<(string Text, string Link)>();

        public int Count => _links.Count;

        /// <param name="docx">Il docx (o la sua copia già riparata): non viene toccato.</param>
        /// <returns>La copia con i gettoni al posto degli allegati estratti; null se non c'era niente da estrarre.</returns>
        public MemoryStream Extract(Stream docx, string directory, string linkPrefix, List<string> warnings)
        {
            var copy = new MemoryStream();
            docx.Position = 0;
            docx.CopyTo(copy);

            var left = 0;
            string whyLeft = null;
            using (var zip = new ZipArchive(copy, ZipArchiveMode.Update, leaveOpen: true))
            {
                var documentEntry = zip.GetEntry("word/document.xml");
                var relsEntry = zip.GetEntry("word/_rels/document.xml.rels");
                if (documentEntry == null || relsEntry == null)
                {
                    return null;
                }
                XDocument document, rels;
                using (var stream = documentEntry.Open()) document = XDocument.Load(stream);
                using (var stream = relsEntry.Open()) rels = XDocument.Load(stream);
                var targets = rels.Root.Elements(Rel + "Relationship")
                    .Where(relationship => (string)relationship.Attribute("TargetMode") != "External")
                    .ToDictionary(relationship => (string)relationship.Attribute("Id"), relationship => (string)relationship.Attribute("Target"));

                foreach (var holder in document.Descendants(W + "object").ToList())
                {
                    var ole = holder.Element(O + "OLEObject");
                    if (ole == null || (string)ole.Attribute("Type") != "Embed")
                    {
                        continue;
                    }
                    var progId = (string)ole.Attribute("ProgID") ?? string.Empty;
                    var title = ((string)holder.Descendants(V + "imagedata").FirstOrDefault()?.Attribute(O + "title") ?? string.Empty).Trim();
                    try
                    {
                        if (!targets.TryGetValue((string)ole.Attribute(R + "id") ?? string.Empty, out var target))
                        {
                            throw new InvalidDataException("il docx non dice dove sta");
                        }
                        var entry = zip.GetEntry(target.StartsWith("/", StringComparison.Ordinal) ? target.TrimStart('/') : "word/" + target)
                            ?? throw new InvalidDataException("nel docx il file dell'allegato non c'è");
                        byte[] bytes;
                        using (var stream = entry.Open())
                        using (var buffer = new MemoryStream())
                        {
                            stream.CopyTo(buffer);
                            bytes = buffer.ToArray();
                        }

                        var (content, extension, originalName) = Unwrap(bytes, progId);
                        var number = _links.Count + 1;
                        var fileName = $"attach{number}{extension}";
                        Directory.CreateDirectory(directory);
                        File.WriteAllBytes(Path.Combine(directory, fileName), content);

                        // Il nome: quello che Word scrive sotto l'icona è il nome del file allegato, ed è l'unico posto dove sta.
                        var label = IconLabel(zip, targets, (string)holder.Descendants(V + "imagedata").FirstOrDefault()?.Attribute(R + "id"));
                        var name = title.Length > 0 ? title
                            : label.Length > 0 ? label
                            : !string.IsNullOrWhiteSpace(originalName) ? originalName
                            : UiText.Say($"Allegato {number}", $"Attachment {number}");
                        var kind = name.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? string.Empty : $" ({extension.TrimStart('.').ToUpperInvariant()})";
                        _links.Add(($"📎 {name}{kind}", linkPrefix + "/" + fileName));
                        holder.ReplaceWith(new XElement(W + "t", new XAttribute(XNamespace.Xml + "space", "preserve"), $" MDEATTACH{number:0000}X "));
                    }
                    catch (InvalidDataException ex)
                    {
                        left++;
                        whyLeft = ex.Message;
                    }
                }

                if (left > 0)
                {
                    warnings.Add(UiText.Say(
                        $"{left} allegati incorporati nel Word non sono stati estratti, nel documento resta la loro icona: {whyLeft}.",
                        $"{left} attachments embedded in the Word document were not extracted, their icon stays in the document: {whyLeft}."));
                }
                if (_links.Count == 0)
                {
                    return null;
                }
                documentEntry.Delete();
                using (var stream = zip.CreateEntry("word/document.xml").Open())
                {
                    document.Save(stream);
                }
            }
            copy.Position = 0;
            return copy;
        }

        private static string IconLabel(ZipArchive zip, Dictionary<string, string> targets, string iconRelationship)
        {
            if (iconRelationship == null || !targets.TryGetValue(iconRelationship, out var target))
            {
                return string.Empty;
            }
            var entry = zip.GetEntry(target.StartsWith("/", StringComparison.Ordinal) ? target.TrimStart('/') : "word/" + target);
            if (entry == null)
            {
                return string.Empty;
            }
            using (var stream = entry.Open())
            using (var buffer = new MemoryStream())
            {
                stream.CopyTo(buffer);
                return MetafileLabel(buffer.ToArray());
            }
        }

        /// <summary>
        /// Il testo disegnato in un'icona EMF (i record «scrivi testo» EMR_EXTTEXTOUTW, uno per riga dell'etichetta),
        /// rimesso su una riga: Windows va a capo dopo uno spazio o in mezzo a una parola, quindi le righe si accostano
        /// così come sono. Vuoto se l'icona non è un EMF o non porta testo.
        /// </summary>
        internal static string MetafileLabel(byte[] emf)
        {
            const int extTextOutW = 84;
            if (emf == null || emf.Length < 88 || BitConverter.ToUInt32(emf, 0) != 1 || BitConverter.ToUInt32(emf, 40) != 0x464D4520)
            {
                return string.Empty;
            }
            var label = new StringBuilder();
            for (long position = 0; position + 8 <= emf.Length; )
            {
                var type = BitConverter.ToUInt32(emf, (int)position);
                var size = BitConverter.ToUInt32(emf, (int)position + 4);
                if (size < 8 || position + size > emf.Length)
                {
                    break;
                }
                if (type == extTextOutW && size >= 52)
                {
                    var chars = BitConverter.ToUInt32(emf, (int)position + 44);
                    var offset = BitConverter.ToUInt32(emf, (int)position + 48);
                    if (offset + 2L * chars <= size)
                    {
                        label.Append(Encoding.Unicode.GetString(emf, (int)(position + offset), (int)(2 * chars)));
                    }
                }
                position += size;
            }
            return Regex.Replace(label.ToString(), @"\s+", " ").Trim();
        }

        /// <summary>I gettoni lasciati da <see cref="Extract"/> diventano i collegamenti agli allegati.</summary>
        public string FinishMarkdown(string markdown)
        {
            return Token.Replace(markdown, match =>
            {
                var index = int.Parse(match.Groups[1].Value) - 1;
                if (index < 0 || index >= _links.Count)
                {
                    return match.Value;
                }
                var text = _links[index].Text.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]");
                return "[" + text + "](" + _links[index].Link + ")";
            });
        }

        /// <summary>Il file vero dentro il contenitore, la sua estensione e, se il contenitore lo porta, il suo nome.</summary>
        /// <exception cref="InvalidDataException">Non si è capito che cosa c'è dentro: dice perché.</exception>
        internal static (byte[] Content, string Extension, string OriginalName) Unwrap(byte[] bytes, string progId)
        {
            // Un Excel o un Word incorporati «puliti» sono già il file, senza contenitore.
            if (IsZip(bytes))
            {
                return (bytes, OfficeExtension(bytes, progId), null);
            }
            if (!CompoundFile.LooksLikeOne(bytes))
            {
                throw new InvalidDataException($"un allegato di tipo «{progId}» è in un formato che MdExplorer non sa aprire");
            }
            var container = new CompoundFile(bytes);

            var package = container.Stream("Package");
            if (package != null && IsZip(package))
            {
                return (package, OfficeExtension(package, progId), null);
            }
            var contents = container.Stream("CONTENTS");
            if (contents != null && StartsWith(contents, "%PDF"))
            {
                return (contents, ".pdf", null);
            }
            var native = container.Stream("\u0001Ole10Native");
            if (native != null)
            {
                return Ole10Native(native);
            }
            // I vecchi .doc/.xls/.ppt incorporati sono loro stessi il file composto.
            var names = new HashSet<string>(container.StreamNames, StringComparer.OrdinalIgnoreCase);
            if (names.Contains("WordDocument")) return (bytes, ".doc", null);
            if (names.Contains("Workbook") || names.Contains("Book")) return (bytes, ".xls", null);
            if (names.Contains("PowerPoint Document")) return (bytes, ".ppt", null);
            if (names.Contains("VisioDocument")) return (bytes, ".vsd", null);
            throw new InvalidDataException($"in un allegato di tipo «{progId}» non c'è un file che MdExplorer sappia riconoscere");
        }

        // Il «pacchetto» generico: misura, tipo, etichetta, nome del file, percorso d'origine, poi i dati.
        private static (byte[] Content, string Extension, string OriginalName) Ole10Native(byte[] native)
        {
            try
            {
                var position = 6;
                var label = ReadText(native, ref position);
                var fileName = ReadText(native, ref position);
                position += 4;
                var pathLength = BitConverter.ToInt32(native, position);
                position += 4 + pathLength;
                var size = BitConverter.ToInt32(native, position);
                position += 4;
                if (size < 0 || position + size > native.Length)
                {
                    throw new InvalidDataException("un allegato generico dichiara una misura che non torna");
                }
                var content = new byte[size];
                Array.Copy(native, position, content, 0, size);
                var name = Path.GetFileName((fileName.Length > 0 ? fileName : label).Replace('\\', '/'));
                var extension = Path.GetExtension(name).ToLowerInvariant();
                if (!Regex.IsMatch(extension, @"^\.[a-z0-9]{1,8}$"))
                {
                    extension = ".bin";
                }
                return (content, extension, name);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is IndexOutOfRangeException)
            {
                throw new InvalidDataException("un allegato generico ha un'intestazione che non si legge");
            }
        }

        private static string ReadText(byte[] bytes, ref int position)
        {
            var end = Array.IndexOf(bytes, (byte)0, position);
            if (end < 0)
            {
                throw new InvalidDataException("un allegato generico ha un'intestazione che non si legge");
            }
            var text = Encoding.Latin1.GetString(bytes, position, end - position);
            position = end + 1;
            return text;
        }

        private static string OfficeExtension(byte[] zipBytes, string progId)
        {
            using (var zip = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read))
            {
                var macros = zip.Entries.Any(entry => entry.FullName.EndsWith("vbaProject.bin", StringComparison.OrdinalIgnoreCase));
                if (zip.Entries.Any(entry => entry.FullName.StartsWith("word/", StringComparison.OrdinalIgnoreCase))) return macros ? ".docm" : ".docx";
                if (zip.Entries.Any(entry => entry.FullName.StartsWith("xl/", StringComparison.OrdinalIgnoreCase))) return macros ? ".xlsm" : ".xlsx";
                if (zip.Entries.Any(entry => entry.FullName.StartsWith("ppt/", StringComparison.OrdinalIgnoreCase))) return macros ? ".pptm" : ".pptx";
                if (zip.Entries.Any(entry => entry.FullName.StartsWith("visio/", StringComparison.OrdinalIgnoreCase))) return ".vsdx";
            }
            return ".zip";
        }

        private static bool IsZip(byte[] bytes) => bytes.Length > 4 && bytes[0] == 0x50 && bytes[1] == 0x4B && bytes[2] == 0x03 && bytes[3] == 0x04;

        private static bool StartsWith(byte[] bytes, string text)
        {
            return bytes.Length >= text.Length && Encoding.ASCII.GetString(bytes, 0, text.Length) == text;
        }
    }
}

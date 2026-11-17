using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>
    /// Toglie da un Word i legami doppi fra uno stile di paragrafo e gli elenchi numerati.
    /// <para>
    /// Uno stile numerato dice lui quale elenco usa (in styles.xml); l'elenco a sua volta porta il nome
    /// dello stile su uno dei suoi livelli (in numbering.xml). Dopo un copia/incolla fra documenti capita
    /// che due elenchi, o due livelli dello stesso elenco, portino lo stesso nome: Word guarda lo stile e non si confonde, Mammoth indicizza
    /// gli elenchi per nome dello stile e si ferma sulla chiave doppia. Qui si fa come Word: il nome resta
    /// solo sul livello che lo stile indica.
    /// </para>
    /// </summary>
    public static class WordNumberingRepair
    {
        /// <returns>
        /// Una copia in memoria del documento con i legami doppi sciolti, oppure null se il documento
        /// non ne ha (e si può leggere così com'è). Il file su disco non viene mai toccato.
        /// </returns>
        public static MemoryStream RepairedCopy(string docxPath)
        {
            string numberingEntryName;
            XDocument numbering;
            using (var zip = ZipFile.OpenRead(docxPath))
            {
                var mainPart = RelatedPart(zip, string.Empty, "/officeDocument");
                if (mainPart == null)
                {
                    return null;
                }
                numberingEntryName = RelatedPart(zip, mainPart, "/numbering");
                var numberingEntry = numberingEntryName == null ? null : zip.GetEntry(numberingEntryName);
                if (numberingEntry == null)
                {
                    return null;
                }
                numbering = Load(numberingEntry);
                var w = numbering.Root.Name.Namespace;

                var contended = numbering.Root.Elements(w + "abstractNum")
                    .SelectMany(a => a.Elements(w + "lvl"))
                    .Select(level => level.Element(w + "pStyle"))
                    .Where(link => link?.Attribute(w + "val") != null)
                    .GroupBy(link => link.Attribute(w + "val").Value)
                    .Where(links => links.Count() > 1)
                    .ToList();
                if (contended.Count == 0)
                {
                    return null;
                }

                var stylesEntryName = RelatedPart(zip, mainPart, "/styles");
                var stylesEntry = stylesEntryName == null ? null : zip.GetEntry(stylesEntryName);
                var styles = stylesEntry == null ? null : Load(stylesEntry);

                foreach (var links in contended)
                {
                    var ownLevel = LevelOf(links.Key, styles, numbering, w);
                    if (!links.Any(link => link.Parent == ownLevel))
                    {
                        throw Ambiguous(links.Key);
                    }
                    foreach (var link in links.Where(link => link.Parent != ownLevel).ToList())
                    {
                        link.Remove();
                    }
                }
            }

            var copy = new MemoryStream();
            using (var source = File.OpenRead(docxPath))
            {
                source.CopyTo(copy);
            }
            using (var zip = new ZipArchive(copy, ZipArchiveMode.Update, leaveOpen: true))
            {
                zip.GetEntry(numberingEntryName).Delete();
                using (var target = zip.CreateEntry(numberingEntryName).Open())
                {
                    numbering.Save(target);
                }
            }
            copy.Position = 0;
            return copy;
        }

        // Il livello di elenco che lo stile indica: stile (o gli stili da cui deriva) → numId e ilvl → num →
        // abstractNum → livello; se quell'abstractNum rimanda a uno stile di elenco (numStyleLink), vale
        // l'abstractNum che lo definisce.
        private static XElement LevelOf(string styleId, XDocument styles, XDocument numbering, XNamespace w)
        {
            // Elenco e livello si ereditano ciascuno per conto suo: uno stile che deriva da «Titolo 2» e
            // cambia solo l'elenco resta al livello di «Titolo 2».
            var numId = InheritedNumbering(styleId, "numId", styles, w);
            if (numId == null)
            {
                throw Ambiguous(styleId);
            }
            var levelIndex = InheritedNumbering(styleId, "ilvl", styles, w) ?? "0";

            var abstractNumId = numbering.Root.Elements(w + "num")
                .Where(num => (string)num.Attribute(w + "numId") == numId)
                .Select(num => (string)num.Element(w + "abstractNumId")?.Attribute(w + "val"))
                .FirstOrDefault();
            var abstractNums = numbering.Root.Elements(w + "abstractNum").ToList();
            var owner = abstractNums.FirstOrDefault(a => (string)a.Attribute(w + "abstractNumId") == abstractNumId);
            if (abstractNumId == null || owner == null)
            {
                throw Ambiguous(styleId);
            }

            var listStyle = (string)owner.Element(w + "numStyleLink")?.Attribute(w + "val");
            if (listStyle != null)
            {
                owner = abstractNums.FirstOrDefault(a => (string)a.Element(w + "styleLink")?.Attribute(w + "val") == listStyle);
                if (owner == null)
                {
                    throw Ambiguous(styleId);
                }
            }

            var levels = owner.Elements(w + "lvl").Where(level => (string)level.Attribute(w + "ilvl") == levelIndex).ToList();
            if (levels.Count != 1)
            {
                throw Ambiguous(styleId);
            }
            return levels[0];
        }

        // Il valore di numPr/«property» dello stile o, se non lo dichiara, del primo stile da cui deriva che lo fa.
        private static string InheritedNumbering(string styleId, string property, XDocument styles, XNamespace w)
        {
            var seen = new HashSet<string>();
            while (styles != null && styleId != null && seen.Add(styleId))
            {
                var style = styles.Root.Elements(w + "style").FirstOrDefault(s =>
                    (string)s.Attribute(w + "styleId") == styleId && (string)s.Attribute(w + "type") == "paragraph");
                if (style == null)
                {
                    return null;
                }
                var value = (string)style.Element(w + "pPr")?.Element(w + "numPr")?.Element(w + property)?.Attribute(w + "val");
                if (value != null)
                {
                    return value;
                }
                styleId = (string)style.Element(w + "basedOn")?.Attribute(w + "val");
            }
            return null;
        }

        private static InvalidDataException Ambiguous(string styleId)
        {
            return new InvalidDataException(UiText.Say(
                $"Nel Word più livelli di elenco numerato dichiarano lo stile «{styleId}» e lo stile non dice quale usa. " +
                $"Apri il documento in Word, riapplica la numerazione allo stile «{styleId}», salva e riprova.",
                $"In the Word document several numbered list levels claim the style \"{styleId}\" and the style does not say which one it uses. " +
                $"Open the document in Word, apply the numbering to the style \"{styleId}\" again, save and retry."));
        }

        // La parte collegata a «sourcePart» (stringa vuota = il pacchetto) con una relazione del tipo dato.
        private static string RelatedPart(ZipArchive zip, string sourcePart, string typeSuffix)
        {
            var slash = sourcePart.LastIndexOf('/');
            var folder = slash < 0 ? string.Empty : sourcePart.Substring(0, slash + 1);
            var relsEntry = zip.GetEntry(folder + "_rels/" + sourcePart.Substring(slash + 1) + ".rels");
            if (relsEntry == null)
            {
                return null;
            }

            var target = Load(relsEntry).Root.Elements()
                .Where(r => r.Name.LocalName == "Relationship"
                    && ((string)r.Attribute("Type") ?? string.Empty).EndsWith(typeSuffix, StringComparison.Ordinal)
                    && (string)r.Attribute("TargetMode") != "External")
                .Select(r => (string)r.Attribute("Target"))
                .FirstOrDefault();
            if (target == null)
            {
                return null;
            }
            if (target.StartsWith("/", StringComparison.Ordinal))
            {
                return target.Substring(1);
            }

            var parts = new List<string>(folder.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
            foreach (var step in target.Split('/'))
            {
                if (step == "..")
                {
                    if (parts.Count > 0)
                    {
                        parts.RemoveAt(parts.Count - 1);
                    }
                }
                else if (step != "." && step.Length > 0)
                {
                    parts.Add(step);
                }
            }
            return string.Join("/", parts);
        }

        private static XDocument Load(ZipArchiveEntry entry)
        {
            using (var stream = entry.Open())
            {
                return XDocument.Load(stream);
            }
        }
    }
}

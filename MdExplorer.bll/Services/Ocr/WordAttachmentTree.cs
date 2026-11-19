using System;
using System.Collections.Generic;
using System.IO;

namespace MdExplorer.Features.Services.Ocr
{
    /// <summary>Un pdf allegato che aspetta l'OCR: il suo markdown esiste già, con scritto che la conversione è in corso.</summary>
    public sealed class PendingPdfAttachment
    {
        public string Name { get; set; }
        public string PdfFullPath { get; set; }
        public string MarkdownFullPath { get; set; }

        /// <summary>Come il markdown indirizza il pdf originale.</summary>
        public string PdfLink { get; set; }
    }

    public sealed class WordAttachmentTreeResult
    {
        /// <summary>Il markdown del documento principale: lo scrive chi ha chiesto la conversione.</summary>
        public string Markdown { get; set; }
        public List<string> Warnings { get; } = new List<string>();

        /// <summary>I markdown scritti per gli allegati Word (percorsi completi): hanno immagini che Mark può sistemare.</summary>
        public List<string> WordDocuments { get; } = new List<string>();

        /// <summary>I markdown scritti per gli allegati Excel.</summary>
        public List<string> ExcelDocuments { get; } = new List<string>();
        /// <summary>
        /// I pdf allegati, ognuno con la sua pagina già scritta. Con l'OCR pronto vanno convertiti subito; senza
        /// (<see cref="WordAttachmentTree"/> costruito con un «perché no») aspettano che l'utente lo scarichi.
        /// </summary>
        public List<PendingPdfAttachment> Pdfs { get; } = new List<PendingPdfAttachment>();
    }

    /// <summary>
    /// Un Word e i suoi allegati, in markdown (utente, 18/11/2026: «dopo aver estratto i documenti embeddati bisogna
    /// innescare, in maniera ricorsiva, la trasformazione in markdown»).
    /// <para>
    /// Un allegato Word si converte con lo stesso algoritmo, e i suoi allegati con lui, fino a
    /// <see cref="MaxDepth"/> livelli. Un Excel diventa tabelle (<see cref="ExcelToMarkdown"/>). Un pdf vuole l'OCR, che
    /// è lento: qui gli si prepara il markdown con scritto «conversione in corso» (o che cosa manca per farla) e lo si
    /// consegna a chi ha chiamato, che lo riempie dopo. Gli altri allegati restano file.
    /// </para>
    /// <para>
    /// Ogni markdown di allegato sta accanto al documento principale e ne porta il nome: «Documento.attach1.md», con
    /// le immagini in «assets/Documento.attach1_img». Nel documento, al posto del solo collegamento al file, vanno
    /// due collegamenti: la versione markdown e l'originale.
    /// </para>
    /// </summary>
    public sealed class WordAttachmentTree
    {
        /// <summary>Allegato dentro allegato dentro allegato: oltre, restano file.</summary>
        public const int MaxDepth = 3;

        private readonly string _documentFolder;
        private readonly Func<string, string> _link;
        private readonly string _whyNoPdf;
        private readonly Func<byte[], string, string, string> _metafileToPng;
        private WordAttachmentTreeResult _result;

        /// <param name="documentFolder">La cartella del documento principale: lì vanno tutti i markdown.</param>
        /// <param name="link">Da un percorso completo a come lo scrive un collegamento del progetto.</param>
        /// <param name="whyNoPdf">null se i pdf allegati si possono convertire; altrimenti perché no (lo si riporta fra gli avvisi).</param>
        public WordAttachmentTree(string documentFolder, Func<string, string> link, string whyNoPdf)
            : this(documentFolder, link, whyNoPdf, MetafileToPng.Convert)
        {
        }

        internal WordAttachmentTree(string documentFolder, Func<string, string> link, string whyNoPdf, Func<byte[], string, string, string> metafileToPng)
        {
            _documentFolder = documentFolder;
            _link = link;
            _whyNoPdf = whyNoPdf;
            _metafileToPng = metafileToPng;
        }

        /// <param name="baseName">Il nome del markdown principale, senza estensione.</param>
        public WordAttachmentTreeResult Convert(string docxPath, string baseName)
        {
            _result = new WordAttachmentTreeResult();
            _result.Markdown = ConvertWord(docxPath, baseName, 1);
            return _result;
        }

        private string ConvertWord(string docxPath, string baseName, int depth)
        {
            var imagesDirectory = Path.Combine(_documentFolder, "assets", baseName + "_img");
            var word = WordToMarkdownConverter.Convert(docxPath, imagesDirectory, _link(imagesDirectory), _metafileToPng);
            // Gli avvisi di un allegato dicono di quale allegato parlano: altrimenti sembrano del documento principale.
            _result.Warnings.AddRange(depth == 1 ? word.Warnings : word.Warnings.ConvertAll(warning => baseName + ".md: " + warning));
            var markdown = word.Markdown;

            var pdfsLeft = 0;
            var tooDeep = 0;
            foreach (var attachment in word.AttachmentFiles)
            {
                var childName = baseName + ".attach" + attachment.Number;
                var childPath = Path.Combine(_documentFolder, childName + ".md");
                var kind = attachment.Extension;
                var isWord = kind == ".docx" || kind == ".docm";
                var isExcel = kind == ".xlsx" || kind == ".xlsm";
                if (!isWord && !isExcel && kind != ".pdf")
                {
                    continue;
                }
                if (kind == ".pdf" && _whyNoPdf != null)
                {
                    pdfsLeft++;
                }
                if (isWord && depth >= MaxDepth)
                {
                    tooDeep++;
                    continue;
                }
                if (File.Exists(childPath))
                {
                    throw new IOException(UiText.Say(
                        $"Esiste già un file {childPath}: non lo sovrascrivo.",
                        $"A file {childPath} already exists: it will not be overwritten."));
                }

                try
                {
                    if (isWord)
                    {
                        File.WriteAllText(childPath, ConvertWord(attachment.FullPath, childName, depth + 1));
                        _result.WordDocuments.Add(childPath);
                    }
                    else if (isExcel)
                    {
                        File.WriteAllText(childPath, ExcelToMarkdown.Convert(attachment.FullPath, attachment.Name, _result.Warnings));
                        _result.ExcelDocuments.Add(childPath);
                    }
                    else
                    {
                        // La pagina c'è comunque: o dice che la conversione è in corso, o che cosa manca per farla.
                        var state = _whyNoPdf == null
                            ? UiText.Say(
                                "_La conversione di questo pdf in markdown è in corso: la pagina si riempie quando finisce._",
                                "_This pdf is being converted to markdown: the page fills in when it is done._")
                            : UiText.Say(
                                "_Questo pdf non è ancora stato convertito in markdown: " + _whyNoPdf + " Con il modello scaricato, aggiungi il pdf da «Aggiungi come collegamento» scegliendo anche la versione markdown._",
                                "_This pdf has not been converted to markdown yet: " + _whyNoPdf + " Once the model is downloaded, add the pdf from \"Add as link\" choosing the markdown version too._");
                        File.WriteAllText(childPath, "# " + attachment.Name + "\n\n" + state + "\n\n["
                            + UiText.Say("Il pdf originale", "The original pdf") + "](" + attachment.Link + ")\n");
                        _result.Pdfs.Add(new PendingPdfAttachment { Name = attachment.Name, PdfFullPath = attachment.FullPath, MarkdownFullPath = childPath, PdfLink = attachment.Link });
                    }
                }
                catch (Exception ex) when (ex is InvalidDataException || ex is DocumentFormat.OpenXml.Packaging.OpenXmlPackageException || ex is System.Xml.XmlException)
                {
                    // Un allegato rovinato non ferma il documento che lo contiene: resta file, e lo si dice.
                    _result.Warnings.Add(UiText.Say(
                        $"L'allegato «{attachment.Name}» non è stato convertito in markdown, resta come file: {ex.Message}",
                        $"The attachment \"{attachment.Name}\" was not converted to markdown, it stays a file: {ex.Message}"));
                    continue;
                }

                markdown = markdown.Replace(attachment.MarkdownLink,
                    attachment.LinkText + " — [" + UiText.Say("versione markdown", "markdown version") + "](" + _link(childPath) + ") · ["
                    + UiText.Say("originale", "original") + "](" + attachment.Link + ")");
            }

            if (pdfsLeft > 0)
            {
                _result.Warnings.Add(UiText.Say(
                    $"{pdfsLeft} pdf allegati aspettano di essere convertiti in markdown: {_whyNoPdf}",
                    $"{pdfsLeft} attached pdf files are waiting to be converted to markdown: {_whyNoPdf}"));
            }
            if (tooDeep > 0)
            {
                _result.Warnings.Add(UiText.Say(
                    $"{tooDeep} Word allegati stanno oltre il {MaxDepth}º livello di allegati dentro allegati: restano come file.",
                    $"{tooDeep} attached Word files sit beyond level {MaxDepth} of attachments inside attachments: they stay files."));
            }
            return markdown;
        }
    }
}

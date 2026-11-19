using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services;
using MdExplorer.Features.Services.Ocr;
using MdExplorer.Features.Utilities;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Service.Services.Ocr
{
    /// <summary>Un documento da portare in markdown: da dove, per quale documento, con quale modello.</summary>
    public class DocumentConversionRequest
    {
        /// <summary>Il pdf o il Word, già copiato nel progetto.</summary>
        public string SourceFullPath { get; set; }

        /// <summary>Il documento markdown che riceve il collegamento.</summary>
        public string MdDocumentFullPath { get; set; }

        public string ProjectPath { get; set; }

        /// <summary>Il modello OCR; serve solo ai pdf.</summary>
        public string ModelId { get; set; }
    }

    /// <summary>Lo stato di una conversione, così come lo legge l'interfaccia.</summary>
    public class DocumentConversionJob
    {
        public string Id { get; set; }
        public string FileName { get; set; }

        /// <summary><c>queued</c>, <c>running</c>, <c>done</c>, <c>error</c>.</summary>
        public string Status { get; set; } = "queued";

        public int Page { get; set; }
        public int Pages { get; set; }

        /// <summary>Il markdown prodotto, dalla radice del progetto (comincia con «/»).</summary>
        public string OutputRelativePath { get; set; }

        /// <summary>Pagine del pdf che il modello non ha finito di scrivere.</summary>
        public List<int> TruncatedPages { get; set; } = new List<int>();

        /// <summary>Ciò che il convertitore Word non ha saputo tradurre (stili sconosciuti…).</summary>
        public List<string> Warnings { get; set; } = new List<string>();

        public string Error { get; set; }

        /// <summary>L'errore si risolve scaricando il modello: l'interfaccia propone il download.</summary>
        public bool NeedsModel { get; set; }

        /// <summary>
        /// I markdown degli allegati Word convertiti insieme al documento (percorsi del progetto): hanno immagini
        /// che Mark può sistemare, dopo quelle del documento principale.
        /// </summary>
        public List<string> AttachmentDocuments { get; set; } = new List<string>();

        /// <summary>
        /// Le conversioni dei pdf allegati, avviate in coda a questa: il documento è già pronto, loro vogliono
        /// mezzo minuto a pagina. Chi segue questa conversione segue anche quelle.
        /// </summary>
        public List<string> FollowUpJobIds { get; set; } = new List<string>();

        /// <summary>
        /// Quanti pdf allegati aspettano il modello OCR, che non è scaricato: chi segue la conversione propone di
        /// scaricarlo, e poi li fa partire con <see cref="IDocumentConversionJobs.StartAttachedPdfs"/>.
        /// </summary>
        public int PdfsWaitingForOcr { get; set; }
    }

    /// <summary>Lo stato dell'installazione di un modello OCR (motore più file del modello).</summary>
    public class OcrInstallState
    {
        public string ModelId { get; set; }

        /// <summary><c>idle</c>, <c>engine</c>, <c>model</c>, <c>done</c>, <c>error</c>.</summary>
        public string Phase { get; set; } = "idle";

        public long BytesDownloaded { get; set; }
        public long TotalBytes { get; set; }
        public string Error { get; set; }
    }

    public interface IDocumentConversionJobs
    {
        DocumentConversionJob Start(DocumentConversionRequest request);
        DocumentConversionJob Find(string id);

        /// <summary>
        /// Avvia la conversione dei pdf allegati che una conversione finita ha lasciato in attesa del modello OCR.
        /// Torna le conversioni avviate (da seguire come le altre).
        /// </summary>
        List<DocumentConversionJob> StartAttachedPdfs(string jobId, string modelId);

        OcrInstallState InstallState { get; }

        /// <summary>Avvia in sottofondo il download di motore e modello; se è già in corso non fa nulla.</summary>
        void StartInstall(string modelId);
    }

    /// <summary>
    /// Le conversioni in markdown girano in sottofondo, una alla volta.
    /// <para>
    /// In sottofondo perché un pdf vuole mezzo minuto a pagina: una richiesta HTTP lunga venti minuti
    /// cade al primo sonno del portatile. Una alla volta perché il modello occupa quasi tutti i core e
    /// 1,6 GB di RAM: due insieme vanno più piano di due in fila.
    /// </para>
    /// </summary>
    public class DocumentConversionJobs : IDocumentConversionJobs
    {
        private readonly IPdfOcrService _pdf;
        private readonly IOcrEngine _engine;
        private readonly IModelDownloadService _models;
        private readonly ILogger<DocumentConversionJobs> _logger;
        private readonly ConcurrentDictionary<string, DocumentConversionJob> _jobs = new ConcurrentDictionary<string, DocumentConversionJob>();
        private readonly SemaphoreSlim _oneAtATime = new SemaphoreSlim(1, 1);
        // conversione finita → i suoi pdf allegati rimasti senza OCR, finché il servizio resta acceso
        private readonly ConcurrentDictionary<string, (List<PendingPdfAttachment> Pdfs, string ProjectPath)> _waitingPdfs
            = new ConcurrentDictionary<string, (List<PendingPdfAttachment> Pdfs, string ProjectPath)>();
        private readonly object _installLock = new object();
        private bool _installing;

        public DocumentConversionJobs(IPdfOcrService pdf, IOcrEngine engine, IModelDownloadService models,
            ILogger<DocumentConversionJobs> logger)
        {
            _pdf = pdf;
            _engine = engine;
            _models = models;
            _logger = logger;
        }

        public OcrInstallState InstallState { get; private set; } = new OcrInstallState();

        public DocumentConversionJob Find(string id) =>
            id != null && _jobs.TryGetValue(id, out var job) ? job : null;

        public DocumentConversionJob Start(DocumentConversionRequest request)
        {
            var extension = Path.GetExtension(request.SourceFullPath).ToLowerInvariant();
            if (extension != ".pdf" && extension != ".docx")
            {
                throw new ArgumentException(UiText.Say(
                    $"MdExplorer converte in markdown i .pdf e i .docx, non i «{extension}».",
                    $"MdExplorer converts .pdf and .docx files to markdown, not \"{extension}\" files."));
            }
            foreach (var path in new[] { request.SourceFullPath, request.MdDocumentFullPath })
            {
                if (!PathContainment.IsInside(request.ProjectPath, path) || !File.Exists(path))
                {
                    throw new ArgumentException(UiText.Say(
                        $"Il file non è nel progetto aperto: {path}", $"The file is not in the open project: {path}"));
                }
            }
            if (extension == ".pdf")
            {
                var model = _models.FindModel(request.ModelId);
                if (model == null || model.Category != "ocr")
                {
                    throw new ArgumentException(UiText.Say(
                        $"«{request.ModelId}» non è un modello OCR del catalogo.",
                        $"\"{request.ModelId}\" is not an OCR model of the catalog."));
                }
                if (!model.IsInstalled || !_engine.GetStatus().IsInstalled)
                {
                    throw new OcrNotReadyException(UiText.Say(
                        $"Il modello OCR «{model.Name}» non è ancora scaricato.",
                        $"The OCR model \"{model.Name}\" has not been downloaded yet."));
                }
            }

            var job = new DocumentConversionJob
            {
                Id = Guid.NewGuid().ToString("N"),
                FileName = Path.GetFileName(request.SourceFullPath)
            };
            _jobs[job.Id] = job;
            _ = Task.Run(() => RunAsync(job, request));
            return job;
        }

        private async Task RunAsync(DocumentConversionJob job, DocumentConversionRequest request)
        {
            await _oneAtATime.WaitAsync();
            try
            {
                job.Status = "running";
                var documentFolder = Path.GetDirectoryName(request.MdDocumentFullPath);
                var baseName = Path.GetFileNameWithoutExtension(request.SourceFullPath);
                var outputPath = Path.Combine(documentFolder, baseName + ".md");
                if (File.Exists(outputPath))
                {
                    throw new IOException(UiText.Say(
                        $"Esiste già un file {outputPath}: non lo sovrascrivo.",
                        $"A file {outputPath} already exists: it will not be overwritten."));
                }

                string markdown;
                var attachedPdfs = new List<PendingPdfAttachment>();
                string attachedPdfModel = null;
                if (Path.GetExtension(request.SourceFullPath).Equals(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    // Il Word e, in modo ricorsivo, i suoi allegati: Word ed Excel subito, i pdf in coda (vogliono l'OCR).
                    string whyNoPdf = null;
                    if (!_engine.GetStatus().IsInstalled)
                    {
                        whyNoPdf = UiText.Say(
                            "il motore OCR non è installato.",
                            "the OCR engine is not installed.");
                    }
                    else
                    {
                        var model = !string.IsNullOrEmpty(request.ModelId) ? _models.FindModel(request.ModelId)
                            : (await _models.GetAvailableModelsAsync()).FirstOrDefault(candidate => candidate.Category == "ocr" && candidate.IsInstalled);
                        if (model == null || model.Category != "ocr" || !model.IsInstalled)
                        {
                            whyNoPdf = UiText.Say(
                                "il modello OCR non è scaricato.",
                                "the OCR model has not been downloaded.");
                        }
                        else
                        {
                            attachedPdfModel = model.Id;
                        }
                    }

                    var tree = new WordAttachmentTree(documentFolder, path => ProjectLink(request.ProjectPath, path), whyNoPdf)
                        .Convert(request.SourceFullPath, baseName);
                    markdown = tree.Markdown;
                    job.Warnings = tree.Warnings;
                    job.AttachmentDocuments = tree.WordDocuments.Select(path => ProjectLink(request.ProjectPath, path)).ToList();
                    attachedPdfs = tree.Pdfs;
                }
                else
                {
                    var pdf = await _pdf.ConvertAsync(request.SourceFullPath, request.ModelId, (page, pages) =>
                    {
                        job.Page = page;
                        job.Pages = pages;
                    }, CancellationToken.None);
                    markdown = pdf.Markdown;
                    job.TruncatedPages = pdf.TruncatedPages;
                }

                File.WriteAllText(outputPath, markdown);
                job.OutputRelativePath = ProjectLink(request.ProjectPath, outputPath);
                AppendLink(request.MdDocumentFullPath, Path.GetFileName(outputPath), job.OutputRelativePath);
                if (attachedPdfModel != null)
                {
                    job.FollowUpJobIds = QueueAttachedPdfs(attachedPdfs, attachedPdfModel, request.ProjectPath).Select(queued => queued.Id).ToList();
                }
                else if (attachedPdfs.Count > 0)
                {
                    _waitingPdfs[job.Id] = (attachedPdfs, request.ProjectPath);
                    job.PdfsWaitingForOcr = attachedPdfs.Count;
                }
                job.Status = "done";
                _logger.LogInformation("[Conversione] {Source} -> {Output} (allegati: {Words} Word, {Pdfs} pdf in coda)",
                    request.SourceFullPath, outputPath, job.AttachmentDocuments.Count, attachedPdfs.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Conversione] {Source} non convertito", request.SourceFullPath);
                job.NeedsModel = ex is OcrNotReadyException;
                job.Error = ex.Message;
                job.Status = "error";
            }
            finally
            {
                _oneAtATime.Release();
            }
        }

        public List<DocumentConversionJob> StartAttachedPdfs(string jobId, string modelId)
        {
            var model = _models.FindModel(modelId);
            if (model == null || model.Category != "ocr")
            {
                throw new ArgumentException(UiText.Say(
                    $"«{modelId}» non è un modello OCR del catalogo.", $"\"{modelId}\" is not an OCR model of the catalog."));
            }
            if (!model.IsInstalled || !_engine.GetStatus().IsInstalled)
            {
                throw new OcrNotReadyException(UiText.Say(
                    $"Il modello OCR «{model.Name}» non è ancora scaricato.",
                    $"The OCR model \"{model.Name}\" has not been downloaded yet."));
            }
            if (jobId == null || !_waitingPdfs.TryRemove(jobId, out var waiting))
            {
                throw new ArgumentException(UiText.Say(
                    "Questa conversione non ha pdf allegati in attesa (MdExplorer è stato riavviato, o sono già partiti).",
                    "This conversion has no attached pdf files waiting (MdExplorer was restarted, or they have already started)."));
            }
            var parent = Find(jobId);
            var queued = QueueAttachedPdfs(waiting.Pdfs, model.Id, waiting.ProjectPath);
            if (parent != null)
            {
                parent.PdfsWaitingForOcr = 0;
                parent.FollowUpJobIds = queued.Select(job => job.Id).ToList();
            }
            return queued;
        }

        private List<DocumentConversionJob> QueueAttachedPdfs(List<PendingPdfAttachment> pdfs, string modelId, string projectPath)
        {
            var queued = new List<DocumentConversionJob>();
            foreach (var pdf in pdfs)
            {
                var followUp = new DocumentConversionJob { Id = Guid.NewGuid().ToString("N"), FileName = PdfName(pdf.Name) };
                _jobs[followUp.Id] = followUp;
                queued.Add(followUp);
                _ = Task.Run(() => RunAttachedPdfAsync(followUp, pdf, modelId, projectPath));
            }
            return queued;
        }

        // Chi segue la conversione decide dal nome che cosa fare dopo: un pdf non passa da Mark.
        private static string PdfName(string name) => name.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? name : name + ".pdf";

        /// <summary>
        /// Un pdf allegato a un Word già convertito: il suo markdown esiste, con scritto che la conversione è in corso.
        /// Qui lo si riempie; se non riesce, nella pagina resta scritto perché, con il collegamento all'originale.
        /// </summary>
        private async Task RunAttachedPdfAsync(DocumentConversionJob job, PendingPdfAttachment attachment, string modelId, string projectPath)
        {
            await _oneAtATime.WaitAsync();
            var heading = "# " + attachment.Name + "\n\n[" + UiText.Say("Il pdf originale", "The original pdf") + "](" + attachment.PdfLink + ")\n\n";
            try
            {
                job.Status = "running";
                var pdf = await _pdf.ConvertAsync(attachment.PdfFullPath, modelId, (page, pages) =>
                {
                    job.Page = page;
                    job.Pages = pages;
                }, CancellationToken.None);
                File.WriteAllText(attachment.MarkdownFullPath, heading + pdf.Markdown);
                job.TruncatedPages = pdf.TruncatedPages;
                job.OutputRelativePath = ProjectLink(projectPath, attachment.MarkdownFullPath);
                job.Status = "done";
                _logger.LogInformation("[Conversione] allegato {Source} -> {Output}", attachment.PdfFullPath, attachment.MarkdownFullPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Conversione] allegato {Source} non convertito", attachment.PdfFullPath);
                try
                {
                    File.WriteAllText(attachment.MarkdownFullPath, heading + UiText.Say(
                        $"_La conversione di questo pdf in markdown non è riuscita: {ex.Message}_\n",
                        $"_The conversion of this pdf to markdown failed: {ex.Message}_\n"));
                }
                catch (IOException write)
                {
                    _logger.LogWarning(write, "[Conversione] non riesco a scrivere l'esito in {Output}", attachment.MarkdownFullPath);
                }
                job.Error = ex.Message;
                job.Status = "error";
            }
            finally
            {
                _oneAtATime.Release();
            }
        }

        /// <summary>Un percorso del progetto come lo scrivono i collegamenti di MDE: dalla radice, con «/».</summary>
        private static string ProjectLink(string projectPath, string fullPath)
        {
            var relative = PathContainment.RelativeOrNull(projectPath, fullPath)
                ?? throw new InvalidOperationException($"{fullPath} is outside the project {projectPath}");
            return "/" + relative.Replace('\\', '/');
        }

        private static void AppendLink(string mdDocumentFullPath, string label, string link)
        {
            var text = File.ReadAllText(mdDocumentFullPath).TrimEnd('\r', '\n');
            File.WriteAllText(mdDocumentFullPath, text + Environment.NewLine + Environment.NewLine + $"[{label}]({link})");
        }

        public void StartInstall(string modelId)
        {
            var model = _models.FindModel(modelId);
            if (model == null || model.Category != "ocr")
            {
                throw new ArgumentException(UiText.Say(
                    $"«{modelId}» non è un modello OCR del catalogo.", $"\"{modelId}\" is not an OCR model of the catalog."));
            }

            lock (_installLock)
            {
                if (_installing)
                {
                    return;
                }
                _installing = true;
                InstallState = new OcrInstallState { ModelId = modelId, Phase = "engine" };
            }

            _ = Task.Run(async () =>
            {
                var state = InstallState;
                try
                {
                    await _engine.InstallAsync(new Immediate(p =>
                    {
                        state.BytesDownloaded = p.BytesDownloaded;
                        state.TotalBytes = p.TotalBytes;
                    }), CancellationToken.None);

                    state.Phase = "model";
                    state.BytesDownloaded = 0;
                    state.TotalBytes = model.TotalSize;
                    string failure = null;
                    var ok = await _models.DownloadModelAsync(modelId, new Immediate(p =>
                    {
                        if (p.Status != null && p.Status.StartsWith("Error"))
                        {
                            failure = p.Status;
                            return;
                        }
                        state.BytesDownloaded = p.BytesDownloaded;
                        state.TotalBytes = p.TotalBytes > 0 ? p.TotalBytes : state.TotalBytes;
                    }), CancellationToken.None);
                    if (!ok)
                    {
                        throw new InvalidOperationException(failure ?? UiText.Say(
                            "Il download del modello non è riuscito.", "The model download failed."));
                    }
                    state.Phase = "done";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[OCR] installazione di {Model} fallita", modelId);
                    state.Error = ex.Message;
                    state.Phase = "error";
                }
                finally
                {
                    lock (_installLock)
                    {
                        _installing = false;
                    }
                }
            });
        }

        /// <summary><c>Progress&lt;T&gt;</c> consegna fuori ordine sul pool: qui l'ultimo valore deve essere l'ultimo.</summary>
        private sealed class Immediate : IProgress<DownloadProgress>
        {
            private readonly Action<DownloadProgress> _report;
            public Immediate(Action<DownloadProgress> report) => _report = report;
            public void Report(DownloadProgress value) => _report(value);
        }
    }
}

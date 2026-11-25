using MdExplorer.Abstractions.DB;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.Entities.EngineDB;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;
using Ad.Tools.Dal.Abstractions.Interfaces;
using MdExplorer.Features.ActionLinkModifiers.Interfaces;
using System.Text.RegularExpressions;

namespace MdExplorer.Features.Refactoring
{
    public class RefactoringManager
    {
        private readonly IEngineDB _engineDB;
        private readonly IWorkLink[] _workLinks;

        public RefactoringManager(
            IEngineDB engineDB,
            IWorkLink[] workLinks)
        {
            _engineDB = engineDB;
            _workLinks = workLinks;
        }

        public void RenameTheMdFileIntoEngineDB(
            string fullPath,
            string fromFileName,
            string toFileName)
        {

            var oldFullPath = fullPath + Path.DirectorySeparatorChar + fromFileName;
            var newFullPath = fullPath + Path.DirectorySeparatorChar + toFileName;
            var markdonwFileDal = _engineDB.GetDal<MarkdownFile>();
            // Il file può non essere ancora nell'indice (spostato mentre il progetto si indicizza): allora lo troverà
            // l'indicizzazione al posto nuovo. Prima qui c'era First(): il file era già spostato, l'errore fermava il
            // ricalcolo dei link e li lasciava rotti.
            var changingFile = markdonwFileDal.GetList().FirstOrDefault(_ => _.Path == oldFullPath);
            if (changingFile == null) return;
            changingFile.Path = newFullPath;
            changingFile.FileName = Path.GetFileName(toFileName);
            markdonwFileDal.Save(changingFile);
            _engineDB.Flush();
        }

        public RefactoringSourceAction SaveRefactoringActionForMoveFile(
            string filename,
            string fromFullPathDirectory,
            string toFullPathDirectory)
        {
            var oldFullPath = fromFullPathDirectory + Path.DirectorySeparatorChar + filename;
            var newFullPath = toFullPathDirectory + Path.DirectorySeparatorChar + filename;

            var sourceActionDal = _engineDB.GetDal<RefactoringSourceAction>();
            var refSourceAct = new RefactoringSourceAction
            {
                Action = "Move File",
                NewFullPath = newFullPath,
                OldFullPath = oldFullPath,
                NewName = filename,
                OldName = filename,
                CreationDate = DateTime.Now,
                Status = "ToDo"
            };
            sourceActionDal.Save(refSourceAct);
            _engineDB.Flush();
            return refSourceAct;
        }

        public void SetInternalLinks(
            string toDestinationFileName,
            string projectBasePath,
            RefactoringSourceAction refSourceAction
            )
        {
            var linkDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var refInvolvedDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();

            // All links INSIDE the moved file (MarkdownFile.Path already updated by RenameTheMdFileIntoEngineDB)
            var linksInMovedFile = linkDal.GetList()
                .Where(_ => _.MarkdownFile.Path == refSourceAction.NewFullPath)
                .ToList();

            var newFileDir = Path.GetDirectoryName(refSourceAction.NewFullPath);

            foreach (var item in linksInMovedFile.Where(_ => !ReadByMarkdig(_.Source)))
            {
                // Skip external links, anchor-only, data URI
                if (string.IsNullOrEmpty(item.Path) ||
                    item.Path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    item.Path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                    item.Path.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
                    item.Path.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                    item.Path.StartsWith("#"))
                    continue;

                // The link target (image, other .md) does NOT move.
                // item.FullPath = absolute path of the target, resolved during indexing.
                var targetAbsolutePath = item.FullPath;

                // Calculate new relative path from the new file location to the target
                var newRelativePath = Path.GetRelativePath(newFileDir, targetAbsolutePath)
                    .Replace(Path.DirectorySeparatorChar, '/');

                // If the path hasn't changed, skip
                if (newRelativePath == item.Path.Replace(Path.DirectorySeparatorChar, '/'))
                    continue;

                // Build new LinkedCommand by replacing the old path with the new one
                var newLinkedCommand = item.LinkedCommand.Replace(item.Path, newRelativePath);

                var workLink = _workLinks.FirstOrDefault(_ => _.GetType().Name == item.Source);
                if (workLink == null) continue;

                var involved = new RefactoringInvolvedFilesAction
                {
                    CreationDate = DateTime.Now,
                    FileName = Path.GetFileName(refSourceAction.NewName),
                    FullPath = refSourceAction.NewFullPath,
                    NewLinkToReplace = newLinkedCommand,
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Relink",
                    RefactoringSourceAction = refSourceAction,
                    LinkInsideMarkdown = item
                };
                refInvolvedDal.Save(involved);
            }
            _engineDB.Flush();
        }

        public void SetExternalLinks(
            string toDestinationFileName,
            RefactoringSourceAction refSourceAction
            )
        {
            var linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var refInvolvedDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();

            // Links in OTHER files whose target (FullPath) points to the old location of the moved file
            var linksToMovedFile = linkInsideMdDal.GetList()
                .ToList()
                .Where(_ => _.FullPath != null && string.Equals(_.FullPath, Path.GetFullPath(refSourceAction.OldFullPath), StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var item in linksToMovedFile.Where(_ => !ReadByMarkdig(_.Source)))
            {
                var workLink = _workLinks.FirstOrDefault(_ => _.GetType().Name == item.Source);
                if (workLink == null) continue;

                // Directory of the file that CONTAINS the link (does not move)
                var referenceFileDir = Path.GetDirectoryName(item.MarkdownFile.Path);

                // Calculate new relative path from the referencing file to the new location of the moved file
                var newRelativePath = Path.GetRelativePath(referenceFileDir, refSourceAction.NewFullPath)
                    .Replace(Path.DirectorySeparatorChar, '/');

                // Build new LinkedCommand
                var newLinkedCommand = item.LinkedCommand.Replace(item.Path, newRelativePath);

                var involved = new RefactoringInvolvedFilesAction
                {
                    CreationDate = DateTime.Now,
                    FileName = item.MarkdownFile.FileName,
                    FullPath = item.MarkdownFile.Path,
                    NewLinkToReplace = newLinkedCommand,
                    OldLinkStored = item.LinkedCommand,
                    SuggestedAction = "Relink",
                    RefactoringSourceAction = refSourceAction,
                    LinkInsideMarkdown = item
                };
                refInvolvedDal.Save(involved);
            }
            _engineDB.Flush();
        }


        public IReadOnlyList<string> RelinkMovedDocument(string projectRoot, string oldFullPath, string newFullPath)
        {
            oldFullPath = Path.GetFullPath(oldFullPath);
            newFullPath = Path.GetFullPath(newFullPath);
            var changed = new List<string>();
            var cmp = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            if (TryRelink(newFullPath, projectRoot, oldFullPath, newFullPath, isMoved: true)) changed.Add(newFullPath);

            var name = Path.GetFileName(oldFullPath);
            var escaped = Uri.EscapeDataString(name);
            // Dal disco E dall'indice: mentre il progetto si indicizza l'indice è incompleto.
            var documents = ProjectDocuments(projectRoot)
                .Concat(_engineDB.GetDal<MarkdownFile>().GetList().Select(m => m.Path).ToList())
                .Where(d => d != null && d.EndsWith(".md", StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var doc in documents.Where(d => d != null && !string.Equals(Path.GetFullPath(d), newFullPath, cmp)).Distinct())
            {
                if (!File.Exists(doc)) continue;
                var text = ReadText(doc, out _);
                if (text.IndexOf(name, cmp) < 0 && text.IndexOf(escaped, cmp) < 0) continue;
                if (TryRelink(doc, projectRoot, oldFullPath, newFullPath, isMoved: false)) changed.Add(doc);
            }
            return changed;
        }

        // Le cartelle che non sono documenti del progetto: git, le copie degli agenti, le dipendenze, i dati di MDE.
        private static readonly HashSet<string> NotDocumentFolders = new(StringComparer.OrdinalIgnoreCase)
            { ".git", ".worktrees", "node_modules", ".md" };

        private static IEnumerable<string> ProjectDocuments(string projectRoot)
        {
            if (string.IsNullOrEmpty(projectRoot) || !Directory.Exists(projectRoot)) yield break;
            var pending = new Stack<string>();
            pending.Push(projectRoot);
            while (pending.Count > 0)
            {
                var dir = pending.Pop();
                string[] files, subdirs;
                try { files = Directory.GetFiles(dir, "*.md"); subdirs = Directory.GetDirectories(dir); }
                catch (UnauthorizedAccessException) { continue; }
                catch (IOException) { continue; }
                foreach (var f in files) yield return f;
                foreach (var d in subdirs)
                    if (!NotDocumentFolders.Contains(Path.GetFileName(d))) pending.Push(d);
            }
        }

        /// <summary>
        /// Una cartella spostata da <paramref name="oldDir"/> a <paramref name="newDir"/> (già spostata sul disco): ogni
        /// documento dentro si legge come stava prima e i suoi link verso fuori si ricalcolano; ogni documento fuori con link
        /// verso qualcosa dentro (documenti, immagini, qualunque file) porta al posto nuovo. I link fra documenti della
        /// cartella non cambiano. I documenti fuori da guardare sono quelli il cui testo contiene il nome della cartella:
        /// un link che entra nella cartella lo deve attraversare.
        /// </summary>
        public IReadOnlyList<string> RelinkMovedFolder(string projectRoot, string oldDir, string newDir)
        {
            oldDir = Path.GetFullPath(oldDir).TrimEnd(Path.DirectorySeparatorChar);
            newDir = Path.GetFullPath(newDir).TrimEnd(Path.DirectorySeparatorChar);
            var cmp = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var newPrefix = newDir + Path.DirectorySeparatorChar;
            string Map(string target) => MarkdownLinks.InsideMovedFolder(target, oldDir, newDir);
            var changed = new List<string>();

            foreach (var doc in ProjectDocuments(newDir))
            {
                var oldDoc = Path.Combine(oldDir, doc.Substring(newPrefix.Length));
                if (TryRelinkMoves(doc, oldDoc, projectRoot, Map)) changed.Add(doc);
            }

            var name = Path.GetFileName(oldDir);
            var escaped = Uri.EscapeDataString(name);
            var others = ProjectDocuments(projectRoot)
                .Concat(_engineDB.GetDal<MarkdownFile>().GetList().Select(m => m.Path).ToList())
                .Where(d => d != null && d.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                            && !Path.GetFullPath(d).StartsWith(newPrefix, cmp)
                            && !Path.GetFullPath(d).StartsWith(oldDir + Path.DirectorySeparatorChar, cmp))
                .Distinct().ToList();
            foreach (var doc in others)
            {
                if (!File.Exists(doc)) continue;
                var text = ReadText(doc, out _);
                if (text.IndexOf(name, cmp) < 0 && text.IndexOf(escaped, cmp) < 0) continue;
                if (TryRelinkMoves(doc, doc, projectRoot, Map)) changed.Add(doc);
            }
            return changed;
        }

        private static bool TryRelinkMoves(string document, string documentOldPath, string projectRoot, Func<string, string> map)
        {
            var text = ReadText(document, out var bom);
            var updated = MarkdownLinks.RelinkForMoves(text, document, documentOldPath, projectRoot, map, out var count);
            if (count == 0 || updated == text) return false;
            File.WriteAllText(document, updated, new System.Text.UTF8Encoding(bom));
            return true;
        }

        private static bool TryRelink(string document, string projectRoot, string oldFullPath, string newFullPath, bool isMoved)
        {
            var text = ReadText(document, out var bom);
            var updated = MarkdownLinks.RelinkForMove(text, document, projectRoot, oldFullPath, newFullPath, isMoved, out var count);
            if (count == 0 || updated == text) return false;
            File.WriteAllText(document, updated, new System.Text.UTF8Encoding(bom));
            return true;
        }

        private static string ReadText(string path, out bool bom)
        {
            var bytes = File.ReadAllBytes(path);
            bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            return new System.Text.UTF8Encoding(false).GetString(bytes, bom ? 3 : 0, bytes.Length - (bom ? 3 : 0));
        }

        /// <summary>
        /// Rilegge i link dei documenti indicati e li riscrive nell'indice, come la pipeline di indicizzazione: dopo uno
        /// spostamento l'indice dice il vero (prima le righe aggiornate non venivano nemmeno salvate).
        /// </summary>
        public void ReindexLinks(string projectRoot, IEnumerable<string> documents)
        {
            var cmp = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var fileDal = _engineDB.GetDal<MarkdownFile>();
            var linkDal = _engineDB.GetDal<LinkInsideMarkdown>();
            var all = fileDal.GetList().ToList();
            foreach (var doc in documents.Distinct())
            {
                var mdf = all.FirstOrDefault(m => m.Path != null && string.Equals(Path.GetFullPath(m.Path), Path.GetFullPath(doc), cmp));
                if (mdf == null || !File.Exists(doc)) continue;
                foreach (var old in linkDal.GetList().Where(l => l.MarkdownFile == mdf).ToList()) linkDal.Delete(old);
                var mdContext = (Path.GetDirectoryName(mdf.Path) ?? string.Empty).Replace(projectRoot ?? string.Empty, string.Empty)
                    .Replace(Path.DirectorySeparatorChar, '/');
                foreach (var parser in _workLinks)
                    foreach (var link in parser.GetLinksFromFile(mdf.Path))
                        linkDal.Save(new LinkInsideMarkdown
                        {
                            FullPath = LinkIndexPath(projectRoot, mdf.Path, link.FullPath),
                            Path = link.FullPath,
                            Source = parser.GetType().Name,
                            LinkedCommand = link.LinkedCommand,
                            HTMLTitle = link.HTMLTitle,
                            MdTitle = link.MdTitle,
                            SectionIndex = link.SectionIndex,
                            MarkdownFile = mdf,
                            MdContext = mdContext,
                        });
            }
            _engineDB.Flush();
        }

        /// <summary>
        /// Il percorso completo che l'indice tiene per un link: il bersaglio vero (dalla radice per <c>/…</c>, dalla cartella
        /// del documento altrimenti) con il separatore del sistema. Lo usano tutti quelli che scrivono l'indice dei link.
        /// </summary>
        public static string LinkIndexPath(string projectRoot, string documentPath, string linkPath)
        {
            var resolved = MarkdownLinks.ResolveTarget(projectRoot, documentPath, linkPath ?? string.Empty);
            return resolved ?? Path.Combine(Path.GetDirectoryName(documentPath) ?? string.Empty, linkPath ?? string.Empty);
        }

        /// <summary>L'azione di spostamento è conclusa: lo stato lo dice.</summary>
        public void CloseAction(RefactoringSourceAction action)
        {
            action.Status = "Done";
            _engineDB.GetDal<RefactoringSourceAction>().Save(action);
            _engineDB.Flush();
        }

        public void UpdateAllInvolvedFilesAndReferencesToDB(
            RefactoringSourceAction refSourceAct)
        {
            var refInvolvedFilesActionDal = _engineDB.GetDal<RefactoringInvolvedFilesAction>();
            var linkInsideMdDal = _engineDB.GetDal<LinkInsideMarkdown>();

            var listOfInvolvedFiles = refInvolvedFilesActionDal
                .GetList().Where(_ => _.RefactoringSourceAction.Id == refSourceAct.Id).ToList();

            foreach (var refactInvolvedFile in listOfInvolvedFiles)
            {
                foreach (var getModifier in _workLinks
                    .Where(_ => _.GetType().Name == refactInvolvedFile.LinkInsideMarkdown.Source))
                {
                    // 1. Update the .md file on disk
                    getModifier.SetLinkIntoFile(refactInvolvedFile.FullPath,
                        refactInvolvedFile.OldLinkStored, refactInvolvedFile.NewLinkToReplace);

                    // 2. Update LinkedCommand in DB
                    refactInvolvedFile.LinkInsideMarkdown.LinkedCommand = refactInvolvedFile.NewLinkToReplace;

                    // 3. Extract the new relative path from the new LinkedCommand using the parser
                    var parsedLinks = getModifier.GetLinksFromMarkdown(refactInvolvedFile.NewLinkToReplace);
                    if (parsedLinks.Length > 0)
                    {
                        var newRelPath = parsedLinks[0].FullPath; // relative path extracted by parser
                        refactInvolvedFile.LinkInsideMarkdown.Path = newRelPath;

                        // 4. Resolve absolute FullPath: containing file directory + relative path
                        var containingFileDir = Path.GetDirectoryName(refactInvolvedFile.FullPath);
                        var resolvedFullPath = Path.GetFullPath(
                            Path.Combine(containingFileDir,
                                newRelPath.Replace('/', Path.DirectorySeparatorChar)));
                        refactInvolvedFile.LinkInsideMarkdown.FullPath = resolvedFullPath;
                    }

                    linkInsideMdDal.Save(refactInvolvedFile.LinkInsideMarkdown);
                }
                refInvolvedFilesActionDal.Save(refactInvolvedFile);
            }
            // Senza, le righe aggiornate restavano nella sessione e l'indice teneva i valori vecchi.
            _engineDB.Flush();
        }

        /// <summary>
        /// I link markdown e le immagini li sposta <see cref="RelinkMovedDocument"/> con Markdig; il meccanismo dei comandi
        /// salvati (<see cref="SetExternalLinks"/>, <see cref="SetInternalLinks"/>) resta per le altre sorgenti (PlantUML, MdShow).
        /// </summary>
        private static bool ReadByMarkdig(string source)
            => source == nameof(MdExplorer.Features.ActionLinkModifiers.WorkLinkFromMarkdown)
               || source == nameof(MdExplorer.Features.LinkModifiers.WorkLinkImagesFromMarkdown);
    }
}

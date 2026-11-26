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

        /// <summary>
        /// Un documento spostato (già sul disco), con i file che si è portato dietro (<paramref name="alsoMoved"/>: da
        /// <see cref="CarryDocumentAssets"/>): i suoi link si ricalcolano dal posto nuovo, e ogni documento che portava a
        /// lui o a uno di quei file porta al posto nuovo. Si guardano i documenti il cui testo contiene uno dei nomi.
        /// </summary>
        public IReadOnlyList<string> RelinkMovedDocument(string projectRoot, string oldFullPath, string newFullPath,
            IReadOnlyCollection<DocumentMove> alsoMoved = null)
        {
            oldFullPath = Path.GetFullPath(oldFullPath);
            newFullPath = Path.GetFullPath(newFullPath);
            var changed = new List<string>();
            var cmp = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var moved = new Dictionary<string, string>(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal)
                { [oldFullPath] = newFullPath };
            foreach (var m in alsoMoved ?? Array.Empty<DocumentMove>()) moved[Path.GetFullPath(m.From)] = Path.GetFullPath(m.To);
            string Map(string target) => moved.TryGetValue(target, out var to) ? to : null;

            if (TryRelinkMoves(newFullPath, oldFullPath, projectRoot, Map)) changed.Add(newFullPath);

            var names = moved.Keys.Select(Path.GetFileName).SelectMany(n => new[] { n, Uri.EscapeDataString(n) }).Distinct().ToList();
            foreach (var doc in OtherDocuments(projectRoot, newFullPath))
            {
                var text = ReadText(doc, out _);
                if (!names.Any(n => text.IndexOf(n, cmp) >= 0)) continue;
                if (TryRelinkMoves(doc, doc, projectRoot, Map)) changed.Add(doc);
            }
            return changed;
        }

        /// <summary>I documenti del progetto che ci sono sul disco, tranne <paramref name="except"/>: dal disco E dall'indice,
        /// perché mentre il progetto si indicizza l'indice è incompleto.</summary>
        private List<string> OtherDocuments(string projectRoot, params string[] except)
        {
            var cmp = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var comparer = OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;
            return ProjectDocuments(projectRoot)
                .Concat(_engineDB.GetDal<MarkdownFile>().GetList().Select(m => m.Path).ToList())
                .Where(d => d != null && d.EndsWith(".md", StringComparison.OrdinalIgnoreCase) && File.Exists(d))
                .Select(Path.GetFullPath)
                .Where(d => !except.Any(e => e != null && string.Equals(d, Path.GetFullPath(e), cmp)))
                .Distinct(comparer).ToList();
        }

        /// <summary>
        /// I file della cartella <c>assets</c> che seguono un documento spostato o rinominato (il documento è già al posto
        /// nuovo). Vanno con lui nella <c>assets</c> della cartella d'arrivo i file a cui portano i suoi link e che nessun
        /// altro documento usa (o che usano solo documenti spostati con lui nella stessa cartella, <paramref name="batch"/>);
        /// quelli usati anche da altri restano dove sono e il documento li raggiunge da lì. Il <c>.reference.docx</c> del
        /// documento (<c>assets/&lt;nome&gt;.md.reference.docx</c>) lo segue sempre e prende il nome nuovo. Mai sovrascrivere:
        /// se all'arrivo c'è già un file con quel nome, quello resta dov'è. Restituisce i file spostati.
        /// </summary>
        public IReadOnlyList<DocumentMove> CarryDocumentAssets(string projectRoot, string oldDoc, string newDoc,
            IReadOnlyCollection<DocumentMove> batch = null)
        {
            oldDoc = Path.GetFullPath(oldDoc);
            newDoc = Path.GetFullPath(newDoc);
            var cmp = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var oldDir = Path.GetDirectoryName(oldDoc);
            var newDir = Path.GetDirectoryName(newDoc);
            var oldAssets = Path.Combine(oldDir, "assets");
            var newAssets = Path.Combine(newDir, "assets");
            var carried = new List<DocumentMove>();
            if (!Directory.Exists(oldAssets) || !File.Exists(newDoc)) return carried;

            var reference = Path.Combine(oldAssets, Path.GetFileName(oldDoc) + ".reference.docx");
            var plan = new List<DocumentMove>();
            if (File.Exists(reference))
                plan.Add(new DocumentMove(reference, Path.Combine(newAssets, Path.GetFileName(newDoc) + ".reference.docx"), false));

            if (!string.Equals(oldDir, newDir, cmp))
            {
                var prefix = oldAssets + Path.DirectorySeparatorChar;
                var used = MarkdownLinks.TargetsOf(ReadText(newDoc, out _), oldDoc, projectRoot)
                    .Where(t => t.StartsWith(prefix, cmp) && File.Exists(t) && !t.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                                && !string.Equals(t, reference, cmp))
                    .Distinct(OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal).ToList();
                if (used.Count > 0)
                {
                    // Dove finirà ogni documento: quelli del gruppo nella loro cartella d'arrivo, gli altri dove sono.
                    string FinalDir(string doc)
                    {
                        var m = batch?.FirstOrDefault(b => string.Equals(Path.GetFullPath(b.From), doc, cmp));
                        return Path.GetDirectoryName(m != null ? Path.GetFullPath(m.To) : doc);
                    }
                    var others = OtherDocuments(projectRoot, newDoc, oldDoc)
                        .Select(d => (Doc: d, Text: ReadText(d, out _))).ToList();
                    foreach (var file in used)
                    {
                        var name = Path.GetFileName(file);
                        var users = others.Where(o => (o.Text.IndexOf(name, cmp) >= 0 || o.Text.IndexOf(Uri.EscapeDataString(name), cmp) >= 0)
                                                      && MarkdownLinks.TargetsOf(o.Text, o.Doc, projectRoot).Any(t => string.Equals(t, file, cmp)));
                        if (users.All(u => string.Equals(FinalDir(u.Doc), newDir, cmp)))
                            plan.Add(new DocumentMove(file, Path.Combine(newAssets, file.Substring(prefix.Length)), false));
                    }
                }
            }

            foreach (var move in plan)
            {
                if (File.Exists(move.To) || Directory.Exists(move.To)) continue;
                Directory.CreateDirectory(Path.GetDirectoryName(move.To));
                File.Move(move.From, move.To, false);
                carried.Add(move);
            }
            // Le cartelle rimaste vuote dentro la vecchia assets (le immagini di un Word convertito, assets stessa).
            foreach (var dir in carried.Select(c => Path.GetDirectoryName(c.From)).Distinct().OrderByDescending(d => d.Length))
                for (var d = dir; d != null && (d.StartsWith(oldAssets + Path.DirectorySeparatorChar, cmp) || string.Equals(d, oldAssets, cmp)); d = Path.GetDirectoryName(d))
                {
                    if (!Directory.Exists(d) || Directory.EnumerateFileSystemEntries(d).Any()) break;
                    Directory.Delete(d);
                }
            return carried;
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

    }
}

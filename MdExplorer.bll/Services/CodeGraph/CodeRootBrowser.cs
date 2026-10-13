using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MdExplorer.Features.Utilities;

namespace MdExplorer.Features.Services.CodeGraph
{
    /// <summary>Un figlio di una cartella di una radice esterna, come lo mostra l'albero (R9).</summary>
    public sealed class CodeRootEntry
    {
        public string Name { get; init; }
        /// <summary>Il percorso logico <c>@alias/…</c>: l'albero lo usa per aprire il file e per chiedere i figli.</summary>
        public string LogicalPath { get; init; }
        public string AbsolutePath { get; init; }
        public bool IsFolder { get; init; }
        /// <summary>Un file si apre come sorgente colorato solo se è testo (deciso dal contenuto, come i file rivelati).</summary>
        public bool IsText { get; init; }
        /// <summary>Perché il file non si è potuto leggere per decidere se è testo (bloccato, permessi); null se letto.</summary>
        public string Unreadable { get; init; }
    }

    /// <summary>
    /// I figli diretti di una cartella di una radice esterna (sprint 2026-10-12-Radici-Di-Codice-Esterne, R9): l'albero
    /// la mostra in sola lettura, una cartella alla volta. Il percorso passa dal risolutore unico, quindi una radice non
    /// collegata o un <c>..</c> che esce sono rifiutati con il motivo. <c>.git</c> e i link simbolici non si mostrano,
    /// come nell'albero del progetto (un link può portare fuori dalla radice).
    /// </summary>
    public static class CodeRootBrowser
    {
        public static IReadOnlyList<CodeRootEntry> List(CodeRootResolver resolver, string logicalPath)
        {
            if (!CodeRootPaths.IsLogical(logicalPath))
                throw new ArgumentException($"'{logicalPath}' non è un percorso di una radice esterna (deve cominciare con «@alias»).");
            var (alias, rest) = CodeRootPaths.Parse(logicalPath);
            var folder = resolver.Resolve(logicalPath);
            if (!Directory.Exists(folder))
                throw new InvalidOperationException($"La cartella '{logicalPath}' non esiste più in {folder}.");

            var folders = new DirectoryInfo(folder).EnumerateDirectories()
                .Where(d => !IsLink(d) && !string.Equals(d.Name, ".git", StringComparison.OrdinalIgnoreCase))
                .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                .Select(d => new CodeRootEntry
                {
                    Name = d.Name,
                    LogicalPath = CodeRootPaths.Format(alias, Join(rest, d.Name)),
                    AbsolutePath = d.FullName,
                    IsFolder = true,
                });
            var files = new DirectoryInfo(folder).EnumerateFiles()
                .Where(f => !IsLink(f))
                .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .Select(f => File(alias, rest, f));
            return folders.Concat(files).ToList();
        }

        private static CodeRootEntry File(string alias, string rest, FileInfo f)
        {
            bool isText = false;
            string unreadable = null;
            try { isText = TextFileView.IsText(f.FullName); }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { unreadable = ex.Message; }
            return new CodeRootEntry
            {
                Name = f.Name,
                LogicalPath = CodeRootPaths.Format(alias, Join(rest, f.Name)),
                AbsolutePath = f.FullName,
                IsText = isText,
                Unreadable = unreadable,
            };
        }

        private static string Join(string rest, string name) => rest.Length == 0 ? name : rest + "/" + name;

        private static bool IsLink(FileSystemInfo info) => info.LinkTarget != null;
    }
}

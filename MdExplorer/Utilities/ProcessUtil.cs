using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Service.Utilities
{
    public class ProcessUtil
    {

        private readonly FileSystemWatcher _fileSystemWatcher;
        private Process _currentVisualStudio;
        private string _lastDocumentOpened;
        private string _editorPath;
        public Process CurrentVisualStudio { get { return _currentVisualStudio; } }

        public ProcessUtil(
            FileSystemWatcher fileSystemWatcher)
        {

            _fileSystemWatcher = fileSystemWatcher;
        }
        public void OpenFileWithVisualStudioCode(string path, string editorPath)
        {
            _editorPath = editorPath;
            _lastDocumentOpened = path;
            var currentPath = path.Replace(@"\\", @"\"); // pulitura da mettere a posto            
            var dosCommand = $@"""{editorPath}"" -a ""{_fileSystemWatcher.Path}"" """ + currentPath + "\"";
            if (_currentVisualStudio == null)
            {
                _currentVisualStudio = Process.Start(dosCommand);
            }
            else {
                Process.Start(dosCommand);
            }            
        }

        public void KillVisualStudioCode()
        {
            if (_currentVisualStudio != null)
            {
                _currentVisualStudio.Kill();
                _currentVisualStudio.Dispose();
                _currentVisualStudio = null;
            }            
        }


        public void ReopenVisualStudioCode(string newDocument)
        {
            if (!string.IsNullOrEmpty(newDocument))
            {
                OpenFileWithVisualStudioCode(newDocument, _editorPath);
            }
        }
    }
}

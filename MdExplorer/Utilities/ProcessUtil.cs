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
        public bool IKilled { get; set; }

        public ProcessUtil(
            FileSystemWatcher fileSystemWatcher)
        {

            _fileSystemWatcher = fileSystemWatcher;
        }
        public void OpenFileWithVisualStudioCode(string path, string editorPath)
        {
            _editorPath = editorPath;
            _lastDocumentOpened = path;
            
            // Clean up the path
            var currentPath = path.Replace(@"\\", System.IO.Path.DirectorySeparatorChar.ToString())
                                  .Replace(@"\", System.IO.Path.DirectorySeparatorChar.ToString());
            
            // Create ProcessStartInfo for cross-platform compatibility
            var startInfo = new ProcessStartInfo
            {
                FileName = editorPath,
                Arguments = $"-a \"{_fileSystemWatcher.Path}\" \"{currentPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };
            
            try
            {
                if (_currentVisualStudio == null || _currentVisualStudio.HasExited)
                {
                    _currentVisualStudio = Process.Start(startInfo);
                }
                else
                {
                    Process.Start(startInfo);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                Console.WriteLine($"Error opening file with VS Code: {ex.Message}");
                Console.WriteLine($"Editor path: {editorPath}");
                Console.WriteLine($"File path: {currentPath}");
            }
        }

        public void KillVisualStudioCode()
        {
            if (_currentVisualStudio != null && !_currentVisualStudio.HasExited)
            {
                _currentVisualStudio.Kill();
                IKilled = true;
                //_currentVisualStudio.Dispose();
                //_currentVisualStudio = null;
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

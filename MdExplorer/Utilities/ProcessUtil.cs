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
            OpenFileWithVisualStudioCode(path, editorPath, _fileSystemWatcher.Path);
        }

        public void OpenFileWithVisualStudioCode(string path, string editorPath, string projectPath)
        {
            _editorPath = editorPath;
            _lastDocumentOpened = path;

            // Clean up the path
            var currentPath = path.Replace(@"\\", System.IO.Path.DirectorySeparatorChar.ToString())
                                  .Replace(@"\", System.IO.Path.DirectorySeparatorChar.ToString());

            // Create ProcessStartInfo for cross-platform compatibility
            // --reuse-window: Reuse existing window instead of opening a new one
            // First parameter: Open project root folder as workspace
            // --goto <file:line>: Open file at specific line (line 1)
            var startInfo = new ProcessStartInfo
            {
                FileName = editorPath,
                Arguments = $"--reuse-window \"{projectPath}\" --goto \"{currentPath}:1\"",
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
                Console.WriteLine($"Project path: {projectPath}");
            }
        }

        public void OpenFileWithIntelliJ(string path, string intellijPath)
        {
            // Clean up the path
            var currentPath = path.Replace(@"\\", System.IO.Path.DirectorySeparatorChar.ToString())
                                  .Replace(@"\", System.IO.Path.DirectorySeparatorChar.ToString());

            // IntelliJ IDEA command line arguments:
            // --line <number> : go to the specified line
            // Opening only the file (not the project) to avoid multiple windows
            var startInfo = new ProcessStartInfo
            {
                FileName = intellijPath,
                Arguments = $"--line 1 \"{currentPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                // Log the error but don't crash the application
                Console.WriteLine($"Error opening file with IntelliJ IDEA: {ex.Message}");
                Console.WriteLine($"IntelliJ path: {intellijPath}");
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

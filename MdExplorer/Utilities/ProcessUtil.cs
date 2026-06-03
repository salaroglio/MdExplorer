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
        private Process _currentVisualStudio;
        private string _lastDocumentOpened;
        private string _editorPath;
        private string _lastProjectPath;
        public Process CurrentVisualStudio { get { return _currentVisualStudio; } }
        public bool IKilled { get; set; }

        public ProcessUtil()
        {
        }

        public void OpenFileWithVisualStudioCode(string path, string editorPath, string projectPath)
        {
            _editorPath = editorPath;
            _lastDocumentOpened = path;
            _lastProjectPath = projectPath;

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

        /// <summary>
        /// Opens an interactive Copilot CLI session in a visible terminal whose
        /// working directory is the project root. Unlike VS Code / IntelliJ (GUI
        /// processes spawned with <c>CreateNoWindow=true</c>), Copilot CLI is a
        /// terminal app, so we launch a PowerShell window (<c>UseShellExecute=true</c>)
        /// that sets its location to the project root and runs <c>copilot</c>.
        /// Resolution of the <c>copilot</c> shim is left to PowerShell, which honors
        /// PATHEXT (copilot.exe/.cmd/.ps1); the caller is expected to pre-check
        /// installation via <see cref="MdExplorer.Features.Services.AI.CopilotAcp.CopilotProcessLauncher.IsResolvable"/>.
        /// </summary>
        public void OpenFolderWithCopilotCli(string projectPath)
        {
            // Escape single quotes for PowerShell single-quoted string literals.
            var safePath = (projectPath ?? string.Empty).Replace("'", "''");

            // PowerShell on Windows is "powershell.exe"; on Linux/macOS it is "pwsh".
            var shell = OperatingSystem.IsWindows() ? "powershell.exe" : "pwsh";

            var startInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = $"-NoExit -Command \"Set-Location -LiteralPath '{safePath}'; copilot\"",
                WorkingDirectory = projectPath,
                UseShellExecute = true   // give the terminal its own visible window
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening Copilot CLI: {ex.Message}");
                Console.WriteLine($"Project path: {projectPath}");
                throw;
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
            if (!string.IsNullOrEmpty(newDocument) && !string.IsNullOrEmpty(_lastProjectPath))
            {
                OpenFileWithVisualStudioCode(newDocument, _editorPath, _lastProjectPath);
            }
        }
    }
}

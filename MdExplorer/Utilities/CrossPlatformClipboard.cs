using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.Utilities
{
    /// <summary>
    /// Cross-platform clipboard utility for handling images
    /// </summary>
    public static class CrossPlatformClipboard
    {
        /// <summary>
        /// Gets an image from the system clipboard
        /// </summary>
        public static async Task<ClipboardResult> GetImageAsync()
        {
            Console.WriteLine($"[CrossPlatformClipboard] GetImageAsync called - OS: {RuntimeInformation.OSDescription}");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine("[CrossPlatformClipboard] Detected Windows platform, calling GetImageWindows()");
                return await GetImageWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await GetImageLinux();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await GetImageMacOS();
            }
            else
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = "Unsupported operating system",
                    PlatformHint = "This feature is only supported on Windows, Linux, and macOS"
                };
            }
        }

        /// <summary>
        /// Windows: chiamate dirette agli appunti (<see cref="WindowsClipboard"/>), senza Windows Forms (07/11/2026).
        /// </summary>
        private static Task<ClipboardResult> GetImageWindows()
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            try
            {
                var png = WindowsClipboard.GetImagePng();
                if (png == null)
                    return Task.FromResult(new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = "No image found in clipboard",
                        PlatformHint = "Copy an image to the clipboard first"
                    });
                Console.WriteLine($"[CrossPlatformClipboard] Image from clipboard: {png.Length} bytes PNG");
                return Task.FromResult(new ClipboardResult { Success = true, ImageData = png });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error accessing clipboard: {ex.Message}",
                    PlatformHint = "Try copying the image again"
                });
            }
        }

        /// <summary>
        /// Linux implementation using xclip or wl-clipboard
        /// </summary>
        private static async Task<ClipboardResult> GetImageLinux()
        {
            // First, check which clipboard tool is available
            var xclipAvailable = await CheckCommandAvailable("xclip");
            var wlPasteAvailable = await CheckCommandAvailable("wl-paste");

            if (!xclipAvailable && !wlPasteAvailable)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = "No clipboard tool found",
                    PlatformHint = "Install xclip (X11) with 'sudo apt-get install xclip' or wl-clipboard (Wayland) with 'sudo apt-get install wl-clipboard'"
                };
            }

            // Try to get image from clipboard
            try
            {
                // First check what formats are available in clipboard
                var checkPsi = new ProcessStartInfo
                {
                    FileName = "xclip",
                    Arguments = "-selection clipboard -t TARGETS -o",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                string availableFormats = "";
                using (var checkProcess = Process.Start(checkPsi))
                {
                    availableFormats = await checkProcess.StandardOutput.ReadToEndAsync();
                    checkProcess.WaitForExit();
                }

                Console.WriteLine($"[CrossPlatformClipboard] Available clipboard formats: {availableFormats.Replace("\n", ", ")}");

                // Try different image formats
                string[] imageFormats = { "image/png", "image/jpeg", "image/jpg", "image/bmp", "image/tiff" };
                
                foreach (var format in imageFormats)
                {
                    if (availableFormats.Contains(format))
                    {
                        Console.WriteLine($"[CrossPlatformClipboard] Trying to read format: {format}");
                        
                        var psi = new ProcessStartInfo
                        {
                            FileName = "xclip",
                            Arguments = $"-selection clipboard -t {format} -o",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        };

                        using (var process = Process.Start(psi))
                        {
                            using (var outputStream = process.StandardOutput.BaseStream)
                            using (var ms = new MemoryStream())
                            {
                                await outputStream.CopyToAsync(ms);
                                process.WaitForExit();

                                if (process.ExitCode == 0 && ms.Length > 0)
                                {
                                    Console.WriteLine($"[CrossPlatformClipboard] Successfully read {ms.Length} bytes of {format} data");
                                    return new ClipboardResult
                                    {
                                        Success = true,
                                        ImageData = ms.ToArray()
                                    };
                                }
                                else
                                {
                                    var error = await process.StandardError.ReadToEndAsync();
                                    Console.WriteLine($"[CrossPlatformClipboard] Failed to read {format}: {error}");
                                }
                            }
                        }
                    }
                }

                // If no image format found, try the default approach
                Console.WriteLine("[CrossPlatformClipboard] No standard image format found, trying default PNG extraction");
                
                var defaultPsi = new ProcessStartInfo
                {
                    FileName = "xclip",
                    Arguments = $"-selection clipboard -t image/png -o",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(defaultPsi))
                {
                    using (var outputStream = process.StandardOutput.BaseStream)
                    using (var ms = new MemoryStream())
                    {
                        await outputStream.CopyToAsync(ms);
                        process.WaitForExit();

                        if (process.ExitCode == 0 && ms.Length > 0)
                        {
                            return new ClipboardResult
                            {
                                Success = true,
                                ImageData = ms.ToArray()
                            };
                        }
                        else
                        {
                            var error = await process.StandardError.ReadToEndAsync();
                            return new ClipboardResult
                            {
                                Success = false,
                                ErrorMessage = "No image in clipboard or wrong format",
                                PlatformHint = $"Available formats: {availableFormats.Replace("\n", ", ")}. Error: {error}"
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error accessing clipboard: {ex.Message}",
                    PlatformHint = "Check if the clipboard tool is properly installed"
                };
            }
        }

        /// <summary>
        /// macOS implementation using osascript or pngpaste
        /// </summary>
        private static async Task<ClipboardResult> GetImageMacOS()
        {
            // Check if pngpaste is available (preferred method)
            var pngpasteAvailable = await CheckCommandAvailable("pngpaste");

            try
            {
                string tempFile = Path.GetTempFileName() + ".png";
                ProcessStartInfo psi;
                bool success = false;

                if (pngpasteAvailable)
                {
                    // Use pngpaste if available (simpler and more reliable)
                    psi = new ProcessStartInfo
                    {
                        FileName = "pngpaste",
                        Arguments = tempFile,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(psi))
                    {
                        process.WaitForExit();
                        success = process.ExitCode == 0;
                    }
                }
                else
                {
                    // Fall back to osascript (built-in but more complex)
                    string script = $@"
                        try
                            set png_data to the clipboard as «class PNGf»
                            set the_file to open for access POSIX file ""{tempFile}"" with write permission
                            write png_data to the_file
                            close access the_file
                            return ""success""
                        on error
                            return ""no_image""
                        end try";

                    psi = new ProcessStartInfo
                    {
                        FileName = "osascript",
                        Arguments = $"-e '{script.Replace("\n", " ").Replace("'", "\\'")}'",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(psi))
                    {
                        var output = await process.StandardOutput.ReadToEndAsync();
                        process.WaitForExit();
                        success = output.Trim() == "success";
                    }
                }

                if (success && File.Exists(tempFile))
                {
                    var imageData = await File.ReadAllBytesAsync(tempFile);
                    File.Delete(tempFile); // Cleanup

                    if (imageData.Length > 0)
                    {
                        return new ClipboardResult
                        {
                            Success = true,
                            ImageData = imageData
                        };
                    }
                }

                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = "No image found in clipboard",
                    PlatformHint = pngpasteAvailable 
                        ? "Copy an image to the clipboard first" 
                        : "Copy an image to clipboard, or install pngpaste with 'brew install pngpaste' for better support"
                };
            }
            catch (Exception ex)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error accessing clipboard: {ex.Message}",
                    PlatformHint = "Try copying the image again"
                };
            }
        }

        /// <summary>
        /// Check if a command is available in the system
        /// </summary>
        private static async Task<bool> CheckCommandAvailable(string command)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "where" : "which",
                    Arguments = command,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode == 0;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Sets a PNG image into the system clipboard
        /// </summary>
        public static async Task<ClipboardResult> SetImageAsync(byte[] pngData)
        {
            if (pngData == null || pngData.Length == 0)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = "No image data provided"
                };
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await SetImageWindows(pngData);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await SetImageLinux(pngData);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await SetImageMacOS(pngData);
            }
            else
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = "Unsupported operating system"
                };
            }
        }

        private static Task<ClipboardResult> SetImageWindows(byte[] pngData)
        {
            if (!OperatingSystem.IsWindows()) throw new PlatformNotSupportedException();
            try
            {
                WindowsClipboard.SetImagePng(pngData);
                return Task.FromResult(new ClipboardResult { Success = true });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error setting clipboard: {ex.Message}"
                });
            }
        }

        private static async Task<ClipboardResult> SetImageLinux(byte[] pngData)
        {
            try
            {
                var xclipAvailable = await CheckCommandAvailable("xclip");
                if (!xclipAvailable)
                {
                    return new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = "xclip not found",
                        PlatformHint = "Install xclip with 'sudo apt-get install xclip'"
                    };
                }

                var psi = new ProcessStartInfo
                {
                    FileName = "xclip",
                    Arguments = "-selection clipboard -t image/png -i",
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    await process.StandardInput.BaseStream.WriteAsync(pngData, 0, pngData.Length);
                    process.StandardInput.Close();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                        return new ClipboardResult { Success = true };

                    var error = await process.StandardError.ReadToEndAsync();
                    return new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = $"xclip failed: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error setting clipboard: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Il testo negli appunti, o <c>null</c> se non c'è (07/11/2026: prima lo leggeva solo un ramo #if mai compilato, e
        /// «Clona progetto» non proponeva mai l'URL copiato). Su Linux e macOS con un timeout breve: xclip può restare appeso.
        /// </summary>
        public static async Task<string> GetTextAsync()
        {
            if (OperatingSystem.IsWindows())
                return WindowsClipboard.GetText();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (await CheckCommandAvailable("xclip")) return await ReadTextFrom("xclip", "-selection clipboard -o");
                if (await CheckCommandAvailable("wl-paste")) return await ReadTextFrom("wl-paste", "--no-newline");
                return null;
            }
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                return await ReadTextFrom("pbpaste", string.Empty);
            return null;
        }

        private static async Task<string> ReadTextFrom(string fileName, string arguments)
        {
            Process process = null;
            try
            {
                process = Process.Start(new ProcessStartInfo
                {
                    FileName = fileName, Arguments = arguments,
                    RedirectStandardOutput = true, RedirectStandardError = true,
                    UseShellExecute = false, CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                });
                if (process == null) return null;
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                var output = process.StandardOutput.ReadToEndAsync(cts.Token);
                await process.WaitForExitAsync(cts.Token);
                return process.ExitCode == 0 ? await output : null;
            }
            catch (OperationCanceledException)
            {
                // gli appunti non rispondono (xclip appeso): il processo si chiude, e per una proposta di URL vale «niente»
                try { process?.Kill(entireProcessTree: true); } catch { /* già uscito */ }
                return null;
            }
            finally
            {
                process?.Dispose();
            }
        }

        /// <summary>
        /// Puts plain text into the system clipboard.
        /// <para>
        /// Text always travels over the child process' stdin as raw UTF-8 bytes: passing it as a
        /// command-line argument would mangle newlines, quotes and non-ASCII characters, and a
        /// runnable script is full of all three.
        /// </para>
        /// </summary>
        public static async Task<ClipboardResult> SetTextAsync(string text)
        {
            text ??= string.Empty;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await SetTextWindows(text);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await SetTextLinux(text);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await PipeTextTo("pbcopy", string.Empty, text);
            }
            else
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = "Unsupported operating system"
                };
            }
        }

        private static async Task<ClipboardResult> SetTextWindows(string text)
        {
            // clip.exe would re-encode stdin with the console code page and destroy accented
            // characters, so the text goes through a UTF-8 temp file read back by PowerShell.
            // Set-Clipboard also needs no STA message pump, unlike Windows.Forms.Clipboard.
            var tempFile = Path.Combine(Path.GetTempPath(), $"mde-clip-{Guid.NewGuid():N}.txt");
            try
            {
                await File.WriteAllTextAsync(tempFile, text, new System.Text.UTF8Encoding(false));
                var escapedPath = tempFile.Replace("'", "''");
                var script = $"Set-Clipboard -Value ([IO.File]::ReadAllText('{escapedPath}', [Text.Encoding]::UTF8))";

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -NonInteractive -Command \"{script}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    var stderr = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                        return new ClipboardResult { Success = true };

                    return new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = $"PowerShell Set-Clipboard failed: {stderr}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error setting clipboard text: {ex.Message}"
                };
            }
            finally
            {
                try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { /* temp file, best effort */ }
            }
        }

        private static async Task<ClipboardResult> SetTextLinux(string text)
        {
            if (await CheckCommandAvailable("xclip"))
                return await PipeTextTo("xclip", "-selection clipboard -i", text);

            if (await CheckCommandAvailable("wl-copy"))
                return await PipeTextTo("wl-copy", string.Empty, text);

            return new ClipboardResult
            {
                Success = false,
                ErrorMessage = "No clipboard tool found",
                PlatformHint = "Install xclip (X11) with 'sudo apt-get install xclip' or wl-clipboard (Wayland) with 'sudo apt-get install wl-clipboard'"
            };
        }

        private static async Task<ClipboardResult> PipeTextTo(string fileName, string arguments, string text)
        {
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                    await process.StandardInput.BaseStream.WriteAsync(bytes, 0, bytes.Length);
                    await process.StandardInput.BaseStream.FlushAsync();
                    process.StandardInput.Close();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                        return new ClipboardResult { Success = true };

                    var error = await process.StandardError.ReadToEndAsync();
                    return new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = $"{fileName} failed: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error setting clipboard text: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Copies a file to the system clipboard as a file drop list,
        /// so it can be pasted into apps like Teams, Explorer, etc.
        /// </summary>
        public static async Task<ClipboardResult> SetFileDropListAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"File not found: {filePath}"
                };
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return await SetFileDropListWindows(filePath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return await SetFileDropListLinux(filePath);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return await SetFileDropListMacOS(filePath);
            }
            else
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = "Unsupported operating system"
                };
            }
        }

        private static async Task<ClipboardResult> SetFileDropListWindows(string filePath)
        {
            try
            {
                // Use PowerShell to set the clipboard as a file drop list.
                // Windows.Forms Clipboard requires a proper message pump on the STA thread,
                // which a background .NET service doesn't have — PowerShell handles this reliably.
                var escapedPath = filePath.Replace("'", "''");
                var script = $"Set-Clipboard -Path '{escapedPath}'";

                var psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -NonInteractive -Command \"{script}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    var stderr = await process.StandardError.ReadToEndAsync();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                        return new ClipboardResult { Success = true };

                    return new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = $"PowerShell clipboard failed: {stderr}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error setting clipboard: {ex.Message}"
                };
            }
        }


        private static async Task<ClipboardResult> SetFileDropListLinux(string filePath)
        {
            try
            {
                var xclipAvailable = await CheckCommandAvailable("xclip");
                if (!xclipAvailable)
                {
                    return new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = "xclip not found",
                        PlatformHint = "Install xclip with 'sudo apt-get install xclip'"
                    };
                }

                // xclip file drop list uses gnome-copied-files format
                var uri = "copy\n" + new Uri(filePath).AbsoluteUri;
                var uriBytes = System.Text.Encoding.UTF8.GetBytes(uri);

                var psi = new ProcessStartInfo
                {
                    FileName = "xclip",
                    Arguments = "-selection clipboard -t x-special/gnome-copied-files -i",
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    await process.StandardInput.BaseStream.WriteAsync(uriBytes, 0, uriBytes.Length);
                    process.StandardInput.Close();
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                        return new ClipboardResult { Success = true };

                    var error = await process.StandardError.ReadToEndAsync();
                    return new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = $"xclip failed: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error setting clipboard: {ex.Message}"
                };
            }
        }

        private static async Task<ClipboardResult> SetFileDropListMacOS(string filePath)
        {
            try
            {
                // Use osascript to set file reference in clipboard
                var escapedPath = filePath.Replace("\"", "\\\"");
                var script = $"set the clipboard to (POSIX file \"{escapedPath}\")";

                var psi = new ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = $"-e '{script}'",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();

                    if (process.ExitCode == 0)
                        return new ClipboardResult { Success = true };

                    var error = await process.StandardError.ReadToEndAsync();
                    return new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = $"osascript failed: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error setting clipboard: {ex.Message}"
                };
            }
        }

        private static async Task<ClipboardResult> SetImageMacOS(byte[] pngData)
        {
            try
            {
                var tempFile = Path.GetTempFileName() + ".png";
                await File.WriteAllBytesAsync(tempFile, pngData);

                var psi = new ProcessStartInfo
                {
                    FileName = "osascript",
                    Arguments = $"-e \"set the clipboard to (read (POSIX file \\\"{tempFile}\\\") as «class PNGf»)\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    File.Delete(tempFile);

                    if (process.ExitCode == 0)
                        return new ClipboardResult { Success = true };

                    var error = await process.StandardError.ReadToEndAsync();
                    return new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = $"osascript failed: {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                return new ClipboardResult
                {
                    Success = false,
                    ErrorMessage = $"Error setting clipboard: {ex.Message}"
                };
            }
        }
    }

    /// <summary>
    /// Result of clipboard operation
    /// </summary>
    public class ClipboardResult
    {
        public bool Success { get; set; }
        public byte[] ImageData { get; set; }
        public string ErrorMessage { get; set; }
        public string PlatformHint { get; set; }
    }
}
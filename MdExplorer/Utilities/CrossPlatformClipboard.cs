using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS_CLIPBOARD_SUPPORT
using System.Windows.Forms;
#endif

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
        /// Windows implementation using Windows.Forms
        /// </summary>
        private static Task<ClipboardResult> GetImageWindows()
        {
            Console.WriteLine("[CrossPlatformClipboard] GetImageWindows() started");
            var tcs = new TaskCompletionSource<ClipboardResult>();

#if WINDOWS_CLIPBOARD_SUPPORT
            Console.WriteLine("[CrossPlatformClipboard] WINDOWS_CLIPBOARD_SUPPORT is defined - creating STA thread for clipboard access");
            Thread thread = new Thread(() =>
            {
                try
                {
                    Console.WriteLine("[CrossPlatformClipboard] STA Thread started - checking if clipboard contains image");
                    if (Clipboard.ContainsImage())
                    {
                        Console.WriteLine("[CrossPlatformClipboard] Clipboard contains an image - attempting to retrieve it");
                        using (var image = Clipboard.GetImage())
                        {
                            if (image != null)
                            {
                                Console.WriteLine($"[CrossPlatformClipboard] Image retrieved - Size: {image.Width}x{image.Height}");
                                using (var ms = new MemoryStream())
                                {
                                    image.Save(ms, ImageFormat.Png);
                                    var imageBytes = ms.ToArray();
                                    Console.WriteLine($"[CrossPlatformClipboard] Image converted to PNG - Size: {imageBytes.Length} bytes");
                                    tcs.SetResult(new ClipboardResult
                                    {
                                        Success = true,
                                        ImageData = imageBytes
                                    });
                                }
                            }
                            else
                            {
                                tcs.SetResult(new ClipboardResult
                                {
                                    Success = false,
                                    ErrorMessage = "Could not retrieve image from clipboard",
                                    PlatformHint = "Make sure an image is copied to the clipboard"
                                });
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("[CrossPlatformClipboard] Clipboard does NOT contain an image");
                        // Check what's in the clipboard
                        try
                        {
                            if (Clipboard.ContainsText())
                            {
                                Console.WriteLine("[CrossPlatformClipboard] Clipboard contains TEXT instead");
                            }
                            if (Clipboard.ContainsFileDropList())
                            {
                                Console.WriteLine("[CrossPlatformClipboard] Clipboard contains FILE DROP LIST instead");
                            }
                        }
                        catch (Exception debugEx)
                        {
                            Console.WriteLine($"[CrossPlatformClipboard] Debug check failed: {debugEx.Message}");
                        }

                        tcs.SetResult(new ClipboardResult
                        {
                            Success = false,
                            ErrorMessage = "No image found in clipboard",
                            PlatformHint = "Copy an image to the clipboard first"
                        });
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetResult(new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = $"Error accessing clipboard: {ex.Message}",
                        PlatformHint = "Try copying the image again"
                    });
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            Console.WriteLine("[CrossPlatformClipboard] STA Thread completed");
#else
            Console.WriteLine("[CrossPlatformClipboard] WINDOWS_CLIPBOARD_SUPPORT is NOT defined - Windows clipboard support not available");
            tcs.SetResult(new ClipboardResult
            {
                Success = false,
                ErrorMessage = "Windows clipboard support not available",
                PlatformHint = "The application needs to be built with WINDOWS_CLIPBOARD_SUPPORT flag enabled"
            });
#endif

            return tcs.Task;
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
            var tcs = new TaskCompletionSource<ClipboardResult>();

#if WINDOWS_CLIPBOARD_SUPPORT
            Thread thread = new Thread(() =>
            {
                try
                {
                    using (var ms = new MemoryStream(pngData))
                    using (var image = Image.FromStream(ms))
                    {
                        var dataObj = new DataObject();
                        dataObj.SetImage(image);
                        Clipboard.SetDataObject(dataObj, true);
                        tcs.SetResult(new ClipboardResult { Success = true });
                    }
                }
                catch (Exception ex)
                {
                    tcs.SetResult(new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = $"Error setting clipboard: {ex.Message}"
                    });
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
#else
            tcs.SetResult(new ClipboardResult
            {
                Success = false,
                ErrorMessage = "Windows clipboard support not available"
            });
#endif

            return tcs.Task;
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

        // Keep original Windows Forms approach as fallback reference
        private static Task<ClipboardResult> SetFileDropListWindowsForms(string filePath)
        {
            var tcs = new TaskCompletionSource<ClipboardResult>();

#if WINDOWS_CLIPBOARD_SUPPORT
            Thread thread = new Thread(() =>
            {
                try
                {
                    var dataObj = new DataObject();

                    // Set file drop list (CF_HDROP)
                    var files = new System.Collections.Specialized.StringCollection();
                    files.Add(filePath);
                    dataObj.SetFileDropList(files);

                    // Set Preferred DropEffect = DROPEFFECT_COPY (1)
                    var dropEffect = new MemoryStream(new byte[] { 1, 0, 0, 0 });
                    dataObj.SetData("Preferred DropEffect", dropEffect);

                    Clipboard.SetDataObject(dataObj, true);
                    tcs.SetResult(new ClipboardResult { Success = true });
                }
                catch (Exception ex)
                {
                    tcs.SetResult(new ClipboardResult
                    {
                        Success = false,
                        ErrorMessage = $"Error setting clipboard: {ex.Message}"
                    });
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
#else
            tcs.SetResult(new ClipboardResult
            {
                Success = false,
                ErrorMessage = "Windows clipboard support not available"
            });
#endif

            return tcs.Task;
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
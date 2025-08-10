using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
#if WINDOWS_FORMS_AVAILABLE
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
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
            var tcs = new TaskCompletionSource<ClipboardResult>();

#if WINDOWS_FORMS_AVAILABLE
            Thread thread = new Thread(() =>
            {
                try
                {
                    if (Clipboard.ContainsImage())
                    {
                        using (var image = Clipboard.GetImage())
                        {
                            if (image != null)
                            {
                                using (var ms = new MemoryStream())
                                {
                                    image.Save(ms, ImageFormat.Png);
                                    tcs.SetResult(new ClipboardResult
                                    {
                                        Success = true,
                                        ImageData = ms.ToArray()
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
#else
            tcs.SetResult(new ClipboardResult
            {
                Success = false,
                ErrorMessage = "Windows Forms support not available",
                PlatformHint = "This build does not include Windows Forms support"
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
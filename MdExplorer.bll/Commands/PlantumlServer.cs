using Ad.Tools.Dal.Abstractions.Interfaces;
using Ad.Tools.Dal.Extensions;
using MdExplorer.Abstractions.DB;
using MdExplorer.Abstractions.Entities.UserDB;
using Microsoft.Extensions.Logging;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExplorer.Features.Commands
{
    public class PlantumlServer
    {
        private readonly IDALFactory<IUserSettingsDB> _dalFactory;
        private readonly ILogger<PlantumlServer> _logger;

        public PlantumlServer(IDALFactory<IUserSettingsDB> dalFactory, ILogger<PlantumlServer> logger)
        {
            _dalFactory = dalFactory;
            _logger = logger;
        }

        public async Task<byte[]> GetSvgFromJar(string plantumlcode)
        {
            using (var session = _dalFactory.OpenSession())
            {
                var paths = ResolvePlantumlPaths(session);
                return await RunPlantumlAsync(paths.JavaPath, paths.JarPath, paths.GraphvizDotPath, plantumlcode, "-tsvg");
            }
        }

        public async Task<byte[]> GetPngFromJar(string plantumlcode)
        {
            using (var session = _dalFactory.OpenSession())
            {
                var paths = ResolvePlantumlPaths(session);
                return await RunPlantumlAsync(paths.JavaPath, paths.JarPath, paths.GraphvizDotPath, plantumlcode, "-tpng");
            }
        }

        /// <summary>
        /// Runs plantuml.jar in <c>-checkonly</c> mode: validates the source WITHOUT rendering
        /// it, and surfaces what the renderers throw away — the exit code and stderr.
        /// <para>
        /// Contract measured on the bundled jar (1.2026.1): a valid diagram exits 0 with empty
        /// stderr; a broken one exits 200 and writes three lines to stderr — the marker
        /// <c>ERROR</c>, the line number, the message.
        /// </para>
        /// <para>
        /// An invalid diagram is NOT an exception here: it is the answer the caller asked for.
        /// A missing java or jar IS different, and comes back in
        /// <see cref="PlantumlCheckOutcome.ToolUnavailable"/> — telling a caller "your diagram is
        /// wrong" when the truth is "java is not installed" would send it correcting a source
        /// that has nothing wrong with it.
        /// </para>
        /// </summary>
        public async Task<PlantumlCheckOutcome> CheckAsync(string plantumlCode)
        {
            using (var session = _dalFactory.OpenSession())
            {
                var paths = ResolvePlantumlPaths(session);

                if (string.IsNullOrEmpty(paths.JarPath) || !File.Exists(paths.JarPath))
                {
                    return PlantumlCheckOutcome.Unavailable(
                        $"plantuml.jar non trovato. Cercato nell'impostazione 'PlantumlLocalPath' e in " +
                        $"'{Path.Combine(AppContext.BaseDirectory, "Binaries")}'.");
                }

                var arguments = new StringBuilder();
                arguments.Append("-Dfile.encoding=UTF-8 -jar \"").Append(paths.JarPath)
                    .Append("\" -pipe -charset UTF-8 -checkonly");

                try
                {
                    var run = await RunProcessAsync(paths.JavaPath, arguments.ToString(), plantumlCode)
                        .ConfigureAwait(false);
                    return PlantumlCheckOutcome.FromProcess(run.ExitCode, run.Stderr);
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    // Java non eseguibile: il messaggio deve dire COSA manca e DOVE si e' guardato,
                    // altrimenti chi chiama lo scambia per un diagramma sbagliato.
                    _logger.LogWarning(ex, "[PlantumlServer] java non eseguibile: {JavaPath}", paths.JavaPath);
                    return PlantumlCheckOutcome.Unavailable(
                        $"java non eseguibile ('{paths.JavaPath}'). Impostane il percorso in 'JavaPath' " +
                        "oppure installa un JRE e rendilo raggiungibile dal PATH.");
                }
            }
        }

        /// <summary>
        /// Invokes plantuml.jar via java in -pipe mode.
        /// </summary>
        /// <remarks>
        /// This deliberately does NOT use PlantUml.Net's local renderer: its
        /// ProcessHelper reads process output inside a fire-and-forget Task.Run
        /// with no exception handling and accesses Process.ExitCode without first
        /// calling WaitForExit — if that getter races the runtime's child-exit
        /// observation it throws, the TaskCompletionSource is never completed and
        /// the render hangs forever. Reproduced deterministically in the headless
        /// Linux container. Here we drain stdout/stderr concurrently and only read
        /// ExitCode after WaitForExitAsync, so it is always safe.
        /// </remarks>
        private async Task<byte[]> RunPlantumlAsync(string javaPath, string jarPath, string graphvizDotPath,
            string plantumlCode, string formatFlag)
        {
            var arguments = new StringBuilder();
            arguments.Append("-Dfile.encoding=UTF-8 -jar \"").Append(jarPath).Append("\" -pipe -charset UTF-8 ")
                .Append(formatFlag);
            if (!string.IsNullOrEmpty(graphvizDotPath))
            {
                arguments.Append(" -graphvizdot \"").Append(graphvizDotPath).Append('"');
            }

            var run = await RunProcessAsync(javaPath, arguments.ToString(), plantumlCode).ConfigureAwait(false);

            if (run.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"PlantUML rendering failed (java exit code {run.ExitCode}): {run.Stderr}");
            }

            return run.Stdout;
        }

        /// <summary>
        /// Runs java with the given arguments, feeding the diagram on stdin, and returns exit
        /// code, stdout and stderr. Shared by the renderers and by <see cref="CheckAsync"/>: the
        /// renderers turn a non-zero exit into an exception, the check reports it as its answer.
        /// </summary>
        private static async Task<(int ExitCode, byte[] Stdout, string Stderr)> RunProcessAsync(
            string javaPath, string arguments, string plantumlCode)
        {
            var startInfo = new ProcessStartInfo(javaPath)
            {
                Arguments = arguments,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardErrorEncoding = Encoding.UTF8,
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                process.Start();

                // Start draining both pipes before writing stdin: a diagram large
                // enough to make java emit output while still reading input would
                // otherwise fill a pipe buffer and deadlock.
                var stdoutTask = ReadAllBytesAsync(process.StandardOutput.BaseStream);
                var stderrTask = process.StandardError.ReadToEndAsync();

                var inputBytes = new UTF8Encoding(false).GetBytes(plantumlCode);
                await process.StandardInput.BaseStream.WriteAsync(inputBytes, 0, inputBytes.Length).ConfigureAwait(false);
                process.StandardInput.Close();

                await process.WaitForExitAsync().ConfigureAwait(false);
                var output = await stdoutTask.ConfigureAwait(false);
                var error = await stderrTask.ConfigureAwait(false);

                return (process.ExitCode, output, error);
            }
        }

        private static async Task<byte[]> ReadAllBytesAsync(Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                return memoryStream.ToArray();
            }
        }

        private (string JavaPath, string JarPath, string GraphvizDotPath) ResolvePlantumlPaths(ISessionDB session)
        {
            var settingDal = session.GetDal<Setting>();
            var currentApplicationPath = AppContext.BaseDirectory;

            // Get settings from database
            var plantumlPathSetting = settingDal.GetList().Where(_ => _.Name == "PlantumlLocalPath").FirstOrDefault()?.ValueString;
            var javaPath = settingDal.GetList().Where(_ => _.Name == "JavaPath").FirstOrDefault()?.ValueString;
            var graphvizPathSetting = settingDal.GetList().Where(_ => _.Name == "LocalGraphvizDotPath").FirstOrDefault()?.ValueString;

            // Build full paths - handle both relative and absolute paths
            string plantumlPath = null;
            if (!string.IsNullOrEmpty(plantumlPathSetting))
            {
                if (Path.IsPathRooted(plantumlPathSetting))
                {
                    plantumlPath = plantumlPathSetting;
                }
                else
                {
                    plantumlPath = Path.Combine(currentApplicationPath, plantumlPathSetting);
                }
            }

            // Handle Graphviz path based on OS
            string localGraphvizDotPath = null;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                // Windows: use the configured path or default to Graphviz\windows\dot.exe
                if (!string.IsNullOrEmpty(graphvizPathSetting))
                {
                    if (Path.IsPathRooted(graphvizPathSetting))
                    {
                        localGraphvizDotPath = graphvizPathSetting;
                    }
                    else
                    {
                        localGraphvizDotPath = Path.Combine(currentApplicationPath, graphvizPathSetting);
                    }
                }
                else
                {
                    localGraphvizDotPath = Path.Combine(currentApplicationPath, "Binaries", "Graphviz", "windows", "dot.exe");
                }
            }
            else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                // Linux: use the bundled Linux binaries
                localGraphvizDotPath = Path.Combine(currentApplicationPath, "Binaries", "Graphviz", "linux", "dot");
            }

            // Auto-discover Java if the configured path doesn't exist or is empty
            if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            {
                var discoveredJavaPath = Utilities.JavaDiscovery.DiscoverJavaPath();
                if (!string.IsNullOrEmpty(discoveredJavaPath))
                {
                    javaPath = discoveredJavaPath;

                    // Optionally update the database with the discovered path
                    var javaSetting = settingDal.GetList().Where(_ => _.Name == "JavaPath").FirstOrDefault();
                    if (javaSetting != null)
                    {
                        javaSetting.ValueString = discoveredJavaPath;
                        settingDal.Save(javaSetting);
                        session.Flush();
                    }
                }
            }

            // Last-resort fallback: rely on PATH resolution.
            if (string.IsNullOrEmpty(javaPath))
            {
                javaPath = "java";
            }

            // If PlantUML path is not set or doesn't exist, try to find it
            if (string.IsNullOrEmpty(plantumlPath) || !File.Exists(plantumlPath))
            {
                // Try common locations
                var possiblePaths = new[]
                {
                    Path.Combine(currentApplicationPath, "Binaries", "plantuml.jar"),
                    Path.Combine(currentApplicationPath, "Binaries", "plantuml-1.jar"),
                    Path.Combine(currentApplicationPath, "..", "..", "..", "..", "Binaries", "plantuml.jar")
                };

                foreach (var path in possiblePaths)
                {
                    if (File.Exists(path))
                    {
                        plantumlPath = path;
                        break;
                    }
                }
            }

            // Make the Linux dot executable if needed
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) &&
                !string.IsNullOrEmpty(localGraphvizDotPath) && File.Exists(localGraphvizDotPath))
            {
                try
                {
                    // Make sure the dot file is executable
                    var chmod = System.Diagnostics.Process.Start("chmod", $"+x {localGraphvizDotPath}");
                    chmod?.WaitForExit();
                }
                catch { }
            }

            return (javaPath, plantumlPath, localGraphvizDotPath);
        }
    }
}

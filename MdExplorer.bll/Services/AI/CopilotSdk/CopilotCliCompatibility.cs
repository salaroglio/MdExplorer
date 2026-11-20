using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MdExplorer.Features.Services.AI.CopilotAcp;

namespace MdExplorer.Features.Services.AI.CopilotSdk
{
    /// <summary>
    /// The Copilot CLI is installed by the user (npm) and updates itself; the SDK inside MdExplorer speaks one protocol
    /// version. When the CLI moves ahead (19/11/2026: CLI 1.0.114 reports protocol 4, SDK 1.0.11 and even 1.0.34 accept only
    /// 3) the SDK throws «SDK protocol version mismatch …» at the first question, with nothing the user can act on.
    /// This turns it into: which CLI is installed, which one MdExplorer is built with, and the command that fixes it.
    /// </summary>
    public static class CopilotCliCompatibility
    {
        /// <summary>The text the SDK throws; used only to RECOGNISE the case, never to guess anything from it.</summary>
        private const string MismatchMarker = "protocol version mismatch";

        private static readonly Regex VersionPattern = new Regex(@"\b(\d+)\.(\d+)\.(\d+)\b", RegexOptions.Compiled);

        /// <summary>
        /// The CLI the SDK package declares it is built with (<c>CopilotCliVersion</c> in its .props), written into this
        /// assembly at build time (MdExplorer.Features.csproj). The build fails if the package stops declaring it.
        /// </summary>
        public static string ExpectedCliVersion { get; } = typeof(CopilotCliCompatibility).Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => a.Key == "CopilotCliVersion")?.Value;

        /// <summary>True when <paramref name="ex"/> (or one of its inner exceptions) is the SDK's protocol check.</summary>
        public static bool IsProtocolMismatch(Exception ex)
        {
            for (var e = ex; e != null; e = e.InnerException)
            {
                if (e.Message?.IndexOf(MismatchMarker, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        /// <summary>The first <c>x.y.z</c> in the output of <c>copilot --version</c>, or null.</summary>
        public static string ParseVersion(string output)
        {
            var m = VersionPattern.Match(output ?? "");
            return m.Success ? m.Value : null;
        }

        /// <summary>Negative, zero or positive like <see cref="string.Compare(string, string)"/>; null when either is not x.y.z.</summary>
        public static int? CompareVersions(string a, string b)
        {
            var ma = VersionPattern.Match(a ?? "");
            var mb = VersionPattern.Match(b ?? "");
            if (!ma.Success || !mb.Success) return null;
            for (var i = 1; i <= 3; i++)
            {
                var c = int.Parse(ma.Groups[i].Value).CompareTo(int.Parse(mb.Groups[i].Value));
                if (c != 0) return c;
            }
            return 0;
        }

        /// <summary>
        /// The message for the user. <paramref name="installed"/> may be null (the version could not be read): the message
        /// says so instead of inventing one. <paramref name="sdkMessage"/> is kept at the end, for whoever reads the log.
        /// </summary>
        public static string DescribeMismatch(string installed, string expected, string sdkMessage)
        {
            var installedIt = installed != null ? $"la CLI di Copilot installata è la {installed}" : "la versione della CLI di Copilot installata non si legge";
            var installedEn = installed != null ? $"the installed Copilot CLI is {installed}" : "the version of the installed Copilot CLI cannot be read";
            var expectedIt = expected != null ? $"MdExplorer è fatto per la {expected}" : "MdExplorer non sa per quale versione è fatto";
            var expectedEn = expected != null ? $"MdExplorer is built for {expected}" : "MdExplorer does not know which version it is built for";
            var fix = expected != null ? $"npm install -g @github/copilot@{expected}" : "npm install -g @github/copilot";
            return UiText.Say(
                $"Copilot e MdExplorer non parlano la stessa versione del protocollo: {installedIt}, {expectedIt}. Esegui `{fix}` e riapri MdExplorer. (Copilot: {sdkMessage})",
                $"Copilot and MdExplorer do not speak the same protocol version: {installedEn}, {expectedEn}. Run `{fix}` and reopen MdExplorer. (Copilot: {sdkMessage})");
        }

        /// <summary>
        /// The installed CLI's version via <c>copilot --version</c> (the same executable the SDK starts), or null when it does
        /// not answer in time or answers something without a version.
        /// </summary>
        public static async Task<string> ReadInstalledVersionAsync(CancellationToken ct = default)
        {
            try
            {
                var (path, prefixArgs) = CopilotProcessLauncher.ResolveStdioTarget();
                var psi = new ProcessStartInfo(path)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };
                foreach (var a in prefixArgs ?? Enumerable.Empty<string>()) psi.ArgumentList.Add(a);
                psi.ArgumentList.Add("--version");

                using var process = Process.Start(psi);
                if (process == null) return null;
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeout.CancelAfter(TimeSpan.FromSeconds(15));
                var stdout = process.StandardOutput.ReadToEndAsync(timeout.Token);
                var stderr = process.StandardError.ReadToEndAsync(timeout.Token);
                try { await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false); }
                catch (OperationCanceledException) { try { process.Kill(entireProcessTree: true); } catch { } return null; }
                return ParseVersion(await stdout.ConfigureAwait(false)) ?? ParseVersion(await stderr.ConfigureAwait(false));
            }
            catch (Exception) when (!ct.IsCancellationRequested)
            {
                // Reading the version is only there to explain an error that is already being reported: if it fails, the
                // message says «cannot be read» (DescribeMismatch), it does not hide the original error.
                return null;
            }
        }

        /// <summary>The actionable exception for a protocol mismatch, with the original as inner exception.</summary>
        public static async Task<InvalidOperationException> ExplainAsync(Exception ex, CancellationToken ct = default)
        {
            var installed = await ReadInstalledVersionAsync(ct).ConfigureAwait(false);
            return new InvalidOperationException(DescribeMismatch(installed, ExpectedCliVersion, ex.Message), ex);
        }
    }
}

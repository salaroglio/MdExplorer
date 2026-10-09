namespace MdExplorer.Features.Commands
{
    /// <summary>
    /// Raw result of running plantuml.jar in <c>-checkonly</c> mode.
    /// <para>
    /// Three states, deliberately distinct: the diagram is valid, the diagram is invalid, or the
    /// tool could not run at all. Collapsing the third into the second is the mistake that sends
    /// a caller correcting a source that has nothing wrong with it.
    /// </para>
    /// </summary>
    public sealed class PlantumlCheckOutcome
    {
        private PlantumlCheckOutcome(bool ok, int exitCode, string stderr, string toolUnavailable)
        {
            Ok = ok;
            ExitCode = exitCode;
            Stderr = stderr;
            ToolUnavailable = toolUnavailable;
        }

        /// <summary>True only when the jar ran AND accepted the diagram.</summary>
        public bool Ok { get; }

        /// <summary>Exit code of java. 0 = valid, 200 = the jar rejected the diagram.</summary>
        public int ExitCode { get; }

        /// <summary>Raw stderr. For a rejected diagram: marker, line number, message.</summary>
        public string Stderr { get; }

        /// <summary>
        /// Non-null when the check could NOT be performed (java or the jar missing). Carries what
        /// is missing and where it was looked for, so the caller can act instead of guessing.
        /// </summary>
        public string ToolUnavailable { get; }

        public static PlantumlCheckOutcome FromProcess(int exitCode, string stderr)
            => new PlantumlCheckOutcome(exitCode == 0, exitCode, stderr, null);

        public static PlantumlCheckOutcome Unavailable(string reason)
            => new PlantumlCheckOutcome(false, -1, null, reason);
    }
}

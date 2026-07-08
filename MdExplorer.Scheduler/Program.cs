using MdExplorer.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

// ─────────────────────────────────────────────────────────────────────────────
// MdExplorer.Scheduler — lightweight satellite that fires the TEMPORAL (cron)
// triggers of *.agent.md schedules while the full MdExplorer Service may be off.
// Event triggers (commit / projectOpen) belong to the Service, never here —
// that split is what prevents double execution.
// ─────────────────────────────────────────────────────────────────────────────

// Singleton guard: Electron spawns this blindly in both full and agent mode; a second
// instance must exit quietly. An EXCLUSIVE lock file (FileShare.None) is used instead
// of a named Mutex: probed empirically on Linux (.NET 8) — a second process could
// acquire the "existing" named mutex, so it does not exclude cross-process there.
// The file lock is released by the OS when the process dies, crash included.
FileStream? singletonLock;
var lockPath = Path.Combine(MdExplorer.Scheduler.SchedulerDb.GetDbDirectory(), "scheduler.lock");
try
{
    Directory.CreateDirectory(Path.GetDirectoryName(lockPath)!);
    singletonLock = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
}
catch (IOException)
{
    Console.WriteLine("[Scheduler] Another instance is already running (lock file busy) — exiting.");
    return 0;
}

var builder = Host.CreateApplicationBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(o =>
{
    o.SingleLine = true;
    o.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
});
builder.Services.AddSingleton<SchedulerDb>();
builder.Services.AddSingleton<CopilotRunner>();
builder.Services.AddHostedService<SchedulerWorker>();

// Diagnostic pidfile next to the DB (the mutex is the real guard).
try
{
    var pidFile = Path.Combine(SchedulerDb.GetDbDirectory(), "scheduler.pid");
    File.WriteAllText(pidFile, Environment.ProcessId.ToString());
}
catch { /* purely diagnostic */ }

await builder.Build().RunAsync();
GC.KeepAlive(singletonLock); // hold the exclusive lock for the whole process lifetime
return 0;

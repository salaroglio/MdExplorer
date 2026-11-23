using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using MdExplorer.Services.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>Processore e memoria dei processi di MdExplorer, per «Invia diagnostica» (23/11/2026).</summary>
    [TestClass]
    public class ResourceMonitor_Should
    {
        private sealed class Lines : ILogger<ResourceMonitor>
        {
            public readonly System.Collections.Generic.List<string> Written = new();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => Written.Add(logLevel + " " + formatter(state, exception));
        }

        [TestMethod]
        public void Measure_this_process_while_it_keeps_a_core_busy()
        {
            var monitor = new ResourceMonitor(NullLogger<ResourceMonitor>.Instance);
            Assert.IsNull(monitor.Sample(), "il primo campione non ha un prima con cui fare la differenza");

            var until = Stopwatch.StartNew();
            while (until.ElapsedMilliseconds < 600) { }   // un core occupato per 0,6 secondi
            var sample = monitor.Sample();

            var me = sample.Processes.Single(p => p.Pid == Environment.ProcessId);
            StringAssert.EndsWith(me.Name, "(servizio)");
            Assert.IsTrue(me.CorePercent > 50, $"un core quasi tutto occupato, misurato {me.CorePercent}%");
            Assert.AreEqual(Math.Round(me.CorePercent / Environment.ProcessorCount, 1), me.CpuPercent, 0.11, "la percentuale della macchina è quella di un core divisa per i processori");
            Assert.IsTrue(me.MemoryMb > 0);
            Assert.IsTrue(sample.MachineMemoryLoadPercent is > 0 and <= 100);
        }

        [TestMethod]
        public void Follow_the_processes_of_mdexplorer_and_leave_the_others_alone()
        {
            Assert.IsTrue(ResourceMonitor.IsOurs("mdexplorer"));
            Assert.IsTrue(ResourceMonitor.IsOurs("MdExplorer.Service"));
            Assert.IsTrue(ResourceMonitor.IsOurs("MdExplorer.Mcp"));
            Assert.IsTrue(ResourceMonitor.IsOurs("java"));
            Assert.IsTrue(ResourceMonitor.IsOurs("llama-server"));
            Assert.IsFalse(ResourceMonitor.IsOurs("chrome"));
            Assert.IsFalse(ResourceMonitor.IsOurs("explorer"));
        }

        [TestMethod]
        public void Write_a_window_that_did_not_answer_in_the_log_and_in_the_package()
        {
            var log = new Lines();
            var monitor = new ResourceMonitor(log);
            monitor.Sample(); Thread.Sleep(50); monitor.Sample();

            monitor.ReportUiStall(new UiStallReport { Count = 4, MaxMs = 2300, TotalMs = 5100, SlowKeys = 7, MaxKeyMs = 900, WindowSeconds = 30, Route = "/main/navigation/document" });

            var line = log.Written.Single(l => l.Contains("non rispondeva"));
            StringAssert.StartsWith(line, "Warning");
            StringAssert.Contains(line, "4 blocchi");
            StringAssert.Contains(line, "2300 ms");
            StringAssert.Contains(line, "il peggiore 900 ms");
            StringAssert.Contains(line, "In quel momento: MdExplorer al");

            var package = JsonSerializer.Deserialize<JsonElement>(monitor.SnapshotJson());
            Assert.AreEqual(Environment.ProcessorCount, package.GetProperty("processoriLogici").GetInt32());
            Assert.AreEqual(2300, package.GetProperty("finestraBloccata")[0].GetProperty("MaxMs").GetInt32());
            Assert.AreEqual(1, package.GetProperty("campioni").GetArrayLength());
        }

        [TestMethod]
        public void Describe_who_is_working_in_one_line()
        {
            var sample = new ResourceSample();
            sample.Processes.Add(new ProcessSample { Pid = 10, Name = "mdexplorer", CpuPercent = 7.2, CorePercent = 115, MemoryMb = 402 });
            sample.Processes.Add(new ProcessSample { Pid = 11, Name = "mdexplorer", CpuPercent = 0, CorePercent = 0, MemoryMb = 8 });
            Assert.AreEqual("mdexplorer#10 7.2% (115% di un core) 402 MB", ResourceMonitor.Describe(sample).Replace(',', '.'));
        }
    }
}

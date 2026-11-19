using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Sprint .NET 10 (18/11/2026): gemello di MdExplorer.Features.Tests.Utilities.RuntimeVersion_Should.
    /// I test d'integrazione girano in un processo di test loro: senza questa prova non si sa se il roll-forward
    /// di F1 è arrivato anche qui. Da F3 è un Assert (maggiore >= 10).
    /// </summary>
    [TestClass]
    public class RuntimeVersion_Should
    {
        [TestMethod]
        public void Report()
        {
            Console.WriteLine($"Environment.Version = {Environment.Version}");
            Console.WriteLine($"FrameworkDescription = {RuntimeInformation.FrameworkDescription}");
            // F3: il framework vero è net10.0, senza roll-forward.
            Assert.IsTrue(Environment.Version.Major >= 10, $"i test girano su {RuntimeInformation.FrameworkDescription}, non su .NET 10");
        }
    }
}

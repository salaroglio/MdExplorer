using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Utilities
{
    /// <summary>
    /// Sprint .NET 10 (18/11/2026): dice su quale runtime gira DAVVERO il processo dei test.
    /// In F1 dimostra che il roll-forward ha portato i binari net8.0 sul runtime 10; senza questa prova
    /// i risultati del pacchetto di prova di F1 non valgono niente. Da F3 è un Assert (maggiore >= 10).
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

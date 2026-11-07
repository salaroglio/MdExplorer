using System.Collections.Generic;
using MdExplorer.Features.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.Features.Tests.Execution
{
    /// <summary>
    /// R1 della valutazione del 07/11: il valore digitato per un parametro di un blocco eseguibile deve arrivare nello script così com'è.
    /// Prima, nella riscrittura di <c>export NOME=</c> e dei <c>param()</c> di PowerShell, il valore finiva nel testo di sostituzione di
    /// <c>Regex.Replace</c>, che interpreta <c>$1</c>, <c>$_</c> (tutto lo script), <c>${x}</c>, <c>$$</c>, <c>$&amp;</c>.
    /// </summary>
    [TestClass]
    public class ParameterSubstitution_Should
    {
        /// <summary>Valori con le sequenze speciali del testo di sostituzione di .NET (e un backtick).</summary>
        private static readonly string[] Tricky = { "pa$1ss", "a$_b", "x${x}y", "costo $$5", "il $& qui", "pre$`post", "fine$'", "tick`tock", "uno$2" };

        private static string Apply(string code, string kind, string name, string value)
            => ParameterSubstitution.Apply(code,
                new List<ExecutionParameter> { new() { Name = name, Kind = kind } },
                new Dictionary<string, string> { [name] = value });

        [TestMethod]
        public void Put_the_typed_value_in_a_bash_export_as_it_is()
        {
            const string code = "#!/bin/bash\nexport PASSWORD=\"vecchia\"\necho \"$PASSWORD\"\n";
            foreach (var value in Tricky)
            {
                var result = Apply(code, "export", "PASSWORD", value);
                var expected = "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("$", "\\$").Replace("`", "\\`") + "\"";
                StringAssert.Contains(result, "export PASSWORD=" + expected + "\n", value);
                Assert.AreEqual(1, Count(result, "#!/bin/bash"), "lo script non si è incollato dentro sé stesso: " + value);
                StringAssert.EndsWith(result, "echo \"$PASSWORD\"\n", value);
            }
        }

        [TestMethod]
        public void Put_the_typed_value_in_a_powershell_param_as_it_is()
        {
            const string code = "param(\n  [string]$Password = 'vecchia',\n  [int]$Count = 1\n)\nWrite-Host $Password\n";
            foreach (var value in Tricky)
            {
                var result = Apply(code, "pwsh-param", "Password", value);
                StringAssert.Contains(result, "[string]$Password = '" + value.Replace("'", "''") + "',", value);
                Assert.AreEqual(1, Count(result, "param("), "lo script non si è incollato dentro sé stesso: " + value);
                StringAssert.Contains(result, "[int]$Count = 1", value);
            }
        }

        [TestMethod]
        public void Put_the_typed_value_in_a_placeholder_as_it_is()
        {
            const string code = "Get-ChildItem | % { $_ } > \"<OUT>\"\n";
            foreach (var value in Tricky)
                Assert.AreEqual("Get-ChildItem | % { $_ } > \"" + value + "\"\n", Apply(code, "placeholder", "OUT", value), value);
        }

        private static int Count(string text, string what)
        {
            var n = 0;
            for (var i = text.IndexOf(what, System.StringComparison.Ordinal); i >= 0; i = text.IndexOf(what, i + what.Length, System.StringComparison.Ordinal)) n++;
            return n;
        }
    }
}

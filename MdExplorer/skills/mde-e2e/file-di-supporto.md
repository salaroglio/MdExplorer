# mde-e2e — I file di supporto degli script

<!-- Lo legge MdExplorer, non l'agente: i blocchi `esempio=` sono il testo dei file che MdExplorer scrive accanto ai test (E2eSupportFiles). Non si installa nei progetti. -->

`E2eTests.csproj`:

```xml esempio=csproj
<Project Sdk="Microsoft.NET.Sdk">
  <!-- MdExplorer, skill mde-e2e: progetto che compila e rigioca gli script dei test *.e2e.md di questa cartella. -->
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>disable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="NUnit" Version="4.2.2" />
    <PackageReference Include="NUnit3TestAdapter" Version="4.6.0" />
    <PackageReference Include="Microsoft.Playwright.NUnit" Version="1.63.0" />
  </ItemGroup>
</Project>
```

`e2e.runsettings` (usa il Chrome installato; per Edge scrivi `msedge`):

```xml esempio=runsettings
<RunSettings>
  <!-- MdExplorer, skill mde-e2e: browser usato per rigiocare gli script. Lo scrive MdExplorer: non modificare. -->
  <Playwright>
    <BrowserName>chromium</BrowserName>
    <LaunchOptions>
      <Channel>chrome</Channel>
      <Headless>true</Headless>
    </LaunchOptions>
  </Playwright>
</RunSettings>
```

`E2eSupport.cs`:

```csharp esempio=supporto
// MdExplorer, skill mde-e2e: supporto comune agli script dei test *.e2e.md. Lo scrive MdExplorer: non modificare.
// versione-supporto: 4
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace MdeE2e
{
    public static class E2e
    {
        private static readonly string RunStamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");

        /// <summary>Cartella dei test (quella dei file .e2e.md): E2E_ROOT, oppure due livelli sopra lo script.</summary>
        public static string Root([CallerFilePath] string caller = "") =>
            Environment.GetEnvironmentVariable("E2E_ROOT")
            ?? Path.GetFullPath(Path.Combine(Path.GetDirectoryName(caller)!, "..", ".."));

        /// <summary>Valore di una chiave del file delle credenziali (righe chiave=valore, # per i commenti).</summary>
        public static string Credential(string file, string key, [CallerFilePath] string caller = "")
        {
            var path = Path.Combine(Root(caller), file);
            if (!File.Exists(path))
                throw new FileNotFoundException(
                    $"File delle credenziali non trovato: {path}. Crealo con righe chiave=valore, oppure imposta E2E_ROOT sulla cartella dei test.", path);
            foreach (var raw in File.ReadAllLines(path))
            {
                var line = raw.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                var eq = line.IndexOf('=');
                if (eq > 0 && line.Substring(0, eq).Trim() == key) return line.Substring(eq + 1).Trim();
            }
            throw new KeyNotFoundException($"Chiave '{key}' assente nel file delle credenziali {path}.");
        }

        /// <summary>Cartella di questa esecuzione dello script: E2E_RUN_DIR, oppure &lt;artefatti&gt;/esecuzioni/&lt;data&gt;_script.</summary>
        public static string RunDir(string artifacts, [CallerFilePath] string caller = "")
        {
            var dir = Environment.GetEnvironmentVariable("E2E_RUN_DIR")
                ?? Path.Combine(Root(caller), artifacts, "esecuzioni", RunStamp + "_script");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public static Task Screenshot(IPage page, string artifacts, string name, [CallerFilePath] string caller = "") =>
            page.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(RunDir(artifacts, caller), name + ".png") });

        /// <summary>Accende i segnali del sito (chiave mde-e2e in localStorage): prima di aprire il sito.</summary>
        public static Task EnableSignals(IBrowserContext context) =>
            context.AddInitScriptAsync("try { localStorage.setItem('mde-e2e', '1'); } catch (e) { }");

        /// <summary>
        /// Aspetta il segnale <paramref name="name"/> con chiave <paramref name="key"/>: «ready» → prosegue;
        /// «error» → il test fallisce; nessuno dei due entro <paramref name="max"/> → il test fallisce col motivo.
        /// </summary>
        public static async Task Signal(IPage page, string name, string key, TimeSpan max)
        {
            var attribute = "data-mde-" + name;
            var done = page.Locator($"[{attribute}=\"ready:{key}\"], [{attribute}=\"error:{key}\"]").First;
            try
            {
                await done.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = (float)max.TotalMilliseconds });
            }
            catch (TimeoutException)
            {
                Assert.Fail($"segnale {name} {key} non arrivato entro {max.TotalSeconds} s");
            }
            if ((await done.GetAttributeAsync(attribute))?.StartsWith("error:") == true)
                Assert.Fail($"il sito ha segnalato un errore su {name} {key}");
        }
    }

    /// <summary>
    /// Base degli script. Quando MdExplorer rigioca (E2E_RUN_DIR impostata) registra le chiamate di rete (HAR) e
    /// la console del test nella cartella dell'esecuzione: MdExplorer le riduce a un registro e cancella il resto.
    /// </summary>
    public abstract class E2eTest : PageTest
    {
        private readonly object _lock = new();
        private StreamWriter? _console;

        private static string? Dir => Environment.GetEnvironmentVariable("E2E_RUN_DIR");

        public override BrowserNewContextOptions ContextOptions()
        {
            var options = base.ContextOptions() ?? new BrowserNewContextOptions();
            if (Dir != null)
            {
                Directory.CreateDirectory(Dir);
                options.RecordHarPath = Path.Combine(Dir, TestContext.CurrentContext.Test.ClassName + ".har");
                options.RecordHarContent = HarContentPolicy.Omit;
            }
            return options;
        }

        [SetUp]
        public void RecordConsole()
        {
            if (Dir == null) return;
            _console = new StreamWriter(Path.Combine(Dir, TestContext.CurrentContext.Test.ClassName + ".console.tsv"));
            Page.Console += (_, message) =>
            {
                lock (_lock)
                    _console?.WriteLine(DateTime.UtcNow.ToString("O") + "\t" + message.Type + "\t" + message.Text.Replace('\t', ' ').Replace('\n', ' '));
            };
        }

        [TearDown]
        public async Task CloseRecording()
        {
            // Playwright chiude il contesto, e quindi salva l'HAR, solo quando il test passa: qui sempre, perché
            // il registro serve soprattutto quando il test fallisce.
            if (Dir != null) await Context.CloseAsync();
            lock (_lock)
            {
                _console?.Dispose();
                _console = null;
            }
        }
    }
}
```

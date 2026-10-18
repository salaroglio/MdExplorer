using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MdExplorer.IntegrationTests
{
    /// <summary>
    /// Impostazioni valide per <b>tutti</b> gli host costruiti dalla suite.
    /// <para>
    /// Il problema che risolvono: ogni test che ospita il Service in-process fa un
    /// <c>HostBuilder.Build()</c>, e <c>Host.CreateDefaultBuilder</c> registra
    /// <c>appsettings.json</c> con <c>reloadOnChange: true</c>. Su Linux ogni watcher costa
    /// un'<b>istanza inotify</b>, e il tetto per utente è 128: eseguendo la suite intera gli host
    /// si susseguono, le istanze si accumulano e da un certo punto in poi ogni
    /// <c>Build()</c> muore con
    /// «The configured user limit (128) on the number of inotify instances has been reached».
    /// I test falliscono a grappolo, in ordine variabile, per una ragione che non ha niente a che
    /// vedere con quello che stanno verificando — e passano tutti se eseguiti da soli.
    /// </para>
    /// <para>
    /// La ricarica a caldo della configurazione a un test non serve: il file non cambia mentre
    /// gira. Spegnerla toglie il watcher alla radice, invece di alzare un limite di sistema che
    /// vale solo sulla macchina di chi l'ha alzato.
    /// </para>
    /// </summary>
    [TestClass]
    public static class TestHostEnvironment
    {
        [AssemblyInitialize]
        public static void Initialize(TestContext _)
        {
            // Letta da Host.CreateDefaultBuilder come `hostBuilder:reloadConfigOnChange`
            // (il doppio underscore è la forma portabile del separatore di sezione).
            Environment.SetEnvironmentVariable("DOTNET_hostBuilder__reloadConfigOnChange", "false");
        }
    }
}

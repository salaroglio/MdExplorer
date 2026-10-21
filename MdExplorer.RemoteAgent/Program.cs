using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using MdExplorer.Remote.Protocol;
using MdExplorer.RemoteAgent;

// mde-remote --version            → stampa la versione ed esce
// mde-remote --root <cartella>    → saluta e serve le richieste su stdin/stdout finché stdin non si chiude

var version = typeof(AgentLoop).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unknown";
if (args.Contains("--version"))
{
    Console.WriteLine(version);
    return 0;
}
var at = Array.IndexOf(args, "--root");
if (at < 0 || at + 1 >= args.Length)
{
    Console.Error.WriteLine("usage: mde-remote --root <folder> | --version");
    return RemoteProtocol.ExitBadArguments;
}
RemoteWorkspace workspace;
try
{
    workspace = new RemoteWorkspace(args[at + 1]);
}
catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
{
    Console.Error.WriteLine(ex.Message);
    return RemoteProtocol.ExitRootNotFound;
}
var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
using var input = new StreamReader(Console.OpenStandardInput(), utf8);
using var output = new StreamWriter(Console.OpenStandardOutput(), utf8) { NewLine = "\n" };
AgentLoop.Run(workspace, version, input, output);
return 0;

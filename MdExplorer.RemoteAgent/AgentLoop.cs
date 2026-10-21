using System.IO;
using System.Text.Json;
using MdExplorer.RemoteProtocol;

namespace MdExplorer.RemoteAgent
{
    /// <summary>
    /// Il ciclo del serverino: saluta (D6), poi una richiesta per riga e una risposta per riga, nell'ordine, finché
    /// stdin non si chiude. L'esecuzione sta in <see cref="WorkspaceDispatcher"/>, condivisa con il backend.
    /// </summary>
    public static class AgentLoop
    {
        public static void Run(RemoteWorkspace workspace, string version, TextReader input, TextWriter output)
        {
            Send(output, new RemoteResponse
            {
                Id = 0,
                Ok = true,
                Hello = new RemoteHello { Protocol = AgentProtocol.Version, Version = version, Root = workspace.Root },
            });
            string? line;
            while ((line = input.ReadLine()) != null)
            {
                if (line.Length == 0) continue;
                Send(output, WorkspaceDispatcher.Handle(workspace, line));
            }
        }

        private static void Send(TextWriter output, RemoteResponse response)
        {
            output.WriteLine(JsonSerializer.Serialize(response, RemoteJson.Default.RemoteResponse));
            output.Flush();
        }
    }
}

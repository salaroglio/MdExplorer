using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using MdExplorer.RemoteProtocol;

namespace MdExplorer.RemoteAgent
{
    /// <summary>
    /// Il ciclo del serverino: saluta (D6), poi una richiesta per riga e una risposta per riga, nell'ordine, finché
    /// stdin non si chiude. Un errore in una richiesta diventa una risposta con il suo codice, non ferma il ciclo.
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
                Send(output, Handle(workspace, line));
            }
        }

        public static RemoteResponse Handle(RemoteWorkspace workspace, string line)
        {
            RemoteRequest? request = null;
            try
            {
                request = JsonSerializer.Deserialize(line, RemoteJson.Default.RemoteRequest)
                          ?? throw new RemoteFailure(RemoteErrorCodes.BadRequest, "empty request");
                var response = Execute(workspace, request);
                response.Id = request.Id;
                response.Ok = true;
                return response;
            }
            catch (Exception ex)
            {
                var (code, message) = Classify(ex);
                return new RemoteResponse { Id = request?.Id ?? -1, Ok = false, Error = new RemoteError { Code = code, Message = message } };
            }
        }

        private static RemoteResponse Execute(RemoteWorkspace ws, RemoteRequest r)
        {
            switch (r.Op)
            {
                case RemoteOps.List:
                    return new RemoteResponse { Entries = ws.List(r.Path) };
                case RemoteOps.Stat:
                    return new RemoteResponse { Entry = ws.Stat(r.Path) };
                case RemoteOps.Read:
                    var (text, stamp, bom) = ws.Read(r.Path);
                    return new RemoteResponse { Text = text, Stamp = stamp, Bom = bom };
                case RemoteOps.Write:
                    return new RemoteResponse { Stamp = ws.Write(r.Path, r.Text, r.Stamp) };
                case RemoteOps.Create:
                    return new RemoteResponse { Stamp = ws.Create(r.Path, r.Text) };
                case RemoteOps.Mkdir:
                    ws.Mkdir(r.Path);
                    return new RemoteResponse();
                case RemoteOps.Delete:
                    ws.Delete(r.Path, r.Recursive);
                    return new RemoteResponse();
                case RemoteOps.Move:
                    ws.Move(r.Path, r.To);
                    return new RemoteResponse();
                case RemoteOps.Grep:
                    var (hits, truncated) = ws.Grep(r.Path, r.Pattern, r.IgnoreCase, r.MaxHits, r.Exclude);
                    return new RemoteResponse { Hits = hits, Truncated = truncated };
                case RemoteOps.GitInfo:
                    return new RemoteResponse { Git = ws.GitInfo() };
                default:
                    throw new RemoteFailure(RemoteErrorCodes.Unsupported, $"unsupported op: {r.Op}");
            }
        }

        private static (string Code, string Message) Classify(Exception ex) => ex switch
        {
            RemoteFailure f => (f.Code, f.Message),
            JsonException => (RemoteErrorCodes.BadRequest, "invalid JSON: " + ex.Message),
            EncoderFallbackException => (RemoteErrorCodes.BadRequest, "text is not valid Unicode: " + ex.Message),
            UnauthorizedAccessException => (RemoteErrorCodes.Denied, ex.Message),
            FileNotFoundException or DirectoryNotFoundException => (RemoteErrorCodes.NotFound, ex.Message),
            RegexMatchTimeoutException => (RemoteErrorCodes.Timeout, ex.Message),
            IOException => (RemoteErrorCodes.Io, ex.Message),
            _ => (RemoteErrorCodes.Internal, ex.GetType().Name + ": " + ex.Message),
        };

        private static void Send(TextWriter output, RemoteResponse response)
        {
            output.WriteLine(JsonSerializer.Serialize(response, RemoteJson.Default.RemoteResponse));
            output.Flush();
        }
    }
}

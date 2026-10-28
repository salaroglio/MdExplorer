using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MdExplorer.RemoteProtocol
{
    /// <summary>
    /// Una richiesta del protocollo eseguita su una radice, con la risposta (un rifiuto diventa una risposta con il suo
    /// codice, non un'eccezione). La usano MDE Remote sul server e il backend per le radici LOCALI (sprint
    /// 2026-10-21-Cartella-Remota-Via-SSH, FL): le stesse regole — dentro la radice, niente link, impronta per scrivere —
    /// ovunque stia la cartella.
    /// </summary>
    public static class WorkspaceDispatcher
    {
        /// <summary>Una riga JSON → la risposta (anche per un JSON illeggibile, con id -1).</summary>
        public static RemoteResponse Handle(RemoteWorkspace workspace, string line)
        {
            RemoteRequest? request;
            try
            {
                request = JsonSerializer.Deserialize(line, RemoteJson.Default.RemoteRequest)
                          ?? throw new RemoteFailure(RemoteErrorCodes.BadRequest, "empty request");
            }
            catch (Exception ex)
            {
                var (code, message) = Classify(ex);
                return new RemoteResponse { Id = -1, Ok = false, Error = new RemoteError { Code = code, Message = message } };
            }
            return Dispatch(workspace, request);
        }

        public static RemoteResponse Dispatch(RemoteWorkspace workspace, RemoteRequest request)
        {
            try
            {
                workspace.ExtensionEncodings = request.ExtensionEncodings;
                var response = Execute(workspace, request);
                response.Id = request.Id;
                response.Ok = true;
                return response;
            }
            catch (Exception ex)
            {
                var (code, message) = Classify(ex);
                return new RemoteResponse { Id = request.Id, Ok = false, Error = new RemoteError { Code = code, Message = message } };
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
                    var (text, stamp, bom, encoding) = ws.Read(r.Path, r.Encoding);
                    return new RemoteResponse { Text = text, Stamp = stamp, Bom = bom, Encoding = encoding };
                case RemoteOps.Write:
                    return new RemoteResponse { Stamp = ws.Write(r.Path, r.Text, r.Stamp, r.Encoding) };
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
    }
}

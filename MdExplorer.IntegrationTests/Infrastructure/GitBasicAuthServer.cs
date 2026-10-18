using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MdExplorer.IntegrationTests.Infrastructure
{
    /// <summary>
    /// Un server git HTTP <b>vero</b>, in-process, con autenticazione Basic: fa da ponte CGI
    /// verso <c>git http-backend</c>, cioè lo stesso programma che serve git su Apache o nginx.
    /// Serve i bare repository sotto <see cref="Root"/> e accetta una sola coppia utente/password.
    /// <para>
    /// Serve a provare ciò che GitHub o un server aziendale fanno davvero con MdExplorer —
    /// 401, credenziali nel credential helper, push autenticato — senza toccare la rete.
    /// Ogni richiesta senza credenziali valide riceve <c>401</c> con <c>WWW-Authenticate: Basic</c>,
    /// che è esattamente ciò che fa scattare il credential helper nel client git.
    /// </para>
    /// </summary>
    public sealed class GitBasicAuthServer : IDisposable
    {
        public string Root { get; }
        public string User { get; }
        public string Password { get; }
        public int Port { get; }
        public string BaseUrl => $"http://127.0.0.1:{Port}";
        /// <summary>Quante richieste sono arrivate senza credenziali valide (utile per asserire che il client ci ha provato).</summary>
        public int Unauthorized => _unauthorized;

        private readonly HttpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly Task _loop;
        private int _unauthorized;

        public GitBasicAuthServer(string root, string user, string password)
        {
            Root = root;
            User = user;
            Password = password;
            Directory.CreateDirectory(root);
            Port = FreePort();
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://127.0.0.1:{Port}/");
            _listener.Start();
            _loop = Task.Run(Loop);
        }

        /// <summary>Crea un bare repository servito a <c>{BaseUrl}/{name}.git</c> e ne ritorna l'URL.</summary>
        public string CreateBareRepository(string name, string defaultBranch = "main")
        {
            var dir = Path.Combine(Root, name + ".git");
            Directory.CreateDirectory(dir);
            Run(dir, "init", "--bare");
            Run(dir, "symbolic-ref", "HEAD", "refs/heads/" + defaultBranch);
            // Con REMOTE_USER impostato http-backend accetta il push da solo; questo lo rende
            // esplicito e non dipende dalla versione di git.
            Run(dir, "config", "http.receivepack", "true");
            return $"{BaseUrl}/{name}.git";
        }

        /// <summary>L'URL con l'utente dentro, come lo scrive MdExplorer quando conosce l'account.</summary>
        public string UrlWithUser(string repoUrl) => repoUrl.Replace("http://", $"http://{User}@");

        /// <summary>L'URL con utente E password dentro: per il setup del test, che non deve dipendere dal credential helper.</summary>
        public string UrlWithCredentials(string repoUrl) => repoUrl.Replace("http://", $"http://{User}:{Password}@");

        private async Task Loop()
        {
            while (!_cts.IsCancellationRequested)
            {
                HttpListenerContext ctx;
                try { ctx = await _listener.GetContextAsync(); }
                catch (Exception) when (_cts.IsCancellationRequested) { return; }
                catch (HttpListenerException) { return; }
                _ = Task.Run(() => Handle(ctx));
            }
        }

        private void Handle(HttpListenerContext ctx)
        {
            try
            {
                if (!IsAuthorized(ctx.Request))
                {
                    Interlocked.Increment(ref _unauthorized);
                    ctx.Response.StatusCode = 401;
                    ctx.Response.AddHeader("WWW-Authenticate", "Basic realm=\"mde-test\"");
                    ctx.Response.Close();
                    return;
                }
                Cgi(ctx);
            }
            catch (Exception ex)
            {
                try
                {
                    ctx.Response.StatusCode = 500;
                    var bytes = Encoding.UTF8.GetBytes(ex.ToString());
                    ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                    ctx.Response.Close();
                }
                catch { }
            }
        }

        private bool IsAuthorized(HttpListenerRequest req)
        {
            var header = req.Headers["Authorization"];
            if (string.IsNullOrEmpty(header) || !header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
                return false;
            string decoded;
            try { decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header.Substring(6).Trim())); }
            catch { return false; }
            return decoded == $"{User}:{Password}";
        }

        private void Cgi(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var body = new MemoryStream();
            req.InputStream.CopyTo(body);

            var psi = new ProcessStartInfo
            {
                FileName = "git",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("http-backend");
            var env = psi.Environment;
            env["GIT_PROJECT_ROOT"] = Root;
            env["GIT_HTTP_EXPORT_ALL"] = "1";
            env["REQUEST_METHOD"] = req.HttpMethod;
            env["PATH_INFO"] = req.Url!.AbsolutePath;
            env["QUERY_STRING"] = req.Url.Query.TrimStart('?');
            env["REMOTE_USER"] = User;
            env["REMOTE_ADDR"] = "127.0.0.1";
            env["SERVER_PROTOCOL"] = "HTTP/1.1";
            env["GATEWAY_INTERFACE"] = "CGI/1.1";
            env["CONTENT_LENGTH"] = body.Length.ToString();
            if (!string.IsNullOrEmpty(req.ContentType)) env["CONTENT_TYPE"] = req.ContentType;
            var enc = req.Headers["Content-Encoding"];
            if (!string.IsNullOrEmpty(enc)) env["HTTP_CONTENT_ENCODING"] = enc;

            using var p = Process.Start(psi)!;
            body.Position = 0;
            body.CopyTo(p.StandardInput.BaseStream);
            p.StandardInput.Close();

            var output = new MemoryStream();
            p.StandardOutput.BaseStream.CopyTo(output);
            var stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();

            var bytes = output.ToArray();
            var split = IndexOfBlankLine(bytes);
            var headerText = Encoding.ASCII.GetString(bytes, 0, split.headerEnd);
            var status = 200;
            foreach (var line in headerText.Split('\n'))
            {
                var l = line.TrimEnd('\r');
                var colon = l.IndexOf(':');
                if (colon <= 0) continue;
                var name = l.Substring(0, colon).Trim();
                var value = l.Substring(colon + 1).Trim();
                if (name.Equals("Status", StringComparison.OrdinalIgnoreCase))
                    int.TryParse(value.Split(' ')[0], out status);
                else if (name.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                    ctx.Response.ContentType = value;
                else if (!name.Equals("Content-Length", StringComparison.OrdinalIgnoreCase))
                    ctx.Response.AddHeader(name, value);
            }
            ctx.Response.StatusCode = status;
            ctx.Response.SendChunked = false;
            var bodyLen = bytes.Length - split.bodyStart;
            ctx.Response.ContentLength64 = bodyLen;
            ctx.Response.OutputStream.Write(bytes, split.bodyStart, bodyLen);
            ctx.Response.Close();
        }

        private static (int headerEnd, int bodyStart) IndexOfBlankLine(byte[] b)
        {
            for (var i = 0; i + 1 < b.Length; i++)
            {
                if (b[i] == '\r' && i + 3 < b.Length && b[i + 1] == '\n' && b[i + 2] == '\r' && b[i + 3] == '\n')
                    return (i, i + 4);
                if (b[i] == '\n' && b[i + 1] == '\n')
                    return (i, i + 2);
            }
            return (b.Length, b.Length);
        }

        private static void Run(string cwd, params string[] args)
        {
            var psi = new ProcessStartInfo("git") { WorkingDirectory = cwd, UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
            foreach (var a in args) psi.ArgumentList.Add(a);
            using var p = Process.Start(psi)!;
            var err = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0) throw new InvalidOperationException($"git {string.Join(' ', args)} in {cwd}: {err}");
        }

        private static int FreePort()
        {
            var l = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            l.Start();
            var port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public void Dispose()
        {
            _cts.Cancel();
            try { _listener.Stop(); _listener.Close(); } catch { }
            try { _loop.Wait(2000); } catch { }
        }
    }
}

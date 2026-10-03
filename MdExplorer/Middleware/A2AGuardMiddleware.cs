using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MdExplorer.Middleware
{
    /// <summary>
    /// Guardia degli endpoint A2A (R12, §10): il browser dell'utente sta <i>dentro</i>
    /// il loopback, quindi una pagina web ostile può tentare CSRF / DNS-rebinding verso
    /// <c>127.0.0.1</c>. Questo middleware protegge le route A2A (<c>/a2a/*</c> e
    /// <c>/api/A2A/*</c>):
    /// <list type="bullet">
    /// <item><b>Host</b> deve essere un host di loopback — un Host rilegato a un dominio
    /// dell'attaccante (DNS rebinding) viene rifiutato;</item>
    /// <item>se presente, <b>Origin</b> deve essere loopback — una richiesta cross-site
    /// dal browser (CSRF) viene rifiutata. Le richieste same-origin del client MDE
    /// (servito dal Service stesso) e i client non-browser (nessun Origin) passano.</item>
    /// </list>
    /// L'autenticazione forte del mittente (RunToken, R2) arriva in Fase 3; qui il
    /// perimetro è la rete (loopback-only) più questa guardia contro il ponte-browser.
    /// </summary>
    public class A2AGuardMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<A2AGuardMiddleware> _logger;

        private static readonly HashSet<string> LoopbackHosts = new(StringComparer.OrdinalIgnoreCase)
        {
            "127.0.0.1", "localhost", "::1", "[::1]",
        };

        public A2AGuardMiddleware(RequestDelegate next, ILogger<A2AGuardMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (IsA2aPath(context.Request.Path))
            {
                if (!IsLoopbackHost(context.Request.Host.Host))
                {
                    _logger.LogWarning("A2A guard: Host non-loopback '{Host}' rifiutato su {Path}",
                        context.Request.Host.Value, context.Request.Path);
                    await Reject(context, "Host non-loopback rifiutato (anti DNS-rebinding).");
                    return;
                }

                var origin = context.Request.Headers["Origin"].ToString();
                if (!string.IsNullOrEmpty(origin) && !IsLoopbackOrigin(origin))
                {
                    _logger.LogWarning("A2A guard: Origin cross-site '{Origin}' rifiutato su {Path}",
                        origin, context.Request.Path);
                    await Reject(context, "Origin cross-site rifiutato (anti CSRF).");
                    return;
                }
            }

            await _next(context);
        }

        internal static bool IsA2aPath(PathString path)
            => path.StartsWithSegments("/a2a", StringComparison.OrdinalIgnoreCase)
            || path.StartsWithSegments("/api/A2A", StringComparison.OrdinalIgnoreCase);

        internal static bool IsLoopbackHost(string host)
            => !string.IsNullOrEmpty(host) && LoopbackHosts.Contains(host);

        internal static bool IsLoopbackOrigin(string origin)
            => Uri.TryCreate(origin, UriKind.Absolute, out var uri)
               && (uri.IsLoopback || LoopbackHosts.Contains(uri.Host));

        private static async Task Reject(HttpContext context, string reason)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = reason });
        }
    }
}

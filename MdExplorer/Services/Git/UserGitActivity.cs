using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MdExplorer.Services.Git
{
    /// <summary>
    /// Le operazioni git dell'utente in corso (N6 della valutazione del 07/11, l'altro verso): il commit automatico di MDE aspetta che
    /// finiscano prima di partire, così le due sequenze non si accavallano sul blocco di git. Il verso opposto (MDE già partito) lo
    /// copre il client, che disattiva i pulsanti con l'evento <c>mdeGitBusy</c>. Un contatore solo per tutto il servizio: le operazioni
    /// dell'utente durano poco, e aspettare anche quelle di un altro repository non fa danni.
    /// </summary>
    public static class UserGitActivity
    {
        private static int _running;

        public static bool Busy => Volatile.Read(ref _running) > 0;

        public static IDisposable Begin()
        {
            Interlocked.Increment(ref _running);
            return new End();
        }

        private sealed class End : IDisposable
        {
            private int _done;
            public void Dispose() { if (Interlocked.Exchange(ref _done, 1) == 0) Interlocked.Decrement(ref _running); }
        }
    }

    /// <summary>
    /// Sui controller dei pulsanti git: ogni richiesta che cambia qualcosa (non le GET) conta come operazione git dell'utente finché
    /// non ha risposto.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class UserGitOperationAttribute : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (HttpMethods.IsGet(context.HttpContext.Request.Method) || HttpMethods.IsHead(context.HttpContext.Request.Method))
            {
                await next();
                return;
            }
            using (UserGitActivity.Begin()) await next();
        }
    }
}

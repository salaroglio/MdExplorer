using System.Threading.Tasks;
using System.Threading;
using System;

namespace MdExplorer.Service.Utilities
{
    public static class ExtensionTask
    {
        public static Task<T> CreateSTATask<T>( Func<T> func)
        {
            var tcs = new TaskCompletionSource<T>();
            var thread = new Thread(() =>
            {
                try
                {
                    var result = func();
                    tcs.SetResult(result);
                }
                catch (Exception e)
                {
                    tcs.SetException(e);
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return tcs.Task;
        }
    }
}

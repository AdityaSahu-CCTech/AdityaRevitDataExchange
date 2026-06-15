using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Autodesk.DataExchange.UI.Core.Interfaces;

namespace AdityaRevitDataExchange.DXSDK
{
    public class RevitInteropInvoker : IMainThreadInvoker
    {
        private readonly Dispatcher dispatcher;

        public RevitInteropInvoker(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
        }

        public async Task InvokeAsync(Action action)
        {
            await dispatcher.InvokeAsync(action);
        }

        public async Task<T> InvokeAsync<T>(Func<T> func)
        {
            return await dispatcher.InvokeAsync(func);
        }

        public async Task<T> InvokeAsync<T>(Func<Task<T>> func)
        {
            return await dispatcher.InvokeAsync(func)
                                   .Task
                                   .Unwrap();
        }
    }
}
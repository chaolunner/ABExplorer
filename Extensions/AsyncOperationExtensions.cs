using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace ABExplorer.Extensions
{
    public static class AsyncOperationExtensions
    {
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += op => { tcs.SetResult(null); };
            return ((Task) tcs.Task).GetAwaiter();
        }
    }
}
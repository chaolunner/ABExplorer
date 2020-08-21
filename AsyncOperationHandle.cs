using UnityEngine;
using System.Threading.Tasks;

namespace ABExplorer
{
    public struct AsyncOperationHandle
    {
        public Task Task => _task;

        private readonly Task _task;
        private System.Action<AsyncOperationHandle> _completed;

        public event System.Action<AsyncOperationHandle> Completed
        {
            add
            {
                AsyncOperationHandle handle = this;
                handle._completed = value;
                RunTask(handle);
                this = handle;
            }
            remove
            {
                AsyncOperationHandle handle = this;
                handle._completed = null;
                this = handle;
            }
        }

        public AsyncOperationHandle(Task task)
        {
            _task = task;
            _completed = null;
        }

        private void RunTask(AsyncOperationHandle handle)
        {
            var task = new Task(async () =>
            {
                await handle._task;
                handle._completed?.Invoke(handle);
            });
            task.RunSynchronously();
        }
    }

    public struct AsyncOperationHandle<T> where T : Object
    {
        public Task<T> Task => _task;

        public T Result => _task.Result;

        private Task<T> _task;
        private System.Action<AsyncOperationHandle<T>> _completed;

        public event System.Action<AsyncOperationHandle<T>> Completed
        {
            add
            {
                AsyncOperationHandle<T> handle = this;
                handle._completed = value;
                var task = new Task(async () =>
                {
                    await handle._task;
                    handle._completed?.Invoke(handle);
                });
                task.RunSynchronously();
                this = handle;
            }
            remove
            {
                AsyncOperationHandle<T> handle = this;
                handle._completed = null;
                this = handle;
            }
        }

        public AsyncOperationHandle(Task<T> task)
        {
            _task = task;
            _completed = null;
        }
    }
}
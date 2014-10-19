using System.Threading.Tasks;

namespace ServiceStack.Text
{
    public static class TaskResult
    {
        public static Task<int> Zero;
        public static readonly Task<bool> True;
        public static readonly Task<bool> False;
        public static readonly Task Finished;
        public static readonly Task Canceled;

        static TaskResult()
        {
            Finished = ((object)null).AsTaskResult();
            True = true.AsTaskResult();
            False = false.AsTaskResult();
            Zero = 0.AsTaskResult();

            var tcs = new TaskCompletionSource<object>();
            tcs.SetCanceled();
            Canceled = tcs.Task;
        }         
    }

    internal class TaskResult<T>
    {
        public static readonly Task<T> Canceled;
        public static readonly Task<T> Default;

        static TaskResult()
        {
            Default = ((T)typeof(T).GetDefaultValue()).AsTaskResult();

            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            Canceled = tcs.Task;
        }
    }
}
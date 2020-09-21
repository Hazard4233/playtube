namespace PlayTube
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;

    /// <summary>
    /// Class which contains extension methods for objects of type <see cref="Task"/>.
    /// </summary>
    public static class TaskExtensions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="FireAndForgetSafeAsync(Task, bool)"/> method
        /// should execute asyncronous tasks in a synchronous manner.
        /// </summary>
        /// <remarks>
        /// WARNING: This should only be enabled in unit tests, never in production.
        /// </remarks>
        public static bool FireAndForgetSynchronous { get; set; } = false;

        /// <summary>
        /// Wraps "async void" methods in an exception handler backed by a logger.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="continueOnCapturedContext">if set to <c>true</c> the task will continue on its captured context, if any.</param>
        /// <remarks>
        /// This is to prevent async/void exceptions being "eaten" invisibly at runtime.
        /// </remarks>
        public static async void FireAndForgetSafeAsync(this Task task, bool continueOnCapturedContext = true)
        {
            try
            {
                if (FireAndForgetSynchronous)
                {
#if !DEBUG
                    throw new NotSupportedException("Cannot run in synchronous mode in a release build.");
#else
                    task.Wait();
#endif
                }
                else
                {
                    if (continueOnCapturedContext)
                    {
                        await task;
                    }
                    else
                    {
                        await task.ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Write(e.ToString());
#if DEBUG
                Debug.Fail("Error in async/void function.");
                Debugger.Break();
#endif
            }
        }
    }
}

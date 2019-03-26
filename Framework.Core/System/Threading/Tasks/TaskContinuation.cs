﻿#if LESSTHAN_NET40 && !PROFILE328

using System.Diagnostics.Contracts;

namespace System.Threading.Tasks
{
    internal abstract class TaskContinuation
    {
        internal abstract void Run(Task completedTask, bool canInlineContinuationTask);

        protected static void InlineIfPossibleOrElseQueue(Task task)
        {
            Contract.Requires(task != null);
            var scheduler = task.ExecutingTaskScheduler;
            if (scheduler == null)
            {
                Contract.Assert(false);
                throw new InvalidOperationException();
            }

            task.Start(task.ExecutingTaskScheduler, true, false);
        }
    }
}

#endif
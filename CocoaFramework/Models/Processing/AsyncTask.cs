// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Maila.Cocoa.Framework.Models.Processing
{
    public class AsyncTask
    {
        internal Task RealTask { get; }

        private AsyncTask(Task realTask)
        {
            RealTask = realTask;
        }

        private static readonly AsyncTask completed = new(Task.CompletedTask);

        public static AsyncTask Wait(int milliseconds)
        {
            if (milliseconds <= 0)
            {
                return completed;
            }
            return new(Task.Delay(milliseconds));
        }
        public static AsyncTask Wait(int milliseconds, CancellationToken cancellationToken)
        {
            if (milliseconds <= 0)
            {
                return completed;
            }
            return new(Task.Delay(milliseconds, cancellationToken));
        }
        public static AsyncTask Wait(TimeSpan delay)
        {
            if (delay <= TimeSpan.Zero)
            {
                return completed;
            }
            return new(Task.Delay(delay));
        }
        public static AsyncTask Wait(TimeSpan delay, CancellationToken cancellationToken)
        {
            if (delay <= TimeSpan.Zero)
            {
                return completed;
            }
            return new(Task.Delay(delay, cancellationToken));
        }
        public static AsyncTask WaitUntil(DateTime time)
        {
            if (time <= DateTime.Now)
            {
                return completed;
            }
            return new(Task.Delay(time - DateTime.Now));
        }
        public static AsyncTask WaitUntil(DateTime time, CancellationToken cancellationToken)
        {
            if (time <= DateTime.Now)
            {
                return completed;
            }
            return new(Task.Delay(time - DateTime.Now, cancellationToken));
        }

        public static AsyncTask FromTask(Task task)
        {
            return new(task);
        }
        public static AsyncTask FromTask<T>(Task<T> task, out GetValue<T> result)
        {
            GetValue<T> getValue = new();
            result = getValue;
            return new(Task.Run(async () => getValue.Value = await task));
        }

        public static AsyncTask Run(Action action)
        {
            return new(Task.Run(action));
        }
        public static AsyncTask Run(Action action, CancellationToken cancellationToken)
        {
            return new(Task.Run(action, cancellationToken));
        }
        public static AsyncTask Run(Func<Task?> function)
        {
            return new(Task.Run(function));
        }
        public static AsyncTask Run(Func<Task?> function, CancellationToken cancellationToken)
        {
            return new(Task.Run(function, cancellationToken));
        }
        public static AsyncTask Run<T>(Func<T> function, out GetValue<T> result)
        {
            GetValue<T> getValue = new();
            result = getValue;
            return new(Task.Run(() => getValue.Value = function()));
        }
        public static AsyncTask Run<T>(Func<Task<T>> function, out GetValue<T> result)
        {
            GetValue<T> getValue = new();
            result = getValue;
            return new(Task.Run(async () => getValue.Value = await function()));
        }
        public static AsyncTask Run<T>(Func<Task<T>> function, out GetValue<T> result, CancellationToken cancellationToken)
        {
            GetValue<T> getValue = new();
            result = getValue;
            return new(Task.Run(async () => getValue.Value = await function(), cancellationToken));
        }
    }
}

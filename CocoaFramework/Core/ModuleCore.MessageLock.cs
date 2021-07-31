// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maila.Cocoa.Framework.Models.Processing;

namespace Maila.Cocoa.Framework.Core
{
    public static partial class ModuleCore
    {
        private static readonly List<Func<MessageSource, QMessage, LockState>> messageLocks = new();

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun)
            => messageLocks.Add(lockRun);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, Predicate<MessageSource> predicate)
            => messageLocks.Add(new MessageLock(lockRun, predicate, TimeSpan.Zero, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, ListeningTarget target)
            => messageLocks.Add(new MessageLock(lockRun, target.Pred, TimeSpan.Zero, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, MessageSource src)
            => messageLocks.Add(new MessageLock(lockRun, s => s.Equals(src), TimeSpan.Zero, null).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, Predicate<MessageSource> predicate, TimeSpan timeout, Action? onTimeout = null)
            => messageLocks.Add(new MessageLock(lockRun, predicate, timeout, onTimeout).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, ListeningTarget target, TimeSpan timeout, Action? onTimeout = null)
            => messageLocks.Add(new MessageLock(lockRun, target.Pred, timeout, onTimeout).Run);

        public static void AddLock(Func<MessageSource, QMessage, LockState> lockRun, MessageSource src, TimeSpan timeout, Action? onTimeout = null)
            => messageLocks.Add(new MessageLock(lockRun, s => s.Equals(src), timeout, onTimeout).Run);

        private class MessageLock
        {
            public readonly Predicate<MessageSource> predicate;
            public readonly Func<MessageSource, QMessage, LockState> run;
            private readonly TimeSpan timeout;
            private readonly Action? onTimeout;
            private int counter;
            private DateTime lastRun;

            private CancellationTokenSource lastToken = new();
            private readonly SemaphoreSlim runningLock = new(1);

            private readonly Action<int, CancellationToken> timeoutAction;

            public MessageLock(Func<MessageSource, QMessage, LockState> run, Predicate<MessageSource> predicate, TimeSpan timeout, Action? onTimeout)
            {
                this.predicate = predicate;
                this.run = run;
                this.timeout = timeout;
                this.onTimeout = onTimeout;
                if (timeout <= TimeSpan.Zero)
                {
                    timeoutAction = (_, _) => { };
                    return;
                }
                lastRun = DateTime.Now;
                timeoutAction = async (count, token) =>
                {
                    try
                    {
                        await Task.Delay(this.timeout, token);
                        // Make time gap with timeout judgment to avoid boundary problems
                        await Task.Delay(10, token);
                    }
                    catch (TaskCanceledException) { return; }

                    if (counter != count || runningLock.CurrentCount < 1)
                    {
                        return;
                    }
                    messageLocks.Remove(Run);
                    this.onTimeout?.Invoke();
                };
                int count = counter;
                Task.Run(() => timeoutAction(count, lastToken.Token));
            }

            public LockState Run(MessageSource src, QMessage msg)
            {
                if (timeout > TimeSpan.Zero && DateTime.Now - lastRun > timeout)
                {
                    return LockState.Continue;
                }
                if (!predicate(src))
                {
                    return LockState.Continue;
                }

                runningLock.Wait();

                var state = run(src, msg);
                if (timeout > TimeSpan.Zero)
                {
                    if ((state & LockState.ContinueAndRemove) != 0) // Whether remove
                    {
                        counter++;
                        lastToken.Cancel();
                    }
                    else if (state == LockState.NotFinished)
                    {
                        lastRun = DateTime.Now;
                        counter++;
                        lastToken.Cancel();
                        lastToken = new();
                        int count = counter;
                        new Task(() => timeoutAction(count, lastToken.Token)).Start();
                    }
                }

                runningLock.Release();
                return state;
            }
        }
    }

    [Flags]
    public enum LockState
    {
        Finished = 0b11,
        NotFinished = 0b01,
        Continue = 0b00,
        ContinueAndRemove = 0b10
    }
}

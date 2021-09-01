// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Maila.Cocoa.Framework.Core;
using Maila.Cocoa.Framework.Models.Processing;

namespace Maila.Cocoa.Framework.Models
{
    public class Meeting
    {
        private readonly IEnumerator proc;
        private ListeningTarget target;
        private MessageReceiver? receiver;
        private Meeting? child;
        private readonly Meeting root;

        private TimeSpan timeout = TimeSpan.Zero;
        private int counter;
        private bool running;
        private bool skip;
        private bool finished;

        private readonly object _lock = new();

        private Meeting(ListeningTarget target, IEnumerator proc)
        {
            this.target = target;
            this.proc = proc;
            root = this;
        }

        private Meeting(ListeningTarget target, IEnumerator proc, Meeting root)
        {
            this.target = target;
            this.proc = proc;
            this.root = root;
        }

        private LockState Run(MessageSource? src, QMessage? msg)
        {
            if (finished)
            {
                return LockState.ContinueAndRemove;
            }

            if (skip)
            {
                return LockState.Continue;
            }

            if (src is not null && !target.Pred(src))
            {
                return LockState.Continue;
            }

            lock (_lock)
            {
                return InternalRun(src, msg);
            }
        }

        private LockState InternalRun(MessageSource? src, QMessage? msg)
        {
            running = true;
            if (child is not null)
            {
                var state = child.InternalRun(src, msg);
                if (state == LockState.Finished)
                {
                    child = null;
                }
                else
                {
                    if (state == LockState.ContinueAndRemove)
                    {
                        finished = true;
                    }
                    running = false;
                    return state;
                }
            }

            if (receiver is not null)
            {
                receiver.Source = src;
                receiver.Message = msg;
                receiver.IsTimeout = src is null && msg is null;
            }

            if (proc.MoveNext())
            {
                switch (proc.Current)
                {
                    case MessageReceiver rec:
                        {
                            receiver = rec;
                            var state = InternalRun(src, msg);
                            return state;
                        }
                    case ListeningTarget tgt:
                        {
                            target = tgt;
                            break;
                        }
                    case MeetingTimeout tout:
                        {
                            timeout = tout.Duration;
                            var state = InternalRun(src, msg);
                            return state;
                        }
                    case AsyncTask task:
                        {
                            skip = true;
                            Task.Run(async () =>
                            {
                                await task.RealTask;
                                InternalRun(src, msg);
                                skip = false;
                            });
                            return LockState.NotFinished;
                        }
                    case NotFit nf:
                        {
                            running = false;
                            if (!nf.Remove)
                            {
                                return LockState.Continue;
                            }
                            counter++;
                            finished = true;
                            return LockState.ContinueAndRemove;
                        }
                    case string or StringBuilder:
                        {
                            var retMsg = proc.Current as string ?? (proc.Current as StringBuilder)!.ToString();
                            src?.Send(retMsg);
                            break;
                        }
                    case IEnumerator or IEnumerable:
                        {
                            var subm = proc.Current as IEnumerator ?? (proc.Current as IEnumerable)!.GetEnumerator();
                            Meeting m = new(target, subm, root);
                            var state = m.InternalRun(src, msg);
                            if (state != LockState.Finished)
                            {
                                counter++;
                                child = m;
                                running = false;
                                return state;
                            }

                            state = InternalRun(src, msg);
                            return state;
                        }
                }

                counter++;
                if (timeout > TimeSpan.Zero)
                {
                    int count = counter;
                    Task.Run(async () =>
                    {
                        await Task.Delay(timeout);
                        if (counter == count && !running)
                        {
                            root.Run(null, null);
                        }
                    });
                }

                running = false;
                return LockState.NotFinished;
            }

            counter++;
            running = false;
            finished = true;
            return LockState.Finished;
        }

        public static void Start(MessageSource src, IEnumerable proc)
        {
            Start(src, proc.GetEnumerator());
        }

        public static void Start(MessageSource src, IEnumerator proc)
        {
            Meeting m = new(ListeningTarget.FromTarget(src), proc);
            if (m.InternalRun(src, null) != LockState.Finished)
            {
                ModuleCore.AddLock(m.Run);
            }
        }

        public static void Start(ListeningTarget target, IEnumerable proc)
        {
            Start(target, proc.GetEnumerator());
        }

        public static void Start(ListeningTarget target, IEnumerator proc)
        {
            Meeting m = new(target, proc);
            if (m.InternalRun(null, null) != LockState.Finished)
            {
                ModuleCore.AddLock(m.Run);
            }
        }
    }
}

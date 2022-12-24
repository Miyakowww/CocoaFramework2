// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Maila.Cocoa.Framework.Core;
using Maila.Cocoa.Framework.Models.Processing;

namespace Maila.Cocoa.Framework
{
    public class AsyncMeeting
    {
        public ListeningTarget Target { get; set; }
        public TimeSpan Timeout { get; set; } = TimeSpan.Zero;

        private readonly MessageSource src;

        internal AsyncMeeting(MessageSource source)
        {
            src = source;
            Target = ListeningTarget.FromTarget(source);
        }

        public int Send(string message)
        {
            return src.Send(message);
        }

        public int Send(MessageBuilder message)
        {
            return src.Send(message);
        }

        public Task<int> SendAsync(string message)
        {
            return src.SendAsync(message);
        }

        public Task<int> SendAsync(MessageBuilder message)
        {
            return src.SendAsync(message);
        }


        public Task<MessageInfo?> Wait()
        {
            return Wrapper(new(Target, Timeout));
        }

        public Task<MessageInfo?> WaitFor(string pattern)
        {
            return Wrapper(new(Target, Timeout, msg => msg.PlainText == pattern));
        }

        public Task<MessageInfo?> WaitFor(Regex regex)
        {
            return Wrapper(new(Target, Timeout, msg => regex.IsMatch(msg.PlainText)));
        }

        public Task<MessageInfo?> WaitFor(Predicate<QMessage> predicator)
        {
            return Wrapper(new(Target, Timeout, predicator));
        }


        public async Task<MessageInfo?> SendAndWait(string message)
        {
            await src.SendAsync(message);
            return await new WaitForMessage(Target, Timeout);
        }

        public async Task<MessageInfo?> SendAndWait(MessageBuilder message)
        {
            await src.SendAsync(message);
            return await new WaitForMessage(Target, Timeout);
        }

        public async Task<MessageInfo?> SendAndWaitFor(string message, string pattern)
        {
            await src.SendAsync(message);
            return await new WaitForMessage(Target, Timeout, msg => msg.PlainText == pattern);
        }

        public async Task<MessageInfo?> SendAndWaitFor(MessageBuilder message, string pattern)
        {
            await src.SendAsync(message);
            return await new WaitForMessage(Target, Timeout, msg => msg.PlainText == pattern);
        }

        public async Task<MessageInfo?> SendAndWaitFor(string message, Regex regex)
        {
            await src.SendAsync(message);
            return await new WaitForMessage(Target, Timeout, msg => regex.IsMatch(msg.PlainText));
        }

        public async Task<MessageInfo?> SendAndWaitFor(MessageBuilder message, Regex regex)
        {
            await src.SendAsync(message);
            return await new WaitForMessage(Target, Timeout, msg => regex.IsMatch(msg.PlainText));
        }

        public async Task<MessageInfo?> SendAndWaitFor(string message, Predicate<QMessage> predicator)
        {
            await src.SendAsync(message);
            return await new WaitForMessage(Target, Timeout, predicator);
        }

        public async Task<MessageInfo?> SendAndWaitFor(MessageBuilder message, Predicate<QMessage> predicator)
        {
            await src.SendAsync(message);
            return await new WaitForMessage(Target, Timeout, predicator);
        }


        public async Task<T> WaitAndSelect<T>(
            Func<MessageInfo?, T> selector,
            Predicate<QMessage>? messagePredicator = null,
            Predicate<T>? resultPredicator = null)
        {
            while (true)
            {
                var msg = await new WaitForMessage(Target, Timeout, messagePredicator);
                var result = selector(msg);
                if (resultPredicator is null || resultPredicator(result))
                {
                    return result;
                }
            }
        }

        private static async Task<MessageInfo?> Wrapper(WaitForMessage waitForMessage)
        {
            return await waitForMessage;
        }
    }

    internal class WaitForMessage
    {
        private Action? onReceived = null;
        private MessageInfo? result;
        private bool received = false;

        private readonly Predicate<QMessage>? predicator;

        public WaitForMessage(ListeningTarget target, TimeSpan timeout, Predicate<QMessage>? predicator = null)
        {
            this.predicator = predicator;

            ModuleCore.AddLock(OnMessage, target.Pred, timeout, () => OnMessage(null, null));
        }

        public Awaiter GetAwaiter() => new(this);

        private readonly object onMessageLock = new();
        public LockState OnMessage(MessageSource? src, QMessage? msg)
        {
            lock (onMessageLock)
            {
                if (received)
                {
                    return LockState.ContinueAndRemove;
                }

                if (msg is not null && !(predicator?.Invoke(msg) ?? true))
                {
                    return LockState.Continue;
                }

                received = true;
            }

            if (src is not null && msg is not null)
            {
                result = new(src, msg);
            }

            onReceived?.Invoke();
            return LockState.Finished;
        }

        public readonly struct Awaiter : INotifyCompletion
        {
            private readonly WaitForMessage waitForMessage;
            public Awaiter(WaitForMessage waitForMessage)
            {
                this.waitForMessage = waitForMessage;
            }

            public MessageInfo? GetResult() => waitForMessage.result;

            public bool IsCompleted => waitForMessage.received;

            public void OnCompleted(Action continuation)
            {
                waitForMessage.onReceived += continuation;
            }
        }
    }
}

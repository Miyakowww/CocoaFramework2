// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using Maila.Cocoa.Beans.Models.Messages;
using Maila.Cocoa.Framework.Support;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Maila.Cocoa.Framework
{
    public abstract class BotMiddlewareBase
    {
        public string DataRoot { get; }

        internal string TypeName { get; }

        private static readonly Type BaseType = typeof(BotModuleBase);

        protected internal BotMiddlewareBase()
        {
            Type realType = GetType();
            TypeName = realType.Name;
            DataRoot = $"MiddlewareData/{TypeName}_{realType.FullName!.CalculateCRC16():X}/";

            InitOverrode = realType.GetMethod(nameof(Init), BindingFlags.Instance | BindingFlags.NonPublic)!.DeclaringType != BaseType;
            DestroyOverrode = realType.GetMethod(nameof(Destroy), BindingFlags.Instance | BindingFlags.NonPublic)!.DeclaringType != BaseType;

            MethodInfo onMessageInfo = realType.GetMethod(nameof(OnMessage), BindingFlags.Instance | BindingFlags.NonPublic)!;
            OnMessageOverrode = onMessageInfo.DeclaringType != BaseType && onMessageInfo.GetCustomAttribute<DisabledAttribute>() is null;
            OnMessageThreadSafe = !OnMessageOverrode
                               || onMessageInfo.GetCustomAttribute<ThreadSafeAttribute>() is not null
                               || onMessageInfo.GetCustomAttribute<AsyncStateMachineAttribute>() is not null;

            MethodInfo onSendMessageInfo = realType.GetMethod(nameof(OnSendMessage), BindingFlags.Instance | BindingFlags.NonPublic)!;
            OnSendMessageOverrode = onSendMessageInfo.DeclaringType != BaseType && onSendMessageInfo.GetCustomAttribute<DisabledAttribute>() is null;
            OnSendMessageThreadSafe = !OnSendMessageOverrode
                                   || onSendMessageInfo.GetCustomAttribute<ThreadSafeAttribute>() is not null
                                   || onSendMessageInfo.GetCustomAttribute<AsyncStateMachineAttribute>() is not null;

            foreach (var field in realType
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.GetCustomAttribute<HostingAttribute>() is not null && f.GetCustomAttribute<DisabledAttribute>() is null))
            {
                DataManager.AddHosting(field, this, $"{DataRoot}Field_{field.Name}");
            }
        }

        internal bool InitOverrode { get; }
        protected internal virtual void Init() { }

        internal bool DestroyOverrode { get; }
        protected internal virtual void Destroy() { }

        internal bool OnMessageOverrode { get; }
        internal bool OnMessageThreadSafe { get; }
        private readonly object onMessageLock = new();

        protected internal virtual void OnMessage(MessageSource src, QMessage msg, Action<MessageSource, QMessage> next)
        {
            next(src, msg);
        }

        internal void OnMessageInternal(MessageSource src, QMessage msg, Action<MessageSource, QMessage> next)
        {
            if (!OnMessageOverrode)
            {
                next(src, msg);
            }
            if (OnMessageThreadSafe)
            {
                OnMessage(src, msg, next);
            }
            else
            {
                lock (onMessageLock)
                {
                    OnMessage(src, msg, next);
                }
            }
        }

        internal bool OnSendMessageOverrode { get; }
        internal bool OnSendMessageThreadSafe { get; }
        private readonly object onSendMessageLock = new();

        protected internal virtual bool OnSendMessage(ref long id, ref bool isGroup, ref IMessage[] chain, ref int? quote)
            => true;

        internal bool OnSendMessageInternal(ref long id, ref bool isGroup, ref IMessage[] chain, ref int? quote)
        {
            if (!OnSendMessageOverrode)
            {
                return true;
            }
            if (OnSendMessageThreadSafe)
            {
                return OnSendMessage(ref id, ref isGroup, ref chain, ref quote);
            }
            else
            {
                lock (onSendMessageLock)
                {
                    return OnSendMessage(ref id, ref isGroup, ref chain, ref quote);
                }
            }
        }
    }
}

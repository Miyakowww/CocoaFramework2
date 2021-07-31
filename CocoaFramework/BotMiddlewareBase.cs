// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Linq;
using System.Reflection;
using Maila.Cocoa.Framework.Support;

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
            OnMessageThreadSafe = !OnMessageOverrode || onMessageInfo.GetCustomAttribute<ThreadSafeAttribute>() is not null;

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
    }
}

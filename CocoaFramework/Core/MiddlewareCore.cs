// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Maila.Cocoa.Beans.Models.Messages;

namespace Maila.Cocoa.Framework.Core
{
    public static class MiddlewareCore
    {
        public static ImmutableArray<BotMiddlewareBase> Middlewares { get; private set; }
        internal static Action<MessageSource, QMessage>? OnMessage { get; private set; }

        internal static void Init(IEnumerable<Type> middlewares)
        {
            Middlewares = middlewares
                .Where(m => m.GetCustomAttribute<DisabledAttribute>() is null)
                .Select(Activator.CreateInstance)
                .Cast<BotMiddlewareBase>()
                .ToImmutableArray();

            foreach (var middleware in Middlewares)
            {
                if (middleware.InitOverrode)
                {
                    middleware.Init();
                }
            }

            if (!middlewares.Any())
            {
                OnMessage = (src, msg) => ModuleCore.OnMessage(src, msg, src, msg);
                return;
            }

            #region === Compile OnMessage ===

            ParameterExpression
                src = Expression.Parameter(typeof(MessageSource)),
                msg = Expression.Parameter(typeof(QMessage)),
                origSrc = Expression.Parameter(typeof(MessageSource)),
                origMsg = Expression.Parameter(typeof(QMessage));

            MethodCallExpression call = Expression.Call(
                typeof(ModuleCore).GetMethod(nameof(ModuleCore.OnMessage), BindingFlags.Static | BindingFlags.NonPublic)!,
                src, msg, origSrc, origMsg);

            MethodInfo onMessageInfo = typeof(BotMiddlewareBase)
                .GetMethod(nameof(BotMiddlewareBase.OnMessageInternal), BindingFlags.Instance | BindingFlags.NonPublic)!;

            for (int i = 1; i <= Middlewares.Length; i++)
            {
                if (!Middlewares[^i].OnMessageOverrode
                  || Middlewares[^i]
                        .GetType()
                        .GetMethod(nameof(BotMiddlewareBase.OnMessage), BindingFlags.Instance | BindingFlags.NonPublic)!
                        .GetCustomAttribute<DisabledAttribute>() is not null)
                {
                    continue;
                }

                LambdaExpression next = Expression.Lambda<Action<MessageSource, QMessage>>(call, src, msg);

                src = Expression.Parameter(typeof(MessageSource));
                msg = Expression.Parameter(typeof(QMessage));

                call = Expression.Call(
                    Expression.Constant(Middlewares[^i]),
                    onMessageInfo,
                    src, msg, next);
            }

            OnMessage = Expression.Lambda<Action<MessageSource, QMessage>>(Expression.Block(
                new[] { src, msg },
                Expression.Assign(src, origSrc),
                Expression.Assign(msg, origMsg),
                call
            ), origSrc, origMsg).Compile();

            #endregion
        }

        internal static void Reset()
        {
            foreach (var middleware in Middlewares.Where(middleware => middleware.DestroyOverrode))
            {
                middleware.Destroy();
            }

            Middlewares = ImmutableArray<BotMiddlewareBase>.Empty;
            OnMessage = null;
        }

        internal static bool OnSendMessage(ref long id, ref bool isGroup, ref IMessage[] chain, ref int? quote)
        {
            foreach (var m in Middlewares)
            {
                bool send = m.OnSendMessageInternal(ref id, ref isGroup, ref chain, ref quote);
                if (!send)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

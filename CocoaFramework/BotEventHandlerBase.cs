// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Maila.Cocoa.Beans.Models.Events;

namespace Maila.Cocoa.Framework
{
    public abstract class BotEventHandlerBase
    {
        internal readonly ImmutableDictionary<Type, Action<Event>> EventListeners;

        private static readonly Type BaseType = typeof(BotEventHandlerBase);

        protected internal BotEventHandlerBase()
        {
            Type realType = GetType();

            Dictionary<Type, Action<Event>> listeners = new();
            foreach (var method in realType
                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.DeclaringType != BaseType && m.GetCustomAttribute<DisabledAttribute>() is null))
            {
                var parameters = method.GetParameters();
                if (parameters.Length != 1 || !parameters[0].ParameterType.IsSubclassOf(typeof(Event)))
                {
                    continue;
                }

                var eventType = parameters[0].ParameterType;
                void handler(Event e)
                {
                    method.Invoke(this, new object[] { e });
                }

                if (listeners.ContainsKey(eventType))
                {
                    listeners[eventType] += handler;
                }
                else
                {
                    listeners[eventType] = handler;
                }
            }

            EventListeners = listeners.ToImmutableDictionary();
        }

        internal void HandleEvent(Event evt)
        {
            if (EventListeners.TryGetValue(evt.GetType(), out var action))
            {
                action(evt);
            }
        }

        protected internal virtual void OnException(Exception e) { }
        protected internal virtual void OnDisconnect() { }
    }
}
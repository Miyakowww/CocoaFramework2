// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Maila.Cocoa.Framework.Core
{
    public static partial class ModuleCore
    {
        public static ImmutableArray<BotModuleBase> Modules { get; private set; }

        internal static void Init(IEnumerable<Assembly> assemblies)
        {
            List<BotModuleBase> modules = new();
            foreach (var assem in assemblies)
            {
                foreach (var type in assem.GetTypes())
                {
                    if (!type.IsAssignableTo(typeof(BotModuleBase)))
                    {
                        continue;
                    }

                    if (type.GetCustomAttribute<DisabledAttribute>() is not null)
                    {
                        continue;
                    }

                    if (type.GetCustomAttribute<BotModuleAttribute>() is null)
                    {
                        continue;
                    }

                    if (Activator.CreateInstance(type) is not BotModuleBase module)
                    {
                        continue;
                    }

                    if (module.InitOverrode)
                    {
                        module.Init();
                    }

                    modules.Add(module);
                }
            }

            modules.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            Modules = modules.ToImmutableArray();
        }

        internal static void Reset()
        {
            foreach (var module in Modules.Where(module => module.DestroyOverrode))
            {
                module.Destroy();
            }

            Modules = ImmutableArray<BotModuleBase>.Empty;
        }

        internal static void OnMessage(MessageSource src, QMessage msg, MessageSource origSrc, QMessage origMsg)
        {
            for (int i = messageLocks.Count - 1; i >= 0; i--)
            {
                var state = messageLocks[i](src, msg);
                if ((state & LockState.ContinueAndRemove) != 0) // Whether remove
                {
                    messageLocks.RemoveAt(i);
                }

                if ((state & LockState.NotFinished) != 0) // Whether end execution
                {
                    OnMessageFinished(src, msg, origSrc, origMsg, true, null);
                    return;
                }
            }

            foreach (var module in Modules.Where(m => m.Enabled
                                                   && m.Pred(src)
                                                   && (src.IsGroup ? m.EnableInGroup : m.EnableInPrivate)))
            {
                try
                {
                    if (!module.OnMessageInternal(src, msg))
                    {
                        continue;
                    }
                }
                catch (Exception e)
                {
                    throw new AggregateException($"Module Run Error: {module.TypeName}", e);
                }
                OnMessageFinished(src, msg, origSrc, origMsg, true, module);
                return;
            }

            OnMessageFinished(src, msg, origSrc, origMsg, false, null);
        }

        private static void OnMessageFinished(MessageSource src, QMessage msg, MessageSource origSrc, QMessage origMsg, bool processed, BotModuleBase? processModule)
        {
            foreach (var module in Modules.Where(module => module.Enabled && (src.IsGroup ? module.EnableInGroup : module.EnableInPrivate)))
            {
                _ = Task.Run(() => module.OnMessageFinishedInternal(src, msg, origSrc, origMsg, processed, processModule));
            }
        }
    }
}

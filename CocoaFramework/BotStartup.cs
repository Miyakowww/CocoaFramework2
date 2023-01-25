// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Maila.Cocoa.Framework.Core;

namespace Maila.Cocoa.Framework
{
    public static class BotStartup
    {
        public static bool Connected => BotCore.Connected;

        [Obsolete("Use ConnectAndInit instead.")]
        public static Task<bool> Connect(BotStartupConfig config)
            => BotCore.Connect(config);

        [Obsolete("Use DisconnectAndSaveData instead.")]
        public static Task Disconnect()
            => BotCore.Disconnect();

        /// <summary>Connect to the server, and automatically release the existing connection if it exists.</summary>
        public static Task<bool> ConnectAndInit(BotStartupConfig config)
            => BotCore.ConnectAndInit(config);

        /// <summary>Disconnect and release related resources.</summary>
        public static Task DisconnectAndSaveData()
            => BotCore.DisconnectAndSaveData();

        /// <summary>Reconnect to the server without releasing resources.</summary>
        public static Task Reconnect()
            => BotCore.Reconnect();
    }

    public class BotStartupConfig
    {
        public string host;
        public int port;
        public string verifyKey;
        public long qqId;

        internal List<Type> Middlewares { get; } = new();
        public List<Assembly> Assemblies { get; } = new();
        public List<BotEventHandlerBase> EventHandlers { get; } = new();
        public TimeSpan autoSave;

        public BotStartupConfig(string verifyKey, long qqId, string host) : this(verifyKey, qqId, host, 80) { }
        public BotStartupConfig(string verifyKey, long qqId, int port) : this(verifyKey, qqId, "127.0.0.1", port) { }
        public BotStartupConfig(string verifyKey, long qqId, string host = "127.0.0.1", int port = 8080)
        {
            this.verifyKey = verifyKey;
            this.qqId = qqId;
            this.host = host;
            this.port = port;
            Assemblies.Add(Assembly.GetEntryAssembly()!);
            autoSave = TimeSpan.FromMinutes(5);
        }

        public BotStartupConfig AddMiddleware<T>() where T : BotMiddlewareBase
        {
            Middlewares.Add(typeof(T));
            return this;
        }

        public BotStartupConfig AddMiddleware(Type type)
        {
            if (type.IsAssignableTo(typeof(BotMiddlewareBase)))
            {
                Middlewares.Add(type);
            }
            return this;
        }

        public BotStartupConfig AddAssembly(Assembly assem)
        {
            Assemblies.Add(assem);
            return this;
        }

        public BotStartupConfig AddEventHandler(BotEventHandlerBase handler)
        {
            EventHandlers.Add(handler);
            return this;
        }
    }
}

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

        /// <summary>Connect to the server.</summary>
        public static Task<bool> Connect(BotStartupConfig config)
            => BotCore.Connect(config);

        /// <summary>Disconnect and release related resources.</summary>
        public static Task Disconnect()
            => BotCore.Disconnect();

        /// <summary>Release current session and connect to a new session.</summary>
        public static Task Reconnect()
            => BotCore.Reconnect();
    }

    public class BotStartupConfig
    {
        public string host;
        public int port;
        public string authKey;
        public long qqId;

        internal List<Type> Middlewares { get; } = new();
        public List<Assembly> Assemblies { get; } = new();
        public TimeSpan autoSave;

        public BotStartupConfig(string authKey, long qqId, string host) : this(authKey, qqId, host, 80) { }
        public BotStartupConfig(string authKey, long qqId, int port) : this(authKey, qqId, "127.0.0.1", port) { }
        public BotStartupConfig(string authKey, long qqId, string host = "127.0.0.1", int port = 8080)
        {
            this.authKey = authKey;
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
    }
}

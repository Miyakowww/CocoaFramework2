// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Threading;
using System.Threading.Tasks;
using Maila.Cocoa.Beans.API;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework.Core
{
    internal static class BotCore
    {
        public static long? BindingQQ { get; private set; }
        public static string? SessionKey { get; private set; }

        public static bool Connected => host is not null && BindingQQ is not null && SessionKey is not null;

        internal static string? host;
        internal static string authKey = string.Empty;

        private static readonly SemaphoreSlim connLock = new(1);

        /// <summary>Test the connection to the server.</summary>
        public static async Task<bool> TestNetwork()
            => host is not null && await MiraiAPI.About(host) is not null;

        public static async Task<bool> Connect(BotStartupConfig config)
        {
            await connLock.WaitAsync();

            if (Connected)
            {
                try
                {
                    await MiraiAPI.Release(host!, SessionKey!, BindingQQ!.Value);
                }
                catch { }
            }

            host = $"{config.host}:{config.port}";

            if (!await TestNetwork())
            {
                host = null;
                SessionKey = null;
                BindingQQ = null;
                connLock.Release();
                return false;
            }

            try
            {
                authKey = config.authKey;
                SessionKey = await MiraiAPI.Auth(host, authKey);
                if (SessionKey is null)
                {
                    host = null;
                    SessionKey = null;
                    BindingQQ = null;
                    connLock.Release();
                    return false;
                }
                await MiraiAPI.Verify(host, SessionKey, config.qqId);
                BindingQQ = config.qqId;
                await MiraiAPI.SetConfig(host, SessionKey, null, true);
            }
            catch
            {
                host = null;
                SessionKey = null;
                BindingQQ = null;
                connLock.Release();
                return false;
            }

            try
            {
                BotAPI.Init();
                BotAuth.Init();
                BotReg.Init();

                await BotInfo.ReloadAll();

                ModuleCore.Init(config.Assemblies);
                MiddlewareCore.Init(config.Middlewares);

                DataManager.StartHosting(config.autoSave);

                return true;
            }
            finally
            {
                connLock.Release();
            }
        }

        public static async Task Disconnect()
        {
            await connLock.WaitAsync();

            if (Connected)
            {
                try
                {
                    await MiraiAPI.Release(host!, SessionKey!, BindingQQ!.Value);
                }
                catch { }
            }

            host = null;
            SessionKey = null;
            BindingQQ = null;

            try
            {
                await DataManager.StopHosting();

                BotAPI.Reset();
                BotAuth.Reset();
                BotReg.Reset();
                BotInfo.Reset();

                ModuleCore.Reset();
                MiddlewareCore.Reset();

                while (DataManager.SavingData)
                {
                    await Task.Delay(10);
                }
            }
            finally
            {
                connLock.Release();
            }
        }

        public static async Task Reconnect()
        {
            if (!Connected)
            {
                return;
            }

            await connLock.WaitAsync();

            try
            {
                await MiraiAPI.Release(host!, SessionKey!, BindingQQ!.Value);
            }
            catch { }

            if (!await TestNetwork())
            {
                SessionKey = null;
                connLock.Release();
                return;
            }

            try
            {
                SessionKey = await MiraiAPI.Auth(host!, authKey);
                if (SessionKey is null)
                {
                    return;
                }
                await MiraiAPI.Verify(host!, SessionKey, BindingQQ!.Value);
                await MiraiAPI.SetConfig(host!, SessionKey, null, true);
            }
            catch
            {
                SessionKey = null;
                return;
            }
            finally
            {
                connLock.Release();
            }
        }

        internal static void OnMessage(MessageSource src, QMessage msg)
        {
            try
            {
                MiddlewareCore.OnMessage?.Invoke(src, msg);
            }
            catch (AggregateException ae)
            {
                Exception e = ae;
                while (e.InnerException is not null)
                {
                    if (e.InnerException.Message.Contains("InternalServerError"))
                    {
                        _ = Reconnect();
                        return;
                    }
                    e = e.InnerException;
                }
                BotAPI.OnException?.Invoke(e);
            }
            catch (Exception e)
            {
                BotAPI.OnException?.Invoke(e);
            }
        }
    }
}

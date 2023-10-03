// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
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

        [MemberNotNullWhen(true, new[] { nameof(host), nameof(BindingQQ), nameof(SessionKey) })]
        public static bool Connected => host is not null && BindingQQ is not null && SessionKey is not null;

        internal static string? host;
        internal static string verifyKey = string.Empty;

        private static readonly SemaphoreSlim connLock = new(1);

        public static async Task<bool> TestNetwork()
            => host is not null && await MiraiAPI.About(host) is not null;

        [Obsolete("Use ConnectAndInit instead.")]
        public static Task<bool> Connect(BotStartupConfig config)
        {
            return ConnectAndInit(config);
        }

        public static async Task<bool> ConnectAndInit(BotStartupConfig config)
        {
            await connLock.WaitAsync();

            if (Connected)
            {
                try
                {
                    await MiraiAPI.Release(host, SessionKey, BindingQQ.Value);
                }
                catch { }
            }

            host = $"{config.host}:{config.port}";

            string? ver = await MiraiAPI.About(host);
            if (ver is null)
            {
                host = null;
                SessionKey = null;
                BindingQQ = null;
                connLock.Release();
                return false;
            }

            bool IsVer2 = ver.StartsWith('2');
            try
            {
                verifyKey = config.verifyKey;
                SessionKey = await (IsVer2 ? MiraiAPI.Verify(host, verifyKey) : MiraiAPI.Authv1(host, verifyKey));
                if (SessionKey is null)
                {
                    host = null;
                    SessionKey = null;
                    BindingQQ = null;
                    connLock.Release();
                    return false;
                }
                await (IsVer2 ? MiraiAPI.Bind(host, SessionKey, config.qqId) : MiraiAPI.Verifyv1(host, SessionKey, config.qqId));
                BindingQQ = config.qqId;
                if (!IsVer2)
                {
                    await MiraiAPI.SetConfig(host, SessionKey, null, true);
                }
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
                BotAPI.Init(config.EventHandlers);
                BotAuth.Init();
                BotReg.Init();

                await BotInfo.ReloadAll();

                ModuleCore.Init(config.Assemblies.Distinct());
                MiddlewareCore.Init(config.Middlewares);

                DataHosting.StartHosting(config.autoSave);

                return true;
            }
            finally
            {
                connLock.Release();
            }
        }

        [Obsolete("Use DisconnectAndSaveData instead.")]
        public static Task Disconnect()
        {
            return DisconnectAndSaveData();
        }

        public static async Task DisconnectAndSaveData()
        {
            await connLock.WaitAsync();

            if (Connected)
            {
                try
                {
                    await MiraiAPI.Release(host, SessionKey, BindingQQ.Value);
                }
                catch { }
            }

            host = null;
            SessionKey = null;
            BindingQQ = null;

            try
            {
                await DataHosting.StopHosting();

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
                await MiraiAPI.Release(host, SessionKey, BindingQQ.Value);
            }
            catch { }

            string? ver = await MiraiAPI.About(host);
            if (ver is null)
            {
                SessionKey = null;
                BotAPI.Reset();
                connLock.Release();
                return;
            }

            bool IsVer2 = ver.StartsWith('2');

            try
            {
                SessionKey = await (IsVer2 ? MiraiAPI.Verify(host, verifyKey) : MiraiAPI.Authv1(host, verifyKey));
                if (SessionKey is null)
                {
                    BotAPI.Reset();
                    return;
                }
                await (IsVer2 ? MiraiAPI.Bind(host, SessionKey, BindingQQ.Value) : MiraiAPI.Verifyv1(host, SessionKey, BindingQQ.Value));
                if (!IsVer2)
                {
                    await MiraiAPI.SetConfig(host, SessionKey, null, true);
                }
            }
            catch
            {
                SessionKey = null;
                BotAPI.Reset();
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
                BotAPI.OnException(e);
            }
            catch (Exception e)
            {
                BotAPI.OnException(e);
            }
        }
    }
}

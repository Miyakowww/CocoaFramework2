// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Maila.Cocoa.Framework.Models.Route;
using Maila.Cocoa.Framework.Support;

namespace Maila.Cocoa.Framework
{
    public abstract class BotModuleBase
    {
        public string? Name { get; }
        public int Priority { get; }

        public bool EnableInGroup { get; }
        public bool EnableInPrivate { get; }

        public bool IsAnonymous { get; }
        public string DataRoot { get; }

        private bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value;
                BotReg.SetBool($"MODULE/{TypeName}/ENABLED", value);
            }
        }

        internal readonly Func<MessageSource, bool> Pred;

        internal string TypeName { get; }

        private readonly List<RouteInfo> routes = new();

        private static readonly Type BaseType = typeof(BotModuleBase);

        protected internal BotModuleBase()
        {
            Type realType = GetType();

            #region === Module Info ===

            TypeName = realType.Name;
            DataRoot = $"ModuleData/{TypeName}_{realType.FullName!.CalculateCRC16():X}/";
            if (realType.GetCustomAttribute<BotModuleAttribute>() is { } moduleInfo)
            {
                Name = moduleInfo.Name;
                Priority = moduleInfo.Priority;
                IsAnonymous = Name is null;
            }

            enabled = BotReg.GetBool($"MODULE/{TypeName}/ENABLED", true);

            #endregion

            #region === Conditions ===

            var reqs = realType.GetCustomAttributes<IdentityRequirementsAttribute>()
                               .Select<IdentityRequirementsAttribute, Func<MessageSource, bool>>(r => src => r.Check(src.User.Identity, src.Permission))
                               .ToList();
            Pred = reqs.Any()
                ? src => reqs.Any(p => p(src))
                : src => true;

            EnableInGroup = realType.GetCustomAttribute<DisableInGroupAttribute>() is null;
            EnableInPrivate = realType.GetCustomAttribute<DisableInPrivateAttribute>() is null;

            #endregion

            #region === Method Info ===

            InitOverrode = realType.GetMethod(nameof(Init), BindingFlags.Instance | BindingFlags.NonPublic)!.DeclaringType != BaseType;
            DestroyOverrode = realType.GetMethod(nameof(Destroy), BindingFlags.Instance | BindingFlags.NonPublic)!.DeclaringType != BaseType;

            MethodInfo onMessageInfo = realType.GetMethod(nameof(OnMessage), BindingFlags.Instance | BindingFlags.NonPublic)!;
            OnMessageOverrode = onMessageInfo.DeclaringType != BaseType && onMessageInfo.GetCustomAttribute<DisabledAttribute>() is null;
            OnMessageThreadSafe = !OnMessageOverrode
                                || onMessageInfo.GetCustomAttribute<ThreadSafeAttribute>() is not null
                                || onMessageInfo.GetCustomAttribute<AsyncStateMachineAttribute>() is not null;
            OnMessageEnableInGroup = OnMessageOverrode && onMessageInfo.GetCustomAttribute<DisableInGroupAttribute>() is null;
            OnMessageEnableInPrivate = OnMessageOverrode && onMessageInfo.GetCustomAttribute<DisableInPrivateAttribute>() is null;
            if (OnMessageOverrode)
            {
                var mreqs = onMessageInfo.GetCustomAttributes<IdentityRequirementsAttribute>()
                                         .Select<IdentityRequirementsAttribute, Func<MessageSource, bool>>(r => src => r.Check(src.User.Identity, src.Permission))
                                         .ToList();
                onMessagePred = mreqs.Any()
                    ? src => mreqs.Any(p => p(src))
                    : src => true;
            }
            else
            {
                onMessagePred = src => false;
            }

            MethodInfo onMessageFinishedInfo = realType.GetMethod(nameof(OnMessageFinished), BindingFlags.Instance | BindingFlags.NonPublic)!;
            OnMessageFinishedOverrode = onMessageFinishedInfo.DeclaringType != BaseType && onMessageFinishedInfo.GetCustomAttribute<DisabledAttribute>() is null;
            OnMessageFinishedThreadSafe = !OnMessageFinishedOverrode
                                        || onMessageFinishedInfo.GetCustomAttribute<ThreadSafeAttribute>() is not null
                                        || onMessageFinishedInfo.GetCustomAttribute<ThreadSafeAttribute>() is not null;
            OnMessageFinishedEnableInGroup = OnMessageFinishedOverrode && onMessageFinishedInfo.GetCustomAttribute<DisableInGroupAttribute>() is null;
            OnMessageFinishedEnableInPrivate = OnMessageFinishedOverrode && onMessageFinishedInfo.GetCustomAttribute<DisableInPrivateAttribute>() is null;

            #endregion

            #region === Data Hosting ===

            foreach (var field in realType
                .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.GetCustomAttribute<HostingAttribute>() is not null
                         && f.GetCustomAttribute<DisabledAttribute>() is null
                         && !(f.IsStatic && f.IsInitOnly)))
            {
                DataHosting.AddHosting(field, this, $"{DataRoot}Field_{field.Name}");
            }

            DataHosting.AddOptimizeEnabledHosting(
                BaseType.GetField(nameof(userAutoData), BindingFlags.NonPublic | BindingFlags.Instance)!,
                this,
                $"{DataRoot}UserAutoData");

            DataHosting.AddOptimizeEnabledHosting(
                BaseType.GetField(nameof(groupAutoData), BindingFlags.NonPublic | BindingFlags.Instance)!,
                this,
                $"{DataRoot}GroupAutoData");

            DataHosting.AddOptimizeEnabledHosting(
                BaseType.GetField(nameof(sourceAutoData), BindingFlags.NonPublic | BindingFlags.Instance)!,
                this,
                $"{DataRoot}SourceAutoData");

            #endregion

            #region === Route ===

            MethodInfo[] methods = realType.GetMethods();
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<DisabledAttribute>() is not null)
                {
                    continue;
                }

                if (method.GetCustomAttributes<TextRouteAttribute>().ToArray() is { Length: > 0 } textRouteInfos)
                {
                    var rreqs = method.GetCustomAttributes<IdentityRequirementsAttribute>()
                                      .Select<IdentityRequirementsAttribute, Func<MessageSource, bool>>(r => src => r.Check(src.User.Identity, src.Permission))
                                      .ToList();
                    string[] texts = textRouteInfos.Select(t => t.Text).ToArray();
                    bool[] ignoreCases = textRouteInfos.Select(t => t.IgnoreCase).ToArray();
                    bool[] atRequireds = textRouteInfos.Select(t => t.AtRequired).ToArray();
                    routes.Add(new TextRouteInfo(this, method, texts, ignoreCases, atRequireds, rreqs.Any()
                                                                                                    ? src => rreqs.Any(p => p(src))
                                                                                                    : src => true));
                }

                if (method.GetCustomAttributes<RegexRouteAttribute>().ToArray() is { Length: > 0 } regexRouteInfos)
                {
                    var rreqs = method.GetCustomAttributes<IdentityRequirementsAttribute>()
                                      .Select<IdentityRequirementsAttribute, Func<MessageSource, bool>>(r => src => r.Check(src.User.Identity, src.Permission))
                                      .ToList();
                    Regex[] regexs = regexRouteInfos.Select(r => r.Regex).ToArray();
                    bool[] atRequireds = regexRouteInfos.Select(r => r.AtRequired).ToArray();
                    routes.Add(new RegexRouteInfo(this, method, regexs, atRequireds, rreqs.Any()
                                                                                         ? src => rreqs.Any(p => p(src))
                                                                                         : src => true));
                }
            }

            #endregion
        }

        #region === Event Handling ===

        internal bool InitOverrode { get; }
        protected internal virtual void Init() { }

        internal bool DestroyOverrode { get; }
        protected internal virtual void Destroy() { }

        private readonly Func<MessageSource, bool> onMessagePred;
        internal bool OnMessageOverrode { get; }
        internal bool OnMessageThreadSafe { get; }
        internal bool OnMessageEnableInGroup { get; }
        internal bool OnMessageEnableInPrivate { get; }
        private readonly object onMessageLock = new();
        protected internal virtual bool OnMessage(MessageSource src, QMessage msg)
        {
            return false;
        }
        internal bool OnMessageInternal(MessageSource src, QMessage msg)
        {
            if (routes.Any(route => route.Run(src, msg)))
            {
                return true;
            }

            if (!OnMessageOverrode)
            {
                return false;
            }
            if (!(src.IsGroup ? OnMessageEnableInGroup : OnMessageEnableInPrivate))
            {
                return false;
            }
            if (!onMessagePred(src))
            {
                return false;
            }
            if (OnMessageThreadSafe)
            {
                return OnMessage(src, msg);
            }
            lock (onMessageLock)
            {
                return OnMessage(src, msg);
            }
        }

        internal bool OnMessageFinishedOverrode { get; }
        internal bool OnMessageFinishedThreadSafe { get; }
        internal bool OnMessageFinishedEnableInGroup { get; }
        internal bool OnMessageFinishedEnableInPrivate { get; }
        private readonly object onMessageFinishedLock = new();
        protected internal virtual void OnMessageFinished(MessageSource src, QMessage msg, MessageSource origSrc, QMessage origMsg, bool processed, BotModuleBase? processModule) { }
        internal void OnMessageFinishedInternal(MessageSource src, QMessage msg, MessageSource origSrc, QMessage origMsg, bool processed, BotModuleBase? processModule)
        {
            if (!OnMessageFinishedOverrode)
            {
                return;
            }
            if (!(src.IsGroup ? OnMessageFinishedEnableInGroup : OnMessageFinishedEnableInPrivate))
            {
                return;
            }
            if (OnMessageFinishedThreadSafe)
            {
                OnMessageFinished(src, msg, origSrc, origMsg, processed, processModule);
            }
            else
            {
                lock (onMessageFinishedLock)
                {
                    OnMessageFinished(src, msg, origSrc, origMsg, processed, processModule);
                }
            }
        }

        #endregion

        #region === AutoData ===

        private static readonly Type UserAutoDataType = typeof(UserAutoData<>);
        private static readonly Type GroupAutoDataType = typeof(GroupAutoData<>);
        private static readonly Type SourceAutoDataType = typeof(SourceAutoData<>);

        internal readonly ConcurrentDictionary<long, ConcurrentDictionary<string, object?>> userAutoData = new();
        internal readonly ConcurrentDictionary<long, ConcurrentDictionary<string, object?>> groupAutoData = new();
        internal readonly ConcurrentDictionary<(long?, long), ConcurrentDictionary<string, object?>> sourceAutoData = new();
        internal readonly ConcurrentDictionary<long, ConcurrentDictionary<string, object?>> userTempData = new();
        internal readonly ConcurrentDictionary<long, ConcurrentDictionary<string, object?>> groupTempData = new();
        internal readonly ConcurrentDictionary<(long?, long), ConcurrentDictionary<string, object?>> sourceTempData = new();

        internal readonly ConcurrentDictionary<long, ConcurrentDictionary<string, object?>> userAutoDataCache = new();
        internal readonly ConcurrentDictionary<long, ConcurrentDictionary<string, object?>> groupAutoDataCache = new();
        internal readonly ConcurrentDictionary<(long?, long), ConcurrentDictionary<string, object?>> sourceAutoDataCache = new();
        internal readonly ConcurrentDictionary<long, ConcurrentDictionary<string, object?>> userTempDataCache = new();
        internal readonly ConcurrentDictionary<long, ConcurrentDictionary<string, object?>> groupTempDataCache = new();
        internal readonly ConcurrentDictionary<(long?, long), ConcurrentDictionary<string, object?>> sourceTempDataCache = new();

        internal object? GetUserAutoData(MessageSource src, string name, Type type)
        {
            var key = src.User.Id;
            if (!userAutoDataCache.TryGetValue(key, out var autoDatas))
            {
                autoDatas = new();
                userAutoDataCache[key] = autoDatas;
            }
            if (autoDatas.TryGetValue(name, out var autoData))
            {
                return autoData;
            }

            if (!userAutoData.TryGetValue(key, out var vals))
            {
                vals = new();
                userAutoData[key] = vals;
            }
            if (!vals.ContainsKey(name))
            {
                vals[name] = null;
            }
            autoData = Activator.CreateInstance(UserAutoDataType.MakeGenericType(type),
                                            BindingFlags.NonPublic | BindingFlags.Instance, null,
                                            new object[] { userAutoData, key, name }, null);
            autoDatas[name] = autoData;
            return autoData;
        }

        internal object? GetGroupAutoData(MessageSource src, string name, Type type)
        {
            var key = src.Group?.Id ?? 0;
            if (!groupAutoDataCache.TryGetValue(key, out var autoDatas))
            {
                autoDatas = new();
                groupAutoDataCache[key] = autoDatas;
            }
            if (autoDatas.TryGetValue(name, out var autoData))
            {
                return autoData;
            }

            if (!groupAutoData.TryGetValue(key, out var vals))
            {
                vals = new();
                groupAutoData[key] = vals;
            }
            if (!vals.ContainsKey(name))
            {
                vals[name] = null;
            }
            autoData = Activator.CreateInstance(GroupAutoDataType.MakeGenericType(type),
                                            BindingFlags.NonPublic | BindingFlags.Instance, null,
                                            new object[] { groupAutoData, key, name }, null);
            autoDatas[name] = autoData;
            return autoData;
        }

        internal object? GetSourceAutoData(MessageSource src, string name, Type type)
        {
            var key = (src.Group?.Id, src.User.Id);
            if (!sourceAutoDataCache.TryGetValue(key, out var autoDatas))
            {
                autoDatas = new();
                sourceAutoDataCache[key] = autoDatas;
            }
            if (autoDatas.TryGetValue(name, out var autoData))
            {
                return autoData;
            }

            if (!sourceAutoData.TryGetValue(key, out var vals))
            {
                vals = new();
                sourceAutoData[key] = vals;
            }
            if (!vals.ContainsKey(name))
            {
                vals[name] = null;
            }
            autoData = Activator.CreateInstance(SourceAutoDataType.MakeGenericType(type),
                                            BindingFlags.NonPublic | BindingFlags.Instance, null,
                                            new object[] { sourceAutoData, key, name }, null);
            autoDatas[name] = autoData;
            return autoData;
        }

        internal object? GetUserTempData(MessageSource src, string name, Type type)
        {
            var key = src.User.Id;
            if (!userTempDataCache.TryGetValue(key, out var autoDatas))
            {
                autoDatas = new();
                userTempDataCache[key] = autoDatas;
            }
            if (autoDatas.TryGetValue(name, out var autoData))
            {
                return autoData;
            }

            if (!userTempData.TryGetValue(key, out var vals))
            {
                vals = new();
                userTempData[key] = vals;
            }
            if (!vals.ContainsKey(name))
            {
                vals[name] = null;
            }
            autoData = Activator.CreateInstance(UserAutoDataType.MakeGenericType(type),
                                            BindingFlags.NonPublic | BindingFlags.Instance, null,
                                            new object[] { userTempData, key, name }, null);
            autoDatas[name] = autoData;
            return autoData;
        }

        internal object? GetGroupTempData(MessageSource src, string name, Type type)
        {
            var key = src.Group?.Id ?? 0;
            if (!groupTempDataCache.TryGetValue(key, out var autoDatas))
            {
                autoDatas = new();
                groupTempDataCache[key] = autoDatas;
            }
            if (autoDatas.TryGetValue(name, out var autoData))
            {
                return autoData;
            }

            if (!groupTempData.TryGetValue(key, out var vals))
            {
                vals = new();
                groupTempData[key] = vals;
            }
            if (!vals.ContainsKey(name))
            {
                vals[name] = null;
            }
            autoData = Activator.CreateInstance(GroupAutoDataType.MakeGenericType(type),
                                            BindingFlags.NonPublic | BindingFlags.Instance, null,
                                            new object[] { groupTempData, key, name }, null);
            autoDatas[name] = autoData;
            return autoData;
        }

        internal object? GetSourceTempData(MessageSource src, string name, Type type)
        {
            var key = (src.Group?.Id, src.User.Id);
            if (!sourceTempDataCache.TryGetValue(key, out var autoDatas))
            {
                autoDatas = new();
                sourceTempDataCache[key] = autoDatas;
            }
            if (autoDatas.TryGetValue(name, out var autoData))
            {
                return autoData;
            }

            if (!sourceTempData.TryGetValue(key, out var vals))
            {
                vals = new();
                sourceTempData[key] = vals;
            }
            if (!vals.ContainsKey(name))
            {
                vals[name] = null;
            }
            autoData = Activator.CreateInstance(SourceAutoDataType.MakeGenericType(type),
                                            BindingFlags.NonPublic | BindingFlags.Instance, null,
                                            new object[] { sourceTempData, key, name }, null);
            autoDatas[name] = autoData;
            return autoData;
        }

        #endregion
    }
}

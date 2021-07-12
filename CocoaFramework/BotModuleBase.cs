// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        internal string? TypeName { get; }

        private readonly List<RouteInfo> routes = new();

        protected internal BotModuleBase()
        {
            Type baseType = typeof(BotModuleBase);
            Type realType = GetType();
            if (realType.GetCustomAttribute<BotModuleAttribute>() is { } moduleInfo)
            {
                Name = moduleInfo.Name;
                Priority = moduleInfo.Priority;
                IsAnonymous = Name is null;
            }

            var reqs = realType.GetCustomAttributes<IdentityRequirementsAttribute>()
                               .Select<IdentityRequirementsAttribute, Func<MessageSource, bool>>(r => src => r.Check(src.User.Identity, src.Permission))
                               .ToList();
            Pred = reqs.Any()
                ? src => reqs.Any(p => p(src))
                : src => true;

            EnableInGroup = realType.GetCustomAttribute<DisableInGroupAttribute>() is null;
            EnableInPrivate = realType.GetCustomAttribute<DisableInPrivateAttribute>() is null;

            InitOverrode = realType.GetMethod(nameof(Init), BindingFlags.Instance | BindingFlags.NonPublic)!.DeclaringType != baseType;
            DestroyOverrode = realType.GetMethod(nameof(Destroy), BindingFlags.Instance | BindingFlags.NonPublic)!.DeclaringType != baseType;

            MethodInfo onMessageInfo = realType.GetMethod(nameof(OnMessage), BindingFlags.Instance | BindingFlags.NonPublic)!;
            OnMessageOverrode = onMessageInfo.DeclaringType != baseType && onMessageInfo.GetCustomAttribute<DisabledAttribute>() is null;
            OnMessageThreadSafe = OnMessageOverrode && onMessageInfo.GetCustomAttribute<ThreadSafeAttribute>() is not null;
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
            OnMessageFinishedOverrode = onMessageFinishedInfo.DeclaringType != baseType && onMessageFinishedInfo.GetCustomAttribute<DisabledAttribute>() is null;
            OnMessageFinishedThreadSafe = OnMessageFinishedOverrode && onMessageFinishedInfo.GetCustomAttribute<ThreadSafeAttribute>() is not null;
            OnMessageFinishedEnableInGroup = OnMessageFinishedOverrode && onMessageFinishedInfo.GetCustomAttribute<DisableInGroupAttribute>() is null;
            OnMessageFinishedEnableInPrivate = OnMessageFinishedOverrode && onMessageFinishedInfo.GetCustomAttribute<DisableInPrivateAttribute>() is null;

            TypeName = realType.Name;

            if (TypeName is not null)
            {
                var crc16 = realType.AssemblyQualifiedName!.CalculateCRC16().ToString("X");
                foreach (var field in realType
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.GetCustomAttribute<HostingAttribute>() is not null && f.GetCustomAttribute<DisabledAttribute>() is null))
                {
                    DataManager.AddHosting(field, this, $"ModuleData/{TypeName}_{crc16}/Field_{field.Name}");
                }
            }

            enabled = BotReg.GetBool($"MODULE/{TypeName}/ENABLED", true);

            MethodInfo[] methods = realType.GetMethods();
            foreach (var method in methods)
            {
                if (method.GetCustomAttribute<DisabledAttribute>() is not null)
                {
                    continue;
                }

                if (method.GetCustomAttributes<RegexRouteAttribute>().ToArray() is { Length: > 0 } regexRouteInfos)
                {
                    var rreqs = method.GetCustomAttributes<IdentityRequirementsAttribute>()
                                      .Select<IdentityRequirementsAttribute, Func<MessageSource, bool>>(r => src => r.Check(src.User.Identity, src.Permission))
                                      .ToList();
                    Regex[] regexs = regexRouteInfos.Select(r => r.Regex).ToArray();
                    routes.Add(new RegexRouteInfo(this, method, regexs, rreqs.Any()
                                                                            ? src => rreqs.Any(p => p(src))
                                                                            : src => true));
                }

                if (method.GetCustomAttributes<TextRouteAttribute>().ToArray() is { Length: > 0 } textRouteInfos)
                {
                    var rreqs = method.GetCustomAttributes<IdentityRequirementsAttribute>()
                                      .Select<IdentityRequirementsAttribute, Func<MessageSource, bool>>(r => src => r.Check(src.User.Identity, src.Permission))
                                      .ToList();
                    string[] texts = textRouteInfos.Select(t => t.Text).ToArray();
                    bool[] ignoreCases = textRouteInfos.Select(t => t.IgnoreCase).ToArray();
                    routes.Add(new TextRouteInfo(this, method, texts, ignoreCases, rreqs.Any()
                                                                                       ? src => rreqs.Any(p => p(src))
                                                                                       : src => true));
                }
            }
        }

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
    }
}

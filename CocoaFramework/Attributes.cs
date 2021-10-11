// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;
using System.Text.RegularExpressions;
using Maila.Cocoa.Beans.Models;

namespace Maila.Cocoa.Framework
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Method |
        AttributeTargets.Field |
        AttributeTargets.Parameter)]
    public sealed class DisabledAttribute : Attribute { }

    #region === Behavior ===

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class BotModuleAttribute : Attribute
    {
        public string? Name { get; }
        public int Priority { get; set; } = 0;

        public BotModuleAttribute() { }
        public BotModuleAttribute(string? name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class IdentityRequirementsAttribute : Attribute
    {
        internal readonly UserIdentity? Identity;
        internal readonly GroupPermission? Permission;

        public IdentityRequirementsAttribute(UserIdentity identity)
        {
            Identity = identity;
        }
        public IdentityRequirementsAttribute(GroupPermission permission)
        {
            Permission = permission;
        }
        public IdentityRequirementsAttribute(UserIdentity identity, GroupPermission permission)
        {
            Identity = identity;
            Permission = permission;
        }

        internal bool Check(UserIdentity identity, GroupPermission? permission)
        {
            if (Identity is not null && !identity.Fit(Identity.Value))
            {
                return false;
            }

            if (Permission is not null && (permission is null || Permission.Value > permission.Value))
            {
                return false;
            }
            return true;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class DisableInGroupAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class DisableInPrivateAttribute : Attribute { }

    #endregion

    #region === Feature ===

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ThreadSafeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class HostingAttribute : Attribute { }

    #endregion

    #region === Route ===

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class RegexRouteAttribute : Attribute
    {
        public Regex Regex { get; }

        public RegexRouteAttribute(string pattern)
        {
            Regex = new(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        public RegexRouteAttribute(string pattern, RegexOptions options)
        {
            Regex = new(pattern, options | RegexOptions.Compiled);
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class TextRouteAttribute : Attribute
    {
        public string Text { get; }
        public bool IgnoreCase { get; set; } = true;

        public TextRouteAttribute(string text)
        {
            Text = text;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class AtRouteAttribute : Attribute
    {
        public string Text { get; }
        public bool IgnoreCase { get; set; } = true;

        public AtRouteAttribute(string text)
        {
            Text = text;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class GroupNameAttribute : Attribute
    {
        public string Name { get; }
        public string? Default { get; }

        public GroupNameAttribute(string name, string? @default = null)
        {
            Name = name;
            Default = @default;
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class MemoryOnlyAttribute : Attribute { }

    #endregion
}

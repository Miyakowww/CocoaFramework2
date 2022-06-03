// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Maila.Cocoa.Framework.Support
{
    public static class BotAuth
    {
        private static Dictionary<long, UserIdentity> identities = new();

        internal static void Init()
        {
            DataHosting.AddOptimizeEnabledHosting(
                typeof(BotAuth).GetField(nameof(identities), BindingFlags.Static | BindingFlags.NonPublic)!,
                null,
                $"BotAuth/{BotAPI.BotQQ}");
        }

        internal static void Reset()
        {
            identities = new();
        }

        /// <summary>
        /// Get user's identity.
        /// </summary>
        public static UserIdentity GetIdentity(long qqId)
            => identities.GetValueOrDefault(qqId, UserIdentity.User);

        /// <summary>
        /// Get all user's identity.
        /// </summary>
        public static KeyValuePair<long, UserIdentity>[] GetStoredIdentity()
            => identities.ToArray();

        /// <summary>
        /// Get all filtered user's identity.
        /// </summary>
        public static KeyValuePair<long, UserIdentity>[] GetStoredIdentity(UserIdentity filter)
            => identities.Where(p => p.Value.Fit(filter)).ToArray();

        /// <summary>
        /// Set user's identity.
        /// </summary>
        public static void SetIdentity(long qqId, UserIdentity identity)
            => identities[qqId] = identity;

        /// <summary>
        /// Append specified identity to the user
        /// </summary>
        public static UserIdentity AddIdentity(long qqId, UserIdentity identity)
            => identities[qqId] = identities.GetValueOrDefault(qqId, UserIdentity.User) | identity;

        /// <summary>
        /// Remove specified identity
        /// </summary>
        public static UserIdentity RemoveIdentity(long qqId, UserIdentity identity)
            => identities[qqId] = identities.GetValueOrDefault(qqId, UserIdentity.User) & ~identity;

        /// <summary>
        /// Set user's identity to User
        /// </summary>
        public static bool ClearIdentity(long qqId)
        {
            if (identities.ContainsKey(qqId))
            {
                identities.Remove(qqId);
                return true;
            }
            return false;
        }
    }
}

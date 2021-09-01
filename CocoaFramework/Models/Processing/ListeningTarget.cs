// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;

namespace Maila.Cocoa.Framework.Models.Processing
{
    public class ListeningTarget
    {
        internal Predicate<MessageSource> Pred;

        private ListeningTarget(long? groupId, long? userId)
        {
            Pred = src =>
            {
                if (src is null)
                {
                    return true;
                }
                if (groupId is null && userId is null)
                {
                    return true;
                }

                bool uFit = userId is null || userId == src.User.Id;
                bool gFit = groupId is null ? !src.IsGroup : groupId == src.Group?.Id;
                return gFit && uFit;
            };
        }
        private ListeningTarget(Predicate<MessageSource> pred)
        {
            Pred = pred;
        }

        public static ListeningTarget All { get; } = new(null, null);

        public static ListeningTarget FromGroup(long groupId) => new(groupId, null);
        public static ListeningTarget FromGroup(QGroup group) => new(group.Id, null);

        public static ListeningTarget FromUser(long userId) => new(null, userId);
        public static ListeningTarget FromUser(QUser user) => new(null, user.Id);

        public static ListeningTarget FromTarget(long groupId, long userId) => new(groupId, userId);
        public static ListeningTarget FromTarget(MessageSource src) => new(src.Group?.Id, src.User.Id);
        public static ListeningTarget CustomTarget(Predicate<MessageSource> pred) => new(pred);
    }
}

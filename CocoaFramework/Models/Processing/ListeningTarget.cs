// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

namespace Maila.Cocoa.Framework.Models.Processing
{
    public class ListeningTarget
    {
        internal long? Group { get; }
        internal long? User { get; }

        internal bool Fit(MessageSource? src)
        {
            bool gFit = Group is null || Group == src?.Group?.Id;
            bool uFit = User is null || User == src?.User.Id;
            return gFit && uFit;
        }

        private ListeningTarget(long? groupId, long? userId)
        {
            Group = groupId;
            User = userId;
        }

        public static ListeningTarget All { get; } = new(null, null);

        public static ListeningTarget FromGroup(long groupId) => new(groupId, null);
        public static ListeningTarget FromGroup(QGroup group) => new(group.Id, null);

        public static ListeningTarget FromUser(long userId) => new(null, userId);
        public static ListeningTarget FromUser(QUser user) => new(null, user.Id);

        public static ListeningTarget FromTarget(long groupId, long userId) => new(groupId, userId);
        public static ListeningTarget FromTarget(MessageSource src) => new(src.Group?.Id, src.User.Id);
    }
}

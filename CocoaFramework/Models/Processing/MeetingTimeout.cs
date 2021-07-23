// Copyright (c) Maila. All rights reserved.
// Licensed under the GNU AGPLv3

using System;

namespace Maila.Cocoa.Framework.Models.Processing
{
    public class MeetingTimeout
    {
        internal TimeSpan Duration { get; }

        private MeetingTimeout(TimeSpan duration)
        {
            Duration = duration;
        }

        public static MeetingTimeout Off { get; } = new(TimeSpan.Zero);

        public static MeetingTimeout FromTimeSpan(TimeSpan time)
        {
            if (time <= TimeSpan.Zero)
            {
                return Off;
            }
            return new(time);
        }
        public static MeetingTimeout FromMinutes(double minutes)
        {
            if (minutes <= 0)
            {
                return Off;
            }
            return new(TimeSpan.FromMinutes(minutes));
        }
        public static MeetingTimeout FromSeconds(double seconds)
        {
            if (seconds <= 0)
            {
                return Off;
            }
            return new(TimeSpan.FromSeconds(seconds));
        }
    }
}

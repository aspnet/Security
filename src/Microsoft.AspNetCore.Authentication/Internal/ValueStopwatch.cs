using System;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Authentication.Internal
{
    // REVIEW: This file will be in a common shared-source package.
    internal struct ValueStopwatch
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private long _startTimestamp;

        private ValueStopwatch(long startTimestamp)
        {
            _startTimestamp = startTimestamp;
        }

        public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

        // REVIEW: Using a method because I want it to be clear there are calculations involved each time.
        public TimeSpan GetElapsedTime()
        {
            // Start timestamp can't be zero. Like... it would have to be literally the first thing executed when the machine boots to be 0.
            if (_startTimestamp == 0)
            {
                throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used");
            }

            var end = Stopwatch.GetTimestamp();
            var timestampDelta = end - _startTimestamp;
            var ticks = (long)(TimestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
        }
    }
}

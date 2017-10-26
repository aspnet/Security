using System;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Authentication.Internal
{
    // REVIEW: This file will be in a common shared-source package.
    internal struct ValueStopwatch
    {
        private static readonly double TimestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        private bool _enabled;
        private long _endTimestamp;
        private long _startTimestamp;

        public TimeSpan Elapsed => ComputeElapsedTime();
        public bool Enabled => _enabled;

        private ValueStopwatch(long startTimestamp)
        {
            _enabled = true;
            _endTimestamp = 0;
            _startTimestamp = startTimestamp;
        }

        public void Stop()
        {
            // Start timestamp can't be zero. Like... it would have to be literally the first thing executed when the machine boots to be 0.
            if (_startTimestamp == 0)
            {
                throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used");
            }

            _endTimestamp = Stopwatch.GetTimestamp();
            _enabled = false;
        }

        public static ValueStopwatch StartNew() => new ValueStopwatch(Stopwatch.GetTimestamp());

        private TimeSpan ComputeElapsedTime()
        {
            // Start timestamp can't be zero. Like... it would have to be literally the first thing executed when the machine boots to be 0.
            if (_startTimestamp == 0)
            {
                throw new InvalidOperationException("An uninitialized, or 'default', ValueStopwatch cannot be used");
            }

            var end = _endTimestamp == 0 ? Stopwatch.GetTimestamp() : _endTimestamp;
            var timestampDelta = end - _startTimestamp;
            var ticks = (long)(TimestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
        }
    }

    internal static class EventSpan
    {
        public static EventSpan<T> Create<T>(T state, Action<T, TimeSpan> endAction) => new EventSpan<T>(endAction, state);
    }

    internal struct EventSpan<T>
    {
        private readonly Action<T, TimeSpan> _endAction;
        private readonly T _state;
        private ValueStopwatch _stopwatch;

        public EventSpan(Action<T, TimeSpan> endAction, T state)
        {
            _endAction = endAction;
            _state = state;
            _stopwatch = ValueStopwatch.StartNew();
        }

        public void End()
        {
            if(_endAction == null)
            {
                throw new InvalidOperationException("Cannot call '.End' on default(EventSpan<T>)!");
            }

            _stopwatch.Stop();
            _endAction(_state, _stopwatch.Elapsed);
        }
    }
}

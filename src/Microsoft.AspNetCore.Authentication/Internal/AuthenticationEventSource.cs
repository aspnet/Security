using System;
using System.Diagnostics.Tracing;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.Internal
{
    [EventSource(Name = "Microsoft-AspNetCore-Authentication")]
    public class AuthenticationEventSource : EventSource
    {
        public static readonly AuthenticationEventSource Log = new AuthenticationEventSource();
        private readonly EventCounter _authenticationMiddlewareDuration;

        private AuthenticationEventSource()
        {
            _authenticationMiddlewareDuration = new EventCounter("AuthenticationMiddlewareDuration", this);
        }

        [NonEvent]
        internal EventSpan<AuthenticationEventSource> AuthenticationMiddlewareStart(string traceIdentifier, PathString path)
        {
            if (IsEnabled(EventLevel.Informational, EventKeywords.None))
            {
                AuthenticationMiddlewareStart(traceIdentifier, path.Value);
            }

            return EventSpan.Create(this, (self, duration) => self.AuthenticationMiddlewareEnd(traceIdentifier, path, duration));
        }

        [NonEvent]
        internal void AuthenticationMiddlewareEnd(string traceIdentifier, PathString path, TimeSpan duration)
        {
            if (IsEnabled())
            {
                if (IsEnabled(EventLevel.Informational, EventKeywords.None))
                {
                    AuthenticationMiddlewareEnd(traceIdentifier, path.Value, duration.TotalMilliseconds);
                }

                _authenticationMiddlewareDuration.WriteMetric((float)duration.TotalMilliseconds);
            }
        }

        [NonEvent]
        internal void AuthenticationMiddlewareFailure(string traceIdentifier, PathString path, Exception ex)
        {
            if(IsEnabled(EventLevel.Error, EventKeywords.None))
            {
                AuthenticationMiddlewareFailure(traceIdentifier, path.Value, ex.GetType().FullName, ex.Message, ex.ToString());
            }
        }

        [Event(eventId: 1, Level = EventLevel.Informational)]
        private void AuthenticationMiddlewareStart(string traceIdentifier, string path) => WriteEvent(1, traceIdentifier, path);

        [Event(eventId: 2, Level = EventLevel.Informational)]
        private void AuthenticationMiddlewareEnd(string traceIdentifier, string path, double durationMilliseconds) => WriteEvent(2, traceIdentifier, path, durationMilliseconds);

        [Event(eventId: 3, Level = EventLevel.Error)]
        private void AuthenticationMiddlewareFailure(string traceIdentifier, string value, string exceptionTypeName, string message, string fullException) => WriteEvent(3, traceIdentifier, value, exceptionTypeName, message, fullException);
    }
}

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
        internal void AuthenticationMiddlewareStart(HttpContext context)
        {
            if (IsEnabled(EventLevel.Informational, EventKeywords.None))
            {
                AuthenticationMiddlewareStart(context.TraceIdentifier, context.Request.Path.Value);
            }
        }

        [NonEvent]
        internal void AuthenticationMiddlewareEnd(HttpContext context, TimeSpan duration)
        {
            if (IsEnabled())
            {
                if (IsEnabled(EventLevel.Informational, EventKeywords.None))
                {
                    AuthenticationMiddlewareEnd(context.TraceIdentifier, context.Request.Path.Value, duration.TotalMilliseconds);
                }

                _authenticationMiddlewareDuration.WriteMetric((float)duration.TotalMilliseconds);
            }
        }

        [NonEvent]
        internal void AuthenticationMiddlewareFailure(HttpContext context, Exception ex)
        {
            if(IsEnabled(EventLevel.Error, EventKeywords.None))
            {
                AuthenticationMiddlewareFailure(context.TraceIdentifier, context.Request.Path.Value, ex.GetType().FullName, ex.Message, ex.ToString());
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

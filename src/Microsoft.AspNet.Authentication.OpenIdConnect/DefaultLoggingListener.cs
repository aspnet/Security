using System;
using System.Diagnostics.Tracing;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Authentication.OpenIdConnect
{
    /// <summary>
    /// Default logging event listener for wilson event source. It logs the data to the target where other Asp.Net logs go.
    /// </summary>
    public class DefaultLoggingListener : EventListener
    {
        private ILogger _logger;

        public DefaultLoggingListener(ILogger logger)
        {
            _logger = logger;
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            // calling Asp.Net logger to log events
            _logger.Log(MapEventLevelToLogLevel(eventData.Level), eventData.EventId, eventData.Payload[0], null, null);
        }

        private LogLevel MapEventLevelToLogLevel(EventLevel level)
        {
            LogLevel logLevel = LogLevel.Information;

            switch(level)
            {
                case EventLevel.Critical: logLevel = LogLevel.Critical;
                    break;
                case EventLevel.Error: logLevel = LogLevel.Error;
                    break;
                case EventLevel.Warning: logLevel = LogLevel.Warning;
                    break;
                case EventLevel.Informational: logLevel = LogLevel.Information;
                    break;
                case EventLevel.Verbose: logLevel = LogLevel.Verbose;
                    break;
                case EventLevel.LogAlways: logLevel = LogLevel.Debug;
                    break;
            }
            return logLevel;
        }
    }
}
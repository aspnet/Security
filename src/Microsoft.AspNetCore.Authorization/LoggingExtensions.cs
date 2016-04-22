// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Authorization;
using System;

namespace Microsoft.Extensions.Logging
{
    internal static class LoggingExtensions
    {
        private static Action<ILogger, string, Exception> _authorizationFailed;
        private static Action<ILogger, string, Exception> _authorizationSucceeded;

        static LoggingExtensions()
        {
            _authorizationSucceeded = LoggerMessage.Define<string>(
                eventId: 1,
                logLevel: LogLevel.Information,
                formatString: "Authorization was successful for {AuthorizationTarget}.");
            _authorizationFailed = LoggerMessage.Define<string>(
                eventId: 2,
                logLevel: LogLevel.Information,
                formatString: "Authorization failed for {AuthorizationTarget}.");
        }

        public static void AuthorizationSucceeded(this ILogger logger, AuthorizationContext context)
        {
            _authorizationSucceeded(logger, GetAuthTarget(context), null);
        }

        public static void AuthorizationFailed(this ILogger logger, AuthorizationContext context)
        {
            _authorizationFailed(logger, GetAuthTarget(context), null);
        }

        private static string GetAuthTarget(AuthorizationContext context)
        {
            var typeName = context.AuthorizationData?.GetType().Name;
            if (string.IsNullOrWhiteSpace(typeName) && string.IsNullOrWhiteSpace(context.Identifier))
            {
                return nameof(context.AuthorizationData);
            }

            return string.IsNullOrEmpty(context.Identifier) ? typeName : context.Identifier;
        }
    }
}

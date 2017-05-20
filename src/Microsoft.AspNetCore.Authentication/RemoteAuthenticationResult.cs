// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Contains the result of an Authenticate call
    /// </summary>
    public class RemoteAuthenticationResult : AuthenticationResult
    {
        /// <summary>
        /// Indicates that stage of authentication was directly handled by user intervention and no
        /// further processing should be attempted.
        /// </summary>
        public bool HandledResponse { get; private set; }

        /// <summary>
        /// Indicates that the default authentication logic should be
        /// skipped and that the rest of the pipeline should be invoked.
        /// </summary>
        public bool Skipped { get; private set; }

        /// <summary>
        /// Indicates that authentication was successful.
        /// </summary>
        /// <param name="ticket">The ticket representing the authentication result.</param>
        /// <returns>The result.</returns>
        public static new RemoteAuthenticationResult Success(AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }
            return new RemoteAuthenticationResult() { Ticket = ticket };
        }

        /// <summary>
        /// Indicates that there was a failure during authentication.
        /// </summary>
        /// <param name="failure">The failure exception.</param>
        /// <returns>The result.</returns>
        public static new RemoteAuthenticationResult Fail(Exception failure)
        {
            return new RemoteAuthenticationResult() { Failure = failure };
        }

        /// <summary>
        /// Indicates that there was a failure during authentication.
        /// </summary>
        /// <param name="failureMessage">The failure message.</param>
        /// <returns>The result.</returns>
        public static new RemoteAuthenticationResult Fail(string failureMessage)
        {
            return new RemoteAuthenticationResult() { Failure = new Exception(failureMessage) };
        }

        /// <summary>
        /// Indicates that there was no information returned for this authentication scheme.
        /// </summary>
        /// <returns>The result.</returns>
        public static new RemoteAuthenticationResult None()
        {
            return new RemoteAuthenticationResult() { Nothing = true };
        }

        /// <summary>
        /// Indicates that stage of authentication was directly handled by user intervention and no
        /// further processing should be attempted.
        /// </summary>
        /// <returns>The result.</returns>
        public static RemoteAuthenticationResult HandleResponse()
        {
            return new RemoteAuthenticationResult() { HandledResponse = true };
        }

        /// <summary>
        /// Indicates that the default authentication logic should be
        /// skipped and that the rest of the pipeline should be invoked.
        /// </summary>
        /// <returns>The result.</returns>
        public static RemoteAuthenticationResult SkipToNextMiddleware()
        {
            return new RemoteAuthenticationResult() { Skipped = true };
        }
    }
}

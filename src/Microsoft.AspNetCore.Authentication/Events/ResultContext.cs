// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Base context for authentication.
    /// </summary>
    public abstract class ResultContext<THandler> : HandlerContext<THandler> where THandler : IAuthenticationHandler
    {
        private AuthenticationProperties _properties;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="handler">The authentication handler.</param>
        /// <param name="context">The context.</param>
        protected ResultContext(THandler handler, HttpContext context) : base(handler, context)
        { }

        /// <summary>
        /// Gets or set the <see cref="AuthenticationTicket"/> containing
        /// the user principal and the authentication properties.
        /// </summary>
        public AuthenticationTicket Ticket { get; set; }

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> containing the user claims.
        /// </summary>
        public ClaimsPrincipal Principal => Ticket?.Principal;

        /// <summary>
        /// Gets or sets the <see cref="AuthenticationProperties"/>.
        /// </summary>
        public AuthenticationProperties Properties
        {
            get => _properties ?? Ticket?.Properties;
            set => _properties = value;
        }

        public bool AuthenticationSkipped { get; private set; }

        public Exception Failure { get; private set; }

        public void CompleteAuthentication(AuthenticationTicket ticket)
        {
            Ticket = ticket;
            AuthenticationSkipped = true;
        }

        public void SkipAuthentication()
        {
            AuthenticationSkipped = true;
        }

        public void RejectAuthentication(Exception failure)
        {
            Failure = failure;
        }

        public void RejectAuthentication(string failureMessage)
        {
            Failure = new Exception(failureMessage);
        }

        public bool IsProcessingComplete(out AuthenticateResult result)
        {
            if (AuthenticationSkipped)
            {
                if (Ticket == null)
                {
                    result = AuthenticateResult.None();
                }
                else
                {
                    result = AuthenticateResult.Success(Ticket);
                }
                return true;
            }
            else if (Failure != null)
            {
                result = AuthenticateResult.Fail(Failure);
                return true;
            }
            result = null;
            return false;
        }
    }
}

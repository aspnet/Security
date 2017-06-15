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
    public abstract class AuthenticateResultContext<TOptions> : BaseContext<TOptions> where TOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="scheme">The authentication scheme.</param>
        /// <param name="options">The authentication options associated with the scheme.</param>
        protected AuthenticateResultContext(
            HttpContext context,
            AuthenticationScheme scheme,
            TOptions options)
            : base(context, scheme, options)
        {
        }

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
        public override AuthenticationProperties Properties
        {
            get => Ticket?.Properties ?? base.Properties;

            set
            {
                if (Ticket != null)
                {
                    Ticket = new AuthenticationTicket(Principal, value, Scheme.Name);
                }

                base.Properties = value;
            }
        }

        public Exception Failure { get; private set; }

        public void CompleteAuthentication(AuthenticationTicket ticket)
        {
            State = EventResultState.BypassDefaultLogic;
            Ticket = ticket;
        }

        public void SkipAuthentication()
        {
            State = EventResultState.BypassDefaultLogic;
            Ticket = null;
        }

        public void RejectAuthentication(Exception failure)
        {
            State = EventResultState.BypassDefaultLogic;
            Failure = failure;
            Ticket = null;
        }

        public void RejectAuthentication(string failureMessage)
        {
            State = EventResultState.BypassDefaultLogic;
            Failure = new Exception(failureMessage);
            Ticket = null;
        }

        public bool IsProcessingComplete(out AuthenticateResult result)
        {
            if (State == EventResultState.BypassDefaultLogic)
            {
                if (Failure != null)
                {
                    result = AuthenticateResult.Fail(Failure);
                }
                else if (Ticket != null)
                {
                    result = AuthenticateResult.Success(Ticket);
                }
                else
                {
                    result = AuthenticateResult.None();
                }
                return true;
            }
            result = null;
            return false;
        }
    }
}

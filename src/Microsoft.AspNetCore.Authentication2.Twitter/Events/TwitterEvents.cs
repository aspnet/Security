// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Authentication2.Twitter
{
    /// <summary>
    /// Default <see cref="TwitterEvents"/> implementation.
    /// </summary>
    public class TwitterEvents : RemoteAuthenticationEvents
    {
        /// <summary>
        /// Gets or sets the function that is invoked when the Authenticated method is invoked.
        /// </summary>
        public Func<TwitterCreatingTicketContext, Task> OnCreatingTicket { get; set; } = context => TaskCache.CompletedTask;

        /// <summary>
        /// Gets or sets the delegate that is invoked when the ApplyRedirect method is invoked.
        /// </summary>
        public Func<TwitterRedirectToAuthorizationEndpointContext, Task> OnRedirectToAuthorizationEndpoint { get; set; } = context =>
        {
            context.Response.Redirect(context.RedirectUri);
            return TaskCache.CompletedTask;
        };

        /// <summary>
        /// Invoked whenever Twitter successfully authenticates a user
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        public virtual Task CreatingTicket(TwitterCreatingTicketContext context) => OnCreatingTicket(context);

        /// <summary>
        /// Called when a Challenge causes a redirect to authorize endpoint in the Twitter middleware
        /// </summary>
        /// <param name="context">Contains redirect URI and <see cref="Http.Authentication.AuthenticationProperties"/> of the challenge </param>
        public virtual Task RedirectToAuthorizationEndpoint(TwitterRedirectToAuthorizationEndpointContext context) => OnRedirectToAuthorizationEndpoint(context);
    }
}

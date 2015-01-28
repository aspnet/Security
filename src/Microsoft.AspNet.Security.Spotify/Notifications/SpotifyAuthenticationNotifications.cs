// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Security.OAuth;

namespace Microsoft.AspNet.Security.Spotify
{
    /// <summary>
    /// The default <see cref="ISpotifyAuthenticationNotifications"/> implementation.
    /// </summary>
    public class SpotifyAuthenticationNotifications : OAuthAuthenticationNotifications, ISpotifyAuthenticationNotifications
    {
        /// <summary>
        /// Initializes a new <see cref="SpotifyAuthenticationNotifications"/>.
        /// </summary>
        public SpotifyAuthenticationNotifications()
        {
            OnAuthenticated = context => Task.FromResult<object>(null);
        }

        /// <summary>
        /// Gets or sets the function that is invoked when the Authenticated method is invoked.
        /// </summary>
        public Func<SpotifyAuthenticatedContext, Task> OnAuthenticated { get; set; }

        /// <summary>
        /// Invoked whenever Spotify succesfully authenticates a user.
        /// </summary>
        /// <param name="context">Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.</param>
        /// <returns>A <see cref="Task"/> representing the completed operation.</returns>
        public virtual Task Authenticated(SpotifyAuthenticatedContext context)
        {
            return OnAuthenticated(context);
        }
    }
}
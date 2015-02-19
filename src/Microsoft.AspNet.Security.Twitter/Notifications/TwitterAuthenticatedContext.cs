﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Security.Notifications;

namespace Microsoft.AspNet.Security.Twitter
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    public class TwitterAuthenticatedContext : BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="TwitterAuthenticatedContext"/>
        /// </summary>
        /// <param name="context">The HTTP environment</param>
        /// <param name="userId">Twitter user ID</param>
        /// <param name="screenName">Twitter screen name</param>
        /// <param name="accessToken">Twitter access token</param>
        /// <param name="accessTokenSecret">Twitter access token secret</param>
        public TwitterAuthenticatedContext(
            HttpContext context,
            string userId,
            string screenName,
            string accessToken,
            string accessTokenSecret)
            : base(context)
        {
            UserId = userId;
            ScreenName = screenName;
            AccessToken = accessToken;
            AccessTokenSecret = accessTokenSecret;
        }

        /// <summary>
        /// Gets the Twitter user ID
        /// </summary>
        public string UserId { get; private set; }

        /// <summary>
        /// Gets the Twitter screen name
        /// </summary>
        public string ScreenName { get; private set; }

        /// <summary>
        /// Gets the Twitter access token
        /// </summary>
        public string AccessToken { get; private set; }

        /// <summary>
        /// Gets the Twitter access token secret
        /// </summary>
        public string AccessTokenSecret { get; private set; }

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> representing the user
        /// </summary>
        public ClaimsPrincipal Principal { get; set; }

        /// <summary>
        /// Gets or sets a property bag for common authentication properties
        /// </summary>
        public AuthenticationProperties Properties { get; set; }
    }
}

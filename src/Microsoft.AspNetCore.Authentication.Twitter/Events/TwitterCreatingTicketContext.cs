// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Authentication.Twitter
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    public class TwitterCreatingTicketContext : BaseTwitterContext
    {
        /// <summary>
        /// Initializes a <see cref="TwitterCreatingTicketContext"/>
        /// </summary>
        /// <param name="context">The HTTP environment</param>
        /// <param name="scheme">The scheme data</param>
        /// <param name="options">The options for Twitter</param>
        /// <param name="userId">Twitter user ID</param>
        /// <param name="screenName">Twitter screen name</param>
        /// <param name="accessToken">Twitter access token</param>
        /// <param name="accessTokenSecret">Twitter access token secret</param>
        /// <param name="user">User details</param>
        /// <param name="properties">AuthenticationProperties.</param>
        public TwitterCreatingTicketContext(
            HttpContext context,
            AuthenticationScheme scheme,
            TwitterOptions options,
            AuthenticationProperties properties,
            string userId,
            string screenName,
            string accessToken,
            string accessTokenSecret,
            JObject user)
            : base(context, scheme, options, properties)
        {
            UserId = userId;
            ScreenName = screenName;
            AccessToken = accessToken;
            AccessTokenSecret = accessTokenSecret;
            User = user ?? new JObject();
        }

        /// <summary>
        /// Gets the Twitter user ID
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Gets the Twitter screen name
        /// </summary>
        public string ScreenName { get; }

        /// <summary>
        /// Gets the Twitter access token
        /// </summary>
        public string AccessToken { get; }

        /// <summary>
        /// Gets the Twitter access token secret
        /// </summary>
        public string AccessTokenSecret { get; }

        /// <summary>
        /// Gets the JSON-serialized user or an empty
        /// <see cref="JObject"/> if it is not available.
        /// </summary>
        public JObject User { get; }

        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> representing the user
        /// </summary>
        public ClaimsPrincipal Principal { get; set; }
    }
}

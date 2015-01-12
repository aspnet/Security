// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Security.Notifications;

namespace Microsoft.AspNet.Security.OAuth
{
    /// <summary>
    /// Contains the context for 'OAuth bearer' authentication.
    /// </summary>
    public class OAuthBearerTokenContext : BaseContext
    {
        /// <summary>
        /// Initializes a new <see cref="OAuthBearerTokenContext"/>
        /// </summary>
        /// <param name="context">HTTP environment</param>
        /// <param name="token">The authorization header value.</param>
        public OAuthBearerTokenContext(
            HttpContext context,
            string token)
            : base(context)
        {
            Token = token;
        }

        /// <summary>
        /// The authorization header value
        /// </summary>
        public string Token { get; set; }
    }
}

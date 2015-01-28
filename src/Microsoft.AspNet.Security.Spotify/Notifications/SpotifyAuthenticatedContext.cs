// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Security.OAuth;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.Security.Spotify.Notifications
{
    public class SpotifyAuthenticatedContext : OAuthAuthenticatedContext
    {
        /// <summary>
        /// Initializes a new <see cref="SpotifyAuthenticatedContext"/>.
        /// </summary>
        /// <param name="context">The HTTP environment.</param>
        /// <param name="user">The JSON-serialized user.</param>
        /// <param name="tokens">The Spotify Access token.</param>
        public SpotifyAuthenticatedContext(HttpContext context, OAuthAuthenticationOptions options, JObject user, TokenResponse tokens)
            : base(context, options, user, tokens)
        {
            Id = TryGetValue(user, "id");
            DisplayName = TryGetValue(user, "display_name");
            ApiLink = TryGetValue(user, "href");
            SpotifyLink = TryGetValue(user, "uri");

            JToken externalUrls;
            user.TryGetValue("external_urls", out externalUrls);
            ExternalLink = externalUrls != null ? externalUrls.Value<string>("spotify") : null;
        }

        /// <summary>
        /// Gets the Spotify user ID.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Gets the name displayed on the user's profile.
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets the external link for the user
        /// </summary>
        public string ExternalLink { get; private set; }

        /// <summary>
        /// Gets the user's link to the Web API endpoint
        /// </summary>
        public string ApiLink { get; private set; }

        /// <summary>
        /// Gets the Spotify URI for the user
        /// </summary>
        public string SpotifyLink { get; private set; }

        private static string TryGetValue(JObject user, string propertyName)
        {
            JToken value;
            return user.TryGetValue(propertyName, out value) ? value.ToString() : null;
        }
    }
}
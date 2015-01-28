// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Security.OAuth;

namespace Microsoft.AspNet.Security.Spotify
{
    /// <summary>
    /// Configuration options for <see cref="SpotifyAuthenticationMiddleware"/>.
    /// </summary>
    public class SpotifyAuthenticationOptions : OAuthAuthenticationOptions<SpotifyAuthenticationNotifications>
    {
        /// <summary>
        /// Initializes a new <see cref="SpotifyAuthenticationOptions"/>.
        /// </summary>
        public SpotifyAuthenticationOptions()
        {
            AuthenticationType = SpotifyAuthenticationDefaults.AuthenticationType;
            Caption = AuthenticationType;
            CallbackPath = new PathString("/signin-spotify");
            AuthorizationEndpoint = SpotifyAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = SpotifyAuthenticationDefaults.TokenEndpoint;
            UserInformationEndpoint = SpotifyAuthenticationDefaults.UserInformationEndpoint;
        }

        /// <summary>
        /// Gets or sets whether or not to force the user to approve the app again if they’ve already done so
        /// </summary>
        public bool ForceDialog { get; set; }
    }
}
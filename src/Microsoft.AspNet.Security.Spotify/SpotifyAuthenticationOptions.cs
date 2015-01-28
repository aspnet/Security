// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Security.OAuth;
using Microsoft.AspNet.Security.Spotify.Notifications;

namespace Microsoft.AspNet.Security.Spotify
{
    public class SpotifyAuthenticationOptions : OAuthAuthenticationOptions<SpotifyAuthenticationNotifications>
    {
        public bool ShowDialog { get; set; }

        public SpotifyAuthenticationOptions()
        {
            AuthenticationType = SpotifyAuthenticationDefaults.AuthenticationType;
            Caption = AuthenticationType;
            CallbackPath = new PathString("/signin-spotify");
            AuthorizationEndpoint = SpotifyAuthenticationDefaults.AuthorizationEndpoint;
            TokenEndpoint = SpotifyAuthenticationDefaults.TokenEndpoint;
            UserInformationEndpoint = SpotifyAuthenticationDefaults.UserInformationEndpoint;
        }
    }
}
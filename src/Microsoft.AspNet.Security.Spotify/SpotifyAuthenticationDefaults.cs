// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Security.Spotify
{
    public static class SpotifyAuthenticationDefaults
    {
        public const string AuthenticationType = "Spotify";
        public const string AuthorizationEndpoint = "https://accounts.spotify.com/authorize";
        public const string TokenEndpoint = "https://accounts.spotify.com/api/token";
        public const string UserInformationEndpoint = "https://api.spotify.com/v1/me";
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Authentication.Google
{
    public static class GoogleDefaults
    {
        public const string AuthenticationScheme = "Google";

        public static readonly string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/auth";

        public static readonly string TokenEndpoint = "https://accounts.google.com/o/oauth2/token";

        public static readonly string UserInformationEndpoint = "https://www.googleapis.com/plus/v1/people/me";
    }
}

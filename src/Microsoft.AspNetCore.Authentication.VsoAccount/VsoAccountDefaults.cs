// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Authentication.VsoAccount
{
    public static class VsoAccountDefaults
    {
        public const string AuthenticationScheme = "Vso";

        public static readonly string DisplayName = "Vso";

        public static readonly string AuthorizationEndpoint = "https://app.vssps.visualstudio.com/oauth2/authorize?";

        public static readonly string TokenEndpoint = "https://app.vssps.visualstudio.com/oauth2/token?mkt=en-US";

        public static readonly string CallbackEndpoint = "sigin-vso";
    }
}

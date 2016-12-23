// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication2.OAuth;

namespace Microsoft.AspNetCore.Authentication2.MicrosoftAccount
{
    /// <summary>
    /// Configuration options for <see cref="MicrosoftAccountHandler"/>.
    /// </summary>
    public class MicrosoftAccountOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="MicrosoftAccountOptions"/>.
        /// </summary>
        public MicrosoftAccountOptions()
        {
            AuthenticationScheme = MicrosoftAccountDefaults.AuthenticationScheme;
            DisplayName = AuthenticationScheme;
            CallbackPath = new PathString("/signin-microsoft");
            AuthorizationEndpoint = MicrosoftAccountDefaults.AuthorizationEndpoint;
            TokenEndpoint = MicrosoftAccountDefaults.TokenEndpoint;
            UserInformationEndpoint = MicrosoftAccountDefaults.UserInformationEndpoint;
            Scope.Add("https://graph.microsoft.com/user.read");
        }
    }
}

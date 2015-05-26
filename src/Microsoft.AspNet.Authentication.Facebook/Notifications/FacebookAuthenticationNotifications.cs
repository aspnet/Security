// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Authentication.OAuth;

namespace Microsoft.AspNet.Authentication.Facebook
{
    /// <summary>
    /// The default <see cref="IFacebookAuthenticationNotifications"/> implementation.
    /// </summary>
    public class FacebookAuthenticationNotifications : OAuthAuthenticationSquaredNotifications<FacebookAuthenticatedContext>, IOAuthAuthenticationSquaredNotifications<FacebookAuthenticatedContext>
    {
    }
}

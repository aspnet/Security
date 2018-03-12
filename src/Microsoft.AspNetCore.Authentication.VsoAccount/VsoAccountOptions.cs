// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.OAuth;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Web;

namespace Microsoft.AspNetCore.Authentication.VsoAccount
{
    /// <summary>
    /// Configuration options for <see cref="VsoAccountHandler"/>.
    /// </summary>
    public class VsoAccountOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="VsoAccountOptions"/>.
        /// </summary>
        public VsoAccountOptions()
        {
            CallbackPath = new PathString("/signin-vso");
            AuthorizationEndpoint = VsoAccountDefaults.AuthorizationEndpoint;
            TokenEndpoint = VsoAccountDefaults.TokenEndpoint;
    
            Events = new OAuthEvents
                {
                    OnRedirectToAuthorizationEndpoint = context =>
                    {
                        var uriBuilder = new UriBuilder(context.RedirectUri);
                        var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                        query["response_type"] = "Assertion";
                        uriBuilder.Query = query.ToString();
                        context.Response.Redirect(uriBuilder.ToString());
                        return Task.CompletedTask;
                    }
                };
        }
    }
}

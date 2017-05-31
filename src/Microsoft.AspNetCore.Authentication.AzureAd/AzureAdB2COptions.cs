// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.AspNetCore.Authentication.AzureAd
{
    public class AzureAdB2COptions : OpenIdConnectOptions
    {
        public AzureAdB2COptions()
        {
            TokenValidationParameters = new TokenValidationParameters() { NameClaimType = "name" };

            Events = new OpenIdConnectEvents()
            {
                OnRemoteFailure = context =>
                {
                    context.HandleResponse();
                    // Handle the error code that Azure AD B2C throws when trying to reset a password from the login page 
                    // because password reset is not supported by a "sign-up or sign-in policy"
                    if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("AADB2C90118"))
                    {
                        // If the user clicked the reset password link, redirect to the reset password route
                        context.Response.Redirect("/Account/ResetPassword");
                    }
                    else if (context.Failure is OpenIdConnectProtocolException && context.Failure.Message.Contains("access_denied"))
                    {
                        context.Response.Redirect("/");
                    }
                    else
                    {
                        context.Response.Redirect("/Home/Error");
                    }
                    return Task.FromResult(0);
                }
            };
        }

        public string Instance { get; set; }
        public string Domain { get; set; }
        public string PolicyId { get; set; }
        public string EditProfilePolicyId { get; set; }
        public string SignInSignUpPolicyId { get; set; }
        public string ResetPasswordPolicyId { get; set; }
    }
}
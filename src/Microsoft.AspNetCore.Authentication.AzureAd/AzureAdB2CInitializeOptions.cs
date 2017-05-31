// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.AzureAd
{
    internal class AzureAdB2CInitializeOptions : IInitializeOptions<AzureAdB2COptions>
    {

        public AzureAdB2CInitializeOptions() { }

        public void Initialize(string name, AzureAdB2COptions options)
        {
            if (string.IsNullOrEmpty(options.Authority))
            {
                // Bind to only to any of the AzureAdB2C policy schemes
                if (name == AzureDefaults.AzureAdB2CEditProfileAuthenticationScheme)
                {
                    options.Authority = $"{options.Instance}/{options.Domain}/{options.EditProfilePolicyId}/v2.0";
                }
                else if (name == AzureDefaults.AzureAdB2CResetPasswordAuthenticationScheme)
                {
                    options.Authority = $"{options.Instance}/{options.Domain}/{options.ResetPasswordPolicyId}/v2.0";
                }
                else if (name == AzureDefaults.AzureAdB2CSignInSignUpAuthenticationScheme)
                {
                    options.Authority = $"{options.Instance}/{options.Domain}/{options.SignInSignUpPolicyId}/v2.0";
                }
            }
        }
    }
}

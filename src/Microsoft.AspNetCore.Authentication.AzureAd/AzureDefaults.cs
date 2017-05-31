// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Authentication.AzureAd
{
    /// <summary>
    /// Default values for AzureAd and AzureAdB2C authentication
    /// </summary>
    public static class AzureDefaults
    {
        public const string AzureAdAuthenticationScheme = "AzureAd";
        public const string AzureAdB2CAuthenticationScheme = "AzureAdB2C";
        public const string AzureAdB2CSignInSignUpAuthenticationScheme = "AzureAdB2C.SignInUp";
        public const string AzureAdB2CResetPasswordAuthenticationScheme = "AzureAdB2C.ResetPassword";
        public const string AzureAdB2CEditProfileAuthenticationScheme = "AzureAdB2C.EditProfile";
    }
}

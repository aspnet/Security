// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Authentication
{
    public static class TokenExtensions
    {
        public static string Get(this ITokenStore store, string authenticationScheme, string userId, string tokenType)
        {
            return store?.Get($"{authenticationScheme};{userId};{tokenType}");
        }

        public static void Set(this ITokenStore store, string authenticationScheme, string userId, string tokenType, string value)
        {
            if (store == null)
            {
                throw new ArgumentNullException(nameof(store));
            }
            if (string.IsNullOrEmpty(authenticationScheme))
            {
                throw new ArgumentException("", nameof(authenticationScheme));
            }
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("", nameof(userId));
            }
            if (string.IsNullOrEmpty(tokenType))
            {
                throw new ArgumentException("", nameof(tokenType));
            }
            store.Set($"{authenticationScheme};{userId};{tokenType}", value);
        }

        public static string Get(this ITokenStore store, string authenticationScheme, ClaimsPrincipal user, string tokenType)
        {
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return null;
            }
            return store.Get(authenticationScheme, userId, tokenType);
        }

        public static void Set(this ITokenStore store, string authenticationScheme, ClaimsPrincipal user, string tokenType, string value)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var claim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null)
            {
                throw new ArgumentException("The user does not have the expected claim: " + ClaimTypes.NameIdentifier, nameof(user));
            }
            var userId = claim.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("The id claim does not have a value.", nameof(user));
            }
            store.Set(authenticationScheme, userId, tokenType, value);
        }

        public static string GetToken(this AuthenticationManager authManager, string authenticationScheme, string tokenType)
        {
            var store = authManager.HttpContext.RequestServices.GetService<ITokenStore>();
            return store.Get(authenticationScheme, authManager.HttpContext.User, tokenType);
        }

        public static void SetToken(this AuthenticationManager authManager, string authenticationScheme, string tokenType, string value)
        {
            var store = authManager.HttpContext.RequestServices.GetRequiredService<ITokenStore>();
            store.Set(authenticationScheme, authManager.HttpContext.User, tokenType, value);
        }

        public static string GetAccessToken(this AuthenticationManager authManager, string authenticationScheme)
        {
            return authManager.GetToken(authenticationScheme, "access_token");
        }

        public static void SetAccessToken(this AuthenticationManager authManager, string authenticationScheme, string value)
        {
            authManager.SetToken(authenticationScheme, "access_token", value);
        }

        public static string GetRefreshToken(this AuthenticationManager authManager, string authenticationScheme)
        {
            return authManager.GetToken(authenticationScheme, "refresh_token");
        }

        public static DateTimeOffset? GetExpiresAt(this AuthenticationManager authManager, string authenticationScheme)
        {
            DateTimeOffset result;
            return DateTimeOffset.TryParse(authManager.GetToken(authenticationScheme, "expires_at"),
                CultureInfo.InvariantCulture, DateTimeStyles.None, out result) ? result : (DateTimeOffset?)null;
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Authentication.MicrosoftAccount
{
    /// <summary>
    /// Contains static methods that allow to extract user's information from a <see cref="JObject"/>
    /// instance retrieved from Microsoft after a successful authentication process.
    /// http://graph.microsoft.io/en-us/docs/api-reference/v1.0/resources/user
    /// </summary>
    public static class MicrosoftAccountHelper
    {
        /// <summary>
        /// Gets the Microsoft Account user ID.
        /// </summary>
        public static string GetId(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return user.Value<string>("id");
        }

        /// <summary>
        /// Gets the user's name.
        /// </summary>
        public static string GetName(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            
            //displayName is required for Azure AD accounts, but not for Microsoft accounts.
            //givenName & surename is required for Microsoft accounts, but not for Azure AD accounts
            //In case of microsoft accounts displayName is null => fallback to givenName + surename
            var name = user.Value<string>("displayName");
            if (string.IsNullOrEmpty(name))
            {
                name = string.Format("{0} {1}", GetFirstName(user), GetLastName(user));
            }

            return name;
        }

        /// <summary>
        /// Gets the user's first name.
        /// </summary>
        public static string GetFirstName(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return user.Value<string>("givenName");
        }

        /// <summary>
        /// Gets the user's last name.
        /// </summary>
        public static string GetLastName(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return user.Value<string>("surname");
        }

        /// <summary>
        /// Gets the user's email address.
        /// </summary>
        public static string GetEmail(JObject user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            return user.Value<string>("mail") ?? user.Value<string>("userPrincipalName");
        }
    }
}

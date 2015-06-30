// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Framework.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNet.Authentication.Google
{
    /// <summary>
    /// Contains static methods that allow to extract user's information from a <see cref="JObject"/>
    /// instance retrieved from Google after a successful authentication process.
    /// </summary>
    public static class GoogleAuthenticationHelper
    {
        /// <summary>
        /// Gets the Google user ID.
        /// </summary>
        public static string GetId([NotNull] JObject user) => user.Value<string>("id");

        /// <summary>
        /// Gets the user's name.
        /// </summary>
        public static string GetName([NotNull] JObject user) => user.Value<string>("displayName");

        /// <summary>
        /// Gets the user's given name.
        /// </summary>
        public static string GetGivenName([NotNull] JObject user) => TryGetValue(user, "name", "givenName");

        /// <summary>
        /// Gets the user's family name.
        /// </summary>
        public static string GetFamilyName([NotNull] JObject user) => TryGetValue(user, "name", "familyName");

        /// <summary>
        /// Gets the user's profile link.
        /// </summary>
        public static string GetProfile([NotNull] JObject user) => user.Value<string>("url");

        /// <summary>
        /// Gets the user's email.
        /// </summary>
        public static string GetEmail([NotNull] JObject user) => TryGetFirstValue(user, "emails", "value");

        // Get the given subProperty from a property.
        private static string TryGetValue(JObject user, string propertyName, string subProperty)
        {
            JToken value;
            if (user.TryGetValue(propertyName, out value))
            {
                var subObject = JObject.Parse(value.ToString());
                if (subObject != null && subObject.TryGetValue(subProperty, out value))
                {
                    return value.ToString();
                }
            }
            return null;
        }

        // Get the given subProperty from a list property.
        private static string TryGetFirstValue(JObject user, string propertyName, string subProperty)
        {
            JToken value;
            if (user.TryGetValue(propertyName, out value))
            {
                var array = JArray.Parse(value.ToString());
                if (array != null && array.Count > 0)
                {
                    var subObject = JObject.Parse(array.First.ToString());
                    if (subObject != null)
                    {
                        if (subObject.TryGetValue(subProperty, out value))
                        {
                            return value.ToString();
                        }
                    }
                }
            }
            return null;
        }
    }
}

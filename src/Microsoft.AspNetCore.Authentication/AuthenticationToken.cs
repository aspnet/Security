// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;

namespace Microsoft.AspNetCore.Authentication
{
    public class AuthenticationToken
    {
        public string Name { get; set; }
        public string Value { get; set; }

        private static string TokenNamesKey = ".TokenNames";

        public static void StoreTokens(AuthenticationProperties properties, IEnumerable<AuthenticationToken> tokens)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }
            if (tokens == null)
            {
                throw new ArgumentNullException(nameof(tokens));
            }

            var tokenNames = new List<string>();
            foreach (var token in tokens)
            {
                // REVIEW: should probably check that there are no ; in the token name and throw or encode
                tokenNames.Add(token.Name);
                properties.Items[token.Name] = token.Value;
            }
            if (tokenNames.Count > 0)
            {
                properties.Items[TokenNamesKey] = string.Join(";", tokenNames.ToArray());
            }
        }

        public static async Task<string> GetTokenAsync(HttpContext context, string signedInScheme, string tokenName)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (signedInScheme == null)
            {
                throw new ArgumentNullException(nameof(signedInScheme));
            }
            if (tokenName == null)
            {
                throw new ArgumentNullException(nameof(tokenName));
            }

            var authContext = new AuthenticateContext(signedInScheme);
            await context.Authentication.AuthenticateAsync(authContext);
            return authContext.Properties.ContainsKey(tokenName) 
                ? authContext.Properties[tokenName]
                : null;
        }

        public static IEnumerable<AuthenticationToken> GetTokens(AuthenticationProperties properties)
        {
            var tokens = new List<AuthenticationToken>();

            if (properties.Items.ContainsKey(TokenNamesKey))
            {
                var tokenNames = properties.Items[TokenNamesKey].Split(';');
                foreach (var name in tokenNames)
                {
                    if (properties.Items.ContainsKey(name))
                    {
                        tokens.Add(new AuthenticationToken { Name = name, Value = properties.Items[name] });
                    }
                }
            }

            return tokens;
        }
    }

}
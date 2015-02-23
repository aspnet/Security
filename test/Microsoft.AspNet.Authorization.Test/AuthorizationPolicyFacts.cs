// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.AspNet.Authorization;
using Xunit;

namespace Microsoft.AspNet.Authroization.Test
{
    public class AuthorizationPolicyFacts
    {
        [Fact]
        public void CanCombineAuthorizeAttributes()
        {
            // Arrange
            var attributes = new AuthorizeAttribute[] {
                new AuthorizeAttribute(),
                new AuthorizeAttribute("1") { ActiveAuthenticationSchemes = "dupe" },
                new AuthorizeAttribute("2") { ActiveAuthenticationSchemes = "dupe" },
                new AuthorizeAttribute { Roles = "r1,r2", ActiveAuthenticationSchemes = "roles" },
            };
            var options = new AuthorizationOptions();
            options.AddPolicy("1", policy => policy.RequiresClaim("1"));
            options.AddPolicy("2", policy => policy.RequiresClaim("2"));

            // Act
            var combined = AuthorizationPolicy.Combine(options, attributes);

            // Assert
            Assert.Equal(2, combined.ActiveAuthenticationSchemes.Count());
            Assert.True(combined.ActiveAuthenticationSchemes.Contains("dupe"));
            Assert.True(combined.ActiveAuthenticationSchemes.Contains("roles"));
            Assert.Equal(4, combined.Requirements.Count());
            Assert.True(combined.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement));
            Assert.Equal(3, combined.Requirements.OfType<ClaimsAuthorizationRequirement>().Count());
        }
    }
}
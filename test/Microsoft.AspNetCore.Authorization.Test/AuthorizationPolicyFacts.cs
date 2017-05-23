// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using Xunit;

namespace Microsoft.AspNetCore.Authroization.Test
{
    public class AuthorizationPolicyFacts
    {
        [Fact]
        public void RequireRoleThrowsIfEmpty()
        {
            Assert.Throws<InvalidOperationException>(() => new AuthorizationPolicyBuilder().RequireRole());
        }

        [Fact]
        public async Task CanCombineAuthorizeAttributes()
        {
            // Arrange
            var attributes = new AuthorizeAttribute[] {
                new AuthorizeAttribute(),
                new AuthorizeAttribute("1") { AuthenticationSchemes = "dupe" },
                new AuthorizeAttribute("2") { AuthenticationSchemes = "dupe" },
                new AuthorizeAttribute { Roles = "r1,r2", AuthenticationSchemes = "roles" },
            };
            var options = new AuthorizationOptions();
            options.AddPolicy("1", policy => policy.RequireClaim("1"));
            options.AddPolicy("2", policy => policy.RequireClaim("2"));

            var provider = new DefaultAuthorizationPolicyProvider(Options.Create(options));

            // Act
            var combined = await AuthorizationPolicy.CombineAsync(provider, attributes);

            // Assert
            Assert.Equal(2, combined.AuthenticationSchemes.Count());
            Assert.True(combined.AuthenticationSchemes.Contains("dupe"));
            Assert.True(combined.AuthenticationSchemes.Contains("roles"));
            Assert.Equal(4, combined.Requirements.Count());
            Assert.True(combined.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement));
            Assert.Equal(2, combined.Requirements.OfType<ClaimsAuthorizationRequirement>().Count());
            Assert.Equal(1, combined.Requirements.OfType<RolesAuthorizationRequirement>().Count());
        }

        [Fact]
        public async Task CanReplaceDefaultPolicy()
        {
            // Arrange
            var attributes = new AuthorizeAttribute[] {
                new AuthorizeAttribute(),
                new AuthorizeAttribute("2") { AuthenticationSchemes = "dupe" }
            };
            var options = new AuthorizationOptions();
            options.DefaultPolicy = new AuthorizationPolicyBuilder("default").RequireClaim("default").Build();
            options.AddPolicy("2", policy => policy.RequireClaim("2"));

            var provider = new DefaultAuthorizationPolicyProvider(Options.Create(options));

            // Act
            var combined = await AuthorizationPolicy.CombineAsync(provider, attributes);

            // Assert
            Assert.Equal(2, combined.AuthenticationSchemes.Count());
            Assert.True(combined.AuthenticationSchemes.Contains("dupe"));
            Assert.True(combined.AuthenticationSchemes.Contains("default"));
            Assert.Equal(2, combined.Requirements.Count());
            Assert.False(combined.Requirements.Any(r => r is DenyAnonymousAuthorizationRequirement));
            Assert.Equal(2, combined.Requirements.OfType<ClaimsAuthorizationRequirement>().Count());
        }

        [Fact]
        public async Task CombineMustTrimRoles()
        {
            // Arrange
            var attributes = new AuthorizeAttribute[] {
                new AuthorizeAttribute() { Roles = "r1 , r2" }
            };
            var options = new AuthorizationOptions();
            var provider = new DefaultAuthorizationPolicyProvider(Options.Create(options));

            // Act
            var combined = await AuthorizationPolicy.CombineAsync(provider, attributes);

            // Assert
            Assert.True(combined.Requirements.Any(r => r is RolesAuthorizationRequirement));
            var rolesAuthorizationRequirement = combined.Requirements.OfType<RolesAuthorizationRequirement>().First();
            Assert.Equal(2, rolesAuthorizationRequirement.AllowedRoles.Count());
            Assert.True(rolesAuthorizationRequirement.AllowedRoles.Any(r => r.Equals("r1")));
            Assert.True(rolesAuthorizationRequirement.AllowedRoles.Any(r => r.Equals("r2")));
        }

        [Fact]
        public async Task CombineMustTrimAuthenticationScheme()
        {
            // Arrange
            var attributes = new AuthorizeAttribute[] {
                new AuthorizeAttribute() { AuthenticationSchemes = "a1 , a2" }
            };
            var options = new AuthorizationOptions();

            var provider = new DefaultAuthorizationPolicyProvider(Options.Create(options));

            // Act
            var combined = await AuthorizationPolicy.CombineAsync(provider, attributes);

            // Assert
            Assert.Equal(2, combined.AuthenticationSchemes.Count());
            Assert.True(combined.AuthenticationSchemes.Any(a => a.Equals("a1")));
            Assert.True(combined.AuthenticationSchemes.Any(a => a.Equals("a2")));
        }
        
        [Fact]
        public async Task CombineMustIgnoreEmptyAuthenticationScheme()
        {
            // Arrange
            var attributes = new AuthorizeAttribute[] {
                new AuthorizeAttribute() { AuthenticationSchemes = "a1 , , ,,, a2" }
            };
            var options = new AuthorizationOptions();

            var provider = new DefaultAuthorizationPolicyProvider(Options.Create(options));

            // Act
            var combined = await AuthorizationPolicy.CombineAsync(provider, attributes);

            // Assert
            Assert.Equal(2, combined.AuthenticationSchemes.Count());
            Assert.True(combined.AuthenticationSchemes.Any(a => a.Equals("a1")));
            Assert.True(combined.AuthenticationSchemes.Any(a => a.Equals("a2")));
        }
        
        [Fact]
        public async Task CombineMustIgnoreEmptyRoles()
        {
            // Arrange
            var attributes = new AuthorizeAttribute[] {
                new AuthorizeAttribute() { Roles = "r1 , ,, , r2" }
            };
            var options = new AuthorizationOptions();
            var provider = new DefaultAuthorizationPolicyProvider(Options.Create(options));

            // Act
            var combined = await AuthorizationPolicy.CombineAsync(provider, attributes);

            // Assert
            Assert.True(combined.Requirements.Any(r => r is RolesAuthorizationRequirement));
            var rolesAuthorizationRequirement = combined.Requirements.OfType<RolesAuthorizationRequirement>().First();
            Assert.Equal(2, rolesAuthorizationRequirement.AllowedRoles.Count());
            Assert.True(rolesAuthorizationRequirement.AllowedRoles.Any(r => r.Equals("r1")));
            Assert.True(rolesAuthorizationRequirement.AllowedRoles.Any(r => r.Equals("r2")));
        }
    }
}
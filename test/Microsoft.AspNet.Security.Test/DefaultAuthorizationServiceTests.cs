// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNet.Security.Test
{
    public class DefaultAuthorizationServiceTests
    {
        [Fact]
        public async Task Check_ShouldAllowIfClaimIsPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage");
            var authorizationService = new DefaultAuthorizationService(null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim("Permission", "CanViewPage") }, "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Check_ShouldAllowIfClaimIsAmongValues()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage", "CanViewAnything");
            var authorizationService = new DefaultAuthorizationService(null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                        new Claim("Permission", "CanViewAnything")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfClaimTypeIsNotPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage", "CanViewAnything");
            var authorizationService = new DefaultAuthorizationService(null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SomethingElse", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfClaimValueIsNotPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage");
            var authorizationService = new DefaultAuthorizationService(null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfNoClaims()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage");
            var authorizationService = new DefaultAuthorizationService(null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[0],
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfUserIsNull()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage");
            var authorizationService = new DefaultAuthorizationService(null);
            ClaimsPrincipal user = null;

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Check_ShouldNotAllowIfUserIsNotAuthenticated()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").Requires("Permission", "CanViewPage");
            var authorizationService = new DefaultAuthorizationService(null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    null)
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }
    }
}

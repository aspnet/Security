// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Framework.OptionsModel;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Security.Test
{
    public class DefaultAuthorizationServiceTests
    {
        [Fact]
        public async Task Authorize_ShouldAllowIfClaimIsPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").RequiresClaim("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim("Permission", "CanViewPage") }, "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldAllowIfClaimIsAmongValues()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").RequiresClaim("Permission", "CanViewPage", "CanViewAnything");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                        new Claim("Permission", "CanViewAnything")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfClaimTypeIsNotPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").RequiresClaim("Permission", "CanViewPage", "CanViewAnything");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SomethingElse", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfClaimValueIsNotPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").RequiresClaim("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfNoClaims()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").RequiresClaim("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[0],
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfUserIsNull()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").RequiresClaim("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            ClaimsPrincipal user = null;

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfUserIsNotAuthenticated()
        {
            // Arrange
            var policy = new AuthorizationPolicy("Basic").RequiresClaim("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    null)
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldAllowWithNoAuthType()
        {
            // Arrange
            var policy = new AuthorizationPolicy().RequiresClaim("Permission", "CanViewPage");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                    },
                    null)
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfUnknownPolicy()
        {
            // Arrange
            var authorizationOptions = new AuthorizationOptions();
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    null)
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_CustomRolePolicy()
        {
            // Arrange
            var policy = new AuthorizationPolicy()
                .RequiresClaim(ClaimTypes.Role, "Administrator")
                .RequiresClaim(ClaimTypes.Role, "User");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.Role, "Administrator")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_HasAnyClaimOfTypePolicy()
        {
            // Arrange
            var policy = new AuthorizationPolicy()
                .RequiresClaim(ClaimTypes.Role);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, ""),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_PolicyRequiresAuthenticationTypeWithNameClaim()
        {
            // Arrange
            var policy = new AuthorizationPolicy("AuthType")
                .RequiresClaim(ClaimTypes.Name);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Name, "Name"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanRequireSingleRole()
        {
            // Arrange
            var policy = new RolesAuthorizationPolicy("AuthType")
                .RequiresRole("Admin");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "Admin"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanRequireOneOfManyRoles()
        {
            // Arrange
            var policy = new RolesAuthorizationPolicy("AuthType")
                .RequiresRole("Admin", "Users");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "Users"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanBlockWrongRole()
        {
            // Arrange
            var policy = new RolesAuthorizationPolicy("AuthType")
                .RequiresRole("Admin", "Users");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "Nope"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task RolePolicyCanBlockNoRole()
        {
            // Arrange
            var policy = new RolesAuthorizationPolicy("AuthType")
                .RequiresRole("Admin", "Users");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, null);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }

        private class GrumpyRequirement : IAuthorizationRequirement
        {
            public IEnumerable<string> AuthenticationTypesFilter
            {
                get
                {
                    return null;
                }
            }

            public Task<bool> CheckAsync(AuthorizationContext context)
            {
                return Task.FromResult(false);
            }
        }

        [Fact]
        public async Task CustomRequirementCanBlock()
        {
            // Arrange
            var policy = new AuthorizationPolicy("AuthType")
                .RequiresClaim(ClaimTypes.Name)
                .Requires(new GrumpyRequirement());
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object);
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Name, "Name"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task PolicyCanApproveWithNoHandlers()
        {
            // Arrange
            var policy = new AuthorizationPolicy("AuthType")
                .RequiresClaim(ClaimTypes.Name);
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, new List<IAuthorizationPolicyHandler>());
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Name, "Name"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task AlwaysApproveWithNoPolicyHandlersOrRequirements()
        {
            // Arrange
            var policy = new AuthorizationPolicy("TotallyBogus");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object);
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        private class AnyAuthenticatedUserRequirement : IAuthorizationRequirement
        {
            public IEnumerable<string> AuthenticationTypesFilter
            {
                get
                {
                    return null;
                }
            }

            public Task<bool> CheckAsync(AuthorizationContext context)
            {
                var user = context.User;
                var userIsAnonymous =
                    user == null ||
                    user.Identity == null ||
                    !user.Identity.IsAuthenticated;
                return Task.FromResult(!userIsAnonymous);
            }
        }

        [Fact]
        public async Task CanApproveAnyAuthenticatedUser()
        {
            // Arrange
            var policy = new AuthorizationPolicy("AuthType")
                .Requires(new AnyAuthenticatedUserRequirement());
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, new List<IAuthorizationPolicyHandler>());
            var user = new ClaimsPrincipal(new ClaimsIdentity("AuthType"));

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task CanBlockNonAuthenticatedUser()
        {
            // Arrange
            var policy = new AuthorizationPolicy("AuthType")
                .Requires(new AnyAuthenticatedUserRequirement());
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var authorizationService = new DefaultAuthorizationService(options.Object, new List<IAuthorizationPolicyHandler>());
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy, user);

            // Assert
            Assert.False(allowed);
        }

    }
}
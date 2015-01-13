// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;
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
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage");
            var context = new Mock<HttpContext>();
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy.Build());
            var authorizationService = SetupAuthService(authorizationOptions, context,
                new ClaimsIdentity(new Claim[] { new Claim("Permission", "CanViewPage") }, "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldAllowIfClaimIsPresentWithSpecifiedAuthType()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage");
            policy.UseOnlyTheseAuthenticationTypes.Add("Basic");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy.Build());
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var handlers = new IAuthorizationHandler[] { new ClaimsRequirementHandler() };
            var authorizationService = new DefaultAuthorizationService(options.Object, handlers);
            var id = new ClaimsIdentity(new Claim[] { new Claim("Permission", "CanViewPage") }, "Basic");
            var authResult = new List<AuthenticationResult>();
            authResult.Add(new AuthenticationResult(id, new AuthenticationProperties(), new AuthenticationDescription()));
            var context = new Mock<HttpContext>();
            context.SetupProperty(c => c.User);
            context.Setup(c => c.AuthenticateAsync(policy.UseOnlyTheseAuthenticationTypes)).ReturnsAsync(authResult).Verifiable();

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.True(allowed);

            context.VerifyAll();
        }

        [Fact]
        public async Task Authorize_ShouldAllowIfClaimIsAmongValues()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage", "CanViewAnything");
            var context = new Mock<HttpContext>();
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy.Build());
            var authorizationService = SetupAuthService(authorizationOptions, context,
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                        new Claim("Permission", "CanViewAnything")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldFailWhenAllRequirementsNotHandled()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage", "CanViewAnything");
            var context = new Mock<HttpContext>();
            var authorizationService = SetupAuthService(context,
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SomethingElse", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfClaimTypeIsNotPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage", "CanViewAnything");
            var context = new Mock<HttpContext>();
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy.Build());
            var authorizationService = SetupAuthService(authorizationOptions, context,
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SomethingElse", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfClaimValueIsNotPresent()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage");
            var context = new Mock<HttpContext>();
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy.Build());
            var authorizationService = SetupAuthService(authorizationOptions, context,
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfNoClaims()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage");
            var context = new Mock<HttpContext>();
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy.Build());
            var authorizationService = SetupAuthService(authorizationOptions, context,
                new ClaimsIdentity(
                    new Claim[0],
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfUserIsNull()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage");
            var context = new Mock<HttpContext>();
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy.Build());
            var authorizationService = SetupAuthService(authorizationOptions, context);
            context.Object.User = null;

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfNotCorrectAuthType()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage");
            policy.UseOnlyTheseAuthenticationTypes.Add("Basic");
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy.Build());
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authorizationOptions);
            var handlers = new IAuthorizationHandler[] { new ClaimsRequirementHandler() };
            var authorizationService = new DefaultAuthorizationService(options.Object, handlers);
            var id = new ClaimsIdentity();
            var authResult = new List<AuthenticationResult>();
            authResult.Add(new AuthenticationResult(id, new AuthenticationProperties(), new AuthenticationDescription()));
            var context = new Mock<HttpContext>();
            context.SetupProperty(c => c.User);
            context.Setup(c => c.AuthenticateAsync(policy.UseOnlyTheseAuthenticationTypes)).ReturnsAsync(authResult).Verifiable();

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldAllowWithNoAuthType()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage");
            var context = new Mock<HttpContext>();
            var authorizationOptions = new AuthorizationOptions();
            authorizationOptions.AddPolicy("Basic", policy.Build());
            var authorizationService = SetupAuthService(authorizationOptions, context,
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                    },
                    null)
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfUnknownPolicy()
        {
            // Arrange
            var context = new Mock<HttpContext>();
            var authorizationService = SetupAuthService(context,
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    null)
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync("Basic", context.Object);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_CustomRolePolicy()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresRole("Administrator")
                .RequiresClaim(ClaimTypes.Role, "User");
            var context = new Mock<HttpContext>();
            var authorizationService = SetupAuthService(context,
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.Role, "Administrator")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy.Build(), context.Object);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_HasAnyClaimOfTypePolicy()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim(ClaimTypes.Role);
            var context = new Mock<HttpContext>();
            var authorizationService = SetupAuthService(context,
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, ""),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy.Build(), context.Object);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_PolicyCanAuthenticationTypeWithNameClaim()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim(ClaimTypes.Name);
            policy.UseOnlyTheseAuthenticationTypes.Add("AuthType");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var handlers = new IAuthorizationHandler[] { new ClaimsRequirementHandler() };
            var authorizationService = new DefaultAuthorizationService(options.Object, handlers);
            var id = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "Name") }, "AuthType");
            var authResult = new List<AuthenticationResult>();
            authResult.Add(new AuthenticationResult(id, new AuthenticationProperties(), new AuthenticationDescription()));
            var context = new Mock<HttpContext>();
            context.SetupProperty(c => c.User);
            context.Setup(c => c.AuthenticateAsync(policy.UseOnlyTheseAuthenticationTypes)).ReturnsAsync(authResult).Verifiable();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy.Build(), context.Object);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanRequireSingleRole()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresRole("Admin");
            policy.UseOnlyTheseAuthenticationTypes.Add("AuthType");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var handlers = new IAuthorizationHandler[] { new ClaimsRequirementHandler() };
            var authorizationService = new DefaultAuthorizationService(options.Object, handlers);
            var id = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "Admin") }, "AuthType");
            var authResult = new List<AuthenticationResult>();
            authResult.Add(new AuthenticationResult(id, new AuthenticationProperties(), new AuthenticationDescription()));
            var context = new Mock<HttpContext>();
            context.SetupProperty(c => c.User);
            context.Setup(c => c.AuthenticateAsync(policy.UseOnlyTheseAuthenticationTypes)).ReturnsAsync(authResult).Verifiable();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy.Build(), context.Object);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanRequireOneOfManyRoles()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresRole("Admin", "Users");
            policy.UseOnlyTheseAuthenticationTypes.Add("AuthType");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var handlers = new IAuthorizationHandler[] { new ClaimsRequirementHandler() };
            var authorizationService = new DefaultAuthorizationService(options.Object, handlers);
            var id = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "Users") }, "AuthType");
            var authResult = new List<AuthenticationResult>();
            authResult.Add(new AuthenticationResult(id, new AuthenticationProperties(), new AuthenticationDescription()));
            var context = new Mock<HttpContext>();
            context.SetupProperty(c => c.User);
            context.Setup(c => c.AuthenticateAsync(policy.UseOnlyTheseAuthenticationTypes)).ReturnsAsync(authResult).Verifiable();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy.Build(), context.Object);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanBlockWrongRole()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresRole("Admin", "Users");
            var context = new Mock<HttpContext>();
            var authorizationService = SetupAuthService(context,
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "Nope"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy.Build(), context.Object);

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task RolePolicyCanBlockNoRole()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresRole("Admin", "Users");
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var context = new Mock<HttpContext>();
            var authorizationService = SetupAuthService(context,
                new ClaimsIdentity(
                    new Claim[] {
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy.Build(), context.Object);

            // Assert
            Assert.False(allowed);
        }

        private IAuthorizationService SetupAuthService(Mock<HttpContext> context, params ClaimsIdentity[] identities)
        {
            return SetupAuthService(null, context, identities);
        }

        private IAuthorizationService SetupAuthService(AuthorizationOptions authOptions, Mock<HttpContext> context, params ClaimsIdentity[] identities)
        {
            authOptions = authOptions ?? new AuthorizationOptions();
            var options = new Mock<IOptions<AuthorizationOptions>>();
            options.Setup(o => o.Options).Returns(authOptions);
            var handlers = new IAuthorizationHandler[] { new ClaimsRequirementHandler() };
            var authorizationService = new DefaultAuthorizationService(options.Object, handlers);
            var user = new ClaimsPrincipal(identities);
            context.SetupProperty(c => c.User);
            context.Object.User = user;
            return authorizationService;
        }

        private IAuthorizationService SetupAuthService(Mock<HttpContext> context, IEnumerable<string> authTypes, params ClaimsIdentity[] identities)
        {
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var handlers = new IAuthorizationHandler[] { new ClaimsRequirementHandler() };
            var authorizationService = new DefaultAuthorizationService(options.Object, handlers);
            var authResult = new List<AuthenticationResult>();
            foreach (var id in identities)
            {
                authResult.Add(new AuthenticationResult(id, new AuthenticationProperties(), new AuthenticationDescription()));
            }
            context.SetupProperty(c => c.User);
            context.Setup(c => c.AuthenticateAsync(authTypes)).ReturnsAsync(authResult).Verifiable();
            return authorizationService;
        }

        [Fact]
        public async Task PolicyCanApproveWithNoRequirements()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder();
            var context = new Mock<HttpContext>();
            var authorizationService = SetupAuthService(context,
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Name, "Name"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy.Build(), context.Object);

            // Assert
            Assert.True(allowed);
        }

        private class AnyAuthenticatedUserRequirement : IAuthorizationRequirement { }

        private class AnyAuthenticatedUserHandler : IAuthorizationHandler
        {
            public Task HandleAsync(AuthorizationContext context)
            {
                var user = context.User;
                var userIsAnonymous =
                    user == null ||
                    user.Identity == null ||
                    !user.Identity.IsAuthenticated;
                foreach (var req in context.Policy.Requirements)
                {
                    if (req is AnyAuthenticatedUserRequirement)
                    {
                        if (!userIsAnonymous)
                        {
                            context.RequirementSucceeded(req);
                        }
                    }
                }
                return Task.FromResult(0);
            }
        }

        [Fact]
        public async Task CanApproveAnyAuthenticatedUser()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder();
            policy.Requirements.Add(new AnyAuthenticatedUserRequirement());
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var handlers = new List<IAuthorizationHandler>();
            handlers.Add(new AnyAuthenticatedUserHandler());
            var authorizationService = new DefaultAuthorizationService(options.Object, handlers);
            var user = new ClaimsPrincipal(new ClaimsIdentity("AuthType"));
            var context = new Mock<HttpContext>();
            context.SetupProperty(c => c.User);
            context.Object.User = user;

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy.Build(), context.Object);

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task CanBlockNonAuthenticatedUser()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder();
            policy.Requirements.Add(new AnyAuthenticatedUserRequirement());
            var options = new Mock<IOptions<AuthorizationOptions>>();
            var handlers = new List<IAuthorizationHandler>();
            handlers.Add(new AnyAuthenticatedUserHandler());
            var authorizationService = new DefaultAuthorizationService(options.Object, handlers);
            var user = new ClaimsPrincipal();
            var context = new Mock<HttpContext>();
            context.SetupProperty(c => c.User);
            context.Object.User = user;

            // Act
            var allowed = await authorizationService.AuthorizeAsync(policy.Build(), context.Object);

            // Assert
            Assert.False(allowed);
        }
    }
}
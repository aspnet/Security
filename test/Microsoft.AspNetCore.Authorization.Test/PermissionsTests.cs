// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Authorization.Test
{
    public class PermissionsTests
    {
        private class ExpenseReport {
            public string Owner;
        }

        private enum MyPermissions
        {
            Read,
            Edit,
            Delete,
            Create
        }

        private class MyPermissionsHandler : AuthorizationPermissionsHandler
        {
            public MyPermissionsHandler() { } // Could get a DbContext here

            protected override Task<bool> CheckPermissionAsync(AuthorizationHandlerContext context, Enum permission)
            {
                switch (permission)
                {
                    case MyPermissions.Create:
                        return Task.FromResult(true);
                    case MyPermissions.Read:
                        return Task.FromResult(true);
                    case MyPermissions.Delete:
                    case MyPermissions.Edit:
                        var report = context.Resource as ExpenseReport;
                        return Task.FromResult(report?.Owner == context?.User.Identity.Name);
                    default:
                        return Task.FromResult(false);
                }
            }

        }

        [Fact]
        public async Task CanAuthorizeAllSuperuserOperations()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddSingleton<IAuthorizationHandler, MyPermissionsHandler>();
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Name, "user")
                    },
                    "AuthType")
                );

            // Act
            // Assert
            Assert.True(await authorizationService.AuthorizeAsync(user, null, new AuthorizationPermissionsRequirement(MyPermissions.Create)));
            Assert.True(await authorizationService.AuthorizeAsync(user, null, new AuthorizationPermissionsRequirement(MyPermissions.Read)));
            Assert.False(await authorizationService.AuthorizeAsync(user, null, new AuthorizationPermissionsRequirement(MyPermissions.Edit)));
            Assert.False(await authorizationService.AuthorizeAsync(user, null, new AuthorizationPermissionsRequirement(MyPermissions.Delete)));
            Assert.True(await authorizationService.AuthorizeAsync(user, new ExpenseReport { Owner = "user" }, new AuthorizationPermissionsRequirement(MyPermissions.Delete, MyPermissions.Edit)));
            Assert.False(await authorizationService.AuthorizeAsync(user, null, new AuthorizationPermissionsRequirement(MyPermissions.Read, MyPermissions.Edit)));
        }

        private IAuthorizationService BuildAuthorizationService(Action<IServiceCollection> setupServices = null)
        {
            var services = new ServiceCollection();
            services.AddAuthorization();
            services.AddLogging();
            services.AddOptions();
            setupServices?.Invoke(services);
            return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
        }
    }
}
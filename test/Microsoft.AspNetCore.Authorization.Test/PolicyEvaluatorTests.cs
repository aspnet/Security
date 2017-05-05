// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Microsoft.AspNetCore.Authorization.Policy.Test
{
    public class PolicyEvaluatorTests
    {
        [Fact]
        public async Task AuthorizeSucceedsEvenIfAuthenticationFails()
        {
            // Arrange
            var evaluator = new PolicyEvaluator(new HappyAuthorization());
            var context = new DefaultHttpContext();
            var policy = new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build();

            // Act
            var result = await evaluator.AuthorizeAsync(policy, PolicyAuthenticationResult.Failed(), context);

            // Assert
            Assert.True(result.Succeeded);
            Assert.False(result.Challenged);
            Assert.False(result.Forbidden);
        }

        [Fact]
        public async Task AuthorizeChallengesIfAuthenticationFails()
        {
            // Arrange
            var evaluator = new PolicyEvaluator(new SadAuthorization());
            var context = new DefaultHttpContext();
            var policy = new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build();

            // Act
            var result = await evaluator.AuthorizeAsync(policy, PolicyAuthenticationResult.Failed(), context);

            // Assert
            Assert.False(result.Succeeded);
            Assert.True(result.Challenged);
            Assert.False(result.Forbidden);
        }

        [Fact]
        public async Task AuthorizeForbidsIfAuthenticationSuceeds()
        {
            // Arrange
            var evaluator = new PolicyEvaluator(new SadAuthorization());
            var context = new DefaultHttpContext();
            var policy = new AuthorizationPolicyBuilder().RequireAssertion(_ => true).Build();

            // Act
            var result = await evaluator.AuthorizeAsync(policy, PolicyAuthenticationResult.Success(), context);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(result.Challenged);
            Assert.True(result.Forbidden);
        }

        public class HappyAuthorization : IAuthorizationService
        {
            public Task<AuthorizeResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
                => Task.FromResult(AuthorizeResult.Success());

            public Task<AuthorizeResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
                => Task.FromResult(AuthorizeResult.Success());
        }

        public class SadAuthorization : IAuthorizationService
        {
            public Task<AuthorizeResult> AuthorizeAsync(ClaimsPrincipal user, object resource, IEnumerable<IAuthorizationRequirement> requirements)
                => Task.FromResult(AuthorizeResult.Failed());

            public Task<AuthorizeResult> AuthorizeAsync(ClaimsPrincipal user, object resource, string policyName)
                => Task.FromResult(AuthorizeResult.Failed());
        }

    }
}
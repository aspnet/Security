// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Authorization.Policy.Test
{
    public class AuthorizationRequestEvaluatorTests
    {
        [Fact]
        public async Task AuthorizeSucceedsOnlyIfResourceSpecified()
        {
            // Arrange
            var evaluator = BuildEvaluator();
            var context = new DefaultHttpContext();
            var request = new AuthorizationRequest();
            request.AuthenticationPolicy = null; // default to context.User
            request.Requirements = new AuthorizationPolicyBuilder().RequireAssertion(c => c.Resource != null).Build().Requirements;

            // Act
            var result = await evaluator.AuthorizeAsync(request, context);
            Assert.False(result.Succeeded);

            // Set the resource and ensure it succeeds
            request.Resource = new object();
            var result2 = await evaluator.AuthorizeAsync(request, context);
            Assert.True(result2.Succeeded);
        }

        // Need to mock Auth failure
        //[Fact]
        //public async Task AuthorizeChallengesIfAuthenticationFails()
        //{
        //    // Arrange
        //    var evaluator = BuildEvaluator();
        //    var context = new DefaultHttpContext();
        //    var request = new AuthorizationRequest();
        //    request.AuthenticationPolicy = null; // default to context.User
        //    request.Requirements = new AuthorizationPolicyBuilder().RequireAssertion(_ => false).Build().Requirements;

        //    // Act
        //    var result = await evaluator.AuthorizeAsync(request, context);

        //    // Assert
        //    Assert.False(result.Succeeded);
        //    Assert.True(result.Challenged);
        //    Assert.False(result.Forbidden);
        //}

        [Fact]
        public async Task AuthorizeForbidsIfAuthenticationSuceeds()
        {
            // Arrange
            var evaluator = BuildEvaluator();
            var context = new DefaultHttpContext();
            var request = new AuthorizationRequest();
            request.AuthenticationPolicy = null; // default to context.User
            request.Requirements = new AuthorizationPolicyBuilder().RequireAssertion(_ => false).Build().Requirements;

            // Act
            var result = await evaluator.AuthorizeAsync(request, context);

            // Assert
            Assert.False(result.Succeeded);
            Assert.False(result.Challenged);
            Assert.True(result.Forbidden);
        }


        private IAuthorizationRequestEvaluator BuildEvaluator(Action<IServiceCollection> setupServices = null)
        {
            var services = new ServiceCollection()
                .AddAuthorization()
                .AddAuthorizationRequestStuff()
                .AddAuthorizationPolicyEvaluator()
                .AddLogging()
                .AddOptions();
            setupServices?.Invoke(services);
            return services.BuildServiceProvider().GetRequiredService<IAuthorizationRequestEvaluator>();
        }
    }
}
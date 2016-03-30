// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Features.Authentication;
using Xunit;

namespace Microsoft.AspNetCore.Authentication
{
    public class TokenExtensionTests
    {
        [Fact]
        public void CanStoreMultipleTokens()
        {
            var props = new AuthenticationProperties();
            var tokens = new List<AuthenticationToken>();
            var tok1 = new AuthenticationToken { Name = "One", Value = "1" };
            var tok2 = new AuthenticationToken { Name = "Two", Value = "2" };
            var tok3 = new AuthenticationToken { Name = "Three", Value = "3" };
            tokens.Add(tok1);
            tokens.Add(tok2);
            tokens.Add(tok3);
            props.StoreTokens(tokens);

            Assert.Equal("1", props.GetTokenValue("One"));
            Assert.Equal("2", props.GetTokenValue("Two"));
            Assert.Equal("3", props.GetTokenValue("Three"));
            Assert.Equal(3, props.GetTokens().Count());
        }

        [Fact]
        public void SubsequentStoreTokenDeletesPreviousTokens()
        {
            var props = new AuthenticationProperties();
            var tokens = new List<AuthenticationToken>();
            var tok1 = new AuthenticationToken { Name = "One", Value = "1" };
            var tok2 = new AuthenticationToken { Name = "Two", Value = "2" };
            var tok3 = new AuthenticationToken { Name = "Three", Value = "3" };
            tokens.Add(tok1);
            tokens.Add(tok2);
            tokens.Add(tok3);

            props.StoreTokens(tokens);

            props.StoreTokens(new[] { new AuthenticationToken { Name = "Zero", Value = "0" } });

            Assert.Equal("0", props.GetTokenValue("Zero"));
            Assert.Equal(null, props.GetTokenValue("One"));
            Assert.Equal(null, props.GetTokenValue("Two"));
            Assert.Equal(null, props.GetTokenValue("Three"));
            Assert.Equal(1, props.GetTokens().Count());
        }

        [Fact]
        public void CanUpdateTokens()
        {
            var props = new AuthenticationProperties();
            var tokens = new List<AuthenticationToken>();
            var tok1 = new AuthenticationToken { Name = "One", Value = "1" };
            var tok2 = new AuthenticationToken { Name = "Two", Value = "2" };
            var tok3 = new AuthenticationToken { Name = "Three", Value = "3" };
            tokens.Add(tok1);
            tokens.Add(tok2);
            tokens.Add(tok3);
            props.StoreTokens(tokens);

            tok1.Value = ".1";
            tok2.Value = ".2";
            tok3.Value = ".3";
            props.StoreTokens(tokens);

            Assert.Equal(".1", props.GetTokenValue("One"));
            Assert.Equal(".2", props.GetTokenValue("Two"));
            Assert.Equal(".3", props.GetTokenValue("Three"));
            Assert.Equal(3, props.GetTokens().Count());
        }

        public class TestAuthHandler : IAuthenticationHandler
        {
            private readonly AuthenticationProperties _props;
            public TestAuthHandler(AuthenticationProperties props)
            {
                _props = props;
            }

            public Task AuthenticateAsync(AuthenticateContext context)
            {
                context.Authenticated(new ClaimsPrincipal(), _props.Items, new Dictionary<string, object>());
                return Task.FromResult(0);
            }

            public Task ChallengeAsync(ChallengeContext context)
            {
                throw new NotImplementedException();
            }

            public void GetDescriptions(DescribeSchemesContext context)
            {
                throw new NotImplementedException();
            }

            public Task SignInAsync(SignInContext context)
            {
                throw new NotImplementedException();
            }

            public Task SignOutAsync(SignOutContext context)
            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public async Task CanGetTokenFromContext()
        {
            var props = new AuthenticationProperties();
            var tokens = new List<AuthenticationToken>();
            var tok1 = new AuthenticationToken { Name = "One", Value = "1" };
            var tok2 = new AuthenticationToken { Name = "Two", Value = "2" };
            var tok3 = new AuthenticationToken { Name = "Three", Value = "3" };
            tokens.Add(tok1);
            tokens.Add(tok2);
            tokens.Add(tok3);
            props.StoreTokens(tokens);

            var context = new DefaultHttpContext();
            var handler = new TestAuthHandler(props);
            context.Features.Set<IHttpAuthenticationFeature>(new HttpAuthenticationFeature() { Handler = handler });

            Assert.Equal("1", await context.Authentication.GetTokenAsync("One"));
            Assert.Equal("2", await context.Authentication.GetTokenAsync("Two"));
            Assert.Equal("3", await context.Authentication.GetTokenAsync("Three"));
        }

    }
}

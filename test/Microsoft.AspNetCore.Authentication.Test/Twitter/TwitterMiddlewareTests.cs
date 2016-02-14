// Copyright (c) .NET Foundation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.AspNetCore.Authentication.Twitter
{
    public class TwitterMiddlewareTests
    {
        [Fact]
        public async Task ChallengeWillTriggerApplyRedirectEvent()
        {
            var server = CreateServer(new TwitterOptions
                {
                    ConsumerKey = "Test Consumer Key",
                    ConsumerSecret = "Test Consumer Secret",
                    Events = new TwitterEvents
                    {
                        OnRedirectToAuthorizationEndpoint = context =>
                        {
                            context.Response.Redirect(context.RedirectUri + "&custom=test");
                            return Task.FromResult(0);
                        }
                    },
                    BackchannelHttpHandler = new TestHttpMessageHandler
                    {
                        Sender = req =>
                        {
                            if (req.RequestUri.AbsoluteUri == "https://api.twitter.com/oauth/request_token")
                            {
                                return new HttpResponseMessage(HttpStatusCode.OK)
                                {
                                    Content =
                                        new StringContent("oauth_callback_confirmed=true&oauth_token=test_oauth_token&oauth_token_secret=test_oauth_token_secret",
                                            Encoding.UTF8,
                                            "application/x-www-form-urlencoded")
                                };
                            }
                            return null;
                        }
                    }
                },
                context => 
                {
                    // REVIEW: Gross
                    context.Authentication.ChallengeAsync("Twitter").GetAwaiter().GetResult();
                    return true;
                });
            var transaction = await server.SendAsync("http://example.com/challenge");
            Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
            var query = transaction.Response.Headers.Location.Query;
            Assert.Contains("custom=test", query);
        }

        [Fact]
        public async Task BadSignInWillThrow()
        {
            var server = CreateServer(new TwitterOptions
            {
                ConsumerKey = "Test Consumer Key",
                ConsumerSecret = "Test Consumer Secret"
            });

            // Send a bogus sign in
            var error = await Assert.ThrowsAnyAsync<Exception>(() => server.SendAsync("https://example.com/signin-twitter"));
            Assert.Equal("Invalid state cookie.", error.GetBaseException().Message);
        }

        [Fact]
        public async Task SignInThrows()
        {
            var server = CreateServer(new TwitterOptions
            {
                ConsumerKey = "Test Consumer Key",
                ConsumerSecret = "Test Consumer Secret"
            });
            var transaction = await server.SendAsync("https://example.com/signIn");
            Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);
        }

        [Fact]
        public async Task SignOutThrows()
        {
            var server = CreateServer(new TwitterOptions
            {
                ConsumerKey = "Test Consumer Key",
                ConsumerSecret = "Test Consumer Secret"
            });
            var transaction = await server.SendAsync("https://example.com/signOut");
            Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);
        }

        [Fact]
        public async Task ForbidThrows()
        {
            var server = CreateServer(new TwitterOptions
            {
                ConsumerKey = "Test Consumer Key",
                ConsumerSecret = "Test Consumer Secret"
            });
            var transaction = await server.SendAsync("https://example.com/signOut");
            Assert.Equal(HttpStatusCode.OK, transaction.Response.StatusCode);
        }


        [Fact]
        public async Task ChallengeWillTriggerRedirection()
        {
            var server = CreateServer(new TwitterOptions
            {
                    ConsumerKey = "Test Consumer Key",
                    ConsumerSecret = "Test Consumer Secret",
                    BackchannelHttpHandler = new TestHttpMessageHandler
                    {
                        Sender = req =>
                        {
                            if (req.RequestUri.AbsoluteUri == "https://api.twitter.com/oauth/request_token")
                            {
                                return new HttpResponseMessage(HttpStatusCode.OK)
                                {
                                    Content =
                                        new StringContent("oauth_callback_confirmed=true&oauth_token=test_oauth_token&oauth_token_secret=test_oauth_token_secret",
                                            Encoding.UTF8,
                                            "application/x-www-form-urlencoded")
                                };
                            }
                            return null;
                        }
                    }
                },
                context =>
                {
                    // REVIEW: gross
                    context.Authentication.ChallengeAsync("Twitter").GetAwaiter().GetResult();
                    return true;
                });
            var transaction = await server.SendAsync("http://example.com/challenge");
            Assert.Equal(HttpStatusCode.Redirect, transaction.Response.StatusCode);
            var location = transaction.Response.Headers.Location.AbsoluteUri;
            Assert.Contains("https://api.twitter.com/oauth/authenticate?oauth_token=", location);
        }

        private static TestServer CreateServer(TwitterOptions options, Func<HttpContext, bool> handler = null)
        {
            var builder = new WebHostBuilder()
                .Configure(app =>
                {
                    app.UseCookieAuthentication(new CookieAuthenticationOptions
                    {
                        AuthenticationScheme = "External"
                    });
                    app.UseTwitterAuthentication(options);
                    app.Use(async (context, next) =>
                    {
                        var req = context.Request;
                        var res = context.Response;
                        if (req.Path == new PathString("/signIn"))
                        {
                            await Assert.ThrowsAsync<NotSupportedException>(() => context.Authentication.SignInAsync("Twitter", new ClaimsPrincipal()));
                        }
                        else if (req.Path == new PathString("/signOut"))
                        {
                            await Assert.ThrowsAsync<NotSupportedException>(() => context.Authentication.SignOutAsync("Twitter"));
                        }
                        else if (req.Path == new PathString("/forbid"))
                        {
                            await Assert.ThrowsAsync<NotSupportedException>(() => context.Authentication.ForbidAsync("Twitter"));
                        }
                        else if (handler == null || !handler(context))
                        {
                            await next();
                        }
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddAntiforgery();
                    services.AddCookieAuthentication();
                    services.Configure<SharedAuthenticationOptions>(authOptions =>
                    {
                        authOptions.SignInScheme = "External";
                    });
                });
            return new TestServer(builder);
        }
    }
}

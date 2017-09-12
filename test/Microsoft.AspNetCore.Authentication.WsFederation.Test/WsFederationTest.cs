// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.Authentication.WsFederation
{
    public class WsFederationTest
    {
        [Fact]
        public async Task ChallengeRedirects()
        {
            var httpClient = CreateClient();

            // Verify if the request is redirected to STS with right parameters
            var response = await httpClient.GetAsync("/");
            Assert.Equal("https://login.windows.net/4afbc689-805b-48cf-a24c-d4aa3248a248/wsfed", response.Headers.Location.GetLeftPart(System.UriPartial.Path));
            var queryItems = QueryHelpers.ParseQuery(response.Headers.Location.Query);

            Assert.Equal("http://Automation1", queryItems["wtrealm"]);
            Assert.True(queryItems["wctx"].ToString().Equals(CustomStateDataFormat.ValidStateData), "wctx does not equal ValidStateData");
            Assert.Equal<string>(httpClient.BaseAddress + "signin-wsfed", queryItems["wreply"]);
            Assert.Equal<string>("wsignin1.0", queryItems["wa"]);
        }

        [Fact]
        public async Task ValidTokenIsAccepted()
        {
            var httpClient = CreateClient();

            // Verify if the request is redirected to STS with right parameters
            var response = await httpClient.GetAsync("/");
            var queryItems = QueryHelpers.ParseQuery(response.Headers.Location.Query);

            // Send an invalid token and verify that the token is not honored
            var kvps = new List<KeyValuePair<string, string>>();
            kvps.Add(new KeyValuePair<string, string>("wa", "wsignin1.0"));
            kvps.Add(new KeyValuePair<string, string>("wresult", File.ReadAllText(@"ValidToken.xml")));
            kvps.Add(new KeyValuePair<string, string>("wctx", queryItems["wctx"]));
            response = await httpClient.PostAsync(queryItems["wreply"], new FormUrlEncodedContent(kvps));

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);

            var request = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location);
            var cookies = SetCookieHeaderValue.ParseList(response.Headers.GetValues(HeaderNames.SetCookie).ToList());
            foreach (var cookie in cookies)
            {
                if (cookie.Value.HasValue)
                {
                    request.Headers.Add(HeaderNames.Cookie, new CookieHeaderValue(cookie.Name, cookie.Value).ToString());
                }
            }
            response = await httpClient.SendAsync(request);

            // Did the request end in the actual resource requested for
            Assert.Equal(WsFederationDefaults.AuthenticationScheme, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task ValidUnsolicitedTokenIsAccepted()
        {
            var httpClient = CreateClient();

            // Send an invalid token and verify that the token is not honored
            var kvps = new List<KeyValuePair<string, string>>();
            kvps.Add(new KeyValuePair<string, string>("wa", "wsignin1.0"));
            kvps.Add(new KeyValuePair<string, string>("wresult", File.ReadAllText(@"ValidToken.xml")));
            kvps.Add(new KeyValuePair<string, string>("suppressWctx", "true"));
            var response = await httpClient.PostAsync(httpClient.BaseAddress + "signin-wsfed", new FormUrlEncodedContent(kvps));

            Assert.Equal(HttpStatusCode.Found, response.StatusCode);

            var request = new HttpRequestMessage(HttpMethod.Get, response.Headers.Location);
            var cookies = SetCookieHeaderValue.ParseList(response.Headers.GetValues(HeaderNames.SetCookie).ToList());
            foreach (var cookie in cookies)
            {
                if (cookie.Value.HasValue)
                {
                    request.Headers.Add(HeaderNames.Cookie, new CookieHeaderValue(cookie.Name, cookie.Value).ToString());
                }
            }
            response = await httpClient.SendAsync(request);

            // Did the request end in the actual resource requested for
            Assert.Equal(WsFederationDefaults.AuthenticationScheme, await response.Content.ReadAsStringAsync());
        }

        [Fact]
        public async Task InvalidTokenIsRejected()
        {
            var httpClient = CreateClient();

            // Verify if the request is redirected to STS with right parameters
            var response = await httpClient.GetAsync("/");
            var queryItems = QueryHelpers.ParseQuery(response.Headers.Location.Query);

            // Send an invalid token and verify that the token is not honored
            var kvps = new List<KeyValuePair<string, string>>();
            kvps.Add(new KeyValuePair<string, string>("wa", "wsignin1.0"));
            kvps.Add(new KeyValuePair<string, string>("wresult", File.ReadAllText(@"InvalidToken.xml")));
            kvps.Add(new KeyValuePair<string, string>("wctx", queryItems["wctx"]));
            response = await httpClient.PostAsync(queryItems["wreply"], new FormUrlEncodedContent(kvps));

            // Did the request end in the actual resource requested for
            Assert.Equal("AuthenticationFailed", await response.Content.ReadAsStringAsync());
        }

        private HttpClient CreateClient()
        {
            var builder = new WebHostBuilder()
                            .ConfigureServices(ConfigureAppServices)
                            .Configure(ConfigureApp);
            var server = new TestServer(builder);
            return server.CreateClient();
        }

        private void ConfigureAppServices(IServiceCollection services)
        {
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = WsFederationDefaults.AuthenticationScheme;
            })
            .AddWsFederation(options =>
            {
                options.Wtrealm = "http://Automation1";
                options.MetadataAddress = "https://login.windows.net/4afbc689-805b-48cf-a24c-d4aa3248a248/federationmetadata/2007-06/federationmetadata.xml";
                options.BackchannelHttpHandler = new WaadMetadataDocumentHandler();
                options.StateDataFormat = new CustomStateDataFormat();
                options.SecurityTokenHandlers = new List<ISecurityTokenValidator>() { new TestSecurityTokenValidator() };
                options.UseTokenLifetime = false;
                options.Events = new WsFederationEvents()
                {
                    MessageReceived = context =>
                    {
                        if (!context.ProtocolMessage.Parameters.TryGetValue("suppressWctx", out var suppress))
                        {
                            Assert.True(context.ProtocolMessage.Wctx.Equals("customValue"), "wctx is not my custom value");
                        }
                        context.HttpContext.Items["MessageReceived"] = true;
                        return Task.FromResult(0);
                    },
                    RedirectToIdentityProvider = context =>
                    {
                        if (context.ProtocolMessage.IsSignInMessage)
                        {
                            // Sign in message
                            context.ProtocolMessage.Wctx = "customValue";
                        }

                        return Task.FromResult(0);
                    },
                    SecurityTokenReceived = context =>
                    {
                        context.HttpContext.Items["SecurityTokenReceived"] = true;
                        return Task.FromResult(0);
                    },
                    SecurityTokenValidated = context =>
                    {
                        Assert.True((bool)context.HttpContext.Items["MessageReceived"], "MessageReceived notification not invoked");
                        Assert.True((bool)context.HttpContext.Items["SecurityTokenReceived"], "SecurityTokenReceived notification not invoked");

                        if (context.Principal != null)
                        {
                            var identity = context.Principal.Identities.Single();
                            identity.AddClaim(new Claim("ReturnEndpoint", "true"));
                            identity.AddClaim(new Claim("Authenticated", "true"));
                            identity.AddClaim(new Claim(identity.RoleClaimType, "Guest", ClaimValueTypes.String));
                        }

                        return Task.FromResult(0);
                    },
                    AuthenticationFailed = context =>
                    {
                        context.HttpContext.Items["AuthenticationFailed"] = true;
                        //Change the request url to something different and skip Wsfed. This new url will handle the request and let us know if this notification was invoked.
                        context.HttpContext.Request.Path = new PathString("/AuthenticationFailed");
                        context.SkipHandler();
                        return Task.FromResult(0);
                    }
                };
            })
            .AddCookie();
        }

        private void ConfigureApp(IApplicationBuilder app)
        {
            app.UseAuthentication();

            app.Map("/Logout", subApp =>
                {
                    subApp.Run(async context =>
                        {
                            if (context.User.Identity.IsAuthenticated)
                            {
                                var authProperties = new AuthenticationProperties() { RedirectUri = context.Request.GetEncodedUrl() };
                                await context.SignOutAsync(WsFederationDefaults.AuthenticationScheme, authProperties);
                                await context.Response.WriteAsync("Signing out...");
                            }
                            else
                            {
                                await context.Response.WriteAsync("SignedOut");
                            }
                        });
                });

            app.Map("/AuthenticationFailed", subApp =>
            {
                subApp.Run(async context =>
                {
                    await context.Response.WriteAsync("AuthenticationFailed");
                });
            });

            app.Map("/signout-wsfed", subApp =>
            {
                subApp.Run(async context =>
                {
                    await context.Response.WriteAsync("signout-wsfed");
                });
            });

            app.Run(async context =>
            {
                var result = context.AuthenticateAsync();
                if (context.User == null || !context.User.Identity.IsAuthenticated)
                {
                    await context.ChallengeAsync(WsFederationDefaults.AuthenticationScheme);
                    await context.Response.WriteAsync("Unauthorized");
                }
                else
                {
                    var identity = context.User.Identities.Single();
                    if (identity.NameClaimType == "Name_Failed" && identity.RoleClaimType == "Role_Failed")
                    {
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync("SignIn_Failed");
                    }
                    else if (!identity.HasClaim("Authenticated", "true") || !identity.HasClaim("ReturnEndpoint", "true") || !identity.HasClaim(identity.RoleClaimType, "Guest"))
                    {
                        await context.Response.WriteAsync("Provider not invoked");
                        return;
                    }
                    else
                    {
                        await context.Response.WriteAsync(WsFederationDefaults.AuthenticationScheme);
                    }
                }
            });
        }

        private class WaadMetadataDocumentHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var metadata = File.ReadAllText(@"federationmetadata.xml");
                var newResponse = new HttpResponseMessage() { Content = new StringContent(metadata, Encoding.UTF8, "text/xml") };
                return Task.FromResult<HttpResponseMessage>(newResponse);
            }
        }
    }
}
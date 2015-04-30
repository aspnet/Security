// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.TestHost;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.WebEncoders;
using Microsoft.IdentityModel.Protocols;
using Shouldly;
using Xunit;

namespace Microsoft.AspNet.Authentication.Tests.OpenIdConnect
{
    public class OpenIdConnectMiddlewareTests
    {
        static string noncePrefix = "OpenIdConnect." + "Nonce.";
        static string nonceDelimiter = ".";
        const string Challenge = "/challenge";
        const string ChallengeWithOutContext = "/challengeWithOutContext";
        const string ChallengeWithProperties = "/challengeWithProperties";
        const string DefaultHost = @"https://example.com";
        const string DefaultAuthority = @"https://login.windows.net/common";
        const string Logout = "/logout";
        const string Signin = "/signin";
        const string Signout = "/signout";

        [Fact]
        public async Task ChallengeWillSetDefaults()
        {
            var stateDataFormat = new AuthenticationPropertiesFormaterKeyValue();
            var queryValues = ExpectedQueryValues.Defaults(DefaultAuthority);
            queryValues.State = OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey + "=" + stateDataFormat.Protect(new AuthenticationProperties());
            var server = CreateServer(options =>
            {
                SetOptions(options, DefaultParameters(), queryValues);
            });

            var transaction = await SendAsync(server, DefaultHost + Challenge);
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            queryValues.CheckValues(transaction.Response.Headers.Location.AbsoluteUri, DefaultParameters());
        }

        [Fact]
        public async Task ChallengeWillSetNonceCookie()
        {
            var server = CreateServer(options =>
            {
                options.Authority = DefaultAuthority;
                options.ClientId = "Test Id";
                options.Configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            });
            var transaction = await SendAsync(server, DefaultHost + Challenge);
            transaction.SetCookie.Single().ShouldContain(OpenIdConnectAuthenticationDefaults.CookieNoncePrefix);
        }

        [Fact]
        public async Task ChallengeWillUseOptionsProperties()
        {
            var queryValues = new ExpectedQueryValues(DefaultAuthority);
            var server = CreateServer(options =>
            {
                SetOptions(options, DefaultParameters(), queryValues);
            });

            var transaction = await SendAsync(server, DefaultHost + Challenge);
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            queryValues.CheckValues(transaction.Response.Headers.Location.AbsoluteUri, DefaultParameters());
        }

        /// <summary>
        /// Tests for users who want to add 'state'. There are two ways to do it.
        /// 1. Users set 'state' (OpenIdConnectMessage.State) in the notification. The runtime appends to that state.
        /// 2. Users add to the AuthenticationProperties (notification.AuthenticationProperties), values will be serialized.
        /// </summary>
        /// <param name="userSetsState"></param>
        /// <returns></returns>
        [Theory]
        [InlineData(true, Challenge)]
        [InlineData(false, Challenge)]
        [InlineData(true, ChallengeWithOutContext)]
        [InlineData(false, ChallengeWithOutContext)]
        [InlineData(true, ChallengeWithProperties)]
        [InlineData(false, ChallengeWithProperties)]
        public async Task ChallengeSettingState(bool userSetsState, string challenge)
        {
            var queryValues = new ExpectedQueryValues(DefaultAuthority);
            var localProperties = new AuthenticationProperties();
            var stateDataFormat = new AuthenticationPropertiesFormaterKeyValue();
            AuthenticationProperties challengeProperties = null;
            if (challenge == ChallengeWithProperties)
            {
                challengeProperties = new AuthenticationProperties();
                challengeProperties.Items.Add("item1", Guid.NewGuid().ToString());
            }

            var server = CreateServer(options =>
            {
                SetOptions(options, DefaultParameters(new string[] { OpenIdConnectParameterNames.State }), queryValues);
                options.AutomaticAuthentication = challenge.Equals(ChallengeWithOutContext);
                options.Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = notification =>
                    {
                        if (userSetsState)
                        {
                            notification.ProtocolMessage.State = queryValues.State;
                        }
                        localProperties = new AuthenticationProperties(notification.AuthenticationProperties.Items);
                        return Task.FromResult<object>(null);
                    }

                };
            }, null, challengeProperties);

            var transaction = await SendAsync(server, DefaultHost + challenge);
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            if (userSetsState)
            {
                queryValues.State = OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey + "=" + stateDataFormat.Protect(localProperties) + "&userstate=" + queryValues.State;
            }
            else
            {
                queryValues.State = OpenIdConnectAuthenticationDefaults.AuthenticationPropertiesKey + "=" + stateDataFormat.Protect(localProperties);
            }

            queryValues.CheckValues(transaction.Response.Headers.Location.AbsoluteUri, DefaultParameters(new string[] { OpenIdConnectParameterNames.State }));
        }

        [Fact]
        public async Task ChallengeWillUseNotifications()
        {
            var queryValues = new ExpectedQueryValues(DefaultAuthority);
            var queryValuesSetInNotification = new ExpectedQueryValues(DefaultAuthority);
            var server = CreateServer(options =>
            {
                SetOptions(options, DefaultParameters(), queryValues);
                options.Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    RedirectToIdentityProvider = notification =>
                    {
                        notification.ProtocolMessage.ClientId = queryValuesSetInNotification.ClientId;
                        notification.ProtocolMessage.RedirectUri = queryValuesSetInNotification.RedirectUri;
                        notification.ProtocolMessage.Resource = queryValuesSetInNotification.Resource;
                        notification.ProtocolMessage.Scope = queryValuesSetInNotification.Scope;
                        return Task.FromResult<object>(null);
                    }
                };
            });

            var transaction = await SendAsync(server, DefaultHost + Challenge);
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            queryValuesSetInNotification.CheckValues(transaction.Response.Headers.Location.AbsoluteUri, DefaultParameters());
        }

        private void SetOptions(OpenIdConnectAuthenticationOptions options, List<string> parameters, ExpectedQueryValues queryValues, ISecureDataFormat<AuthenticationProperties> secureDataFormat = null)
        {
            foreach (var param in parameters)
            {
                if (param.Equals(OpenIdConnectParameterNames.ClientId))
                    options.ClientId = queryValues.ClientId;
                else if (param.Equals(OpenIdConnectParameterNames.RedirectUri))
                    options.RedirectUri = queryValues.RedirectUri;
                else if (param.Equals(OpenIdConnectParameterNames.Resource))
                    options.Resource = queryValues.Resource;
                else if (param.Equals(OpenIdConnectParameterNames.Scope))
                    options.Scope = queryValues.Scope;
            }

            options.Authority = queryValues.Authority;
            options.Configuration = queryValues.Configuration;
            options.StateDataFormat = secureDataFormat ?? new AuthenticationPropertiesFormaterKeyValue();
        }

        private List<string> DefaultParameters(string[] additionalParams = null)
        {
            var parameters =
                new List<string>
                {
                    OpenIdConnectParameterNames.ClientId,
                    OpenIdConnectParameterNames.RedirectUri,
                    OpenIdConnectParameterNames.Resource,
                    OpenIdConnectParameterNames.ResponseMode,
                    OpenIdConnectParameterNames.Scope,
                };

            if (additionalParams != null)
                parameters.AddRange(additionalParams);

            return parameters;
        }

        [Fact]
        public async Task SignOutWithDefaultRedirectUri()
        {
            var configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            var server = CreateServer(options =>
            {
                options.Authority = DefaultAuthority;
                options.ClientId = "Test Id";
                options.Configuration = configuration;
            });

            var transaction = await SendAsync(server, DefaultHost + Signout);
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.AbsoluteUri.ShouldBe(configuration.EndSessionEndpoint);
        }

        [Fact]
        public async Task SignOutWithCustomRedirectUri()
        {
            var configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            var server = CreateServer(options =>
            {
                options.Authority = DefaultAuthority;
                options.ClientId = "Test Id";
                options.Configuration = configuration;
                options.PostLogoutRedirectUri = "https://example.com/logout";
            });

            var transaction = await SendAsync(server, DefaultHost + Signout);
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.AbsoluteUri.ShouldContain(UrlEncoder.Default.UrlEncode("https://example.com/logout"));
        }

        [Fact]
        public async Task SignOutWith_Specific_RedirectUri_From_Authentication_Properites()
        {
            var configuration = ConfigurationManager.DefaultOpenIdConnectConfiguration();
            var server = CreateServer(options =>
            {
                options.Authority = DefaultAuthority;
                options.ClientId = "Test Id";
                options.Configuration = configuration;
                options.PostLogoutRedirectUri = "https://example.com/logout";
            });

            var transaction = await SendAsync(server, "https://example.com/signout_with_specific_redirect_uri");
            transaction.Response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            transaction.Response.Headers.Location.AbsoluteUri.ShouldContain(UrlEncoder.Default.UrlEncode("http://www.example.com/specific_redirect_uri"));
        }

        private static TestServer CreateServer(Action<OpenIdConnectAuthenticationOptions> configureOptions, Func<HttpContext, Task> handler = null, AuthenticationProperties properties = null)
        {
            return TestServer.Create(app =>
            {
                app.UseCookieAuthentication(options =>
                {
                    options.AuthenticationScheme = OpenIdConnectAuthenticationDefaults.AuthenticationScheme;
                });
                app.UseOpenIdConnectAuthentication(configureOptions);
                app.Use(async (context, next) =>
                {
                    var req = context.Request;
                    var res = context.Response;

                    if (req.Path == new PathString(Challenge))
                    {
                        context.Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationScheme);
                        res.StatusCode = 401;
                    }
                    else if (req.Path == new PathString(ChallengeWithProperties))
                    {
                        context.Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationScheme, properties);
                        res.StatusCode = 401;
                    }
                    else if (req.Path == new PathString(ChallengeWithOutContext))
                    {
                        res.StatusCode = 401;
                    }
                    else if (req.Path == new PathString(Signin))
                    {
                        // REVIEW: this used to just be res.SignIn()
                        context.Authentication.SignIn(OpenIdConnectAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal());
                    }
                    else if (req.Path == new PathString(Signout))
                    {
                        context.Authentication.SignOut(OpenIdConnectAuthenticationDefaults.AuthenticationScheme);
                    }
                    else if (req.Path == new PathString("/signout_with_specific_redirect_uri"))
                    {
                        context.Authentication.SignOut(
                            OpenIdConnectAuthenticationDefaults.AuthenticationScheme,
                            new AuthenticationProperties() { RedirectUri = "http://www.example.com/specific_redirect_uri" });
                    }
                    else if (handler != null)
                    {
                        await handler(context);
                    }
                    else
                    {
                        await next();
                    }
                });
            },
            services =>
            {
                services.AddAuthentication();
                services.Configure<ExternalAuthenticationOptions>(options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                });
            });
        }

        private static async Task<Transaction> SendAsync(TestServer server, string uri, string cookieHeader = null)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            if (!string.IsNullOrEmpty(cookieHeader))
            {
                request.Headers.Add("Cookie", cookieHeader);
            }
            var transaction = new Transaction
            {
                Request = request,
                Response = await server.CreateClient().SendAsync(request),
            };
            if (transaction.Response.Headers.Contains("Set-Cookie"))
            {
                transaction.SetCookie = transaction.Response.Headers.GetValues("Set-Cookie").ToList();
            }
            transaction.ResponseText = await transaction.Response.Content.ReadAsStringAsync();

            if (transaction.Response.Content != null &&
                transaction.Response.Content.Headers.ContentType != null &&
                transaction.Response.Content.Headers.ContentType.MediaType == "text/xml")
            {
                transaction.ResponseElement = XElement.Parse(transaction.ResponseText);
            }
            return transaction;
        }

        private class Transaction
        {
            public HttpRequestMessage Request { get; set; }

            public HttpResponseMessage Response { get; set; }

            public IList<string> SetCookie { get; set; }

            public string ResponseText { get; set; }

            public XElement ResponseElement { get; set; }

            public string AuthenticationCookieValue
            {
                get
                {
                    if (SetCookie != null && SetCookie.Count > 0)
                    {
                        var authCookie = SetCookie.SingleOrDefault(c => c.Contains(".AspNet.Cookie="));
                        if (authCookie != null)
                        {
                            return authCookie.Substring(0, authCookie.IndexOf(';'));
                        }
                    }

                    return null;
                }
            }
        }

        [Fact]
        // Test Cases for calculating the expiration time of cookie from cookie name
        public void NonceCookieExpirationTime()
        {
            DateTime utcNow = DateTime.UtcNow;

            GetNonceExpirationTime(noncePrefix + DateTime.MaxValue.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter, TimeSpan.FromHours(1)).ShouldBe(DateTime.MaxValue);

            GetNonceExpirationTime(noncePrefix + DateTime.MinValue.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter, TimeSpan.FromHours(1)).ShouldBe(DateTime.MinValue + TimeSpan.FromHours(1));

            GetNonceExpirationTime(noncePrefix + utcNow.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter, TimeSpan.FromHours(1)).ShouldBe(utcNow + TimeSpan.FromHours(1));

            GetNonceExpirationTime(noncePrefix, TimeSpan.FromHours(1)).ShouldBe(DateTime.MinValue);

            GetNonceExpirationTime("", TimeSpan.FromHours(1)).ShouldBe(DateTime.MinValue);

            GetNonceExpirationTime(noncePrefix + noncePrefix, TimeSpan.FromHours(1)).ShouldBe(DateTime.MinValue);

            GetNonceExpirationTime(noncePrefix + utcNow.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter + utcNow.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter, TimeSpan.FromHours(1)).ShouldBe(utcNow + TimeSpan.FromHours(1));

            GetNonceExpirationTime(utcNow.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter + utcNow.Ticks.ToString(CultureInfo.InvariantCulture) + nonceDelimiter, TimeSpan.FromHours(1)).ShouldBe(DateTime.MinValue);
        }

        private static DateTime GetNonceExpirationTime(string keyname, TimeSpan nonceLifetime)
        {
            DateTime nonceTime = DateTime.MinValue;
            string timestamp = null;
            int endOfTimestamp;
            if (keyname.StartsWith(noncePrefix, StringComparison.Ordinal))
            {
                timestamp = keyname.Substring(noncePrefix.Length);
                endOfTimestamp = timestamp.IndexOf('.');

                if (endOfTimestamp != -1)
                {
                    timestamp = timestamp.Substring(0, endOfTimestamp);
                    try
                    {
                        nonceTime = DateTime.FromBinary(Convert.ToInt64(timestamp, CultureInfo.InvariantCulture));
                        if ((nonceTime >= DateTime.UtcNow) && ((DateTime.MaxValue - nonceTime) < nonceLifetime))
                            nonceTime = DateTime.MaxValue;
                        else
                            nonceTime += nonceLifetime;
                    }
                    catch
                    {
                    }
                }
            }

            return nonceTime;
        }
    }
}
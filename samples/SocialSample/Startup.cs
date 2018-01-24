using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace SocialSample
{
    /* Note all servers must use the same address and port because these are pre-registered with the various providers. */
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            //if (string.IsNullOrEmpty(Configuration["facebook:appid"]))
            //{
            //    // User-Secrets: https://docs.asp.net/en/latest/security/app-secrets.html
            //    // See below for registration instructions for each provider.
            //    throw new InvalidOperationException("User secrets must be configured for each authentication provider.");
            //}

            services.AddAuthentication()
                .AddCookie(o => o.LoginPath = new PathString("/login"))
                // You must first create an app with Facebook and add its ID and Secret to your user-secrets.
                // https://developers.facebook.com/apps/
                //    .AddFacebook(o =>
                //{
                //    o.AppId = Configuration["facebook:appid"];
                //    o.AppSecret = Configuration["facebook:appsecret"];
                //    o.Scope.Add("email");
                //    o.Fields.Add("name");
                //    o.Fields.Add("email");
                //    o.SaveTokens = true;
                //    o.Events = new OAuthEvents()
                //    {
                //        OnRemoteFailure = HandleOnRemoteFailure
                //    };
                //})
                // You must first create an app with Google and add its ID and Secret to your user-secrets.
                // https://console.developers.google.com/project
            //    .AddOAuth("Google-AccessToken", "Google AccessToken only", o =>
            //{
            //    o.ClientId = Configuration["google:clientid"];
            //    o.ClientSecret = Configuration["google:clientsecret"];
            //    o.CallbackPath = new PathString("/signin-google-token");
            //    o.AuthorizationEndpoint = GoogleDefaults.AuthorizationEndpoint;
            //    o.TokenEndpoint = GoogleDefaults.TokenEndpoint;
            //    o.Scope.Add("openid");
            //    o.Scope.Add("profile");
            //    o.Scope.Add("email");
            //    o.SaveTokens = true;
            //    o.Events = new OAuthEvents()
            //    {
            //        OnRemoteFailure = HandleOnRemoteFailure
            //    };
            //})
                // You must first create an app with Google and add its ID and Secret to your user-secrets.
                // https://console.developers.google.com/project
                .UseGoogleSignIn(o =>
            {
                o.ClientId = Configuration["google:clientid"];
                o.ClientSecret = Configuration["google:clientsecret"];
                o.AuthorizationEndpoint += "?prompt=consent"; // Hack so we always get a refresh token, it only comes on the first authorization response
                o.AccessType = "offline";
                o.SaveTokens = true;
                o.Events = new OAuthEvents()
                {
                    OnRemoteFailure = HandleOnRemoteFailure
                };
                o.ClaimActions.MapJsonSubKey("urn:google:image", "image", "url");
                o.ClaimActions.Remove(ClaimTypes.GivenName);
            });
            //    // You must first create an app with Twitter and add its key and Secret to your user-secrets.
            //    // https://apps.twitter.com/
            //    .AddTwitter(o =>
            //{
            //    o.ConsumerKey = Configuration["twitter:consumerkey"];
            //    o.ConsumerSecret = Configuration["twitter:consumersecret"];
            //    // http://stackoverflow.com/questions/22627083/can-we-get-email-id-from-twitter-oauth-api/32852370#32852370
            //    // http://stackoverflow.com/questions/36330675/get-users-email-from-twitter-api-for-external-login-authentication-asp-net-mvc?lq=1
            //    o.RetrieveUserDetails = true;
            //    o.SaveTokens = true;
            //    o.ClaimActions.MapJsonKey("urn:twitter:profilepicture", "profile_image_url", ClaimTypes.Uri);
            //    o.Events = new TwitterEvents()
            //    {
            //        OnRemoteFailure = HandleOnRemoteFailure
            //    };
            //})
            //    /* Azure AD app model v2 has restrictions that prevent the use of plain HTTP for redirect URLs.
            //       Therefore, to authenticate through microsoft accounts, tryout the sample using the following URL:
            //       https://localhost:44318/
            //    */
            //    // You must first create an app with Microsoft Account and add its ID and Secret to your user-secrets.
            //    // https://apps.dev.microsoft.com/
            //    .AddOAuth("Microsoft-AccessToken", "Microsoft AccessToken only", o =>
            //{
            //    o.ClientId = Configuration["microsoftaccount:clientid"];
            //    o.ClientSecret = Configuration["microsoftaccount:clientsecret"];
            //    o.CallbackPath = new PathString("/signin-microsoft-token");
            //    o.AuthorizationEndpoint = MicrosoftAccountDefaults.AuthorizationEndpoint;
            //    o.TokenEndpoint = MicrosoftAccountDefaults.TokenEndpoint;
            //    o.Scope.Add("https://graph.microsoft.com/user.read");
            //    o.SaveTokens = true;
            //    o.Events = new OAuthEvents()
            //    {
            //        OnRemoteFailure = HandleOnRemoteFailure
            //    };
            //})
            //    // You must first create an app with Microsoft Account and add its ID and Secret to your user-secrets.
            //    // https://azure.microsoft.com/en-us/documentation/articles/active-directory-v2-app-registration/
            //    .AddMicrosoftAccount(o =>
            //{
            //    o.ClientId = Configuration["microsoftaccount:clientid"];
            //    o.ClientSecret = Configuration["microsoftaccount:clientsecret"];
            //    o.SaveTokens = true;
            //    o.Scope.Add("offline_access");
            //    o.Events = new OAuthEvents()
            //    {
            //        OnRemoteFailure = HandleOnRemoteFailure
            //    };
            //})
            //    // You must first create an app with GitHub and add its ID and Secret to your user-secrets.
            //    // https://github.com/settings/applications/
            //    .AddOAuth("GitHub-AccessToken", "GitHub AccessToken only", o =>
            //{
            //    o.ClientId = Configuration["github-token:clientid"];
            //    o.ClientSecret = Configuration["github-token:clientsecret"];
            //    o.CallbackPath = new PathString("/signin-github-token");
            //    o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
            //    o.TokenEndpoint = "https://github.com/login/oauth/access_token";
            //    o.SaveTokens = true;
            //    o.Events = new OAuthEvents()
            //    {
            //        OnRemoteFailure = HandleOnRemoteFailure
            //    };
            //})
            //    // You must first create an app with GitHub and add its ID and Secret to your user-secrets.
            //    // https://github.com/settings/applications/
            //    .AddOAuth("GitHub", "Github", o =>
            //{
            //    o.ClientId = Configuration["github:clientid"];
            //    o.ClientSecret = Configuration["github:clientsecret"];
            //    o.CallbackPath = new PathString("/signin-github");
            //    o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
            //    o.TokenEndpoint = "https://github.com/login/oauth/access_token";
            //    o.UserInformationEndpoint = "https://api.github.com/user";
            //    o.ClaimsIssuer = "OAuth2-Github";
            //    o.SaveTokens = true;
            //    // Retrieving user information is unique to each provider.
            //    o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            //    o.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
            //    o.ClaimActions.MapJsonKey("urn:github:name", "name");
            //    o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
            //    o.ClaimActions.MapJsonKey("urn:github:url", "url");
            //    o.Events = new OAuthEvents
            //    {
            //        OnRemoteFailure = HandleOnRemoteFailure,
            //        OnCreatingTicket = async context =>
            //        {
            //            // Get the GitHub user
            //            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            //            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            //            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            //            response.EnsureSuccessStatusCode();

            //            var user = JObject.Parse(await response.Content.ReadAsStringAsync());

            //            context.RunClaimActions(user);
            //        }
            //    };
            //});
        }

        private async Task HandleOnRemoteFailure(RemoteFailureContext context)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync("<html><body>");
            await context.Response.WriteAsync("A remote failure has occurred: " + UrlEncoder.Default.Encode(context.Failure.Message) + "<br>");

            if (context.Properties != null)
            {
                await context.Response.WriteAsync("Properties:<br>");
                foreach (var pair in context.Properties.Items)
                {
                    await context.Response.WriteAsync($"-{ UrlEncoder.Default.Encode(pair.Key)}={ UrlEncoder.Default.Encode(pair.Value)}<br>");
                }
            }

            await context.Response.WriteAsync("<a href=\"/\">Home</a>");
            await context.Response.WriteAsync("</body></html>");

            // context.Response.Redirect("/error?FailureMessage=" + UrlEncoder.Default.Encode(context.Failure.Message));

            context.HandleResponse();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseAuthentication();

            // Choose an authentication type
            app.Map("/login", signinApp =>
            {
                signinApp.Run(async context =>
                {
                    var authType = context.Request.Query["authscheme"];
                    if (!string.IsNullOrEmpty(authType))
                    {
                        // By default the client will be redirect back to the URL that issued the challenge (/login?authtype=foo),
                        // send them to the home page instead (/).
                        await context.ChallengeAsync(authType, new AuthenticationProperties() { RedirectUri = "/" });
                        return;
                    }

                    var response = context.Response;
                    response.ContentType = "text/html";
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("Choose an authentication scheme: <br>");
                    var schemeProvider = context.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
                    foreach (var provider in await schemeProvider.GetAllSchemesAsync())
                    {
                        await response.WriteAsync("<a href=\"?authscheme=" + provider.Name + "\">" + (provider.DisplayName ?? "(suppressed)") + "</a><br>");
                    }
                    await response.WriteAsync("</body></html>");
                });
            });

            // Refresh the access token
            app.Map("/refresh_token", signinApp =>
            {
                signinApp.Run(async context =>
                {
                    var response = context.Response;

                    // Setting DefaultAuthenticateScheme causes User to be set
                    // var user = context.User;

                    // This is what [Authorize] calls
                    var userResult = await context.AuthenticateAsync();
                    var user = userResult.Principal;
                    var authProperties = userResult.Properties;

                    // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                    // var user = await context.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);

                    // Deny anonymous request beyond this point.
                    if (!userResult.Succeeded || user == null || !user.Identities.Any(identity => identity.IsAuthenticated))
                    {
                        // This is what [Authorize] calls
                        // The cookie middleware will handle this and redirect to /login
                        await context.ChallengeAsync();

                        // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                        // await context.ChallengeAsync(MicrosoftAccountDefaults.AuthenticationScheme);

                        return;
                    }

                    var currentAuthType = user.Identities.First().AuthenticationType;
                    if (string.Equals(GoogleDefaults.AuthenticationScheme, currentAuthType)
                        || string.Equals(MicrosoftAccountDefaults.AuthenticationScheme, currentAuthType))
                    {
                        var refreshToken = authProperties.GetTokenValue("refresh_token");

                        if (string.IsNullOrEmpty(refreshToken))
                        {
                            response.ContentType = "text/html";
                            await response.WriteAsync("<html><body>");
                            await response.WriteAsync("No refresh_token is available.<br>");
                            await response.WriteAsync("<a href=\"/\">Home</a>");
                            await response.WriteAsync("</body></html>");
                            return;
                        }

                        var options = await GetOAuthOptionsAsync(context, currentAuthType);

                        var pairs = new Dictionary<string, string>()
                        {
                            { "client_id", options.ClientId },
                            { "client_secret", options.ClientSecret },
                            { "grant_type", "refresh_token" },
                            { "refresh_token", refreshToken }
                        };
                        var content = new FormUrlEncodedContent(pairs);
                        var refreshResponse = await options.Backchannel.PostAsync(options.TokenEndpoint, content, context.RequestAborted);
                        refreshResponse.EnsureSuccessStatusCode();

                        var payload = JObject.Parse(await refreshResponse.Content.ReadAsStringAsync());

                        // Persist the new acess token
                        authProperties.UpdateTokenValue("access_token", payload.Value<string>("access_token"));
                        refreshToken = payload.Value<string>("refresh_token");
                        if (!string.IsNullOrEmpty(refreshToken))
                        {
                            authProperties.UpdateTokenValue("refresh_token", refreshToken);
                        }
                        if (int.TryParse(payload.Value<string>("expires_in"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                        {
                            var expiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                            authProperties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));
                        }
                        await context.SignInAsync(user, authProperties);

                        await PrintRefreshedTokensAsync(response, payload, authProperties);

                        return;
                    }
                    // https://developers.facebook.com/docs/facebook-login/access-tokens/expiration-and-extension
                    else if (string.Equals(FacebookDefaults.AuthenticationScheme, currentAuthType))
                    {
                        var options = await GetOAuthOptionsAsync(context, currentAuthType);

                        var accessToken = authProperties.GetTokenValue("access_token");

                        var query = new QueryBuilder()
                        {
                            { "grant_type", "fb_exchange_token" },
                            { "client_id", options.ClientId },
                            { "client_secret", options.ClientSecret },
                            { "fb_exchange_token", accessToken },
                        }.ToQueryString();

                        var refreshResponse = await options.Backchannel.GetStringAsync(options.TokenEndpoint + query);
                        var payload = JObject.Parse(refreshResponse);

                        authProperties.UpdateTokenValue("access_token", payload.Value<string>("access_token"));
                        if (int.TryParse(payload.Value<string>("expires_in"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                        {
                            var expiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                            authProperties.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));
                        }
                        await context.SignInAsync(user, authProperties);

                        await PrintRefreshedTokensAsync(response, payload, authProperties);

                        return;
                    }

                    response.ContentType = "text/html";
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("Refresh has not been implemented for this provider.<br>");
                    await response.WriteAsync("<a href=\"/\">Home</a>");
                    await response.WriteAsync("</body></html>");
                });
            });

            // Sign-out to remove the user cookie.
            app.Map("/logout", signoutApp =>
            {
                signoutApp.Run(async context =>
                {
                    var response = context.Response;
                    response.ContentType = "text/html";
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("You have been logged out. Goodbye " + context.User.Identity.Name + "<br>");
                    await response.WriteAsync("<a href=\"/\">Home</a>");
                    await response.WriteAsync("</body></html>");
                });
            });

            // Display the remote error
            app.Map("/error", errorApp =>
            {
                errorApp.Run(async context =>
                {
                    var response = context.Response;
                    response.ContentType = "text/html";
                    await response.WriteAsync("<html><body>");
                    await response.WriteAsync("An remote failure has occurred: " + context.Request.Query["FailureMessage"] + "<br>");
                    await response.WriteAsync("<a href=\"/\">Home</a>");
                    await response.WriteAsync("</body></html>");
                });
            });


            app.Run(async context =>
            {
                // Setting DefaultAuthenticateScheme causes User to be set
                var user = context.User;

                // This is what [Authorize] calls
                // var user = await context.AuthenticateAsync();

                // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                // var user = await context.AuthenticateAsync(MicrosoftAccountDefaults.AuthenticationScheme);

                // Deny anonymous request beyond this point.
                if (user == null || !user.Identities.Any(identity => identity.IsAuthenticated))
                {
                    // This is what [Authorize] calls
                    // The cookie middleware will handle this and redirect to /login
                    await context.ChallengeAsync();

                    // This is what [Authorize(ActiveAuthenticationSchemes = MicrosoftAccountDefaults.AuthenticationScheme)] calls
                    // await context.ChallengeAsync(MicrosoftAccountDefaults.AuthenticationScheme);

                    return;
                }

                // Display user information
                var response = context.Response;
                response.ContentType = "text/html";
                await response.WriteAsync("<html><body>");
                await response.WriteAsync("Hello " + (context.User.Identity.Name ?? "anonymous") + "<br>");
                foreach (var claim in context.User.Claims)
                {
                    await response.WriteAsync(claim.Type + ": " + claim.Value + "<br>");
                }

                await response.WriteAsync("Tokens:<br>");

                await response.WriteAsync("Access Token: " + await context.GetTokenAsync("access_token") + "<br>");
                await response.WriteAsync("Refresh Token: " + await context.GetTokenAsync("refresh_token") + "<br>");
                await response.WriteAsync("Token Type: " + await context.GetTokenAsync("token_type") + "<br>");
                await response.WriteAsync("expires_at: " + await context.GetTokenAsync("expires_at") + "<br>");
                await response.WriteAsync("<a href=\"/logout\">Logout</a><br>");
                await response.WriteAsync("<a href=\"/refresh_token\">Refresh Token</a><br>");
                await response.WriteAsync("</body></html>");
            });
        }

        private Task<OAuthOptions> GetOAuthOptionsAsync(HttpContext context, string currentAuthType)
        {
            if (string.Equals(GoogleDefaults.AuthenticationScheme, currentAuthType))
            {
                return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<GoogleOptions>>().Get(currentAuthType));
            }
            else if (string.Equals(MicrosoftAccountDefaults.AuthenticationScheme, currentAuthType))
            {
                return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<MicrosoftAccountOptions>>().Get(currentAuthType));
            }
            else if (string.Equals(FacebookDefaults.AuthenticationScheme, currentAuthType))
            {
                return Task.FromResult<OAuthOptions>(context.RequestServices.GetRequiredService<IOptionsMonitor<FacebookOptions>>().Get(currentAuthType));
            }

            throw new NotImplementedException(currentAuthType);
        }

        private async Task PrintRefreshedTokensAsync(HttpResponse response, JObject payload, AuthenticationProperties authProperties)
        {
            response.ContentType = "text/html";
            await response.WriteAsync("<html><body>");
            await response.WriteAsync("Refreshed.<br>");
            await response.WriteAsync(HtmlEncoder.Default.Encode(payload.ToString()).Replace(",", ",<br>") + "<br>");

            await response.WriteAsync("<br>Tokens:<br>");

            await response.WriteAsync("Access Token: " + authProperties.GetTokenValue("access_token") + "<br>");
            await response.WriteAsync("Refresh Token: " + authProperties.GetTokenValue("refresh_token") + "<br>");
            await response.WriteAsync("Token Type: " + authProperties.GetTokenValue("token_type") + "<br>");
            await response.WriteAsync("expires_at: " + authProperties.GetTokenValue("expires_at") + "<br>");

            await response.WriteAsync("<a href=\"/\">Home</a><br>");
            await response.WriteAsync("<a href=\"/refresh_token\">Refresh Token</a><br>");
            await response.WriteAsync("</body></html>");
        }
    }
}


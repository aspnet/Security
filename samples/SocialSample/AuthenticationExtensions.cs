﻿using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SocialSample
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddCookieAuthentication(this IServiceCollection services)
        {
            services.AddAuthentication(options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
            return services;
        }

        public static IApplicationBuilder UseAuthentication(this IApplicationBuilder app, IConfiguration Configuration)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                LoginPath = new PathString("/login")
            });

            // You must first create an app with facebook and add it's ID and Secret to your config.json or user-secrets.
            // https://developers.facebook.com/apps/
            app.UseFacebookAuthentication(new FacebookOptions
            {
                AppId = Configuration["facebook:appid"],
                AppSecret = Configuration["facebook:appsecret"],
                Scope = { "email" },
                Fields = { "name", "email" },
                SaveTokens = true,
            });

            // See config.json
            app.UseOAuthAuthentication(new OAuthOptions
            {
                AuthenticationScheme = "Google-AccessToken",
                DisplayName = "Google-AccessToken",
                ClientId = Configuration["google:clientid"],
                ClientSecret = Configuration["google:clientsecret"],
                CallbackPath = new PathString("/signin-google-token"),
                AuthorizationEndpoint = GoogleDefaults.AuthorizationEndpoint,
                TokenEndpoint = GoogleDefaults.TokenEndpoint,
                Scope = { "openid", "profile", "email" },
                SaveTokens = true
            });

            // See config.json
            // https://console.developers.google.com/project
            app.UseGoogleAuthentication(new GoogleOptions
            {
                ClientId = Configuration["google:clientid"],
                ClientSecret = Configuration["google:clientsecret"],
                SaveTokens = true,
                Events = new OAuthEvents()
                {
                    OnRemoteFailure = ctx =>
                    {
                        ctx.Response.Redirect("/error?FailureMessage=" + UrlEncoder.Default.Encode(ctx.Failure.Message));
                        ctx.HandleResponse();
                        return Task.FromResult(0);
                    }
                }
            });

            // See config.json
            // https://apps.twitter.com/
            app.UseTwitterAuthentication(new TwitterOptions
            {
                ConsumerKey = Configuration["twitter:consumerkey"],
                ConsumerSecret = Configuration["twitter:consumersecret"],
                SaveTokens = true,
                Events = new TwitterEvents()
                {
                    OnRemoteFailure = ctx =>
                    {
                        ctx.Response.Redirect("/error?FailureMessage=" + UrlEncoder.Default.Encode(ctx.Failure.Message));
                        ctx.HandleResponse();
                        return Task.FromResult(0);
                    }
                }
            });

            /* Azure AD app model v2 has restrictions that prevent the use of plain HTTP for redirect URLs.
               Therefore, to authenticate through microsoft accounts, tryout the sample using the following URL:
               https://localhost:54541/
            */
            // See config.json
            // https://apps.dev.microsoft.com/
            app.UseOAuthAuthentication(new OAuthOptions
            {
                AuthenticationScheme = "Microsoft-AccessToken",
                DisplayName = "MicrosoftAccount-AccessToken",
                ClientId = Configuration["msa:clientid"],
                ClientSecret = Configuration["msa:clientsecret"],
                CallbackPath = new PathString("/signin-microsoft-token"),
                AuthorizationEndpoint = MicrosoftAccountDefaults.AuthorizationEndpoint,
                TokenEndpoint = MicrosoftAccountDefaults.TokenEndpoint,
                Scope = { "https://graph.microsoft.com/user.read" },
                SaveTokens = true
            });

            // See config.json
            // https://azure.microsoft.com/en-us/documentation/articles/active-directory-v2-app-registration/
            app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions
            {
                DisplayName = "MicrosoftAccount",
                ClientId = Configuration["msa:clientid"],
                ClientSecret = Configuration["msa:clientsecret"],
                SaveTokens = true
            });

            // See config.json
            // https://github.com/settings/applications/
            app.UseOAuthAuthentication(new OAuthOptions
            {
                AuthenticationScheme = "GitHub-AccessToken",
                DisplayName = "Github-AccessToken",
                ClientId = Configuration["github-token:clientid"],
                ClientSecret = Configuration["github-token:clientsecret"],
                CallbackPath = new PathString("/signin-github-token"),
                AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
                TokenEndpoint = "https://github.com/login/oauth/access_token",
                SaveTokens = true
            });

            // See config.json
            app.UseOAuthAuthentication(new OAuthOptions
            {
                AuthenticationScheme = "GitHub",
                DisplayName = "Github",
                ClientId = Configuration["github:clientid"],
                ClientSecret = Configuration["github:clientsecret"],
                CallbackPath = new PathString("/signin-github"),
                AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
                TokenEndpoint = "https://github.com/login/oauth/access_token",
                UserInformationEndpoint = "https://api.github.com/user",
                ClaimsIssuer = "OAuth2-Github",
                SaveTokens = true,
                // Retrieving user information is unique to each provider.
                Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        // Get the GitHub user
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        var user = JObject.Parse(await response.Content.ReadAsStringAsync());

                        var identifier = user.Value<string>("id");
                        if (!string.IsNullOrEmpty(identifier))
                        {
                            context.Identity.AddClaim(new Claim(
                                ClaimTypes.NameIdentifier, identifier,
                                ClaimValueTypes.String, context.Options.ClaimsIssuer));
                        }

                        var userName = user.Value<string>("login");
                        if (!string.IsNullOrEmpty(userName))
                        {
                            context.Identity.AddClaim(new Claim(
                                ClaimsIdentity.DefaultNameClaimType, userName,
                                ClaimValueTypes.String, context.Options.ClaimsIssuer));
                        }

                        var name = user.Value<string>("name");
                        if (!string.IsNullOrEmpty(name))
                        {
                            context.Identity.AddClaim(new Claim(
                                "urn:github:name", name,
                                ClaimValueTypes.String, context.Options.ClaimsIssuer));
                        }

                        var link = user.Value<string>("url");
                        if (!string.IsNullOrEmpty(link))
                        {
                            context.Identity.AddClaim(new Claim(
                                "urn:github:url", link,
                                ClaimValueTypes.String, context.Options.ClaimsIssuer));
                        }
                    }
                }
            });

            // Choose an authentication type
            app.Map("/login", signoutApp =>
            {
                signoutApp.Run(async context =>
                {
                    var authType = context.Request.Query["authscheme"];
                    if (!string.IsNullOrEmpty(authType))
                    {
                        // By default the client will be redirect back to the URL that issued the challenge (/login?authtype=foo),
                        // send them to the home page instead (/).
                        await context.Authentication.ChallengeAsync(authType, new AuthenticationProperties() { RedirectUri = "/" });
                        return;
                    }

                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("<html><body>");
                    await context.Response.WriteAsync("Choose an authentication scheme: <br>");
                    foreach (var type in context.Authentication.GetAuthenticationSchemes())
                    {
                        await context.Response.WriteAsync("<a href=\"?authscheme=" + type.AuthenticationScheme + "\">" + (type.DisplayName ?? "(suppressed)") + "</a><br>");
                    }
                    await context.Response.WriteAsync("</body></html>");
                });
            });

            // Sign-out to remove the user cookie.
            app.Map("/logout", signoutApp =>
            {
                signoutApp.Run(async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await context.Response.WriteAsync("<html><body>");
                    await context.Response.WriteAsync("You have been logged out. Goodbye " + context.User.Identity.Name + "<br>");
                    await context.Response.WriteAsync("<a href=\"/\">Home</a>");
                    await context.Response.WriteAsync("</body></html>");
                });
            });


            // Display the remote error
            app.Map("/error", errorApp =>
            {
                errorApp.Run(async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("<html><body>");
                    await context.Response.WriteAsync("An remote failure has occurred: " + context.Request.Query["FailureMessage"] + "<br>");
                    await context.Response.WriteAsync("<a href=\"/\">Home</a>");
                    await context.Response.WriteAsync("</body></html>");
                });
            });

            return app;
        }
    }
}

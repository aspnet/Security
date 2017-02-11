using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication2;
using Microsoft.AspNetCore.Authentication2.Cookies;
using Microsoft.AspNetCore.Authentication2.Google;
using Microsoft.AspNetCore.Authentication2.OAuth;
//using Microsoft.AspNetCore.Authentication2.MicrosoftAccount;
using Microsoft.AspNetCore.Authentication2.Twitter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                //builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCookieAuthentication(o => o.LoginPath = "/login");
            services.AddTwitterAuthentication(options =>
            {
                options.ConsumerKey = "BSdJJ0CrDuvEhpkchnukXZBUv";
                options.ConsumerSecret = "xKUNuKhsRdHD03eLn67xhPAyE1wFFEndFo1X2UJaK2m1jdAxf4";
                options.RetrieveUserDetails = true;
                options.SaveTokens = true;
                options.Events = new TwitterEvents()
                {
                    OnCreatingTicket = ctx =>
                    {
                        var profilePic = ctx.User.Value<string>("profile_image_url");
                        ctx.Principal.Identities.First().AddClaim(new Claim("urn:twitter:profilepicture", profilePic, ClaimTypes.Uri, ctx.Options.ClaimsIssuer));
                        return Task.FromResult(0);
                    },
                    OnRemoteFailure = ctx =>
                    {
                        ctx.Response.Redirect("/error?FailureMessage=" + UrlEncoder.Default.Encode(ctx.Failure.Message));
                        ctx.HandleResponse();
                        return Task.FromResult(0);
                    }
                };
            });
            services.AddGoogleAuthentication(options =>
            {
                options.ClientId = "667949426586-0bor1qj05d9fjqkvhil6tvoupjfv46fr.apps.googleusercontent.com";
                options.ClientSecret = "tPycZp08PGQBNDz2XycEB145";
                options.SaveTokens = true;
                options.Events = new OAuthEvents()
                {
                    OnRemoteFailure = ctx =>
                    {
                        ctx.Response.Redirect("/error?FailureMessage=" + UrlEncoder.Default.Encode(ctx.Failure.Message));
                        ctx.HandleResponse();
                        return Task.FromResult(0);
                    }
                };
            });


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                //options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole(LogLevel.Information);

            // Simple error page to avoid a repo dependency.
            app.Use(async (context, next) =>
            {
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    if (context.Response.HasStarted)
                    {
                        throw;
                    }
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(ex.ToString());
                }
            });

            app.UseAuthentication();

            //if (string.IsNullOrEmpty(Configuration["facebook:appid"]))
            //{
            //    // User-Secrets: https://docs.asp.net/en/latest/security/app-secrets.html
            //    // See below for registration instructions for each provider.
            //    throw new InvalidOperationException("User secrets must be configured for each authentication provider.");
            //}

            //// You must first create an app with Facebook and add its ID and Secret to your user-secrets.
            //// https://developers.facebook.com/apps/
            //app.UseFacebookAuthentication(new FacebookOptions
            //{
            //    AppId = Configuration["facebook:appid"],
            //    AppSecret = Configuration["facebook:appsecret"],
            //    Scope = { "email" },
            //    Fields = { "name", "email" },
            //    SaveTokens = true,
            //});

            //// You must first create an app with Google and add its ID and Secret to your user-secrets.
            //// https://console.developers.google.com/project
            //app.UseOAuthAuthentication(new OAuthOptions
            //{
            //    AuthenticationScheme = "Google-AccessToken",
            //    DisplayName = "Google-AccessToken",
            //    ClientId = Configuration["google:clientid"],
            //    ClientSecret = Configuration["google:clientsecret"],
            //    CallbackPath = new PathString("/signin-google-token"),
            //    AuthorizationEndpoint = GoogleDefaults.AuthorizationEndpoint,
            //    TokenEndpoint = GoogleDefaults.TokenEndpoint,
            //    Scope = { "openid", "profile", "email" },
            //    SaveTokens = true
            //});

            ///* Azure AD app model v2 has restrictions that prevent the use of plain HTTP for redirect URLs.
            //   Therefore, to authenticate through microsoft accounts, tryout the sample using the following URL:
            //   https://localhost:44318/
            //*/
            //// You must first create an app with Microsoft Account and add its ID and Secret to your user-secrets.
            //// https://apps.dev.microsoft.com/
            //app.UseOAuthAuthentication(new OAuthOptions
            //{
            //    AuthenticationScheme = "Microsoft-AccessToken",
            //    DisplayName = "MicrosoftAccount-AccessToken",
            //    ClientId = Configuration["microsoftaccount:clientid"],
            //    ClientSecret = Configuration["microsoftaccount:clientsecret"],
            //    CallbackPath = new PathString("/signin-microsoft-token"),
            //    AuthorizationEndpoint = MicrosoftAccountDefaults.AuthorizationEndpoint,
            //    TokenEndpoint = MicrosoftAccountDefaults.TokenEndpoint,
            //    Scope = { "https://graph.microsoft.com/user.read" },
            //    SaveTokens = true
            //});

            //// You must first create an app with Microsoft Account and add its ID and Secret to your user-secrets.
            //// https://azure.microsoft.com/en-us/documentation/articles/active-directory-v2-app-registration/
            //app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions
            //{
            //    DisplayName = "MicrosoftAccount",
            //    ClientId = Configuration["microsoftaccount:clientid"],
            //    ClientSecret = Configuration["microsoftaccount:clientsecret"],
            //    SaveTokens = true
            //});

            //// You must first create an app with GitHub and add its ID and Secret to your user-secrets.
            //// https://github.com/settings/applications/
            //app.UseOAuthAuthentication(new OAuthOptions
            //{
            //    AuthenticationScheme = "GitHub-AccessToken",
            //    DisplayName = "Github-AccessToken",
            //    ClientId = Configuration["github-token:clientid"],
            //    ClientSecret = Configuration["github-token:clientsecret"],
            //    CallbackPath = new PathString("/signin-github-token"),
            //    AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
            //    TokenEndpoint = "https://github.com/login/oauth/access_token",
            //    SaveTokens = true
            //});

            //// You must first create an app with GitHub and add its ID and Secret to your user-secrets.
            //// https://github.com/settings/applications/
            //app.UseOAuthAuthentication(new OAuthOptions
            //{
            //    AuthenticationScheme = "GitHub",
            //    DisplayName = "Github",
            //    ClientId = Configuration["github:clientid"],
            //    ClientSecret = Configuration["github:clientsecret"],
            //    CallbackPath = new PathString("/signin-github"),
            //    AuthorizationEndpoint = "https://github.com/login/oauth/authorize",
            //    TokenEndpoint = "https://github.com/login/oauth/access_token",
            //    UserInformationEndpoint = "https://api.github.com/user",
            //    ClaimsIssuer = "OAuth2-Github",
            //    SaveTokens = true,
            //    // Retrieving user information is unique to each provider.
            //    Events = new OAuthEvents
            //    {
            //        OnCreatingTicket = async context =>
            //        {
            //            // Get the GitHub user
            //            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            //            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
            //            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //            var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
            //            response.EnsureSuccessStatusCode();

            //            var user = JObject.Parse(await response.Content.ReadAsStringAsync());

            //            var identifier = user.Value<string>("id");
            //            if (!string.IsNullOrEmpty(identifier))
            //            {
            //                context.Identity.AddClaim(new Claim(
            //                    ClaimTypes.NameIdentifier, identifier,
            //                    ClaimValueTypes.String, context.Options.ClaimsIssuer));
            //            }

            //            var userName = user.Value<string>("login");
            //            if (!string.IsNullOrEmpty(userName))
            //            {
            //                context.Identity.AddClaim(new Claim(
            //                    ClaimsIdentity.DefaultNameClaimType, userName,
            //                    ClaimValueTypes.String, context.Options.ClaimsIssuer));
            //            }

            //            var name = user.Value<string>("name");
            //            if (!string.IsNullOrEmpty(name))
            //            {
            //                context.Identity.AddClaim(new Claim(
            //                    "urn:github:name", name,
            //                    ClaimValueTypes.String, context.Options.ClaimsIssuer));
            //            }

            //            var email = user.Value<string>("email");
            //            if (!string.IsNullOrEmpty(email))
            //            {
            //                context.Identity.AddClaim(new Claim(
            //                    ClaimTypes.Email, email,
            //                    ClaimValueTypes.Email, context.Options.ClaimsIssuer));
            //            }

            //            var link = user.Value<string>("url");
            //            if (!string.IsNullOrEmpty(link))
            //            {
            //                context.Identity.AddClaim(new Claim(
            //                    "urn:github:url", link,
            //                    ClaimValueTypes.String, context.Options.ClaimsIssuer));
            //            }
            //        }
            //    }
            //});

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
                        await context.ChallengeAsync(authType, new AuthenticationProperties2() { RedirectUri = "/" });
                        return;
                    }

                    context.Response.ContentType = "text/html";
                    await context.Response.WriteAsync("<html><body>");
                    await context.Response.WriteAsync("Choose an authentication scheme: <br>");

                    //foreach (var type in context.Authentication.GetAuthenticationSchemes())
                    foreach (var type in context.RequestServices.GetRequiredService<IOptions<AuthenticationOptions2>>().Value.Schemes)
                    {
                        // TODO: display name (lives on schema instance options)?
                        await context.Response.WriteAsync("<a href=\"?authscheme=" + type.Name + "\">" + (type.Name ?? "(suppressed)") + "</a><br>");
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
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
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


            app.Run(async context =>
            {
                // CookieAuthenticationOptions.AutomaticAuthenticate = true (default) causes User to be set
                var user = context.User;

                // Display user information
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync("<html><body>");
                await context.Response.WriteAsync("Hello " + (context.User.Identity.Name ?? "anonymous") + "<br>");
                foreach (var claim in context.User.Claims)
                {
                    await context.Response.WriteAsync(claim.Type + ": " + claim.Value + "<br>");
                }

                await context.Response.WriteAsync("Tokens:<br>");
                
                //await context.Response.WriteAsync("Access Token: " + await context.Authentication.GetTokenAsync("access_token") + "<br>");
                //await context.Response.WriteAsync("Refresh Token: " + await context.Authentication.GetTokenAsync("refresh_token") + "<br>");
                //await context.Response.WriteAsync("Token Type: " + await context.Authentication.GetTokenAsync("token_type") + "<br>");
                //await context.Response.WriteAsync("expires_at: " + await context.Authentication.GetTokenAsync("expires_at") + "<br>");
                await context.Response.WriteAsync("<a href=\"/logout\">Logout</a><br>");
                await context.Response.WriteAsync("</body></html>");
            });
        }
    }
}


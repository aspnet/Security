using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Newtonsoft.Json.Linq;

namespace OpenIdConnectSample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Environment = env;

            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; set; }

        public IHostingEnvironment Environment { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                sharedOptions.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                .AddCookie()
                .AddOpenIdConnect(o =>
            {
                o.ClientId = Configuration["oidc:clientid"];
                o.ClientSecret = Configuration["oidc:clientsecret"]; // for code flow
                o.Authority = Configuration["oidc:authority"];

                o.ResponseType = OpenIdConnectResponseType.CodeIdToken;
                o.SaveTokens = true;
                o.GetClaimsFromUserInfoEndpoint = true;
                o.AccessDeniedPath = "/access-denied-from-remote";

                o.ClaimActions.MapAllExcept("aud", "iss", "iat", "nbf", "exp", "aio", "c_hash", "uti", "nonce");

                o.Events = new OpenIdConnectEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.HandleResponse();

                        c.Response.StatusCode = 500;
                        c.Response.ContentType = "text/plain";
                        if (Environment.IsDevelopment())
                        {
                            // Debug only, in production do not share exceptions with the remote host.
                            return c.Response.WriteAsync(c.Exception.ToString());
                        }
                        return c.Response.WriteAsync("An error occurred processing your authentication.");
                    }
                };
            });
        }

        public void Configure(IApplicationBuilder app, IOptionsMonitor<OpenIdConnectOptions> optionsMonitor)
        {
            app.UseDeveloperExceptionPage();
            app.UseAuthentication();

            app.Run(async context =>
            {
                var response = context.Response;

                if (context.Request.Path.Equals("/signedout"))
                {
                    await WriteHtmlAsync(response, async res =>
                    {
                        await res.WriteAsync($"<h1>You have been signed out.</h1>");
                        await res.WriteAsync("<a class=\"btn btn-default\" href=\"/\">Home</a>");
                    });
                    return;
                }

                if (context.Request.Path.Equals("/signout"))
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await WriteHtmlAsync(response, async res =>
                    {
                        await res.WriteAsync($"<h1>Signed out {HtmlEncode(context.User.Identity.Name)}</h1>");
                        await res.WriteAsync("<a class=\"btn btn-default\" href=\"/\">Home</a>");
                    });
                    return;
                }

                if (context.Request.Path.Equals("/signout-remote"))
                {
                    // Redirects
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties()
                    {
                        RedirectUri = "/signedout"
                    });
                    return;
                }

                if (context.Request.Path.Equals("/access-denied-from-remote"))
                {
                    await WriteHtmlAsync(response, async res =>
                    {
                        await res.WriteAsync($"<h1>Access Denied error received from the remote authorization server</h1>");
                        await res.WriteAsync("<a class=\"btn btn-default\" href=\"/\">Home</a>");
                    });
                    return;
                }

                if (context.Request.Path.Equals("/Account/AccessDenied"))
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    await WriteHtmlAsync(response, async res =>
                    {
                        await res.WriteAsync($"<h1>Access Denied for user {HtmlEncode(context.User.Identity.Name)} to resource '{HtmlEncode(context.Request.Query["ReturnUrl"])}'</h1>");
                        await res.WriteAsync("<a class=\"btn btn-default\" href=\"/signout\">Sign Out</a>");
                        await res.WriteAsync("<a class=\"btn btn-default\" href=\"/\">Home</a>");
                    });
                    return;
                }

                // DefaultAuthenticateScheme causes User to be set
                // var user = context.User;

                // This is what [Authorize] calls
                var userResult = await context.AuthenticateAsync();
                var user = userResult.Principal;
                var props = userResult.Properties;

                // This is what [Authorize(ActiveAuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)] calls
                // var user = await context.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);

                // Not authenticated
                if (user == null || !user.Identities.Any(identity => identity.IsAuthenticated))
                {
                    // This is what [Authorize] calls
                    await context.ChallengeAsync();

                    // This is what [Authorize(ActiveAuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)] calls
                    // await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme);

                    return;
                }

                // Authenticated, but not authorized
                if (context.Request.Path.Equals("/restricted") && !user.Identities.Any(identity => identity.HasClaim("special", "true")))
                {
                    await context.ForbidAsync();
                    return;
                }

                if (context.Request.Path.Equals("/refresh"))
                {
                    var refreshToken = props.GetTokenValue("refresh_token");

                    if (string.IsNullOrEmpty(refreshToken))
                    {
                        await WriteHtmlAsync(response, async res =>
                        {
                            await res.WriteAsync($"No refresh_token is available.<br>");
                            await res.WriteAsync("<a class=\"btn btn-link\" href=\"/signout\">Sign Out</a>");
                        });

                        return;
                    }

                    var options = optionsMonitor.Get(OpenIdConnectDefaults.AuthenticationScheme);
                    var metadata = await options.ConfigurationManager.GetConfigurationAsync(context.RequestAborted);

                    var pairs = new Dictionary<string, string>()
                    {
                        { "client_id", options.ClientId },
                        { "client_secret", options.ClientSecret },
                        { "grant_type", "refresh_token" },
                        { "refresh_token", refreshToken }
                    };
                    var content = new FormUrlEncodedContent(pairs);
                    var tokenResponse = await options.Backchannel.PostAsync(metadata.TokenEndpoint, content, context.RequestAborted);
                    tokenResponse.EnsureSuccessStatusCode();

                    var payload = JObject.Parse(await tokenResponse.Content.ReadAsStringAsync());

                    // Persist the new acess token
                    props.UpdateTokenValue("access_token", payload.Value<string>("access_token"));
                    props.UpdateTokenValue("refresh_token", payload.Value<string>("refresh_token"));
                    if (int.TryParse(payload.Value<string>("expires_in"), NumberStyles.Integer, CultureInfo.InvariantCulture, out var seconds))
                    {
                        var expiresAt = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(seconds);
                        props.UpdateTokenValue("expires_at", expiresAt.ToString("o", CultureInfo.InvariantCulture));
                    }
                    await context.SignInAsync(user, props);

                    await WriteHtmlAsync(response, async res =>
                    {
                        await res.WriteAsync($"<h1>Refreshed.</h1>");
                        await res.WriteAsync("<a class=\"btn btn-default\" href=\"/refresh\">Refresh tokens</a>");
                        await res.WriteAsync("<a class=\"btn btn-default\" href=\"/\">Home</a>");

                        await res.WriteAsync("<h2>Tokens:</h2>");
                        await WriteTableHeader(res, new string[] { "Token Type", "Value" }, props.GetTokens().Select(token => new string[] { token.Name, token.Value }));

                        await res.WriteAsync("<h2>Payload:</h2>");
                        await res.WriteAsync(HtmlEncoder.Default.Encode(payload.ToString()).Replace(",", ",<br>") + "<br>");
                    });

                    return;
                }

                if (context.Request.Path.Equals("/login-challenge"))
                {
                    // Challenge the user authentication, and force a login prompt by overwriting the
                    // "prompt". This could be used for example to require the user to re-enter their
                    // credentials at the authentication provider, to add an extra confirmation layer.
                    await context.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new OpenIdConnectChallengeProperties()
                    {
                        Prompt = "login",

                        // it is also possible to specify different scopes, e.g.
                        // Scope = new string[] { "openid", "profile", "other" }
                    });

                    return;
                }

                await WriteHtmlAsync(response, async res =>
                {
                    await res.WriteAsync($"<h1>Hello Authenticated User {HtmlEncode(user.Identity.Name)}</h1>");
                    await res.WriteAsync("<a class=\"btn btn-default\" href=\"/refresh\">Refresh tokens</a>");
                    await res.WriteAsync("<a class=\"btn btn-default\" href=\"/restricted\">Restricted</a>");
                    await res.WriteAsync("<a class=\"btn btn-default\" href=\"/login-challenge\">Login challenge</a>");
                    await res.WriteAsync("<a class=\"btn btn-default\" href=\"/signout\">Sign Out</a>");
                    await res.WriteAsync("<a class=\"btn btn-default\" href=\"/signout-remote\">Sign Out Remote</a>");

                    await res.WriteAsync("<h2>Claims:</h2>");
                    await WriteTableHeader(res, new string[] { "Claim Type", "Value" }, context.User.Claims.Select(c => new string[] { c.Type, c.Value }));

                    await res.WriteAsync("<h2>Tokens:</h2>");
                    await WriteTableHeader(res, new string[] { "Token Type", "Value" }, props.GetTokens().Select(token => new string[] { token.Name, token.Value }));
                });
            });
        }

        private static async Task WriteHtmlAsync(HttpResponse response, Func<HttpResponse, Task> writeContent)
        {
            var bootstrap = "<link rel=\"stylesheet\" href=\"https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css\" integrity=\"sha384-BVYiiSIFeK1dGmJRAkycuHAHRg32OmUcww7on3RYdg4Va+PmSTsz/K68vbdEjh4u\" crossorigin=\"anonymous\">";

            response.ContentType = "text/html";
            await response.WriteAsync($"<html><head>{bootstrap}</head><body><div class=\"container\">");
            await writeContent(response);
            await response.WriteAsync("</div></body></html>");
        }

        private static async Task WriteTableHeader(HttpResponse response, IEnumerable<string> columns, IEnumerable<IEnumerable<string>> data)
        {
            await response.WriteAsync("<table class=\"table table-condensed\">");
            await response.WriteAsync("<tr>");
            foreach (var column in columns)
            {
                await response.WriteAsync($"<th>{HtmlEncode(column)}</th>");
            }
            await response.WriteAsync("</tr>");
            foreach (var row in data)
            {
                await response.WriteAsync("<tr>");
                foreach (var column in row)
                {
                    await response.WriteAsync($"<td>{HtmlEncode(column)}</td>");
                }
                await response.WriteAsync("</tr>");
            }
            await response.WriteAsync("</table>");
        }

        private static string HtmlEncode(string content) =>
            string.IsNullOrEmpty(content) ? string.Empty : HtmlEncoder.Default.Encode(content);
    }
}


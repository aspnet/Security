using System.Linq;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Http.Features.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace OpenIdConnectSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(sharedOptions => sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole(LogLevel.Information);

            app.UseIISPlatformHandler();

            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthenticate = true;
            });

            app.UseOpenIdConnectAuthentication(options =>
            {
                options.ClientId = "63a87a83-64b9-4ac1-b2c5-092126f8474f";
                options.ClientSecret = "Yse2iP7tO1Azq0iDajNisMaTSnIDv+FXmAsFuXr+Cy8="; // for code flow
                options.Authority = "https://login.windows.net/tratcheroutlook.onmicrosoft.com";
                options.ResponseType = OpenIdConnectResponseTypes.Code;
                options.GetClaimsFromUserInfoEndpoint = true;
            });

            app.Run(async context =>
            {
                var authContext = new AuthenticateContext(CookieAuthenticationDefaults.AuthenticationScheme);
                await context.Authentication.AuthenticateAsync(authContext);
                if (context.Request.Path == new PathString("/remote-sign-out"))
                {
                    var session = authContext.Properties[OpenIdConnectSessionProperties.SessionState];
                    var sid = context.Request.Query["sid"];
                    if (string.Equals(sid, session, System.StringComparison.Ordinal))
                    {
                        await context.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                        await context.Response.WriteAsync("Signed out remotely.");
                        return;
                    }
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Missing or mismatched sid.");
                    return;
                }

                if (!context.User.Identities.Any(identity => identity.IsAuthenticated))
                {
                    await context.Authentication.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });

                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Hello First timer");
                    return;
                }

                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Hello Authenticated User\r\n");
                foreach (var claim in context.User.Claims)
                {
                    await context.Response.WriteAsync($"{claim.Type}, {claim.Value}\r\n");
                }
                foreach (var property in authContext.Properties)
                {
                    await context.Response.WriteAsync($"{property.Key}, {property.Value}\r\n");
                }

                await context.Response.WriteAsync($"Logout: /remote-sign-out?sid={authContext.Properties[OpenIdConnectSessionProperties.SessionState]}\r\n");
            });
        }
    }
}

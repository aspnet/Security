﻿using System.Linq;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace OpenIdConnectSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options => options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole(LogLevel.Information);

            app.UseCookieAuthentication(options =>
            {
                options.AutomaticAuthentication = true;
            });

            app.UseOpenIdConnectAuthentication(options =>
            {
                options.ClientId = "63a87a83-64b9-4ac1-b2c5-092126f8474f";
                options.Authority = "https://login.windows.net/tratcheroutlook.onmicrosoft.com";
                options.RedirectUri = "http://localhost:42023";
            });

            app.Run(async context =>
            {
                if (!context.User.Identities.Any(identity => identity.IsAuthenticated))
                {
                    await context.Authentication.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });

                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Hello First timer");
                    return;
                }

                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Hello Authenticated User");
            });
        }
    }
}

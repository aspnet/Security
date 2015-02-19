﻿using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Security;
using Microsoft.AspNet.Security.Cookies;
using Microsoft.AspNet.Security.OpenIdConnect;
using Microsoft.Framework.DependencyInjection;

namespace OpenIdConnectSample
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseServices(services =>
            {
                services.AddDataProtection();
                services.Configure<ExternalAuthenticationOptions>(options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                });

            });

            app.UseCookieAuthentication(options =>
            {
            });

            app.UseOpenIdConnectAuthentication(options =>
                {
                    options.ClientId = "fe78e0b4-6fe7-47e6-812c-fb75cee266a4";
                    options.Authority = "https://login.windows.net/cyrano.onmicrosoft.com";
                    options.RedirectUri = "http://localhost:42023";
                });

            app.Run(async context =>
            {
                if (context.User == null || !context.User.Identity.IsAuthenticated)
                {
                    context.Response.Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectAuthenticationDefaults.AuthenticationScheme);

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

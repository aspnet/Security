using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Authentication;
using Microsoft.AspNet.Authentication;
using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Authentication.OpenIdConnect;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;

namespace OpenIdConnectSample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication();
            services.Configure<ExternalAuthenticationOptions>(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });
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
                    options.ClientId = "ba7651c2-53c2-442a-97c2-3d60ea42f403";
                    options.Authority = "https://login.windows.net/tushartest.onmicrosoft.com";
                    options.RedirectUri = "http://localhost:42023";
                });

            app.Run(async context =>
            {
                if (string.IsNullOrEmpty(context.User.Identity.Name))
                {
                    context.Authentication.Challenge(OpenIdConnectAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });

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

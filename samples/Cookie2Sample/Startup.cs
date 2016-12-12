using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication2;
using Microsoft.AspNetCore.Authentication2.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cookie2Sample
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(o => o.AddCookies(CookieAuthenticationDefaults.AuthenticationScheme, cookieOpt => { }));
            services.AddTransient<CookieAuthenticationHandler>();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole(LogLevel.Information);

            app.UseAuthentication();

            app.Run(async context =>
            {
                var auth = context.RequestServices.GetRequiredService<IAuthenticationManager2>();
                if (!context.User.Identities.Any(identity => identity.IsAuthenticated))
                {
                    var user = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "bob") }, CookieAuthenticationDefaults.AuthenticationScheme));
                    await auth.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user);

                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Hello First timer");
                    return;
                }

                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("Hello old timer");
            });
        }
    }
}

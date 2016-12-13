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
            services.AddCookieAuthentication();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole(LogLevel.Information);

            app.UseAuthentication();

            app.Run(async context =>
            {
                if (context.Request.Path == CookieAuthenticationDefaults.AccessDeniedPath)
                {
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Access Denied");
                    return;
                }

                if (context.Request.Path == "/login")
                {
                    var u = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "bob") }, CookieAuthenticationDefaults.AuthenticationScheme));
                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, u);

                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Logged in");
                    return;
                }

                if (context.Request.Path == "/logout")
                {
                    await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Logged out");
                    return;
                }

                if (context.Request.Path == "/forbid")
                {
                    await context.ForbidAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    return;
                }

                if (context.Request.Path == CookieAuthenticationDefaults.LoginPath)
                {
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Normally this would log you in, but you have to go to /login");
                    return;
                }

                // [Authorize] would usually handle this
                var user = context.User; // We can do this because of UseAuthentication
                if (user?.Identity?.IsAuthenticated ?? false)
                {
                    context.Response.ContentType = "text/plain";
                    await context.Response.WriteAsync("Hello old timer");
                }
                else
                {
                    await context.ChallengeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                }
            });
        }
    }
}

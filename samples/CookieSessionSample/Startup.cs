using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Security.Cookies;

namespace CookieSessionSample
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseServices(services => { });
            app.UseCookieAuthentication(options => 
            {
                options.SessionStore = new MemoryCacheSessionStore();
            });

            app.Run(async context =>
            {
                if (context.User.Identity == null || !context.User.Identity.IsAuthenticated)
                {
                    // Make a large identity
                    var claims = new List<Claim>(1001);
                    claims.Add(new Claim("name", "bob"));
                    for (int i = 0; i < 1000; i++)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, "SomeRandomGroup" + i, ClaimValueTypes.String, "IssuedByBob", "OriginalIssuerJoe"));
                    }
                    context.Response.SignIn(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationType));                    

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
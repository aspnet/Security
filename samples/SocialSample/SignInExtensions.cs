using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace SocialSample
{
    public static class SignInExtensions {
        // Adds google auth and a default cookie auth for sign in purposes
        public static AuthenticationBuilder UseGoogleSignIn(this AuthenticationBuilder builder, Action<GoogleOptions> configureOptions)
        {
            builder.AddGoogle(GoogleDefaults.AuthenticationScheme, o =>
            {
                configureOptions?.Invoke(o);
                // Override instead of default since this method is opinionated on the cookie scheme name.
                o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            });

            builder.ConfigureCookie(GoogleDefaults.AuthenticationScheme);
            return builder;
        }

        // Add the defalut cookie if needed, set it to default and configure the cookie to forward challenge
        private static void ConfigureCookie(this AuthenticationBuilder builder, string forwardChallenge)
        {
            builder.Services.Configure<AuthenticationOptions>(o =>
            {
                o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                if (!o.SchemeMap.ContainsKey(CookieAuthenticationDefaults.AuthenticationScheme))
                {
                    // TODO: make it easier to reuse AddCookie
                    builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPostConfigureOptions<CookieAuthenticationOptions>, PostConfigureCookieAuthenticationOptions>());
                    builder.AddScheme<CookieAuthenticationOptions, CookieAuthenticationHandler>(CookieAuthenticationDefaults.AuthenticationScheme, CookieAuthenticationDefaults.AuthenticationScheme, _ => { });
                }
            });
            // Always configure the cookie's challenge
            builder.Services.Configure<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme, o => o.ForwardChallenge = forwardChallenge);
        }
    }
}


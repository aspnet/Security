using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication.Vkontakte
{
    public static class VkontakteAppBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="VkontakteMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables Vkontakte authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseVkontakteAuthentication(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<VkontakteMiddleware>();
        }

        /// <summary>
        /// Adds the <see cref="VkontakteMiddleware"/> middleware to the specified <see cref="IApplicationBuilder"/>, which enables Vkontakte authentication capabilities.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> to add the middleware to.</param>
        /// <param name="options">A <see cref="VkontakteOptions"/> that specifies options for the middleware.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IApplicationBuilder UseVkontakteAuthentication(this IApplicationBuilder app, VkontakteOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return app.UseMiddleware<VkontakteMiddleware>(Options.Create(options));
        }
    }
}
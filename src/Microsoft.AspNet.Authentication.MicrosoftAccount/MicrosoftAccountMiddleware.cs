// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Authentication.OAuth;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.DataProtection;
using Microsoft.Framework.Internal;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Authentication.MicrosoftAccount
{
    /// <summary>
    /// An ASP.NET middleware for authenticating users using the Microsoft Account service.
    /// </summary>
    public class MicrosoftAccountMiddleware : OAuthMiddleware<MicrosoftAccountOptions>
    {
        /// <summary>
        /// Initializes a new <see cref="MicrosoftAccountMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the HTTP pipeline to invoke.</param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="encoder"></param>
        /// <param name="sharedOptions"></param>
        /// <param name="options">Configuration options for the middleware.</param>
        /// <param name="configureOptions"></param>
        public MicrosoftAccountMiddleware(
            [NotNull] RequestDelegate next,
            [NotNull] IDataProtectionProvider dataProtectionProvider,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IUrlEncoder encoder,
            [NotNull] IOptions<SharedAuthenticationOptions> sharedOptions,
            [NotNull] MicrosoftAccountOptions options)
            : base(next, dataProtectionProvider, loggerFactory, encoder, sharedOptions, options)
        {
            if (Options.Scope.Count == 0)
            {
                // LiveID requires a scope string, so if the user didn't set one we go for the least possible.
                // TODO: Should we just add these by default when we create the Options?
                Options.Scope.Add("wl.basic");
            }
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="MicrosoftAccountOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<MicrosoftAccountOptions> CreateHandler()
        {
            return new MicrosoftAccountHandler(Backchannel);
        }
    }
}

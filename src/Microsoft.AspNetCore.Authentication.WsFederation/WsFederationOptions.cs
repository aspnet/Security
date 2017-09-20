﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.WsFederation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens.Saml;
using Microsoft.IdentityModel.Tokens.Saml2;

namespace Microsoft.AspNetCore.Authentication.WsFederation
{
    /// <summary>
    /// Configuration options for <see cref="WsFederationHandler"/>
    /// </summary>
    public class WsFederationOptions : RemoteAuthenticationOptions
    {
        private ICollection<ISecurityTokenValidator> _securityTokenHandlers = new Collection<ISecurityTokenValidator>()
        {
            new Saml2SecurityTokenHandler(),
            new SamlSecurityTokenHandler(),
            new JwtSecurityTokenHandler()
        };
        private TokenValidationParameters _tokenValidationParameters = new TokenValidationParameters();

        /// <summary>
        /// Initializes a new <see cref="WsFederationOptions"/>
        /// </summary>
        public WsFederationOptions()
        {
            CallbackPath = "/signin-wsfed";
            Events = new WsFederationEvents();
        }

        /// <summary>
        /// Check that the options are valid.  Should throw an exception if things are not ok.
        /// </summary>
        public override void Validate()
        {
            base.Validate();

            if (ConfigurationManager == null)
            {
                throw new InvalidOperationException($"Provide {nameof(MetadataAddress)}, "
                + $"{nameof(Configuration)}, or {nameof(ConfigurationManager)} to {nameof(WsFederationOptions)}");
            }
        }

        /// <summary>
        /// Configuration provided directly by the developer. If provided, then MetadataAddress and the Backchannel properties
        /// will not be used. This information should not be updated during request processing.
        /// </summary>
        public WsFederationConfiguration Configuration { get; set; }

        /// <summary>
        /// Gets or sets the address to retrieve the wsFederation metadata
        /// </summary>
        public string MetadataAddress { get; set; }

        /// <summary>
        /// Responsible for retrieving, caching, and refreshing the configuration from metadata.
        /// If not provided, then one will be created using the MetadataAddress and Backchannel properties.
        /// </summary>
        public IConfigurationManager<WsFederationConfiguration> ConfigurationManager { get; set; }

        /// <summary>
        /// Gets or sets if a metadata refresh should be attempted after a SecurityTokenSignatureKeyNotFoundException. This allows for automatic
        /// recovery in the event of a signature key rollover. This is enabled by default.
        /// </summary>
        public bool RefreshOnIssuerKeyNotFound { get; set; } = true;

        /// <summary>
        /// Indicates if requests to the CallbackPath may also be for other components. If enabled the handler will pass
        /// requests through that do not contain OpenIdConnect authentication responses. Disabling this and setting the
        /// CallbackPath to a dedicated endpoint may provide better error handling.
        /// This is disabled by default.
        /// </summary>
        public bool SkipUnrecognizedRequests { get; set; } = false;

        /// <summary>
        /// Gets or sets the <see cref="WsFederationEvents"/> to call when processing WsFederation messages.
        /// </summary>
        public new WsFederationEvents Events
        {
            get => (WsFederationEvents)base.Events;
            set => base.Events = value;
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="ISecurityTokenValidator"/> used to read and validate the <see cref="SecurityToken"/>s.
        /// </summary>
        public ICollection<ISecurityTokenValidator> SecurityTokenHandlers
        {
            get
            {
                return _securityTokenHandlers;
            }
            set
            {
                _securityTokenHandlers = value ?? throw new ArgumentNullException(nameof(SecurityTokenHandlers));
            }
        }

        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TokenValidationParameters"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"> if 'TokenValidationParameters' is null.</exception>
        public TokenValidationParameters TokenValidationParameters
        {
            get
            {
                return _tokenValidationParameters;
            }
            set
            {
                _tokenValidationParameters = value ?? throw new ArgumentNullException(nameof(TokenValidationParameters));
            }
        }

        /// <summary>
        /// Gets or sets the 'wreply'.
        /// </summary>
        public string Wreply { get; set; }

        /// <summary>
        /// Gets or sets the 'wreply' value used during sign-out.
        /// If none is specified then the value from the Wreply field is used.
        /// </summary>
        public string SignOutWreply { get; set; }
        
        /// <summary>
        /// Gets or sets the 'wtrealm'.
        /// </summary>
        public string Wtrealm { get; set; }

        /// <summary>
        /// Indicates that the authentication session lifetime (e.g. cookies) should match that of the authentication token.
        /// If the token does not provide lifetime information then normal session lifetimes will be used.
        /// This is enabled by default.
        /// </summary>
        public bool UseTokenLifetime { get; set; } = true;

        /// <summary>
        /// Gets or sets if HTTPS is required for the metadata address or authority.
        /// The default is true. This should be disabled only in development environments.
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = true;
    }
}
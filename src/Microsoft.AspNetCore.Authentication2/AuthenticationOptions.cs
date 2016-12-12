// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Base Options for all authentication middleware.
    /// </summary>
    public abstract class AuthenticationOptions
    {
        private string _authenticationScheme;

        /// <summary>
        /// The AuthenticationScheme in the options corresponds to the logical name for a particular authentication scheme. A different
        /// value may be assigned in order to use the same authentication middleware type more than once in a pipeline.
        /// </summary>
        public string AuthenticationScheme
        {
            get { return _authenticationScheme; }
            set
            {
                _authenticationScheme = value;
                Description.AuthenticationScheme = value;
            }
        }

        /// <summary>
        /// If true the authentication middleware alter the request user coming in. If false the authentication middleware will only provide
        /// identity when explicitly indicated by the AuthenticationScheme.
        /// </summary>
        public bool AutomaticAuthenticate { get; set; }

        /// <summary>
        /// If true the authentication middleware should handle automatic challenge.
        /// If false the authentication middleware will only alter responses when explicitly indicated by the AuthenticationScheme.
        /// </summary>
        public bool AutomaticChallenge { get; set; }

        /// <summary>
        /// Gets or sets the issuer that should be used for any claims that are created
        /// </summary>
        public string ClaimsIssuer { get; set; }

        /// <summary>
        /// Additional information about the authentication type which is made available to the application.
        /// </summary>
        public AuthenticationDescription Description { get; set; } = new AuthenticationDescription();
    }
}

namespace Microsoft.AspNetCore.Authentication2
{
    public class AuthenticationOptions2
    {
        public IDictionary<string, AuthenticationScheme> SchemeMap { get; }

        public void AddScheme(string name, Action<AuthenticationSchemeBuilder> configureBuilder)
        {
            var builder = new AuthenticationSchemeBuilder(name);
            configureBuilder(builder);
            SchemeMap[name] = builder.Build();
        }

        public string DefaultAuthenticationScheme { get; set; }

        public string DefaultChallengeScheme { get; set; }
    }

    public class AuthenticationSchemeBuilder
    {
        public AuthenticationSchemeBuilder(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Type HandlerType { get; set; }

        // Holds things like configured options instances for the handler
        public Dictionary<string, object> Settings { get; set; }


        // TODO: add back the description/display name metadata bag here

        public AuthenticationScheme Build() => new AuthenticationScheme(Name, HandlerType, Settings);
    }

    public class AuthenticationScheme
    {
        public AuthenticationScheme(string name, Type handlerType, Dictionary<string, object> settings)
        {
            // todo: throw for null/empty name, handlerType null or not IAuthenticationSchemeHandler
            Name = name;
            HandlerType = handlerType;
            Settings = settings;
        }

        public string Name { get; }
        public Type HandlerType { get; }

        public IReadOnlyDictionary<string, object> Settings { get; }

        public IAuthenticationSchemeHandler ResolveHandler(HttpContext context)
        {
            return context.RequestServices.GetService(HandlerType) as IAuthenticationSchemeHandler;
        }

        // TODO: add back the description/display name metadata bag here
    }
}

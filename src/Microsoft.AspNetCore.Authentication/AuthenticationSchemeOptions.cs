// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Contains the options used by the <see cref="AuthenticationHandler{T}"/>.
    /// </summary>
    public class AuthenticationSchemeOptions
    {
        /// <summary>
        /// Check that the options are valid. Should throw an exception if things are not ok.
        /// </summary>
        public virtual void Validate() { }

        /// <summary>
        /// Checks that the options are valid for a specific scheme
        /// </summary>
        /// <param name="scheme">The scheme being validated.</param>
        public virtual void Validate(string scheme)
            => Validate();

        /// <summary>
        /// Gets or sets the issuer that should be used for any claims that are created
        /// </summary>
        public string ClaimsIssuer { get; set; }

        /// <summary>
        /// Instance used for events
        /// </summary>
        public object Events { get; set; }

        /// <summary>
        /// If set, will be used as the service type to get the Events instance instead of the property.
        /// </summary>
        public Type EventsType { get; set; }

        /// <summary>
        /// Used to forward authentication handler methods.
        /// </summary>
        public SchemeForwardingOptions SchemeForwarding { get; set; } = new SchemeForwardingOptions();

        /// <summary>
        /// Used to redirect authentication methods to another scheme
        /// </summary>
        public class SchemeForwardingOptions
        {
            /// <summary>
            /// If enabled, the authentication methods should be retargetted to the specified scheme.
            /// </summary>
            public bool Enabled { get; set; }

            public string DefaultTarget { get; set; }

            public string AuthenticateTarget { get; set; }
            public string ChallengeTarget { get; set; }
            public string ForbidTarget { get; set; }
            public string SignInTarget { get; set; }
            public string SignOutTarget { get; set; }

            /// <summary>
            /// Used to 
            /// </summary>
            public Func<HttpContext, string> DefaultTargetSelector { get; set; }

        }

    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Contains the options used by the <see cref="AuthenticationSchemeHandler{T}"/>.
    /// </summary>
    public abstract class AuthenticationSchemeOptions
    {
        /// <summary>
        /// Check that the options are valid.  Should throw an exception if things are not ok.
        /// </summary>
        public virtual void Validate()
        {
        }

        /// <summary>
        /// The AuthenticationScheme in the options corresponds to the logical name for a particular authentication scheme. A different
        /// value may be assigned in order to use the same authentication middleware type more than once in a pipeline.
        /// </summary>
        public string AuthenticationScheme { get; set; }

        /// <summary>
        /// Gets or sets the display name for the authentication provider.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the issuer that should be used for any claims that are created
        /// </summary>
        public string ClaimsIssuer { get; set; }

        /// <summary>
        /// Instance used for events TODO
        /// </summary>
        public object Events { get; set; }

        /// <summary>
        /// If set, will be used as the service type to get the Events instance instead of the property.
        /// </summary>
        public Type EventsType { get; set; }
    }
}

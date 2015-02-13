// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Http.Security;

namespace Microsoft.AspNet.Security
{
    /// <summary>
    /// Base Options for all authentication middleware
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
        /// If Active the authentication middleware alter the request user coming in and
        /// alter 401 Unauthorized responses going out. If Passive the authentication middleware will only provide
        /// identity and alter responses when explicitly indicated by the AuthenticationScheme.
        /// </summary>
        public AuthenticationMode AuthenticationMode { get; set; } = AuthenticationMode.Active;

        /// <summary>
        /// Additional information about the authentication type which is made available to the application.
        /// </summary>
        public AuthenticationDescription Description { get; set; } = new AuthenticationDescription();
    }
}

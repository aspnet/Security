// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    public class SharedAuthenticationOptions
    {
        public SharedAuthenticationOptions(IEnumerable<IConfigureOptions<SharedAuthenticationOptions>> configureOptions = null)
        {
            if (configureOptions != null)
            {
                foreach (var configure in configureOptions)
                {
                    configure.Configure(this);
                }
            }
        }

        /// <summary>
        /// Gets or sets the authentication scheme corresponding to the default middleware
        /// responsible of persisting user's identity after a successful authentication.
        /// This value typically corresponds to a cookie middleware registered in the Startup class.
        /// </summary>
        public string SignInScheme { get; set; }
    }
}

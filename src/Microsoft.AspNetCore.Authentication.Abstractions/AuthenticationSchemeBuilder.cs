// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Authentication
{
    public class AuthenticationSchemeBuilder
    {
        public AuthenticationSchemeBuilder(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public Type HandlerType { get; set; }

        // Holds things like the configured options instances for the handler
        public Dictionary<string, object> Settings { get; set; } = new Dictionary<string, object>(); // casing?

        public AuthenticationScheme Build() => new AuthenticationScheme(Name, HandlerType, Settings);
    }
}

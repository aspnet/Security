// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

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

        public AuthenticationScheme Build() => new AuthenticationScheme(Name, HandlerType);
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Authorization
{
    public interface IAuthorizationData
    {
        string Policy { get; set; }

        // REVIEW: can we get rid of the , deliminated in Roles/AuthTypes
        string Roles { get; set; }

        string ActiveAuthenticationSchemes { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : Attribute, IAuthorizationData
    {
        public AuthorizeAttribute() { }

        public AuthorizeAttribute(string policy)
        {
            Policy = policy;
        }

        public string Policy { get; set; }

        // REVIEW: can we get rid of the , deliminated in Roles/AuthTypes
        public string Roles { get; set; }

        public string ActiveAuthenticationSchemes { get; set; }
    }
}

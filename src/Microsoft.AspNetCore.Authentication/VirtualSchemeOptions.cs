// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Used to redirect authentication methods to another scheme
    /// </summary>
    public class VirtualSchemeOptions
    {
        public string DefaultTarget { get; set; }

        public string AuthenticateTarget { get; set; }
        public string ChallengeTarget { get; set; }
        public string ForbidTarget { get; set; }
        public string SignInTarget { get; set; }
        public string SignOutTarget { get; set; }

        /// <summary>
        /// Used to select a default target scheme based on the request.
        /// </summary>
        public Func<HttpContext, string> DefaultTargetSelector { get; set; }
    }
}
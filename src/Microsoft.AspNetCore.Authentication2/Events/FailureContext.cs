// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    /// <summary>
    /// Provides failure context information to middleware providers.
    /// </summary>
    public class FailureContext : BaseControlContext
    {
        public FailureContext(HttpContext context, Exception failure)
            : base(context)
        {
            Failure = failure;
        }

        /// <summary>
        /// User friendly error message for the error.
        /// </summary>
        public Exception Failure { get; set; }
    }
}

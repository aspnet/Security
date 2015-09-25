// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNet.Http;

namespace Microsoft.AspNet.Authentication
{
    /// <summary>
    /// Provides error context information to middleware providers.
    /// </summary>
    public class ErrorContext : BaseContext
    {
        public ErrorContext(HttpContext context, string errorMessage)
            : base(context)
        {
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// Exception which caused the error.
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// User friendly error message for the error.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Uri to redirect to with errors.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "By design")]
        public string ErrorHandlerUri { get; set; }

        /// <summary>
        /// Used to determine if the error was handled (via redirect or another means)
        /// </summary>
        public bool Handled { get; set; }
    }
}

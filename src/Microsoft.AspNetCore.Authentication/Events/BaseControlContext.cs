// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    public class BaseControlContext<TOptions> : BaseContext<TOptions> where TOptions : AuthenticationSchemeOptions
    {
        protected BaseControlContext(
            HttpContext context,
            AuthenticationScheme scheme,
            TOptions options)
            : base(context, scheme, options)
        {
        }

        public EventResultState State { get; set; }

        public bool HandledResponse
        {
            get { return State == EventResultState.HandledResponse; }
        }

        public bool Skipped
        {
            get { return State == EventResultState.Skipped; }
        }

        /// <summary>
        /// Discontinue all processing for this request and return to the client.
        /// The caller is responsible for generating the full response.
        /// </summary>
        public void HandleResponse()
        {
            State = EventResultState.HandledResponse;
        }

        /// <summary>
        /// Discontinue processing the request in the current handler.
        /// </summary>
        public void SkipToNextMiddleware()
        {
            State = EventResultState.Skipped;
        }
    }
}
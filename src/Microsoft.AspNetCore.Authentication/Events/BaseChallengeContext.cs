//// Copyright (c) .NET Foundation. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//using System;
//using System.Security.Claims;
//using Microsoft.AspNetCore.Http;

//namespace Microsoft.AspNetCore.Authentication
//{
//    /// <summary>
//    /// Base context for authentication.
//    /// </summary>
//    public abstract class BaseChallengeContext<THandler> : HandlerContext<THandler> where THandler : IAuthenticationHandler
//    {
//        /// <summary>
//        /// Constructor.
//        /// </summary>
//        /// <param name="context">The context.</param>
//        /// <param name="scheme">The authentication scheme.</param>
//        /// <param name="options">The authentication options associated with the scheme.</param>
//        /// <param name="properties">The authentication properties.</param>
//        protected BaseChallengeContext(THandler handler, HttpContext context, AuthenticationProperties properties)
//            : base(context, scheme, options)
//        {
//            Properties = properties;
//        }

//        public AuthenticationProperties Properties { get; set; }
//    }
//}
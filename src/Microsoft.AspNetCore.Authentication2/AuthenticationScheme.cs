// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication2
{
    public class AuthenticationScheme
    {
        public AuthenticationScheme(string name, Type handlerType, Dictionary<string, object> settings, AuthenticationOptions2 sharedOptions)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            if (!typeof(IAuthenticationSchemeHandler).IsAssignableFrom(handlerType))
            {
                throw new ArgumentException("handlerType must implement IAuthenticationSchemeHandler.");
            }
            HandlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            SharedOptions = sharedOptions ?? throw new ArgumentNullException(nameof(sharedOptions));
        }

        public string Name { get; }
        public Type HandlerType { get; }

        public Func<HttpContext, IAuthenticationSchemeHandler> ResolveHandlerFunc { get; set; }

        // Holds things like the configured options instances for the handler
        // Also replacement for AuthenticationDescription
        public IReadOnlyDictionary<string, object> Settings { get; }

        public AuthenticationOptions2 SharedOptions { get; }

        public virtual IAuthenticationSchemeHandler ResolveHandler(HttpContext context)
        {
            return ResolveHandlerFunc?.Invoke(context) ?? 
                context.RequestServices.GetService(HandlerType) as IAuthenticationSchemeHandler;
        }
    }
}

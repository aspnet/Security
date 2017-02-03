// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication
{
    public class DefaultAuthenticationHandlerResolver : IAuthenticationHandlerResolver
    {
        public DefaultAuthenticationHandlerResolver(IAuthenticationSchemeProvider schemes)
        {
            Schemes = schemes;
        }

        public IAuthenticationSchemeProvider Schemes { get; }

        // handler instance cache, need to initialize once per request
        private Dictionary<string, IAuthenticationHandler> _handlerMap = new Dictionary<string, IAuthenticationHandler>();

        public async Task<IAuthenticationHandler> ResolveHandlerAsync(HttpContext context, string authenticationScheme)
        {
            if (_handlerMap.ContainsKey(authenticationScheme))
            {
                return _handlerMap[authenticationScheme];
            }

            var scheme = await Schemes.GetSchemeAsync(authenticationScheme);
            var handler = scheme?.ResolveHandler(context);
            if (handler != null)
            {
                await handler.InitializeAsync(scheme, context);
                _handlerMap[authenticationScheme] = handler;
            }
            return handler;
        }
    }
}

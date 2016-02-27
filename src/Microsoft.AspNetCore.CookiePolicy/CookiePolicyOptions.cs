// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    public class CookiePolicyOptions
    {
        public CookiePolicyOptions(IEnumerable<IConfigureOptions<CookiePolicyOptions>> configureOptions = null)
        {
            if (configureOptions != null)
            {
                foreach (var configure in configureOptions)
                {
                    configure.Configure(this);
                }
            }
        }

        public HttpOnlyPolicy HttpOnly { get; set; } = HttpOnlyPolicy.None;
        public SecurePolicy Secure { get; set; } = SecurePolicy.None;

        public Action<AppendCookieContext> OnAppendCookie { get; set; }
        public Action<DeleteCookieContext> OnDeleteCookie { get; set; }
    }
}
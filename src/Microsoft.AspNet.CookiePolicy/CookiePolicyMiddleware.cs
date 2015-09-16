// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Features;
using Microsoft.AspNet.Http.Features.Internal;

namespace Microsoft.AspNet.CookiePolicy
{
    public class CookiePolicyMiddleware
    {
        private readonly RequestDelegate _next;

        public CookiePolicyMiddleware(
            RequestDelegate next,
            CookiePolicyOptions options)
        {
            Options = options;
            _next = next;
        }

        public CookiePolicyOptions Options { get; set; }

        public Task Invoke(HttpContext context)
        {
            // REVIEW: Do we need to check if there is a Cookie feature already present like SendFile??
            context.Features.Set<IResponseCookiesFeature>(new CookiesWrapperFeature(context, Options));

            return _next(context);
        }

        private class CookiesWrapperFeature : IResponseCookiesFeature
        {
            public CookiesWrapperFeature(HttpContext context, CookiePolicyOptions options)
            {
                Wrapper = new CookiesWrapper(context, options, context.Response.Cookies);
            }

            public IResponseCookies Wrapper { get; }

            public IResponseCookies Cookies
            {
                get
                {
                    return Wrapper;
                }
            }
        }

        private class CookiesWrapper : IResponseCookies
        {
            public CookiesWrapper(HttpContext context, CookiePolicyOptions options, IResponseCookies cookies)
            {
                Context = context;
                Cookies = cookies;
                Policy = options;
            }

            public HttpContext Context { get; }

            public IResponseCookies Cookies { get; }

            public CookiePolicyOptions Policy { get; }

            private bool PolicyRequiresCookieOptions()
            {
                return Policy.HttpOnly != HttpOnlyPolicy.None || Policy.Secure != SecurePolicy.None;
            }

            public void Append(string key, string value)
            {
                if (PolicyRequiresCookieOptions())
                {
                    Append(key, value, new CookieOptions());
                    return;
                }

                if (Policy.OnAppendCookie != null)
                {
                    var context = new AppendCookieContext(Context, options: null, name: key, value: value);
                    Policy.OnAppendCookie(context);
                    key = context.CookieName;
                    value = context.CookieValue;
                }
                Cookies.Append(key, value);
            }

            public void Append(string key, string value, CookieOptions options)
            {
                ApplyPolicy(options);
                if (Policy.OnAppendCookie != null)
                {
                    var context = new AppendCookieContext(Context, options, key, value);
                    Policy.OnAppendCookie(context);
                    key = context.CookieName;
                    value = context.CookieValue;
                }
                Cookies.Append(key, value, options);
            }

            public void Delete(string key)
            {
                if (PolicyRequiresCookieOptions())
                {
                    Delete(key, new CookieOptions());
                    return;
                }


                if (Policy.OnDeleteCookie != null)
                {
                    var context = new DeleteCookieContext(Context, options: null, name: key);
                    Policy.OnDeleteCookie(context);
                    key = context.CookieName;
                }
                Cookies.Delete(key);
            }

            public void Delete(string key, CookieOptions options)
            {
                ApplyPolicy(options);
                if (Policy.OnDeleteCookie != null)
                {
                    var context = new DeleteCookieContext(Context, options, key);
                    Policy.OnDeleteCookie(context);
                    key = context.CookieName;
                }
                Cookies.Delete(key, options);
            }

            private void ApplyPolicy(CookieOptions options)
            {
                switch (Policy.Secure)
                {
                    case SecurePolicy.Always:
                        options.Secure = true;
                        break;
                    case SecurePolicy.SameAsRequest:
                        options.Secure = Context.Request.IsHttps;
                        break;
                    case SecurePolicy.None:
                        break;
                }
                switch (Policy.HttpOnly)
                {
                    case HttpOnlyPolicy.Always:
                        options.HttpOnly = true;
                        break;
                    case HttpOnlyPolicy.None:
                        break;
                }
            }
        }
    }
}
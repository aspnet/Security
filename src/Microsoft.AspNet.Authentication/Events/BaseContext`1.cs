//// Copyright (c) .NET Foundation. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//using Microsoft.AspNet.Http;

//namespace Microsoft.AspNet.Authentication
//{
//    /// <summary>
//    /// Base class used for certain event contexts
//    /// </summary>
//    public abstract class BaseContext
//    {
//        protected BaseContext(HttpContext context, object options)
//        {
//            HttpContext = context;
//            Options = options;
//        }

//        public HttpContext HttpContext { get; private set; }

//        public object Options { get; private set; }

//        public TOptions Options<TOptions> { get { return Options as TOptions; } }

//        public HttpRequest Request
//        {
//            get { return HttpContext.Request; }
//        }

//        public HttpResponse Response
//        {
//            get { return HttpContext.Response; }
//        }
//    }
//}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Authentication2{

    public abstract class AuthenticationSchemeHandler<TOptions> : IAuthenticationSchemeHandler where TOptions : AuthenticationSchemeOptions, new()
    {
        private Task<AuthenticateResult> _authenticateTask;

        protected AuthenticationScheme Scheme { get; private set; }
        protected TOptions Options { get; private set; }
        protected HttpContext Context { get; private set; }
        protected PathString OriginalPathBase { get; private set; }

        protected PathString OriginalPath { get; private set; }

        protected ILogger Logger { get; private set; }

        protected UrlEncoder UrlEncoder { get; private set; }

        /// <summary>
        /// The handler calls methods on the events which give the application control at certain points where processing is occurring. 
        /// If it is not provided a default instance is supplied which does nothing when the methods are called.
        /// </summary>
        protected virtual object Events { get; set; }

        protected HttpRequest Request
        {
            get { return Context.Request; }
        }

        protected HttpResponse Response
        {
            get { return Context.Response; }
        }

        protected string CurrentUri
        {
            get
            {
                return Request.Scheme + "://" + Request.Host + Request.PathBase + Request.Path + Request.QueryString;
            }
        }

        // Can we get rid of this?? (Cookies.FinishResponse / renew uses 
        protected bool SignInAccepted { get; set; }
        protected bool SignOutAccepted { get; set; }

        protected AuthenticationSchemeHandler(ILoggerFactory logger, UrlEncoder encoder)
        {
            Logger = logger.CreateLogger(this.GetType().FullName);
            UrlEncoder = encoder;
        }

        public virtual async Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            Scheme = scheme;
            Context = context;
            OriginalPathBase = Request.PathBase;
            OriginalPath = Request.Path;
            Options = scheme.Settings["Options"] as TOptions;
            await InitializeOptionsAsync();
        }

        protected virtual Task InitializeOptionsAsync()
        {
            Events = Options.Events;
            if (Options.EventsType != null)
            {
                Events = Context.RequestServices.GetRequiredService(Options.EventsType);
            }
            if (string.IsNullOrEmpty(Options.ClaimsIssuer))
            {
                // Default to something reasonable
                Options.ClaimsIssuer = Scheme.Name;
            }
            return TaskCache.CompletedTask;
        }

        protected string BuildRedirectUri(string targetPath)
        {
            return Request.Scheme + "://" + Request.Host + OriginalPathBase + targetPath;
        }

        public async Task<AuthenticateResult> AuthenticateAsync(AuthenticateContext context)
        {
            // Calling Authenticate more than once should always return the original value.
            var result = await HandleAuthenticateOnceAsync();
            if (result?.Failure == null)
            {
                var ticket = result?.Ticket;
                if (ticket?.Principal != null)
                {
                    Logger.AuthenticationSchemeAuthenticated(Options.AuthenticationScheme);
                }
                else
                {
                    Logger.AuthenticationSchemeNotAuthenticated(Options.AuthenticationScheme);
                }
            }
            return result;
        }

        /// <summary>
        /// Used to ensure HandleAuthenticateAsync is only invoked once. The subsequent calls
        /// will return the same authenticate result.
        /// </summary>
        protected Task<AuthenticateResult> HandleAuthenticateOnceAsync()
        {
            if (_authenticateTask == null)
            {
                _authenticateTask = HandleAuthenticateAsync();
            }

            return _authenticateTask;
        }

        /// <summary>
        /// Used to ensure HandleAuthenticateAsync is only invoked once safely. The subsequent
        /// calls will return the same authentication result. Any exceptions will be converted
        /// into a failed authentication result containing the exception.
        /// </summary>
        protected async Task<AuthenticateResult> HandleAuthenticateOnceSafeAsync()
        {
            try
            {
                return await HandleAuthenticateOnceAsync();
            }
            catch (Exception ex)
            {
                return AuthenticateResult.Fail(ex);
            }
        }

        protected abstract Task<AuthenticateResult> HandleAuthenticateAsync();

        public async Task SignInAsync(SignInContext context)
        {
            await HandleSignInAsync(context);
            Logger.AuthenticationSchemeSignedIn(Options.AuthenticationScheme);
        }

        protected virtual Task HandleSignInAsync(SignInContext context)
        {
            return TaskCache.CompletedTask;
        }

        public async Task SignOutAsync(SignOutContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await HandleSignOutAsync(context);
            Logger.AuthenticationSchemeSignedOut(Options.AuthenticationScheme);
        }

        protected virtual Task HandleSignOutAsync(SignOutContext context)
        {
            return TaskCache.CompletedTask;
        }

        /// <summary>
        /// Override this method to deal with a challenge that is forbidden.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The returned boolean is ignored.</returns>
        protected virtual Task<bool> HandleForbiddenAsync(ChallengeContext context)
        {
            Response.StatusCode = 403;
            return Task.FromResult(true);
        }

        /// <summary>
        /// Override this method to deal with 401 challenge concerns, if an authentication scheme in question
        /// deals an authentication interaction as part of it's request flow. (like adding a response header, or
        /// changing the 401 result to 302 of a login page or external sign-in location.)
        /// </summary>
        /// <param name="context"></param>
        /// <returns>The returned boolean is no longer used.</returns>
        protected virtual Task<bool> HandleUnauthorizedAsync(ChallengeContext context)
        {
            Response.StatusCode = 401;
            return Task.FromResult(false);
        }

        public async Task ChallengeAsync(ChallengeContext context)
        {
            switch (context.Behavior)
            {
                case ChallengeBehavior.Automatic:
                    // If there is a principal already, invoke the forbidden code path
                    var result = await HandleAuthenticateOnceSafeAsync();
                    if (result?.Ticket?.Principal != null)
                    {
                        goto case ChallengeBehavior.Forbidden;
                    }
                    goto case ChallengeBehavior.Unauthorized;
                case ChallengeBehavior.Unauthorized:
                    await HandleUnauthorizedAsync(context);
                    Logger.AuthenticationSchemeChallenged(Options.AuthenticationScheme);
                    break;
                case ChallengeBehavior.Forbidden:
                    await HandleForbiddenAsync(context);
                    Logger.AuthenticationSchemeForbidden(Options.AuthenticationScheme);
                    break;
            }
        }

        /// <summary>
        /// Called once by common code after initialization. If an authentication middleware responds directly to
        /// specifically known paths it must override this virtual, compare the request path to it's known paths,
        /// provide any response information as appropriate, and true to stop further processing.
        /// </summary>
        /// <returns>Returning Continue will cause the common code to call the next middleware in line. Returning Handled will
        /// cause the common code to begin the async completion journey without calling the rest of the middleware
        /// pipeline.</returns>
        public virtual Task<AuthenticationRequestResult> HandleRequestAsync()
        {
            // TODO: review
            //if (InitializeResult?.Handled == true)
            //{
            //    return Task.FromResult(true);
            //}
            return Task.FromResult(AuthenticationRequestResult.Skip);
        }
    }
}

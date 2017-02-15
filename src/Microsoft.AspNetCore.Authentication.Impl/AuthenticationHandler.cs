// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication{

    public abstract class AuthenticationHandler<TOptions> : IAuthenticationHandler where TOptions : AuthenticationSchemeOptions, new()
    {
        private Task<AuthenticateResult> _authenticateTask;

        protected AuthenticationOptions SharedOptions { get; }

        public AuthenticationScheme Scheme { get; private set; }
        public TOptions Options { get; private set; }
        protected HttpContext Context { get; private set; }
        protected PathString OriginalPathBase => Context.Features.Get<IAuthenticationFeature>()?.OriginalPathBase ?? Request.PathBase;

        protected PathString OriginalPath => Context.Features.Get<IAuthenticationFeature>()?.OriginalPath ?? Request.Path;

        protected ILogger Logger { get; }

        protected UrlEncoder UrlEncoder { get; }

        protected ISystemClock Clock { get; }

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

        protected AuthenticationHandler(IOptions<AuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
        {
            Logger = logger.CreateLogger(this.GetType().FullName);
            UrlEncoder = encoder;
            Clock = clock;
            SharedOptions = options.Value;
        }

        public virtual Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            Scheme = scheme ?? throw new ArgumentNullException(nameof(scheme));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            if (scheme.Settings.ContainsKey("Options"))
            {
                Options = scheme.Settings["Options"] as TOptions;
            }
            Options = Options ?? new TOptions();
            Events = Options.Events;
            if (Options.EventsType != null)
            {
                Events = Context.RequestServices.GetRequiredService(Options.EventsType);
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
                    Logger.AuthenticationSchemeAuthenticated(Scheme.Name);
                }
                else
                {
                    Logger.AuthenticationSchemeNotAuthenticated(Scheme.Name);
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
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            await HandleSignInAsync(context);
            Logger.AuthenticationSchemeSignedIn(Scheme.Name);
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
            Logger.AuthenticationSchemeSignedOut(Scheme.Name);
        }

        protected virtual Task HandleSignOutAsync(SignOutContext context)
        {
            return TaskCache.CompletedTask;
        }

        /// <summary>
        /// Override this method to deal with a challenge that is forbidden.
        /// </summary>
        /// <param name="context"></param>
        /// <returns>A Task.</returns>
        protected virtual Task HandleForbiddenAsync(ChallengeContext context)
        {
            Response.StatusCode = 403;
            return TaskCache.CompletedTask;
        }

        /// <summary>
        /// Override this method to deal with 401 challenge concerns, if an authentication scheme in question
        /// deals an authentication interaction as part of it's request flow. (like adding a response header, or
        /// changing the 401 result to 302 of a login page or external sign-in location.)
        /// </summary>
        /// <param name="context"></param>
        /// <returns>A Task.</returns>
        protected virtual Task HandleUnauthorizedAsync(ChallengeContext context)
        {
            Response.StatusCode = 401;
            return TaskCache.CompletedTask;
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
                    Logger.AuthenticationSchemeChallenged(Scheme.Name);
                    break;
                case ChallengeBehavior.Forbidden:
                    await HandleForbiddenAsync(context);
                    Logger.AuthenticationSchemeForbidden(Scheme.Name);
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
        public virtual Task<AuthenticationRequestStatus> HandleRequestAsync()
        {
            // TODO: review
            //if (InitializeResult?.Handled == true)
            //{
            //    return Task.FromResult(true);
            //}
            return Task.FromResult(AuthenticationRequestStatus.Skip);
        }
    }
}

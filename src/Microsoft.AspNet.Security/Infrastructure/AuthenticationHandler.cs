// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Http.Interfaces.Security;
using Microsoft.AspNet.Security.DataHandler.Encoder;
using Microsoft.Framework.Logging;

namespace Microsoft.AspNet.Security.Infrastructure
{
    /// <summary>
    /// Base class for the per-request work performed by most authentication middleware.
    /// </summary>
    public abstract class AuthenticationHandler : IAuthenticationHandler
    {
        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();

        private Task<AuthenticationTicket> _authenticate;
        private bool _authenticateInitialized;
        private object _authenticateSyncLock;
        private bool _authenticateCalled;

        private Task _applyResponse;
        private bool _applyResponseInitialized;
        private object _applyResponseSyncLock;

        private AuthenticationOptions _baseOptions;

        protected IChallengeContext ChallengeContext { get; set; }
        protected SignInIdentityContext SignInIdentityContext { get; set; }
        protected ISignOutContext SignOutContext { get; set; }

        protected HttpContext Context { get; private set; }

        protected HttpRequest Request
        {
            get { return Context.Request; }
        }

        protected HttpResponse Response
        {
            get { return Context.Response; }
        }

        protected PathString RequestPathBase { get; private set; }

        internal AuthenticationOptions BaseOptions
        {
            get { return _baseOptions; }
        }

        public IAuthenticationHandler PriorHandler { get; set; }

        public bool Faulted { get; set; }

        protected async Task BaseInitializeAsync(AuthenticationOptions options, HttpContext context)
        {
            _baseOptions = options;
            Context = context;
            RequestPathBase = Request.PathBase;

            RegisterAuthenticationHandler();

            Response.OnSendingHeaders(OnSendingHeaderCallback, this);

            await InitializeCoreAsync();

            if (BaseOptions.AuthenticationMode == AuthenticationMode.Active)
            {
                AuthenticationTicket ticket = await AuthenticateAsync();
                if (ticket != null)
                {
                    if ( ticket.Identity != null)
                        SecurityHelper.AddUserIdentity(Context, ticket.Identity);
                    else if (ticket.Principal != null)
                        SecurityHelper.AddUserIdentity(Context, ticket.Principal.Identity);
                }
            }
        }

        private static void OnSendingHeaderCallback(object state)
        {
            AuthenticationHandler handler = (AuthenticationHandler)state;
            handler.ApplyResponse();
        }

        protected virtual Task InitializeCoreAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Called once per request after Initialize and Invoke.
        /// </summary>
        /// <returns>async completion</returns>
        internal async Task TeardownAsync()
        {
            try
            {
                await ApplyResponseAsync();
            }
            catch (Exception)
            {
                try
                {
                    await TeardownCoreAsync();
                }
                catch (Exception)
                {
                    // Don't mask the original exception
                }
                UnregisterAuthenticationHandler();
                throw;
            }

            await TeardownCoreAsync();
            UnregisterAuthenticationHandler();
        }

        protected virtual Task TeardownCoreAsync()
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Called once by common code after initialization. If an authentication middleware responds directly to
        /// specifically known paths it must override this virtual, compare the request path to it's known paths, 
        /// provide any response information as appropriate, and true to stop further processing.
        /// </summary>
        /// <returns>Returning false will cause the common code to call the next middleware in line. Returning true will
        /// cause the common code to begin the async completion journey without calling the rest of the middleware
        /// pipeline.</returns>
        public virtual Task<bool> InvokeAsync()
        {
            return Task.FromResult<bool>(false);
        }

        public virtual void GetDescriptions(IAuthTypeContext authTypeContext)
        {
            authTypeContext.Accept(BaseOptions.Description.Dictionary);

            if (PriorHandler != null)
            {
                PriorHandler.GetDescriptions(authTypeContext);
            }
        }

        public virtual void Authenticate(IAuthenticateContext context)
        {
            if (context.AuthenticationTypes.Contains(BaseOptions.AuthenticationType, StringComparer.Ordinal))
            {
                AuthenticationTicket ticket = Authenticate();
                if (ticket != null && ticket.Identity != null)
                {
                    _authenticateCalled = true;
                    context.Authenticated(ticket.Identity, ticket.Properties.Dictionary, BaseOptions.Description.Dictionary);
                }
                else
                {
                    context.NotAuthenticated(BaseOptions.AuthenticationType, properties: null, description: BaseOptions.Description.Dictionary);
                }
            }

            if (PriorHandler != null)
            {
                PriorHandler.Authenticate(context);
            }
        }

        public virtual async Task AuthenticateAsync(IAuthenticateContext context)
        {
            if (context.AuthenticationTypes.Contains(BaseOptions.AuthenticationType, StringComparer.Ordinal))
            {
                AuthenticationTicket ticket = await AuthenticateAsync();
                if (ticket != null && ticket.Identity != null)
                {
                    _authenticateCalled = true;
                    context.Authenticated(ticket.Identity, ticket.Properties.Dictionary, BaseOptions.Description.Dictionary);
                }
                else
                {
                    context.NotAuthenticated(BaseOptions.AuthenticationType, properties: null, description: BaseOptions.Description.Dictionary);
                }
            }

            if (PriorHandler != null)
            {
                await PriorHandler.AuthenticateAsync(context);
            }
        }

        public AuthenticationTicket Authenticate()
        {
            return LazyInitializer.EnsureInitialized(
                ref _authenticate,
                ref _authenticateInitialized,
                ref _authenticateSyncLock,
                () =>
                {
                    return Task.FromResult(AuthenticateCore());
                }).GetAwaiter().GetResult();
        }

        protected abstract AuthenticationTicket AuthenticateCore();

        /// <summary>
        /// Causes the authentication logic in AuthenticateCore to be performed for the current request 
        /// at most once and returns the results. Calling Authenticate more than once will always return 
        /// the original value. 
        /// 
        /// This method should always be called instead of calling AuthenticateCore directly.
        /// </summary>
        /// <returns>The ticket data provided by the authentication logic</returns>
        public Task<AuthenticationTicket> AuthenticateAsync()
        {
            return LazyInitializer.EnsureInitialized(
                ref _authenticate,
                ref _authenticateInitialized,
                ref _authenticateSyncLock,
                AuthenticateCoreAsync);
        }

        /// <summary>
        /// The core authentication logic which must be provided by the handler. Will be invoked at most
        /// once per request. Do not call directly, call the wrapping Authenticate method instead.
        /// </summary>
        /// <returns>The ticket data provided by the authentication logic</returns>
        protected virtual Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            return Task.FromResult(AuthenticateCore());
        }

        private void ApplyResponse()
        {
            // If ApplyResponse already failed in the OnSendingHeaderCallback or TeardownAsync code path then a
            // failed task is cached. If called again the same error will be re-thrown. This breaks error handling
            // scenarios like the ability to display the error page or re-execute the request.
            try
            {
                if (!Faulted)
                {
                    LazyInitializer.EnsureInitialized(
                        ref _applyResponse,
                        ref _applyResponseInitialized,
                        ref _applyResponseSyncLock,
                        () =>
                        {
                            ApplyResponseCore();
                            return Task.FromResult(0);
                        }).GetAwaiter().GetResult(); // Block if the async version is in progress.
                }
            }
            catch (Exception)
            {
                Faulted = true;
                throw;
            }
        }

        protected virtual void ApplyResponseCore()
        {
            ApplyResponseGrant();
            ApplyResponseChallenge();
        }

        /// <summary>
        /// Causes the ApplyResponseCore to be invoked at most once per request. This method will be
        /// invoked either earlier, when the response headers are sent as a result of a response write or flush,
        /// or later, as the last step when the original async call to the middleware is returning.
        /// </summary>
        /// <returns></returns>
        private async Task ApplyResponseAsync()
        {
            // If ApplyResponse already failed in the OnSendingHeaderCallback or TeardownAsync code path then a
            // failed task is cached. If called again the same error will be re-thrown. This breaks error handling
            // scenarios like the ability to display the error page or re-execute the request.
            try
            {
                if (!Faulted)
                {
                    await LazyInitializer.EnsureInitialized(
                        ref _applyResponse,
                        ref _applyResponseInitialized,
                        ref _applyResponseSyncLock,
                        ApplyResponseCoreAsync);
                }
            }
            catch (Exception)
            {
                Faulted = true;
                throw;
            }
        }

        /// <summary>
        /// Core method that may be overridden by handler. The default behavior is to call two common response 
        /// activities, one that deals with sign-in/sign-out concerns, and a second to deal with 401 challenges.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task ApplyResponseCoreAsync()
        {
            await ApplyResponseGrantAsync();

            // If authenticate was called and the the status is still 401, authZ failed so set 403 and stop
            // REVIEW: Does this need to ensure that there's the 401 is challenge for this auth type?
            if (Response.StatusCode == 401 && _authenticateCalled)
            {
                Response.StatusCode = 403;
                return;
            }
            else
            {
                await ApplyResponseChallengeAsync();
            }
        }

        protected abstract void ApplyResponseGrant();

        /// <summary>
        /// Override this method to dela with sign-in/sign-out concerns, if an authentication scheme in question
        /// deals with grant/revoke as part of it's request flow. (like setting/deleting cookies)
        /// </summary>
        /// <returns></returns>
        protected virtual Task ApplyResponseGrantAsync()
        {
            ApplyResponseGrant();
            return Task.FromResult(0);
        }

        public virtual void SignIn(ISignInContext context)
        {
            ClaimsIdentity identity;
            if (SecurityHelper.LookupSignIn(context.Identities, BaseOptions.AuthenticationType, out identity))
            {
                SignInIdentityContext = new SignInIdentityContext(identity, new AuthenticationProperties(context.Properties));
                SignOutContext = null;
                context.Accept(BaseOptions.AuthenticationType, BaseOptions.Description.Dictionary);
            }

            if (PriorHandler != null)
            {
                PriorHandler.SignIn(context);
            }
        }

        public virtual void SignOut(ISignOutContext context)
        {
            if (SecurityHelper.LookupSignOut(context.AuthenticationTypes, BaseOptions.AuthenticationType, BaseOptions.AuthenticationMode))
            {
                SignInIdentityContext = null;
                SignOutContext = context;
                context.Accept(BaseOptions.AuthenticationType, BaseOptions.Description.Dictionary);
            }

            if (PriorHandler != null)
            {
                PriorHandler.SignOut(context);
            }
        }

        public virtual void Challenge(IChallengeContext context)
        {
            if (SecurityHelper.LookupChallenge(context.AuthenticationTypes, BaseOptions.AuthenticationType, BaseOptions.AuthenticationMode))
            {
                ChallengeContext = context;
                context.Accept(BaseOptions.AuthenticationType, BaseOptions.Description.Dictionary);
            }

            if (PriorHandler != null)
            {
                PriorHandler.Challenge(context);
            }
        }

        protected abstract void ApplyResponseChallenge();

        /// <summary>
        /// Override this method to deal with 401 challenge concerns, if an authentication scheme in question
        /// deals an authentication interaction as part of it's request flow. (like adding a response header, or
        /// changing the 401 result to 302 of a login page or external sign-in location.)
        /// </summary>
        /// <returns></returns>
        protected virtual Task ApplyResponseChallengeAsync()
        {
            ApplyResponseChallenge();
            return Task.FromResult(0);
        }

        protected void GenerateCorrelationId([NotNull] AuthenticationProperties properties)
        {
            string correlationKey = Constants.CorrelationPrefix + BaseOptions.AuthenticationType;

            var nonceBytes = new byte[32];
            CryptoRandom.GetBytes(nonceBytes);
            string correlationId = TextEncodings.Base64Url.Encode(nonceBytes);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsSecure
            };

            properties.Dictionary[correlationKey] = correlationId;

            Response.Cookies.Append(correlationKey, correlationId, cookieOptions);
        }

        protected bool ValidateCorrelationId([NotNull] AuthenticationProperties properties, [NotNull] ILogger logger)
        {
            string correlationKey = Constants.CorrelationPrefix + BaseOptions.AuthenticationType;

            string correlationCookie = Request.Cookies[correlationKey];
            if (string.IsNullOrWhiteSpace(correlationCookie))
            {
                logger.WriteWarning("{0} cookie not found.", correlationKey);
                return false;
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsSecure
            };
            Response.Cookies.Delete(correlationKey, cookieOptions);

            string correlationExtra;
            if (!properties.Dictionary.TryGetValue(
                correlationKey,
                out correlationExtra))
            {
                logger.WriteWarning("{0} state property not found.", correlationKey);
                return false;
            }

            properties.Dictionary.Remove(correlationKey);

            if (!string.Equals(correlationCookie, correlationExtra, StringComparison.Ordinal))
            {
                logger.WriteWarning("{0} correlation cookie and state property mismatch.", correlationKey);
                return false;
            }

            return true;
        }

        private void RegisterAuthenticationHandler()
        {
            var auth = Context.GetAuthentication();
            PriorHandler = auth.Handler;
            auth.Handler = this;
        }

        private void UnregisterAuthenticationHandler()
        {
            var auth = Context.GetAuthentication();
            auth.Handler = PriorHandler;
        }
    }
}

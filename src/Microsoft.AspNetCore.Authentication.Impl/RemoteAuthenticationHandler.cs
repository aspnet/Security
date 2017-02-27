// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Authentication
{
    public abstract class RemoteAuthenticationHandler<TOptions> : AuthenticationHandler<TOptions> where TOptions : RemoteAuthenticationOptions, new()
    {
        private const string CorrelationPrefix = ".AspNetCore.Correlation.";
        private const string CorrelationProperty = ".xsrf";
        private const string CorrelationMarker = "N";
        private const string AuthSchemeKey = ".AuthScheme";

        private static readonly RandomNumberGenerator CryptoRandom = RandomNumberGenerator.Create();

        protected RemoteAuthenticationHandler(IOptions<AuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
            : base(options, logger, encoder, clock)
        { }

        public override async Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            await base.InitializeAsync(scheme, context);
            Events = Events ?? new RemoteAuthenticationEvents();

            // TODO: this needs to be done once (but we don't have access to scheme data in ext method)
            if (Options.SignInScheme == null && SharedOptions.DefaultSignInScheme != null)
            {
                Options.SignInScheme = SharedOptions.DefaultSignInScheme;
            }

            if (string.IsNullOrEmpty(Options.SignInScheme))
            {
                throw new ArgumentException(Resources.FormatException_OptionMustBeProvided(nameof(Options.SignInScheme)), nameof(Options.SignInScheme));
            }
        }

        public override async Task<bool> HandleRequestAsync()
        {
            AuthenticationTicket ticket = null;
            Exception exception = null;

            try
            {
                var authResult = await HandleRemoteAuthenticateAsync();
                if (authResult == null)
                {
                    exception = new InvalidOperationException("Invalid return state, unable to redirect.");
                }
                else if (authResult.Handled)
                {
                    return true;
                }
                else if (authResult.Skipped)
                {
                    return false;
                }
                else if (!authResult.Succeeded)
                {
                    exception = authResult.Failure ??
                                new InvalidOperationException("Invalid return state, unable to redirect.");
                }

                ticket = authResult.Ticket;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            if (exception != null)
            {
                Logger.RemoteAuthenticationError(exception.Message);
                var errorContext = new FailureContext(Context, exception);
                await Options.Events.RemoteFailure(errorContext);

                if (errorContext.HandledResponse)
                {
                    return true;
                }
                else if (errorContext.Skipped)
                {
                    return false;
                }

                throw new AggregateException("Unhandled remote failure.", exception);
            }

            // We have a ticket if we get here
            var ticketContext = new TicketReceivedContext(Context, Options, ticket)
            {
                ReturnUri = ticket.Properties.RedirectUri,
            };
            // REVIEW: is this safe or good?
            ticket.Properties.RedirectUri = null;

            // Mark which provider produced this identity so we can cross-check later in HandleAuthenticateAsync
            ticketContext.Properties.Items[AuthSchemeKey] = Scheme.Name;

            await Options.Events.TicketReceived(ticketContext);

            if (ticketContext.HandledResponse)
            {
                Logger.SigninHandled();
                return true;
            }
            else if (ticketContext.Skipped)
            {
                Logger.SigninSkipped();
                return false;
            };

            await Context.SignInAsync(Options.SignInScheme, ticketContext.Principal, ticketContext.Properties);

            // Default redirect path is the base path
            if (string.IsNullOrEmpty(ticketContext.ReturnUri))
            {
                ticketContext.ReturnUri = "/";
            }

            Response.Redirect(ticketContext.ReturnUri);
            return true;
        }

        /// <summary>
        /// Authenticate the user identity with the identity provider.
        ///
        /// The method process the request on the endpoint defined by CallbackPath.
        /// </summary>
        protected abstract Task<AuthenticateResult> HandleRemoteAuthenticateAsync();

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var result = await Context.AuthenticateAsync(Options.SignInScheme);
            if (result != null)
            {
                // todo error
                if (result.Failure != null)
                {
                    return AuthenticateResult.Fail(result.Failure);
                }

                // The SignInScheme may be shared with multiple providers, make sure this middleware issued the identity.
                string authenticatedScheme;
                var ticket = result.Ticket;
                if (ticket != null && ticket.Principal != null && ticket.Properties != null
                    && ticket.Properties.Items.TryGetValue(AuthSchemeKey, out authenticatedScheme)
                    && string.Equals(Scheme.Name, authenticatedScheme, StringComparison.Ordinal))
                {
                    return AuthenticateResult.Success(new AuthenticationTicket(ticket.Principal,
                        ticket.Properties, Scheme.Name));
                }

                return AuthenticateResult.Fail("Not authenticated");
            }

            return AuthenticateResult.Fail("Remote authentication does not directly support authenticate");
        }

        // REVIEW: should this forward to sign in scheme as well?
        protected override Task HandleSignOutAsync(SignOutContext context)
        {
            throw new NotSupportedException();
        }

        // REVIEW: should this forward to sign in scheme as well?
        protected override Task HandleSignInAsync(SignInContext context)
        {
            throw new NotSupportedException();
        }

        // REVIEW: This behaviour needs a test (forwarding of forbidden to sign in scheme)
        protected override Task HandleForbiddenAsync(ChallengeContext context)
        {
            return Context.ForbidAsync(Options.SignInScheme);
        }

        protected virtual void GenerateCorrelationId(AuthenticationProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            var bytes = new byte[32];
            CryptoRandom.GetBytes(bytes);
            var correlationId = Base64UrlTextEncoder.Encode(bytes);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                Expires = Clock.UtcNow.Add(Options.RemoteAuthenticationTimeout),
            };

            properties.Items[CorrelationProperty] = correlationId;

            var cookieName = CorrelationPrefix + Scheme.Name + "." + correlationId;

            Response.Cookies.Append(cookieName, CorrelationMarker, cookieOptions);
        }

        protected virtual bool ValidateCorrelationId(AuthenticationProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            string correlationId;
            if (!properties.Items.TryGetValue(CorrelationProperty, out correlationId))
            {
                Logger.CorrelationPropertyNotFound(CorrelationPrefix);
                return false;
            }

            properties.Items.Remove(CorrelationProperty);

            var cookieName = CorrelationPrefix + Scheme.Name + "." + correlationId;

            var correlationCookie = Request.Cookies[cookieName];
            if (string.IsNullOrEmpty(correlationCookie))
            {
                Logger.CorrelationCookieNotFound(cookieName);
                return false;
            }

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps
            };
            Response.Cookies.Delete(cookieName, cookieOptions);

            if (!string.Equals(correlationCookie, CorrelationMarker, StringComparison.Ordinal))
            {
                Logger.UnexpectedCorrelationCookieValue(cookieName, correlationCookie);
                return false;
            }

            return true;
        }
    }
}
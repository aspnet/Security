// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Authentication.Google
{
    internal class GoogleHandler : OAuthHandler<GoogleOptions>
    {
        public GoogleHandler(IOptions<AuthenticationOptions> sharedOptions, IOptionsFactory<GoogleOptions> options, ILoggerFactory logger, UrlEncoder encoder, IDataProtectionProvider dataProtection, ISystemClock clock)
            : base(sharedOptions, options, logger, encoder, dataProtection, clock)
        { }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(
            ClaimsIdentity identity,
            AuthenticationProperties properties,
            OAuthTokenResponse tokens)
        {
            // Get the Google user
            var request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);

            var response = await Backchannel.SendAsync(request, Context.RequestAborted);
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"An error occurred when retrieving user information ({response.StatusCode}). Please check if the authentication information is correct and the corresponding Google+ API is enabled.");
            }

            var payload = JObject.Parse(await response.Content.ReadAsStringAsync());

            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, properties, Scheme.Name);
            var context = new OAuthCreatingTicketContext(ticket, Context, Scheme, Options, Backchannel, tokens, payload);

            var identifier = GoogleHelper.GetId(payload);
            if (!string.IsNullOrEmpty(identifier))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identifier, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var givenName = GoogleHelper.GetGivenName(payload);
            if (!string.IsNullOrEmpty(givenName))
            {
                identity.AddClaim(new Claim(ClaimTypes.GivenName, givenName, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var familyName = GoogleHelper.GetFamilyName(payload);
            if (!string.IsNullOrEmpty(familyName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Surname, familyName, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var name = GoogleHelper.GetName(payload);
            if (!string.IsNullOrEmpty(name))
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, name, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var email = GoogleHelper.GetEmail(payload);
            if (!string.IsNullOrEmpty(email))
            {
                identity.AddClaim(new Claim(ClaimTypes.Email, email, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            var profile = GoogleHelper.GetProfile(payload);
            if (!string.IsNullOrEmpty(profile))
            {
                identity.AddClaim(new Claim("urn:google:profile", profile, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            await Options.Events.CreatingTicket(context);

            return context.Ticket;
        }

        // TODO: Abstract this properties override pattern into the base class?
        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            // Google Identity Platform Manual:
            // https://developers.google.com/identity/protocols/OAuth2WebServer

            var queryStrings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            queryStrings.Add("response_type", "code");
            queryStrings.Add("client_id", Options.ClientId);
            queryStrings.Add("redirect_uri", redirectUri);

            AddQueryString(queryStrings, properties, "scope", FormatScope());
            AddQueryString(queryStrings, properties, "access_type", Options.AccessType);
            AddQueryString(queryStrings, properties, "approval_prompt");
            AddQueryString(queryStrings, properties, "prompt");
            AddQueryString(queryStrings, properties, "login_hint");
            AddQueryString(queryStrings, properties, "include_granted_scopes");

            var state = Options.StateDataFormat.Protect(properties);
            queryStrings.Add("state", state);

            var authorizationEndpoint = QueryHelpers.AddQueryString(Options.AuthorizationEndpoint, queryStrings);
            return authorizationEndpoint;
        }

        private static void AddQueryString(
            IDictionary<string, string> queryStrings,
            AuthenticationProperties properties,
            string name,
            string defaultValue = null)
        {
            string value;
            if (!properties.Items.TryGetValue(name, out value))
            {
                value = defaultValue;
            }
            else
            {
                // Remove the parameter from AuthenticationProperties so it won't be serialized to state parameter
                properties.Items.Remove(name);
            }

            if (value == null)
            {
                return;
            }

            queryStrings[name] = value;
        }
    }
}

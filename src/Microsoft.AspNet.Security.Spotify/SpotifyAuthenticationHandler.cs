// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http.Security;
using Microsoft.AspNet.Security.OAuth;
using Microsoft.AspNet.WebUtilities;
using Microsoft.Framework.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.AspNet.Security.Spotify.Notifications;
using System.Net.Http.Headers;

namespace Microsoft.AspNet.Security.Spotify
{
    public class SpotifyAuthenticationHandler : OAuthAuthenticationHandler<SpotifyAuthenticationOptions, SpotifyAuthenticationNotifications>
    {
        public SpotifyAuthenticationHandler(HttpClient httpClient, ILogger logger)
            : base(httpClient, logger)
        {
        }

        protected override async Task<AuthenticationTicket> GetUserInformationAsync(AuthenticationProperties properties, TokenResponse tokens)
        {
            // Get the Spotify user
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, Options.UserInformationEndpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            HttpResponseMessage graphResponse = await Backchannel.SendAsync(request, Context.RequestAborted);
            graphResponse.EnsureSuccessStatusCode();
            var text = await graphResponse.Content.ReadAsStringAsync();
            JObject user = JObject.Parse(text);

            var context = new SpotifyAuthenticatedContext(Context, Options, user, tokens);
            context.Identity = new ClaimsIdentity(
                Options.AuthenticationType,
                ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);

            if (!string.IsNullOrEmpty(context.Id))
            {
                context.Identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, context.Id,
                    ClaimValueTypes.String, Options.AuthenticationType));
            }
            if (!string.IsNullOrEmpty(context.DisplayName))
            {
                context.Identity.AddClaim(new Claim(ClaimsIdentity.DefaultNameClaimType, context.DisplayName,
                    ClaimValueTypes.String, Options.AuthenticationType));
            }
            if (!string.IsNullOrEmpty(context.ExternalLink))
            {
                context.Identity.AddClaim(new Claim("urn:spotify:externallink", context.ExternalLink,
                    ClaimValueTypes.String, Options.AuthenticationType));
            }
            if (!string.IsNullOrEmpty(context.SpotifyLink))
            {
                context.Identity.AddClaim(new Claim("urn:spotify:spotifylink", context.SpotifyLink,
                    ClaimValueTypes.String, Options.AuthenticationType));
            }
            if (!string.IsNullOrEmpty(context.ApiLink))
            {
                context.Identity.AddClaim(new Claim("urn:spotify:apilink", context.ApiLink,
                    ClaimValueTypes.String, Options.AuthenticationType));
            }

            context.Properties = properties;

            await Options.Notifications.Authenticated(context);

            return new AuthenticationTicket(context.Identity, context.Properties);
        }

        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            return QueryHelpers.AddQueryString(base.BuildChallengeUrl(properties, redirectUri), "show_dialog", Options.ForceDialog.ToString());
        }
    }
}
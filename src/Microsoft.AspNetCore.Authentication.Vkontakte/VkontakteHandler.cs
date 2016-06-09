using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json.Linq;

namespace Microsoft.AspNetCore.Authentication.Vkontakte
{
    internal class VkontakteHandler : OAuthHandler<VkontakteOptions>
    {
        public VkontakteHandler(HttpClient httpClient) : base(httpClient)
        {
        }

        protected override async Task<AuthenticationTicket> CreateTicketAsync(ClaimsIdentity identity, AuthenticationProperties properties, OAuthTokenResponse tokens)
        {
            string endpoint = QueryHelpers.AddQueryString(Options.UserInformationEndpoint, "access_token", tokens.AccessToken);

            if (Options.Fields.Count > 0)
            {
                endpoint = QueryHelpers.AddQueryString(endpoint, "fields", string.Join(",", Options.Fields));
            }

            HttpResponseMessage response = await Backchannel.GetAsync(endpoint, Context.RequestAborted);
            response.EnsureSuccessStatusCode();

            string responseText = await response.Content.ReadAsStringAsync();

            JObject payload = JObject.Parse(responseText);
            
            AuthenticationTicket ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), properties, Options.AuthenticationScheme);
            OAuthCreatingTicketContext context = new OAuthCreatingTicketContext(ticket, Context, Options, Backchannel, tokens, payload);

            string identifier = VkontakteHelper.GetId(payload);
            if (!string.IsNullOrEmpty(identifier))
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, identifier, ClaimValueTypes.String, Options.ClaimsIssuer));
            }
            
            string firstName = VkontakteHelper.GetFirstName(payload);
            if (!string.IsNullOrEmpty(firstName))
            {
                identity.AddClaim(new Claim(ClaimTypes.GivenName, firstName, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            string lastName = VkontakteHelper.GetLastName(payload);
            if (!string.IsNullOrEmpty(lastName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Surname, lastName, ClaimValueTypes.String, Options.ClaimsIssuer));
            }

            string name = VkontakteHelper.GetScreenName(payload);
            if (!string.IsNullOrEmpty(name))
            {
                identity.AddClaim(new Claim(ClaimTypes.Name, name, ClaimValueTypes.String, Options.ClaimsIssuer));
            }
            
            string photo = VkontakteHelper.GetPhoto(payload);
            if (!string.IsNullOrEmpty(photo))
            {
                identity.AddClaim(new Claim("urn:vkontakte:link", photo, ClaimValueTypes.String, Options.ClaimsIssuer));
            }
            
            await Options.Events.CreatingTicket(context);

            return context.Ticket;
        }
    }
}
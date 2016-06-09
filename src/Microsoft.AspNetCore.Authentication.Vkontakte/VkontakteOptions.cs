using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Authentication.Vkontakte
{
    /// <summary>
    /// Configuration options for <see cref="VkontakteMiddleware"/>.
    /// </summary>
    public class VkontakteOptions : OAuthOptions
    {
        /// <summary>
        /// Initializes a new <see cref="VkontakteOptions"/>.
        /// </summary>
        public VkontakteOptions()
        {
            AuthenticationScheme = Constants.AuthenticationScheme;
            DisplayName = AuthenticationScheme;
            CallbackPath = new PathString("/signin-vkontakte");
            AuthorizationEndpoint = Constants.AuthorizeEndpoint;
            TokenEndpoint = Constants.TokenEndpoint;
            UserInformationEndpoint = Constants.GraphApiEndpoint;
            Fields = new[] { "uid", "first_name", "last_name", "photo_50", "screen_name" };
        }

        /// <summary>
        /// The list of fields to retrieve from the UserInformationEndpoint.
        /// https://vk.com/dev/fields
        /// </summary>
        public ICollection<string> Fields { get; }
    }
}
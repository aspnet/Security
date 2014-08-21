using System;
using System.Net.Http;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;
using System.Collections.Generic;

namespace Microsoft.AspNet.Security.Facebook
{
    /// <summary>
    /// Summary description for IFacebookAuthenticationOptions
    /// </summary>
    public interface IFacebookAuthenticationOptions : IAuthenticationOptions
    {
		/// <summary>
		/// Gets or sets the Facebook-assigned appId
		/// </summary>
		string AppId { get; set; }

		/// <summary>
		/// Gets or sets the Facebook-assigned app secret
		/// </summary>
		string AppSecret { get; set; }
#if NET45
        /// <summary>
        /// Gets or sets the a pinned certificate validator to use to validate the endpoints used
        /// in back channel communications belong to Facebook.
        /// </summary>
        /// <value>
        /// The pinned certificate validator.
        /// </value>
        /// <remarks>If this property is null then the default certificate checks are performed,
        /// validating the subject name and if the signing chain is a trusted party.</remarks>
        ICertificateValidator BackchannelCertificateValidator { get; set; }
#endif
		/// <summary>
		/// Gets or sets timeout value in milliseconds for back channel communications with Facebook.
		/// </summary>
		/// <value>
		/// The back channel timeout in milliseconds.
		/// </value>
		TimeSpan BackchannelTimeout { get; set; }

		/// <summary>
		/// The HttpMessageHandler used to communicate with Facebook.
		/// This cannot be set at the same time as BackchannelCertificateValidator unless the value 
		/// can be downcast to a WebRequestHandler.
		/// </summary>
		HttpMessageHandler BackchannelHttpHandler { get; set; }

		/// <summary>
		/// Get or sets the text that the user can display on a sign in user interface.
		/// </summary>
		string Caption { get; set; }

		/// <summary>
		/// The request path within the application's base path where the user-agent will be returned.
		/// The middleware will process this request when it arrives.
		/// Default value is "/signin-facebook".
		/// </summary>
		PathString CallbackPath { get; set; }

		/// <summary>
		/// Gets or sets the name of another authentication middleware which will be responsible for actually issuing a user <see cref="System.Security.Claims.ClaimsIdentity"/>.
		/// </summary>
		string SignInAsAuthenticationType { get; set; }

		/// <summary>
		/// Gets or sets the <see cref="IFacebookAuthenticationNotifications"/> used to handle authentication events.
		/// </summary>
		IFacebookAuthenticationNotifications Notifications { get; set; }

		/// <summary>
		/// Gets or sets the type used to secure data handled by the middleware.
		/// </summary>
		ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

		/// <summary>
		/// A list of permissions to request.
		/// </summary>
		IList<string> Scope { get; }

		/// <summary>
		/// Gets or sets if the appsecret_proof should be generated and sent with Facebook API calls.
		/// This is enabled by default.
		/// </summary>
		bool SendAppSecretProof { get; set; }
	}
}
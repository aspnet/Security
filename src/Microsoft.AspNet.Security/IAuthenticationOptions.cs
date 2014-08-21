using Microsoft.AspNet.Http.Security;

namespace Microsoft.AspNet.Security
{
	/// <summary>
	/// Interface for Base Options for all authentication middleware
	/// </summary>
	public interface IAuthenticationOptions
    {
		/// <summary>
		/// The AuthenticationType in the options corresponds to the IIdentity AuthenticationType property. A different
		/// value may be assigned in order to use the same authentication middleware type more than once in a pipeline.
		/// </summary>
		string AuthenticationType { get;set; }

		/// <summary>
		/// If Active the authentication middleware alter the request user coming in and
		/// alter 401 Unauthorized responses going out. If Passive the authentication middleware will only provide
		/// identity and alter responses when explicitly indicated by the AuthenticationType.
		/// </summary>
		AuthenticationMode AuthenticationMode { get; set; }

		/// <summary>
		/// Additional information about the authentication type which is made available to the application.
		/// </summary>
		AuthenticationDescription Description { get; set; }
	}
}
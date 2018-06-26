namespace Microsoft.AspNetCore.Authentication.OpenIdConnect
{
    /// <summary>
    /// Configure the client authentication mode to call access_token endpoint
    /// </summary>
    public enum OpenIdConnectClientAuthenticationMode
    {
        /// <summary>
        /// Send client id and client secret in the request body 
        /// </summary>
        Post,
        /// <summary>
        /// Use basic authorization header
        /// </summary>
        Basic
    }
}
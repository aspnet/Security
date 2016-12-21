using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Microsoft.AspNetCore.Authentication2.Test.OpenIdConnect
{
    internal class MockOpenIdConnectMessage : OpenIdConnectMessage
    {
        public string TestAuthorizeEndpoint { get; set; }

        public string TestLogoutRequest { get; set; }

        public override string CreateAuthenticationRequestUrl()
        {
            return TestAuthorizeEndpoint ?? base.CreateAuthenticationRequestUrl();
        }

        public override string CreateLogoutRequestUrl()
        {
            return TestLogoutRequest ?? base.CreateLogoutRequestUrl();
        }
    }
}
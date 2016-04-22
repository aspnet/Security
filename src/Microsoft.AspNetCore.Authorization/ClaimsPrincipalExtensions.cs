using System.Security.Principal;

namespace System.Security.Claims
{
    public static class ObjectExtensions
    {
        internal static string GetUserName(this ClaimsPrincipal user)
        {
            var identity = user?.Identity;
            if (identity != null)
            {
                var name = identity.Name;
                if (name != null)
                {
                    return name;
                }
                return GetClaimValue(identity, "sub")
                    ?? GetClaimValue(identity, ClaimTypes.Name)
                    ?? GetClaimValue(identity, ClaimTypes.NameIdentifier);
            }
            return null;
        }

        private static string GetClaimValue(IIdentity identity, string claimsType)
        {
            return (identity as ClaimsIdentity)?.FindFirst(claimsType)?.Value;
        }
    }
}

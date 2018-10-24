using System;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Represents a special exception thrown when an external
    /// authorization demand was denied by the remote server. 
    /// </summary>
    public class AccessDeniedException : Exception
    {
        public AccessDeniedException(string message)
            : base(message)
        {
        }
    }
}
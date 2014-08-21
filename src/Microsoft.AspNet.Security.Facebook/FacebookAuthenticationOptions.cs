// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;

namespace Microsoft.AspNet.Security.Facebook
{
    /// <summary>
    /// Configuration options for <see cref="FacebookAuthenticationMiddleware"/>
    /// </summary>
    public class FacebookAuthenticationOptions : AuthenticationOptions, IFacebookAuthenticationOptions
    {
        /// <summary>
        /// Initializes a new <see cref="FacebookAuthenticationOptions"/>
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "Microsoft.AspNet.Security.Facebook.FacebookAuthenticationOptions.set_Caption(System.String)", Justification = "Not localizable.")]
        public FacebookAuthenticationOptions()
            : base(FacebookAuthenticationDefaults.AuthenticationType)
        {
            Caption = FacebookAuthenticationDefaults.AuthenticationType;
            CallbackPath = new PathString("/signin-facebook");
            AuthenticationMode = AuthenticationMode.Passive;
            Scope = new List<string>();
            BackchannelTimeout = TimeSpan.FromSeconds(60);
            SendAppSecretProof = true;
        }

        /// <summary>
        /// Gets or sets the Facebook-assigned appId
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Gets or sets the Facebook-assigned app secret
        /// </summary>
        public string AppSecret { get; set; }
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
        public ICertificateValidator BackchannelCertificateValidator { get; set; }
#endif
        /// <summary>
        /// Gets or sets timeout value in milliseconds for back channel communications with Facebook.
        /// </summary>
        /// <value>
        /// The back channel timeout in milliseconds.
        /// </value>
        public TimeSpan BackchannelTimeout { get; set; }

        /// <summary>
        /// The HttpMessageHandler used to communicate with Facebook.
        /// This cannot be set at the same time as BackchannelCertificateValidator unless the value 
        /// can be downcast to a WebRequestHandler.
        /// </summary>
        public HttpMessageHandler BackchannelHttpHandler { get; set; }

        /// <summary>
        /// Get or sets the text that the user can display on a sign in user interface.
        /// </summary>
        public string Caption
        {
            get { return Description.Caption; }
            set { Description.Caption = value; }
        }

        /// <summary>
        /// The request path within the application's base path where the user-agent will be returned.
        /// The middleware will process this request when it arrives.
        /// Default value is "/signin-facebook".
        /// </summary>
        public PathString CallbackPath { get; set; }

        /// <summary>
        /// Gets or sets the name of another authentication middleware which will be responsible for actually issuing a user <see cref="System.Security.Claims.ClaimsIdentity"/>.
        /// </summary>
        public string SignInAsAuthenticationType { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="IFacebookAuthenticationNotifications"/> used to handle authentication events.
        /// </summary>
        public IFacebookAuthenticationNotifications Notifications { get; set; }

        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }

        /// <summary>
        /// A list of permissions to request.
        /// </summary>
        public IList<string> Scope { get; private set; }

        /// <summary>
        /// Gets or sets if the appsecret_proof should be generated and sent with Facebook API calls.
        /// This is enabled by default.
        /// </summary>
        public bool SendAppSecretProof { get; set; }
    }
}

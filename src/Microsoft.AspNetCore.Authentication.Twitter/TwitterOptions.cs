// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Twitter;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder
{
    /// <summary>
    /// Options for the Twitter authentication middleware.
    /// </summary>
    public class TwitterOptions : RemoteAuthenticationOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TwitterOptions"/> class.
        /// </summary>
        public TwitterOptions(IEnumerable<IConfigureOptions<TwitterOptions>> configureOptions = null)
        {
            if (configureOptions != null)
            {
                foreach (var configure in configureOptions)
                {
                    configure.Configure(this);
                }
            }

            AuthenticationScheme = TwitterDefaults.AuthenticationScheme;
            DisplayName = AuthenticationScheme;
            CallbackPath = new PathString("/signin-twitter");
            BackchannelTimeout = TimeSpan.FromSeconds(60);
            Events = new TwitterEvents();
        }

        /// <summary>
        /// Gets or sets the consumer key used to communicate with Twitter.
        /// </summary>
        /// <value>The consumer key used to communicate with Twitter.</value>
        public string ConsumerKey { get; set; }

        /// <summary>
        /// Gets or sets the consumer secret used to sign requests to Twitter.
        /// </summary>
        /// <value>The consumer secret used to sign requests to Twitter.</value>
        public string ConsumerSecret { get; set; }

        /// <summary>
        /// Gets or sets the type used to secure data handled by the middleware.
        /// </summary>
        public ISecureDataFormat<RequestToken> StateDataFormat { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ITwitterEvents"/> used to handle authentication events.
        /// </summary>
        public new ITwitterEvents Events
        {
            get { return (ITwitterEvents)base.Events; }
            set { base.Events = value; }
        }

        /// <summary>
        /// For testing purposes only.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ISystemClock SystemClock { get; set; } = new SystemClock();
    }
}

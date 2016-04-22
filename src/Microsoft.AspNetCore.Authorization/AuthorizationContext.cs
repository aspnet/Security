// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Authorization
{
    /// <summary>
    /// Contains authorization information used by <see cref="IAuthorizationHandler"/>.
    /// </summary>
    public class AuthorizationContext
    {
        private HashSet<IAuthorizationRequirement> _pendingRequirements;
        private bool _failCalled;
        private bool _succeedCalled;

        public AuthorizationContext(
            IEnumerable<IAuthorizationRequirement> requirements,
            object authorizationData,
            object resource)
        {
            if (requirements == null)
            {
                throw new ArgumentNullException(nameof(requirements));
            }

            Requirements = requirements;
            AuthorizationData = authorizationData;
            Resource = resource;
            _pendingRequirements = new HashSet<IAuthorizationRequirement>(requirements);
        }

        public IEnumerable<IAuthorizationRequirement> Requirements { get; }
        public object AuthorizationData { get; }
        public object Resource { get; }
        
        public string Identifier { get; private set; }

        public IEnumerable<IAuthorizationRequirement> PendingRequirements { get { return _pendingRequirements; } }

        public bool HasFailed { get { return _failCalled; } }
        
        public bool HasSucceeded
        {
            get
            {
                return !_failCalled && _succeedCalled && !PendingRequirements.Any();
            }
        }

        public void Fail()
        {
            _failCalled = true;
        }

        public void Succeed(IAuthorizationRequirement requirement)
        {
            Succeed(requirement, null);
        }

        public void Succeed(IAuthorizationRequirement requirement, string identifier)
        {
            _succeedCalled = true;
            _pendingRequirements.Remove(requirement);
            Identifier = identifier;
        }
    }
}
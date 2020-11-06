// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class UserExtensionsCreateDerivation : DomainDerivation
    {
        public UserExtensionsCreateDerivation(M m) : base(m, new Guid("6ceeef02-2f30-4b97-8100-765997651f29")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.User.Interface),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<User>())
            {
                if (@this.ExistUserName && !@this.ExistUserProfile)
                {
                    @this.UserProfile = new UserProfileBuilder(@this.Strategy.Session).Build();
                }
            }
        }
    }
}

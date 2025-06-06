// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class RequestItemDeniedPermissionRule : Rule
    {
        public RequestItemDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("45bfa303-4cff-4e8f-889a-eac847d02849")) =>
            this.Patterns = new Pattern[]
        {
            m.RequestItem.RolePattern(v => v.TransitionalRevocations),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestItem>())
            {
                @this.Revocations = @this.TransitionalRevocations;

                var revocation = new Revocations(@this.Strategy.Transaction).RequestItemDeleteRevocation;
                if (@this.IsDeletable)
                {
                    @this.RemoveRevocation(revocation);
                }
                else
                {
                    @this.AddRevocation(revocation);
                }
            }
        }
    }
}

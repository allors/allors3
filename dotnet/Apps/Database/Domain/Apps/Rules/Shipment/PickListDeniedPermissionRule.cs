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

    public class PickListDeniedPermissionRule : Rule
    {
        public PickListDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("5650fa14-e2bb-4b7c-b08d-976b11994dea")) =>
            this.Patterns = new Pattern[]
        {
           m.PickList.RolePattern(v => v.TransitionalRevocations),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PickList>())
            {
                @this.Revocations = @this.TransitionalRevocations;
            }
        }
    }
}

// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;

    public class CustomerReturnDeniedPermissionRule : Rule
    {
        public CustomerReturnDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("c075cce8-97e4-4385-a8f4-982dfc416f03")) =>
            this.Patterns = new Pattern[]
        {
            m.CustomerReturn.RolePattern(v => v.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<CustomerReturn>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}

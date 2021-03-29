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

    public class EngineeringChangeDeniedPermissionRule : Rule
    {
        public EngineeringChangeDeniedPermissionRule(M m) : base(m, new Guid("1e2ca1f7-3c8c-45e6-a8c1-640200d0beed")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.EngineeringChange, m.EngineeringChange.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<EngineeringChange>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}

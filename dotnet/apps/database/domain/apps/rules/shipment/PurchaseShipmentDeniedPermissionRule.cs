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
    using Derivations.Rules;

    public class PurchaseShipmentDeniedPermissionRule : Rule
    {
        public PurchaseShipmentDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("5fb5bbff-84d0-4f66-9d75-3b7078a7019c")) =>
            this.Patterns = new Pattern[]
        {
            m.PurchaseShipment.RolePattern(v => v.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PurchaseShipment>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}

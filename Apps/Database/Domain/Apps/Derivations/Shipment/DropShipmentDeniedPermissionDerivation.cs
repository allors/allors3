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

    public class DropShipmentDeniedPermissionDerivation : DomainDerivation
    {
        public DropShipmentDeniedPermissionDerivation(M m) : base(m, new Guid("e1455944-f25a-4e89-a39e-92f17969d3e0")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.DropShipment, m.DropShipment.TransitionalDeniedPermissions),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<DropShipment>())
            {
                @this.DeniedPermissions = @this.TransitionalDeniedPermissions;
            }
        }
    }
}

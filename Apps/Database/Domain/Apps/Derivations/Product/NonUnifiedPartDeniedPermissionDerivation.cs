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

    public class NonUnifiedPartDeniedPermissionDerivation : DomainDerivation
    {
        public NonUnifiedPartDeniedPermissionDerivation(M m) : base(m, new Guid("ec943224-e151-4b7a-9ed9-6bb47f285932")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(m.WorkEffortInventoryProduced.Part){ Steps =  new IPropertyType[] {m.WorkEffortInventoryProduced.Part}, OfType = m.NonUnifiedPart.Class },
            new AssociationPattern(m.WorkEffortPartStandard.Part){ Steps =  new IPropertyType[] {m.WorkEffortPartStandard.Part}, OfType = m.NonUnifiedPart.Class },
            new AssociationPattern(m.PartBillOfMaterial.Part){ Steps =  new IPropertyType[] {m.PartBillOfMaterial.Part}, OfType = m.NonUnifiedPart.Class },
            new AssociationPattern(m.PartBillOfMaterial.ComponentPart){ Steps =  new IPropertyType[] {m.PartBillOfMaterial.ComponentPart}, OfType = m.NonUnifiedPart.Class },
            new AssociationPattern(m.InventoryItemTransaction.Part){ Steps =  new IPropertyType[] {m.InventoryItemTransaction.Part}, OfType = m.NonUnifiedPart.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta.ObjectType, @this.Meta.Delete);

                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}

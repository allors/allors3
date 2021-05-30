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

    public class NonUnifiedPartDeniedPermissionRule : Rule
    {
        public NonUnifiedPartDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("ec943224-e151-4b7a-9ed9-6bb47f285932")) =>
            this.Patterns = new Pattern[]
        {
            m.Part.AssociationPattern(v => v.WorkEffortInventoryProducedsWherePart, m.NonUnifiedPart),
            m.Part.AssociationPattern(v => v.WorkEffortPartStandardsWherePart, m.NonUnifiedPart),
            m.Part.AssociationPattern(v => v.PartBillOfMaterialsWherePart, m.NonUnifiedPart),
            m.Part.AssociationPattern(v => v.PartBillOfMaterialsWhereComponentPart, m.NonUnifiedPart),
            m.Part.AssociationPattern(v => v.NonUnifiedGoodsWherePart, m.NonUnifiedPart),
            m.Part.AssociationPattern(v => v.PurchaseInvoiceItemsWherePart, m.NonUnifiedPart),
            m.Part.AssociationPattern(v => v.PurchaseOrderItemsWherePart, m.NonUnifiedPart),
            m.Part.AssociationPattern(v => v.SalesInvoiceItemsWherePart, m.NonUnifiedPart),
            m.Part.AssociationPattern(v => v.ShipmentItemsWherePart, m.NonUnifiedPart),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                var deletePermission = new Permissions(@this.Strategy.Transaction).Get(@this.Meta, @this.Meta.Delete);

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

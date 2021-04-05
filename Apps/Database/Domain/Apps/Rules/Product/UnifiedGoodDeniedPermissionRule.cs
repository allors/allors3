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

    public class UnifiedGoodDeniedPermissionRule : Rule
    {
        public UnifiedGoodDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("2326b806-f01c-4bfb-9ed7-a2ca28a4d554")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(m.Deployment.ProductOffering) { OfType = m.UnifiedGood },
            new AssociationPattern(m.EngagementItem.Product) { OfType = m.UnifiedGood },
            new AssociationPattern(m.GeneralLedgerAccount.DerivedCostUnitsAllowed) { OfType = m.UnifiedGood },
            new AssociationPattern(m.GeneralLedgerAccount.DefaultCostUnit) { OfType = m.UnifiedGood },
            new AssociationPattern(m.QuoteItem.Product) { OfType = m.UnifiedGood },
            new AssociationPattern(m.ShipmentItem.Good) { OfType = m.UnifiedGood },
            new AssociationPattern(m.WorkEffortGoodStandard.UnifiedProduct) { OfType = m.UnifiedGood },
            new AssociationPattern(m.MarketingPackage.ProductsUsedIn) { OfType = m.UnifiedGood },
            new AssociationPattern(m.MarketingPackage.Product) { OfType = m.UnifiedGood },
            new AssociationPattern(m.OrganisationGlAccount.Product) { OfType = m.UnifiedGood },
            new AssociationPattern(m.ProductConfiguration.ProductsUsedIn) { OfType = m.UnifiedGood },
            new AssociationPattern(m.ProductConfiguration.Product) { OfType = m.UnifiedGood },
            new AssociationPattern(m.RequestItem.Product) { OfType = m.UnifiedGood },
            new AssociationPattern(m.SalesInvoiceItem.Product) { OfType = m.UnifiedGood },
            new AssociationPattern(m.SalesOrderItem.Product) { OfType = m.UnifiedGood },
            new AssociationPattern(m.WorkEffortType.ProductToProduce) { OfType = m.UnifiedGood },
            new AssociationPattern(m.WorkEffortInventoryProduced.Part) { OfType = m.UnifiedGood },
            new AssociationPattern(m.WorkEffortPartStandard.Part) { OfType = m.UnifiedGood },
            new AssociationPattern(m.PartBillOfMaterial.Part) { OfType = m.UnifiedGood },
            new AssociationPattern(m.PartBillOfMaterial.ComponentPart) { OfType = m.UnifiedGood },
            new AssociationPattern(m.InventoryItemTransaction.Part) { OfType = m.UnifiedGood },
            new RolePattern(m.UnifiedGood, m.UnifiedGood.SerialisedItems),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UnifiedGood>())
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

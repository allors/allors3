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

    public class UnifiedGoodDeniedPermissionDerivation : DomainDerivation
    {
        public UnifiedGoodDeniedPermissionDerivation(M m) : base(m, new Guid("2326b806-f01c-4bfb-9ed7-a2ca28a4d554")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.Deployment.ProductOffering) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.EngagementItem.Product) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.GeneralLedgerAccount.DerivedCostUnitsAllowed) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.GeneralLedgerAccount.DefaultCostUnit) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.QuoteItem.Product) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.ShipmentItem.Good) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.WorkEffortGoodStandard.UnifiedProduct) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.MarketingPackage.ProductsUsedIn) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.MarketingPackage.Product) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.OrganisationGlAccount.Product) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.ProductConfiguration.ProductsUsedIn) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.ProductConfiguration.Product) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.RequestItem.Product) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.SalesInvoiceItem.Product) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.SalesOrderItem.Product) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.WorkEffortType.ProductToProduce) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.WorkEffortInventoryProduced.Part) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.WorkEffortPartStandard.Part) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.PartBillOfMaterial.Part) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.PartBillOfMaterial.ComponentPart) { OfType = m.UnifiedGood.Class },
            new RolePattern(m.InventoryItemTransaction.Part) { OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.UnifiedGood.SerialisedItems),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UnifiedGood>())
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

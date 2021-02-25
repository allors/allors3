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
            new AssociationPattern(m.Deployment.ProductOffering){ Steps =  new IPropertyType[] {m.Deployment.ProductOffering}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.EngagementItem.Product){ Steps =  new IPropertyType[] {m.EngagementItem.Product}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.GeneralLedgerAccount.DerivedCostUnitsAllowed){ Steps =  new IPropertyType[] {m.GeneralLedgerAccount.DerivedCostUnitsAllowed}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.GeneralLedgerAccount.DefaultCostUnit){ Steps =  new IPropertyType[] {m.GeneralLedgerAccount.DefaultCostUnit}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.QuoteItem.Product){ Steps =  new IPropertyType[] {m.QuoteItem.Product}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.ShipmentItem.Good){ Steps =  new IPropertyType[] {m.ShipmentItem.Good}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.WorkEffortGoodStandard.UnifiedProduct){ Steps =  new IPropertyType[] {m.WorkEffortGoodStandard.UnifiedProduct}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.MarketingPackage.ProductsUsedIn){ Steps =  new IPropertyType[] {m.MarketingPackage.ProductsUsedIn}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.MarketingPackage.Product){ Steps =  new IPropertyType[] {m.MarketingPackage.Product}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.OrganisationGlAccount.Product){ Steps =  new IPropertyType[] {m.OrganisationGlAccount.Product}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.ProductConfiguration.ProductsUsedIn){ Steps =  new IPropertyType[] {m.ProductConfiguration.ProductsUsedIn}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.ProductConfiguration.Product){ Steps =  new IPropertyType[] {m.ProductConfiguration.Product}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.RequestItem.Product){ Steps =  new IPropertyType[] {m.RequestItem.Product}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.SalesInvoiceItem.Product){ Steps =  new IPropertyType[] {m.SalesInvoiceItem.Product}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.SalesOrderItem.Product){ Steps =  new IPropertyType[] {m.SalesOrderItem.Product}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.WorkEffortType.ProductToProduce){ Steps =  new IPropertyType[] {m.WorkEffortType.ProductToProduce}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.WorkEffortInventoryProduced.Part){ Steps =  new IPropertyType[] {m.WorkEffortInventoryProduced.Part}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.WorkEffortPartStandard.Part){ Steps =  new IPropertyType[] {m.WorkEffortPartStandard.Part}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.PartBillOfMaterial.Part){ Steps =  new IPropertyType[] {m.PartBillOfMaterial.Part}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.PartBillOfMaterial.ComponentPart){ Steps =  new IPropertyType[] {m.PartBillOfMaterial.ComponentPart}, OfType = m.UnifiedGood.Class },
            new AssociationPattern(m.InventoryItemTransaction.Part){ Steps =  new IPropertyType[] {m.InventoryItemTransaction.Part}, OfType = m.UnifiedGood.Class },
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

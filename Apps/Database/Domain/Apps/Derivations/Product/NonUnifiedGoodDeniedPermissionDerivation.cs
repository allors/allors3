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

    public class NonUnifiedGoodDeniedPermissionDerivation : DomainDerivation
    {
        public NonUnifiedGoodDeniedPermissionDerivation(M m) : base(m, new Guid("af1b5c08-9903-4d80-ad7c-d8588e324e3d")) =>
            this.Patterns = new Pattern[]
        {
            new AssociationPattern(this.M.NonUnifiedGood.Part),
            new AssociationPattern(this.M.Deployment.ProductOffering) { Steps = new IPropertyType[] {m.Deployment.ProductOffering }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.GoodOrderItem.Product) { Steps = new IPropertyType[] {m.GoodOrderItem.Product }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.GeneralLedgerAccount.DerivedCostUnitsAllowed) { Steps = new IPropertyType[] {m.GeneralLedgerAccount.DerivedCostUnitsAllowed }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.GeneralLedgerAccount.DefaultCostUnit) { Steps = new IPropertyType[] {m.GeneralLedgerAccount.DefaultCostUnit }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.QuoteItem.Product) { Steps = new IPropertyType[] {m.QuoteItem.Product }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.ShipmentItem.Good) { Steps = new IPropertyType[] {m.ShipmentItem.Good }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.WorkEffortGoodStandard.UnifiedProduct) { Steps = new IPropertyType[] {m.WorkEffortGoodStandard.UnifiedProduct }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.MarketingPackage.ProductsUsedIn) { Steps = new IPropertyType[] {m.MarketingPackage.ProductsUsedIn }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.MarketingPackage.Product) { Steps = new IPropertyType[] {m.MarketingPackage.Product }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.OrganisationGlAccount.Product) { Steps = new IPropertyType[] {m.OrganisationGlAccount.Product }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.ProductConfiguration.ProductsUsedIn) { Steps = new IPropertyType[] {m.ProductConfiguration.ProductsUsedIn }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.ProductConfiguration.Product) { Steps = new IPropertyType[] {m.ProductConfiguration.Product }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.RequestItem.Product) { Steps = new IPropertyType[] {m.RequestItem.Product }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.SalesInvoiceItem.Product) { Steps = new IPropertyType[] {m.SalesInvoiceItem.Product }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.SalesOrderItem.Product) { Steps = new IPropertyType[] {m.SalesOrderItem.Product }, OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(this.M.WorkEffortType.ProductToProduce) { Steps = new IPropertyType[] {m.WorkEffortType.ProductToProduce }, OfType = m.NonUnifiedGood.Class },
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedGood>())
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

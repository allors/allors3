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

    public class NonUnifiedGoodDeniedPermissionRule : Rule
    {
        public NonUnifiedGoodDeniedPermissionRule(M m) : base(m, new Guid("af1b5c08-9903-4d80-ad7c-d8588e324e3d")) =>
            this.Patterns = new Pattern[]
        {
            new RolePattern(m.NonUnifiedGood, m.NonUnifiedGood.Part),
            new AssociationPattern(m.Deployment.ProductOffering) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.GoodOrderItem.Product) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.GeneralLedgerAccount.DerivedCostUnitsAllowed) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.GeneralLedgerAccount.DefaultCostUnit) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.QuoteItem.Product) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.ShipmentItem.Good) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.WorkEffortGoodStandard.UnifiedProduct) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.MarketingPackage.ProductsUsedIn) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.MarketingPackage.Product) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.OrganisationGlAccount.Product) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.ProductConfiguration.ProductsUsedIn) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.ProductConfiguration.Product) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.RequestItem.Product) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.SalesInvoiceItem.Product) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.SalesOrderItem.Product) { OfType = m.NonUnifiedGood.Class },
            new AssociationPattern(m.WorkEffortType.ProductToProduce) { OfType = m.NonUnifiedGood.Class },
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

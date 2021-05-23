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
            m.UnifiedGood.RolePattern(v => v.SerialisedItems),
            m.Good.AssociationPattern(v => v.DeploymentsWhereProductOffering, m.UnifiedGood),
            m.Good.AssociationPattern(v => v.ShipmentItemsWhereGood, m.UnifiedGood),
            m.Part.AssociationPattern(v => v.WorkEffortInventoryProducedsWherePart, m.UnifiedGood),
            m.Part.AssociationPattern(v => v.WorkEffortPartStandardsWherePart, m.UnifiedGood),
            m.Part.AssociationPattern(v => v.PartBillOfMaterialsWherePart, m.UnifiedGood),
            m.Part.AssociationPattern(v => v.InventoryItemTransactionsWherePart, m.UnifiedGood),
            m.Part.AssociationPattern(v => v.PartBillOfMaterialsWhereComponentPart, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.EngagementItemsWhereProduct, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.QuoteItemsWhereProduct, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.MarketingPackagesWhereProduct, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.OrganisationGlAccountsWhereProduct, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.ProductConfigurationsWhereProduct, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.RequestItemsWhereProduct, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.SalesInvoiceItemsWhereProduct, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.SalesOrderItemsWhereProduct, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.GeneralLedgerAccountsWhereAssignedCostUnitsAllowed, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.GeneralLedgerAccountsWhereDefaultCostUnit, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.MarketingPackageWhereProductsUsedIn, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.ProductConfigurationsWhereProductsUsedIn, m.UnifiedGood),
            m.Product.AssociationPattern(v => v.WorkEffortTypesWhereProductToProduce, m.UnifiedGood),
            m.UnifiedProduct.AssociationPattern(v => v.WorkEffortGoodStandardsWhereUnifiedProduct, m.UnifiedGood),
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

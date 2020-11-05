// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class UnifiedGoodDeniedPermissionDerivation : DomainDerivation
    {
        public UnifiedGoodDeniedPermissionDerivation(M m) : base(m, new Guid("2326b806-f01c-4bfb-9ed7-a2ca28a4d554")) =>
            this.Patterns = new Pattern[]
        {
            new ChangedPattern(m.Deployment.ProductOffering){ Steps =  new IPropertyType[] {m.Deployment.ProductOffering}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.EngagementItem.Product){ Steps =  new IPropertyType[] {m.EngagementItem.Product}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.GeneralLedgerAccount.CostUnitsAllowed){ Steps =  new IPropertyType[] {m.GeneralLedgerAccount.CostUnitsAllowed}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.GeneralLedgerAccount.DefaultCostUnit){ Steps =  new IPropertyType[] {m.GeneralLedgerAccount.DefaultCostUnit}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.QuoteItem.Product){ Steps =  new IPropertyType[] {m.QuoteItem.Product}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.ShipmentItem.Good){ Steps =  new IPropertyType[] {m.ShipmentItem.Good}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.WorkEffortGoodStandard.UnifiedProduct){ Steps =  new IPropertyType[] {m.WorkEffortGoodStandard.UnifiedProduct}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.MarketingPackage.ProductsUsedIn){ Steps =  new IPropertyType[] {m.MarketingPackage.ProductsUsedIn}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.MarketingPackage.Product){ Steps =  new IPropertyType[] {m.MarketingPackage.Product}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.OrganisationGlAccount.Product){ Steps =  new IPropertyType[] {m.OrganisationGlAccount.Product}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.ProductConfiguration.ProductsUsedIn){ Steps =  new IPropertyType[] {m.ProductConfiguration.ProductsUsedIn}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.ProductConfiguration.Product){ Steps =  new IPropertyType[] {m.ProductConfiguration.Product}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.RequestItem.Product){ Steps =  new IPropertyType[] {m.RequestItem.Product}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.SalesInvoiceItem.Product){ Steps =  new IPropertyType[] {m.SalesInvoiceItem.Product}, OfType = m.UnifiedGood.Class },
            new ChangedPattern(m.SalesOrderItem.Product){ Steps =  new IPropertyType[] {m.SalesOrderItem.Product}, OfType = m.UnifiedGood.Class },

        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UnifiedGood>())
            {
                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);

                if (!@this.ExistDeploymentsWhereProductOffering &&
                                !@this.ExistEngagementItemsWhereProduct &&
                                !@this.ExistGeneralLedgerAccountsWhereCostUnitsAllowed &&
                                !@this.ExistGeneralLedgerAccountsWhereDefaultCostUnit &&
                                !@this.ExistQuoteItemsWhereProduct &&
                                !@this.ExistShipmentItemsWhereGood &&
                                !@this.ExistWorkEffortGoodStandardsWhereUnifiedProduct &&
                                !@this.ExistMarketingPackageWhereProductsUsedIn &&
                                !@this.ExistMarketingPackagesWhereProduct &&
                                !@this.ExistOrganisationGlAccountsWhereProduct &&
                                !@this.ExistProductConfigurationsWhereProductsUsedIn &&
                                !@this.ExistProductConfigurationsWhereProduct &&
                                !@this.ExistRequestItemsWhereProduct &&
                                !@this.ExistSalesInvoiceItemsWhereProduct &&

                                !@this.ExistSalesOrderItemsWhereProduct &&
                                !@this.ExistWorkEffortTypesWhereProductToProduce &&
                                !@this.ExistWorkEffortInventoryProducedsWherePart &&
                                !@this.ExistWorkEffortPartStandardsWherePart &&
                                !@this.ExistPartBillOfMaterialsWherePart &&
                                !@this.ExistPartBillOfMaterialsWhereComponentPart &&
                                !@this.ExistInventoryItemTransactionsWherePart &&
                                !@this.ExistSerialisedItems)
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

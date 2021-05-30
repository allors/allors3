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

    public class NonUnifiedGoodDeniedPermissionRule : Rule
    {
        public NonUnifiedGoodDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("af1b5c08-9903-4d80-ad7c-d8588e324e3d")) =>
            this.Patterns = new Pattern[]
        {
            m.NonUnifiedGood.RolePattern(v => v.Part),
            m.Good.AssociationPattern(v => v.DeploymentsWhereProductOffering, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.EngagementItemsWhereProduct, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.GeneralLedgerAccountsWhereDerivedCostUnitsAllowed, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.GeneralLedgerAccountsWhereDefaultCostUnit, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.QuoteItemsWhereProduct, m.NonUnifiedGood),
            m.Good.AssociationPattern(v => v.ShipmentItemsWhereGood, m.NonUnifiedGood),
            m.UnifiedProduct.AssociationPattern(v => v.WorkEffortGoodStandardsWhereUnifiedProduct, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.MarketingPackageWhereProductsUsedIn, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.MarketingPackagesWhereProduct, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.OrganisationGlAccountsWhereProduct, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.ProductConfigurationsWhereProductsUsedIn, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.ProductConfigurationsWhereProduct, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.RequestItemsWhereProduct, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.SalesInvoiceItemsWhereProduct, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.SalesOrderItemsWhereProduct, m.NonUnifiedGood),
            m.Product.AssociationPattern(v => v.WorkEffortTypesWhereProductToProduce, m.NonUnifiedGood),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedGood>())
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

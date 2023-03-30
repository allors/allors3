// <copyright file="NonUnifiedGood.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class NonUnifiedGood
    {
        public bool IsDeletable => !this.ExistPart &&
                                    !this.ExistDeploymentsWhereProductOffering &&
                                    !this.ExistEngagementItemsWhereProduct &&
                                    !this.ExistGeneralLedgerAccountsWhereAssignedCostUnitsAllowed &&
                                    !this.ExistGeneralLedgerAccountsWhereDefaultCostUnit &&
                                    !this.ExistQuoteItemsWhereProduct &&
                                    !this.ExistShipmentItemsWhereGood &&
                                    !this.ExistWorkEffortGoodStandardsWhereUnifiedProduct &&
                                    !this.ExistMarketingPackageWhereProductsUsedIn &&
                                    !this.ExistMarketingPackagesWhereProduct &&
                                    !this.ExistOrganisationGlAccountsWhereProduct &&
                                    !this.ExistProductConfigurationsWhereProductsUsedIn &&
                                    !this.ExistProductConfigurationsWhereProduct &&
                                    !this.ExistRequestItemsWhereProduct &&
                                    !this.ExistSalesInvoiceItemsWhereProduct &&
                                    !this.ExistSalesOrderItemsWhereProduct &&
                                    !this.ExistWorkEffortTypesWhereProductToProduce;

        public void AppsOnPostDerive(ObjectOnPostDerive method)
        {
            if (!this.ExistVariants && !this.ExistUnifiedProductWhereVariant)
            {
                method.Derivation.Validation.AssertExists(this, this.M.NonUnifiedGood.Part);
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (var deletable in this.AllVersions)
                {
                    deletable.Strategy.Delete();
                }

                foreach (var productIdentification in this.ProductIdentifications)
                {
                    productIdentification.CascadingDelete();
                }

                foreach (var localisedText in this.LocalisedNames)
                {
                    localisedText.CascadingDelete();
                }

                foreach (var localisedText in this.LocalisedDescriptions)
                {
                    localisedText.CascadingDelete();
                }

                foreach (var priceComponent in this.VirtualProductPriceComponents)
                {
                    priceComponent.CascadingDelete();
                }

                foreach (var estimatedProductCosts in this.EstimatedProductCosts)
                {
                    estimatedProductCosts.CascadingDelete();
                }
            }
        }
    }
}

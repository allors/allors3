// <copyright file="UnifiedGood.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class UnifiedGood
    {
        public bool IsDeletable => !this.ExistDeploymentsWhereProductOffering &&
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
                                    !this.ExistWorkEffortTypesWhereProductToProduce &&
                                    !this.ExistWorkEffortInventoryProducedsWherePart &&
                                    !this.ExistWorkEffortPartStandardsWherePart &&
                                    !this.ExistPartBillOfMaterialsWherePart &&
                                    !this.ExistPartBillOfMaterialsWhereComponentPart &&
                                    !this.ExistInventoryItemTransactionsWherePart &&
                                    !this.ExistSerialisedItems;

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistInventoryItemKind)
            {
                this.InventoryItemKind = new InventoryItemKinds(this.Strategy.Transaction).NonSerialised;
            }

            if (!this.ExistUnitOfMeasure)
            {
                this.UnitOfMeasure = new UnitsOfMeasure(this.Strategy.Transaction).Piece;
            }

            if (!this.ExistDefaultFacility)
            {
                this.DefaultFacility = this.Strategy.Transaction.GetSingleton().Settings.DefaultFacility;
            }

            this.DerivationTrigger = Guid.NewGuid();
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

                foreach (var productFeatureApplicability in this.ProductFeatureApplicabilitiesWhereAvailableFor)
                {
                    productFeatureApplicability.CascadingDelete();
                }

                foreach (var inventoryItem in this.InventoryItemsWherePart)
                {
                    inventoryItem.CascadingDelete();
                }

                foreach (var partSubstitute in this.PartSubstitutesWherePart)
                {
                    partSubstitute.CascadingDelete();
                }

                foreach (var partSubstitute in this.PartSubstitutesWhereSubstitutionPart)
                {
                    partSubstitute.CascadingDelete();
                }

                foreach (var supplierOffering in this.SupplierOfferingsWherePart)
                {
                    supplierOffering.CascadingDelete();
                }
            }
        }
    }
}

// <copyright file="UnifiedGood.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Allors.Database.Domain
{
    public partial class UnifiedGood
    {
        private bool IsDeletable => !this.ExistDeploymentsWhereProductOffering &&
                                    !this.ExistEngagementItemsWhereProduct &&
                                    !this.ExistGeneralLedgerAccountsWhereCostUnitsAllowed &&
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
                this.InventoryItemKind = new InventoryItemKinds(this.Strategy.Session).NonSerialised;
            }

            if (!this.ExistUnitOfMeasure)
            {
                this.UnitOfMeasure = new UnitsOfMeasure(this.Strategy.Session).Piece;
            }

            if (!this.ExistDefaultFacility)
            {
                this.DefaultFacility = this.Strategy.Session.GetSingleton().Settings.DefaultFacility;
            }

            this.DerivationTrigger = Guid.NewGuid();
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (ProductIdentification productIdentification in this.ProductIdentifications)
                {
                    productIdentification.Delete();
                }

                foreach (LocalisedText localisedText in this.LocalisedNames)
                {
                    localisedText.Delete();
                }

                foreach (LocalisedText localisedText in this.LocalisedDescriptions)
                {
                    localisedText.Delete();
                }

                foreach (PriceComponent priceComponent in this.VirtualProductPriceComponents)
                {
                    priceComponent.Delete();
                }

                foreach (EstimatedProductCost estimatedProductCosts in this.EstimatedProductCosts)
                {
                    estimatedProductCosts.Delete();
                }

                foreach (ProductFeatureApplicability productFeatureApplicability in this.ProductFeatureApplicabilitiesWhereAvailableFor)
                {
                    productFeatureApplicability.Delete();
                }

                foreach (InventoryItem inventoryItem in this.InventoryItemsWherePart)
                {
                    inventoryItem.Delete();
                }

                foreach (PartSubstitute partSubstitute in this.PartSubstitutesWherePart)
                {
                    partSubstitute.Delete();
                }

                foreach (PartSubstitute partSubstitute in this.PartSubstitutesWhereSubstitutionPart)
                {
                    partSubstitute.Delete();
                }

                foreach (SupplierOffering supplierOffering in this.SupplierOfferingsWherePart)
                {
                    supplierOffering.Delete();
                }
            }
        }
    }
}

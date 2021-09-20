// <copyright file="UnifiedGood.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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
                foreach (var productIdentification in this.ProductIdentifications)
                {
                    productIdentification.Delete();
                }

                foreach (var localisedText in this.LocalisedNames)
                {
                    localisedText.Delete();
                }

                foreach (var localisedText in this.LocalisedDescriptions)
                {
                    localisedText.Delete();
                }

                foreach (var priceComponent in this.VirtualProductPriceComponents)
                {
                    priceComponent.Delete();
                }

                foreach (var estimatedProductCosts in this.EstimatedProductCosts)
                {
                    estimatedProductCosts.Delete();
                }

                foreach (var productFeatureApplicability in this.ProductFeatureApplicabilitiesWhereAvailableFor)
                {
                    productFeatureApplicability.Delete();
                }

                foreach (var inventoryItem in this.InventoryItemsWherePart)
                {
                    inventoryItem.Delete();
                }

                foreach (var partSubstitute in this.PartSubstitutesWherePart)
                {
                    partSubstitute.Delete();
                }

                foreach (var partSubstitute in this.PartSubstitutesWhereSubstitutionPart)
                {
                    partSubstitute.Delete();
                }

                foreach (var supplierOffering in this.SupplierOfferingsWherePart)
                {
                    supplierOffering.Delete();
                }
            }
        }
    }
}

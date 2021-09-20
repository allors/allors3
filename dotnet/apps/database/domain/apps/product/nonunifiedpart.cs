// <copyright file="NonUnifiedPart.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class NonUnifiedPart
    {
        public bool IsDeletable => !this.ExistNonUnifiedGoodsWherePart
            && !this.ExistWorkEffortInventoryProducedsWherePart
            && !this.ExistWorkEffortPartStandardsWherePart
            && !this.ExistPartBillOfMaterialsWherePart
            && !this.ExistPartBillOfMaterialsWhereComponentPart
            && !this.ExistPurchaseInvoiceItemsWherePart
            && !this.ExistPurchaseOrderItemsWherePart
            && !this.ExistSalesInvoiceItemsWherePart
            && !this.ExistShipmentItemsWherePart;

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

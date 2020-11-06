// <copyright file="NonUnifiedPart.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class NonUnifiedPart
    {
        private bool IsDeletable =>
                               !this.ExistWorkEffortInventoryProducedsWherePart &&
                               !this.ExistWorkEffortPartStandardsWherePart &&
                               !this.ExistPartBillOfMaterialsWherePart &&
                               !this.ExistPartBillOfMaterialsWhereComponentPart &&
                               !this.ExistInventoryItemTransactionsWherePart;

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

            this.DeriveName();
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

        private void DeriveName()
        {
            if (!this.ExistName)
            {
                this.Name = "Part " + (this.PartIdentification() ?? this.UniqueId.ToString());
            }
        }
    }
}

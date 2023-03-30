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
                foreach (var @this in this.ProductIdentifications)
                {
                    @this.CascadingDelete();
                }

                foreach (var @this in this.LocalisedNames)
                {
                    @this.CascadingDelete();
                }

                foreach (var @this in this.LocalisedDescriptions)
                {
                    @this.CascadingDelete();
                }

                foreach (var @this in this.InventoryItemsWherePart)
                {
                    @this.CascadingDelete();
                }

                foreach (var @this in this.PartSubstitutesWherePart)
                {
                    @this.CascadingDelete();
                }

                foreach (var @this in this.PartSubstitutesWhereSubstitutionPart)
                {
                    @this.CascadingDelete();
                }

                foreach (var @this in this.SupplierOfferingsWherePart)
                {
                    @this.CascadingDelete();
                }

                foreach (var @this in this.PurchaseOrderItemByProductsWhereUnifiedProduct)
                {
                    @this.Strategy.Delete();
                }
            }
        }
    }
}

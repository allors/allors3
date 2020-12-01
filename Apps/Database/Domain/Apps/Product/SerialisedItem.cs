// <copyright file="SerialisedItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Linq;

    public partial class SerialisedItem
    {
        private bool IsDeletable =>
            !this.ExistInventoryItemTransactionsWhereSerialisedItem
            && !this.ExistPurchaseInvoiceItemsWhereSerialisedItem
            && !this.ExistPurchaseOrderItemsWhereSerialisedItem
            && !this.ExistQuoteItemsWhereSerialisedItem
            && !this.ExistSalesInvoiceItemsWhereSerialisedItem
            && !this.ExistSalesOrderItemsWhereSerialisedItem
            && !this.ExistSerialisedInventoryItemsWhereSerialisedItem
            && !this.ExistShipmentItemsWhereSerialisedItem;

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistSerialisedItemState)
            {
                this.SerialisedItemState = new SerialisedItemStates(this.Strategy.Session).NA;
            }

            if (!this.ExistItemNumber)
            {
                this.ItemNumber = this.Strategy.Session.GetSingleton().Settings.NextSerialisedItemNumber();
            }

            this.DerivationTrigger = Guid.NewGuid();
        }

        public void AppsDeriveDisplayProductCategories(SerialisedItemDeriveDisplayProductCategories method)
        {
            if (!method.Result.HasValue)
            {
                if (this.ExistPartWhereSerialisedItem && this.PartWhereSerialisedItem.GetType().Name == typeof(UnifiedGood).Name)
                {
                    var unifiedGood = this.PartWhereSerialisedItem as UnifiedGood;
                    this.DisplayProductCategories = string.Join(", ", unifiedGood.ProductCategoriesWhereProduct.Select(v => v.DisplayName));
                }

                method.Result = true;
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (LocalisedText deletable in this.LocalisedComments)
                {
                    deletable.Delete();
                }

                foreach (LocalisedText deletable in this.LocalisedNames)
                {
                    deletable.Delete();
                }

                foreach (LocalisedText deletable in this.LocalisedDescriptions)
                {
                    deletable.Delete();
                }

                foreach (LocalisedText deletable in this.LocalisedKeywords)
                {
                    deletable.Delete();
                }

                foreach (Media deletable in this.PublicElectronicDocuments)
                {
                    deletable.Delete();
                }

                foreach (Media deletable in this.PublicLocalisedElectronicDocuments)
                {
                    deletable.Delete();
                }

                foreach (Media deletable in this.PrivateElectronicDocuments)
                {
                    deletable.Delete();
                }

                foreach (Media deletable in this.PrivateLocalisedElectronicDocuments)
                {
                    deletable.Delete();
                }

                foreach (SerialisedItemCharacteristic deletable in this.SerialisedItemCharacteristics)
                {
                    deletable.Delete();
                }

                foreach (SerialisedItemVersion version in this.AllVersions)
                {
                    version.Delete();
                }
            }
        }
    }
}

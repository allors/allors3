// <copyright file="SerialisedItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SerialisedItem
    {
        public bool IsDeletable =>
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
                this.SerialisedItemState = new SerialisedItemStates(this.Strategy.Transaction).NA;
            }

            if (!this.ExistItemNumber)
            {
                this.ItemNumber = this.Strategy.Transaction.GetSingleton().Settings.NextSerialisedItemNumber();
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

                foreach (var deletable in this.LocalisedComments)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.LocalisedDescriptions)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.LocalisedKeywords)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.PublicElectronicDocuments)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.PublicLocalisedElectronicDocuments)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.PrivateElectronicDocuments)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.PrivateLocalisedElectronicDocuments)
                {
                    deletable.CascadingDelete();
                }

                foreach (var deletable in this.SerialisedItemCharacteristics)
                {
                    deletable.CascadingDelete();
                }

                foreach (var version in this.AllVersions)
                {
                    version.Strategy.Delete();
                }
            }
        }
    }
}

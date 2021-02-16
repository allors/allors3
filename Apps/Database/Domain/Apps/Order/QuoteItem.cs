// <copyright file="QuoteItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Text;

    public partial class QuoteItem
    {
        public decimal LineTotal => this.Quantity * this.UnitPrice;

        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.QuoteItem, this.M.QuoteItem.QuoteItemState),
        };

        public bool IsValid => !(this.QuoteItemState.IsCancelled || this.QuoteItemState.IsRejected);

        public bool WasValid => this.ExistLastObjectStates && !(this.LastQuoteItemState.IsCancelled || this.LastQuoteItemState.IsRejected);

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistQuoteItemState)
            {
                this.QuoteItemState = new QuoteItemStates(this.Strategy.Transaction).Draft;
            }

            if (this.ExistProduct && !this.ExistInvoiceItemType)
            {
                this.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Transaction).ProductItem;
            }
        }

        public void AppsDelete(QuoteItemDelete method)
        {
            if (this.ExistSerialisedItem)
            {
                this.SerialisedItem.DerivationTrigger = Guid.NewGuid();
            }
        }

        public void AppsSend(QuoteItemSend method) => this.QuoteItemState = new QuoteItemStates(this.Strategy.Transaction).AwaitingAcceptance;

        public void AppsCancel(QuoteItemCancel method) => this.QuoteItemState = new QuoteItemStates(this.Strategy.Transaction).Cancelled;

        public void AppsReject(QuoteItemReject method) => this.QuoteItemState = new QuoteItemStates(this.Strategy.Transaction).Rejected;

        public void AppsOrder(QuoteItemOrder method) => this.QuoteItemState = new QuoteItemStates(this.Strategy.Transaction).Ordered;

        public void AppsSubmit(QuoteItemSubmit method) => this.QuoteItemState = new QuoteItemStates(this.Strategy.Transaction).Submitted;

        public void Sync(Quote quote) => this.SyncedQuote = quote;
    }
}

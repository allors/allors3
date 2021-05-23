// <copyright file="RequestForQuote.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class RequestForQuote
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.RequestForQuote, this.M.RequestForQuote.RequestState),
        };

        public void AppsCreateQuote(RequestForQuoteCreateQuote method)
        {
            this.RequestState = new RequestStates(this.Strategy.Transaction).Quoted;
            this.QuoteThis();

            method.StopPropagation = true;
        }

        private ProductQuote QuoteThis()
        {
            var productQuote = new ProductQuoteBuilder(this.Strategy.Transaction)
                .WithRequest(this)
                .WithIssuer(this.Recipient)
                .WithContactPerson(this.ContactPerson)
                .WithDescription(this.Description)
                .WithReceiver(this.Originator)
                .WithRequiredResponseDate(this.RequiredResponseDate)
                .WithAssignedCurrency(this.DerivedCurrency)
                .WithFullfillContactMechanism(this.FullfillContactMechanism)
                .Build();

            var sourceItems = this.RequestItems.Where(i => i.RequestItemState.Equals(new RequestItemStates(this.Strategy.Transaction).Submitted)).ToArray();

            foreach (var requestItem in sourceItems)
            {
                requestItem.RequestItemState = new RequestItemStates(this.Strategy.Transaction).Quoted;

                productQuote.AddQuoteItem(
                    new QuoteItemBuilder(this.Strategy.Transaction)
                    .WithProduct(requestItem.Product)
                    .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction()).ProductItem)
                    .WithSerialisedItem(requestItem.SerialisedItem)
                    .WithProductFeature(requestItem.ProductFeature)
                    .WithQuantity(requestItem.Quantity)
                    .WithAssignedUnitPrice(requestItem.AssignedUnitPrice)
                    .WithUnitOfMeasure(requestItem.UnitOfMeasure)
                    .WithRequestItem(requestItem)
                    .WithInternalComment(requestItem.InternalComment).Build());
            }

            return productQuote;
        }
    }
}

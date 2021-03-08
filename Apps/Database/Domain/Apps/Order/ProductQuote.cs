// <copyright file="ProductQuote.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class ProductQuote
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[]{
            new TransitionalConfiguration(this.M.ProductQuote, this.M.ProductQuote.QuoteState),
        };

        private bool AppsNeedsApproval => false;

        public void AppsSetReadyForProcessing(ProductQuoteSetReadyForProcessing method)
        {
            this.QuoteState = this.AppsNeedsApproval
                ? new QuoteStates(this.Strategy.Transaction).AwaitingApproval : new QuoteStates(this.Strategy.Transaction).InProcess;

            method.StopPropagation = true;
        }

        public void AppsOrder(ProductQuoteOrder method)
        {
            this.QuoteState = new QuoteStates(this.Strategy.Transaction).Ordered;

            var quoteItemStates = new QuoteItemStates(this.Transaction());
            foreach (QuoteItem quoteItem in this.QuoteItems)
            {
                if (Equals(quoteItem.QuoteItemState, quoteItemStates.Accepted))
                {
                    quoteItem.QuoteItemState = quoteItemStates.Ordered;
                }
            }

            var salesOrder = new SalesOrderBuilder(this.Strategy.Transaction)
                .WithTakenBy(this.Issuer)
                .WithBillToCustomer(this.Receiver)
                .WithDescription(this.Description)
                .WithAssignedVatRegime(this.DerivedVatRegime)
                .WithAssignedIrpfRegime(this.DerivedIrpfRegime)
                .WithShipToContactPerson(this.ContactPerson)
                .WithBillToContactPerson(this.ContactPerson)
                .WithQuote(this)
                .WithAssignedCurrency(this.DerivedCurrency)
                .WithLocale(this.DerivedLocale)
                .Build();

            var quoteItems = this.ValidQuoteItems
                .Where(i => i.QuoteItemState.Equals(new QuoteItemStates(this.Strategy.Transaction).Ordered))
                .ToArray();

            foreach (var quoteItem in quoteItems)
            {
                quoteItem.QuoteItemState = new QuoteItemStates(this.Strategy.Transaction).Ordered;

                salesOrder.AddSalesOrderItem(
                    new SalesOrderItemBuilder(this.Strategy.Transaction)
                        .WithInvoiceItemType(quoteItem.InvoiceItemType)
                        .WithInternalComment(quoteItem.InternalComment)
                        .WithAssignedDeliveryDate(quoteItem.EstimatedDeliveryDate)
                        .WithAssignedUnitPrice(quoteItem.UnitPrice)
                        .WithAssignedVatRegime(quoteItem.AssignedVatRegime)
                        .WithAssignedIrpfRegime(quoteItem.AssignedIrpfRegime)
                        .WithProduct(quoteItem.Product)
                        .WithSerialisedItem(quoteItem.SerialisedItem)
                        .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction()).Sold)
                        .WithProductFeature(quoteItem.ProductFeature)
                        .WithQuantityOrdered(quoteItem.Quantity)
                        .WithInternalComment(quoteItem.InternalComment)
                        .Build());
            }

            method.StopPropagation = true;
        }

        public void AppsPrint(PrintablePrint method)
        {
            var singleton = this.Strategy.Transaction.GetSingleton();
            var logo = this.Issuer?.ExistLogoImage == true ?
                            this.Issuer.LogoImage.MediaContent.Data :
                            singleton.LogoImage.MediaContent.Data;

            var images = new Dictionary<string, byte[]>
                                {
                                    { "Logo1", logo },
                                    { "Logo2", logo },
                                };

            if (this.ExistQuoteNumber)
            {
                var transaction = this.Strategy.Transaction;
                var barcodeService = transaction.Database.Context().BarcodeGenerator;
                var barcode = barcodeService.Generate(this.QuoteNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                images.Add("Barcode", barcode);
            }

            var printModel = new Print.ProductQuoteModel.Model(this, images);
            this.RenderPrintDocument(this.Issuer?.ProductQuoteTemplate, printModel, images);

            this.PrintDocument.Media.InFileName = $"{this.QuoteNumber}.odt";

            method.StopPropagation = true;
        }
    }
}

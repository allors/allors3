// <copyright file="ProductQuote.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.State;

    public partial class ProductQuote
    {
        

        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[]{
            new TransitionalConfiguration(this.M.ProductQuote, this.M.ProductQuote.QuoteState),
        };

        private bool AppsNeedsApproval => false;

        public void AppsSetReadyForProcessing(ProductQuoteSetReadyForProcessing method)
        {
            if (!method.Result.HasValue)
            {
                this.QuoteState = this.AppsNeedsApproval
                    ? new QuoteStates(this.Strategy.Session).AwaitingApproval : new QuoteStates(this.Strategy.Session).InProcess;

                method.Result = true;
            }
        }

        public void AppsOrder(ProductQuoteOrder method)
        {
            this.QuoteState = new QuoteStates(this.Strategy.Session).Ordered;

            var quoteItemStates = new QuoteItemStates(this.Session());
            foreach (QuoteItem quoteItem in this.QuoteItems)
            {
                if (Equals(quoteItem.QuoteItemState, quoteItemStates.Accepted))
                {
                    quoteItem.QuoteItemState = quoteItemStates.Ordered;
                }
            }

            this.OrderThis();
        }

        public void AppsPrint(PrintablePrint method)
        {
            if (!method.IsPrinted)
            {
                var singleton = this.Strategy.Session.GetSingleton();
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
                    var session = this.Strategy.Session;
                    var barcodeService = session.Database.State().BarcodeGenerator;
                    var barcode = barcodeService.Generate(this.QuoteNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                var printModel = new Print.ProductQuoteModel.Model(this, images);
                this.RenderPrintDocument(this.Issuer?.ProductQuoteTemplate, printModel, images);

                this.PrintDocument.Media.InFileName = $"{this.QuoteNumber}.odt";
            }
        }

        private SalesOrder OrderThis()
        {
            var salesOrder = new SalesOrderBuilder(this.Strategy.Session)
                .WithTakenBy(this.Issuer)
                .WithBillToCustomer(this.Receiver)
                .WithDescription(this.Description)
                .WithVatRegime(this.VatRegime)
                .WithIrpfRegime(this.IrpfRegime)
                .WithShipToContactPerson(this.ContactPerson)
                .WithBillToContactPerson(this.ContactPerson)
                .WithQuote(this)
                .WithDescription(this.Description)
                .WithCurrency(this.Currency)
                .Build();

            var quoteItems = this.ValidQuoteItems
                .Where(i => i.QuoteItemState.Equals(new QuoteItemStates(this.Strategy.Session).Ordered))
                .ToArray();

            foreach (var quoteItem in quoteItems)
            {
                quoteItem.QuoteItemState = new QuoteItemStates(this.Strategy.Session).Ordered;

                salesOrder.AddSalesOrderItem(
                    new SalesOrderItemBuilder(this.Strategy.Session)
                        .WithInvoiceItemType(quoteItem.InvoiceItemType)
                        .WithInternalComment(quoteItem.InternalComment)
                        .WithAssignedDeliveryDate(quoteItem.EstimatedDeliveryDate)
                        .WithAssignedUnitPrice(quoteItem.UnitPrice)
                        .WithAssignedVatRegime(quoteItem.AssignedVatRegime)
                        .WithAssignedIrpfRegime(quoteItem.AssignedIrpfRegime)
                        .WithProduct(quoteItem.Product)
                        .WithSerialisedItem(quoteItem.SerialisedItem)
                        .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Session()).Sold)
                        .WithProductFeature(quoteItem.ProductFeature)
                        .WithQuantityOrdered(quoteItem.Quantity)
                        .WithInternalComment(quoteItem.InternalComment)
                        .Build());
            }

            return salesOrder;
        }
    }
}

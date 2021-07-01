// <copyright file="PurchaseInvoice.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class PurchaseInvoice
    {
        public bool IsDeletable =>
            (this.PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(this.Strategy.Transaction).Created)
                || this.PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(this.Strategy.Transaction).Cancelled)
                || this.PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(this.Strategy.Transaction).Rejected))
            && !this.ExistSalesInvoiceWherePurchaseInvoice;

        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[]{
            new TransitionalConfiguration(this.M.PurchaseInvoice, this.M.PurchaseInvoice.PurchaseInvoiceState),
        };

        public InvoiceItem[] InvoiceItems => this.PurchaseInvoiceItems.ToArray();

        public Task[] OpenTasks => this.TasksWhereWorkItem.Where(v => !v.ExistDateClosed).ToArray();

        public bool NeedsApproval
        {
            get
            {
                if (this.PurchaseOrders.Any())
                {
                    var orderTotal = this.PurchaseInvoiceItems.SelectMany(v => v.OrderItemBillingsWhereInvoiceItem).Select(o => o.OrderItem).Sum(i => i.TotalExVat);
                    return this.TotalExVat > this.ActualInvoiceAmount;
                }

                return true;
            }
        }

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistPurchaseInvoiceState)
            {
                this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Transaction).Created;
            }

            if (!this.ExistInvoiceDate)
            {
                this.InvoiceDate = this.Transaction().Now();
            }

            if (!this.ExistEntryDate)
            {
                this.EntryDate = this.Transaction().Now();
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(this.Strategy.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!this.ExistBilledTo && internalOrganisations.Count() == 1)
            {
                this.BilledTo = internalOrganisations.First();
            }
        }

        public void AppsPrint(PrintablePrint method)
        {
            var singleton = this.Strategy.Transaction.GetSingleton();
            var logo = this.BilledTo?.ExistLogoImage == true ?
                this.BilledTo.LogoImage.MediaContent.Data :
                singleton.LogoImage.MediaContent.Data;

            var images = new Dictionary<string, byte[]>
            {
                { "Logo", logo },
            };

            if (this.ExistInvoiceNumber)
            {
                var transaction = this.Strategy.Transaction;
                var barcodeService = transaction.Database.Services().BarcodeGenerator;
                var barcode = barcodeService.Generate(this.InvoiceNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                images.Add("Barcode", barcode);
            }

            var model = new Print.PurchaseInvoiceModel.Model(this);
            this.RenderPrintDocument(this.BilledTo?.PurchaseInvoiceTemplate, model, images);

            this.PrintDocument.Media.InFileName = $"{this.InvoiceNumber}.odt";

            method.StopPropagation = true;
        }

        public void AppsConfirm(PurchaseInvoiceConfirm method)
        {
            this.PurchaseInvoiceState = this.NeedsApproval ? new PurchaseInvoiceStates(this.Strategy.Transaction).AwaitingApproval : new PurchaseInvoiceStates(this.Strategy.Transaction).NotPaid;
            method.StopPropagation = true;
        }

        public void AppsCancel(PurchaseInvoiceCancel method)
        {
            this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Transaction).Cancelled;
            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.ValidInvoiceItems)
            {
                purchaseInvoiceItem.CancelFromInvoice();
            }

            method.StopPropagation = true;
        }

        public void AppsReject(PurchaseInvoiceReject method)
        {
            this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Transaction).Rejected;
            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.ValidInvoiceItems)
            {
                purchaseInvoiceItem.Reject();
            }

            method.StopPropagation = true;
        }

        public void AppsReopen(PurchaseInvoiceReopen method)
        {
            this.PurchaseInvoiceState = this.PreviousPurchaseInvoiceState;
            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.InvoiceItems)
            {
                purchaseInvoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItem.PreviousPurchaseInvoiceItemState;
            }

            method.StopPropagation = true;
        }

        public void AppsApprove(PurchaseInvoiceApprove method)
        {
            this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Transaction).NotPaid;

            var openTasks = this.TasksWhereWorkItem.Where(v => !v.ExistDateClosed).ToArray();

            if (openTasks.OfType<PurchaseInvoiceApproval>().Any())
            {
                openTasks.First().DateClosed = this.Transaction().Now();
            }

            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.ValidInvoiceItems)
            {
                if (purchaseInvoiceItem.ExistPart)
                {
                    var previousOffering = purchaseInvoiceItem.Part.SupplierOfferingsWherePart.FirstOrDefault(v =>
                        v.Supplier.Equals(this.BilledFrom) && v.FromDate <= this.Transaction().Now() &&
                        (!v.ExistThroughDate || v.ThroughDate >= this.Transaction().Now()));

                    if (previousOffering != null)
                    {
                        if (purchaseInvoiceItem.UnitBasePrice != previousOffering.Price)
                        {
                            previousOffering.ThroughDate = this.Transaction().Now();

                            var newOffering = new SupplierOfferingBuilder(this.Transaction())
                                .WithSupplier(this.BilledFrom)
                                .WithPart(purchaseInvoiceItem.Part)
                                .WithPrice(purchaseInvoiceItem.UnitBasePrice)
                                .WithFromDate(this.Transaction().Now())
                                .WithComment(previousOffering.Comment)
                                .WithMinimalOrderQuantity(previousOffering.MinimalOrderQuantity)
                                .WithPreference(previousOffering.Preference)
                                .WithQuantityIncrements(previousOffering.QuantityIncrements)
                                .WithRating(previousOffering.Rating)
                                .WithStandardLeadTime(previousOffering.StandardLeadTime)
                                .WithSupplierProductId(previousOffering.SupplierProductId)
                                .WithSupplierProductName(previousOffering.SupplierProductName)
                                .WithUnitOfMeasure(previousOffering.UnitOfMeasure)
                                .Build();

                            newOffering.LocalisedComments = previousOffering.LocalisedComments;
                        }
                    }
                    else
                    {
                        new SupplierOfferingBuilder(this.Transaction())
                            .WithSupplier(this.BilledFrom)
                            .WithPart(purchaseInvoiceItem.Part)
                            .WithUnitOfMeasure(purchaseInvoiceItem.Part?.UnitOfMeasure)
                            .WithPrice(purchaseInvoiceItem.UnitBasePrice)
                            .WithFromDate(this.Transaction().Now())
                            .Build();
                    }

                    foreach (var orderItemBilling in purchaseInvoiceItem.OrderItemBillingsWhereInvoiceItem)
                    {
                        foreach (var receipt in orderItemBilling.OrderItem.ShipmentReceiptsWhereOrderItem)
                        {
                            receipt.ShipmentItem.InventoryItemTransactionWhereShipmentItem.Cost = purchaseInvoiceItem.UnitBasePrice;
                        }
                    }
                }
            }

            method.StopPropagation = true;
        }

        public void AppsRevise(PurchaseInvoiceRevise method)
        {
            this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Transaction).Revising;
            method.StopPropagation = true;
        }

        public void AppsFinishRevising(PurchaseInvoiceFinishRevising method)
        {
            this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Transaction).Created;
            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.ValidInvoiceItems)
            {
                purchaseInvoiceItem.FinishRevising();
            }

            method.StopPropagation = true;
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (var orderAdjustment in this.OrderAdjustments)
                {
                    orderAdjustment.Delete();
                }

                foreach (var invoiceItem in this.PurchaseInvoiceItems)
                {
                    invoiceItem.Delete();
                }

                foreach (var salesTerm in this.SalesTerms)
                {
                    salesTerm.Delete();
                }
            }
        }

        public void AppsCreateSalesInvoice(PurchaseInvoiceCreateSalesInvoice method)
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Strategy.Transaction)
                .WithPurchaseInvoice(this)
                .WithBilledFrom(this.BilledTo)
                .WithBilledFromContactPerson(this.BilledToContactPerson)
                .WithBillToCustomer(this.BillToEndCustomer)
                .WithAssignedBillToContactMechanism(this.DerivedBillToEndCustomerContactMechanism)
                .WithBillToContactPerson(this.BillToEndCustomerContactPerson)
                .WithShipToCustomer(this.ShipToEndCustomer)
                .WithAssignedShipToAddress(this.DerivedShipToEndCustomerAddress)
                .WithShipToContactPerson(this.ShipToEndCustomerContactPerson)
                .WithDescription(this.Description)
                .WithInvoiceDate(this.Transaction().Now())
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Strategy.Transaction).SalesInvoice)
                .WithCustomerReference(this.CustomerReference)
                .WithAssignedPaymentMethod(this.DerivedBillToCustomerPaymentMethod)
                .WithComment(this.Comment)
                .WithInternalComment(this.InternalComment)
                .Build();

            foreach (var orderAdjustment in this.OrderAdjustments)
            {
                OrderAdjustment newAdjustment = null;
                if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                {
                    newAdjustment = new DiscountAdjustmentBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                {
                    newAdjustment = new SurchargeAdjustmentBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                {
                    newAdjustment = new FeeBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                {
                    newAdjustment = new ShippingAndHandlingChargeBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                {
                    newAdjustment = new MiscellaneousChargeBuilder(this.Transaction()).Build();
                }

                newAdjustment.Amount ??= orderAdjustment.Amount;
                newAdjustment.Percentage ??= orderAdjustment.Percentage;
                salesInvoice.AddOrderAdjustment(newAdjustment);
            }

            foreach (var purchaseInvoiceItem in this.PurchaseInvoiceItems)
            {
                var invoiceItem = new SalesInvoiceItemBuilder(this.Strategy.Transaction)
                    .WithInvoiceItemType(purchaseInvoiceItem.InvoiceItemType)
                    .WithAssignedUnitPrice(purchaseInvoiceItem.AssignedUnitPrice)
                    .WithProduct(purchaseInvoiceItem.Part as UnifiedGood)
                    .WithSerialisedItem(purchaseInvoiceItem.SerialisedItem)
                    .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Transaction()).Sold)
                    .WithQuantity(purchaseInvoiceItem.Quantity)
                    .WithComment(purchaseInvoiceItem.Comment)
                    .WithInternalComment(purchaseInvoiceItem.InternalComment)
                    .Build();

                salesInvoice.AddSalesInvoiceItem(invoiceItem);
            }

            var internalOrganisation = salesInvoice.BilledFrom;
            if (!internalOrganisation.ActiveCustomers.Contains(salesInvoice.BillToCustomer))
            {
                new CustomerRelationshipBuilder(this.Strategy.Transaction)
                    .WithCustomer(salesInvoice.BillToCustomer)
                    .WithInternalOrganisation(internalOrganisation)
                    .Build();
            }

            this.AddDeniedPermission(new Permissions(this.Strategy.Transaction).Get(this.Meta, this.Meta.CreateSalesInvoice));

            method.StopPropagation = true;
        }
    }
}

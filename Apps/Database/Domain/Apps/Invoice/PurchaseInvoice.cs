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
            (this.PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(this.Strategy.Session).Created)
                || this.PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(this.Strategy.Session).Cancelled)
                || this.PurchaseInvoiceState.Equals(new PurchaseInvoiceStates(this.Strategy.Session).Rejected))
            && !this.ExistSalesInvoiceWherePurchaseInvoice;

        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[]{
            new TransitionalConfiguration(this.M.PurchaseInvoice, this.M.PurchaseInvoice.PurchaseInvoiceState),
        };

        public InvoiceItem[] InvoiceItems => this.PurchaseInvoiceItems;

        public Task[] OpenTasks => this.TasksWhereWorkItem.Where(v => !v.ExistDateClosed).ToArray();

        public bool NeedsApproval
        {
            get
            {
                if (this.PurchaseOrders.Count > 0)
                {
                    var orderTotal = this.PurchaseInvoiceItems.SelectMany(v => v.OrderItemBillingsWhereInvoiceItem).Select(o => o.OrderItem).Sum(i => i.TotalExVat);
                    if (this.TotalExVat > this.ActualInvoiceAmount)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistPurchaseInvoiceState)
            {
                this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Session).Created;
            }

            if (!this.ExistInvoiceDate)
            {
                this.InvoiceDate = this.Session().Now();
            }

            if (!this.ExistEntryDate)
            {
                this.EntryDate = this.Session().Now();
            }
        }

        public void AppsOnInit(ObjectOnInit method)
        {
            var internalOrganisations = new Organisations(this.Strategy.Session).Extent().Where(v => Equals(v.IsInternalOrganisation, true)).ToArray();

            if (!this.ExistBilledTo && internalOrganisations.Count() == 1)
            {
                this.BilledTo = internalOrganisations.First();
            }
        }

        public void AppsPrint(PrintablePrint method)
        {
            if (!method.IsPrinted)
            {
                var singleton = this.Strategy.Session.GetSingleton();
                var logo = this.BilledTo?.ExistLogoImage == true ?
                    this.BilledTo.LogoImage.MediaContent.Data :
                    singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]>
                {
                    { "Logo", logo },
                };

                if (this.ExistInvoiceNumber)
                {
                    var session = this.Strategy.Session;
                    var barcodeService = session.Database.Context().BarcodeGenerator;
                    var barcode = barcodeService.Generate(this.InvoiceNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                var model = new Print.PurchaseInvoiceModel.Model(this);
                this.RenderPrintDocument(this.BilledTo?.PurchaseInvoiceTemplate, model, images);

                this.PrintDocument.Media.InFileName = $"{this.InvoiceNumber}.odt";
            }
        }

        public void AppsConfirm(PurchaseInvoiceConfirm method) => this.PurchaseInvoiceState = this.NeedsApproval ? new PurchaseInvoiceStates(this.Strategy.Session).AwaitingApproval : new PurchaseInvoiceStates(this.Strategy.Session).NotPaid;

        public void AppsCancel(PurchaseInvoiceCancel method)
        {
            this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Session).Cancelled;
            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.ValidInvoiceItems)
            {
                purchaseInvoiceItem.CancelFromInvoice();
            }
        }

        public void AppsReject(PurchaseInvoiceReject method)
        {
            this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Session).Rejected;
            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.ValidInvoiceItems)
            {
                purchaseInvoiceItem.Reject();
            }
        }

        public void AppsReopen(PurchaseInvoiceReopen method)
        {
            this.PurchaseInvoiceState = this.PreviousPurchaseInvoiceState;
            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.InvoiceItems)
            {
                purchaseInvoiceItem.PurchaseInvoiceItemState = purchaseInvoiceItem.PreviousPurchaseInvoiceItemState;
            }
        }

        public void AppsApprove(PurchaseInvoiceApprove method)
        {
            this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Session).NotPaid;

            var openTasks = this.TasksWhereWorkItem.Where(v => !v.ExistDateClosed).ToArray();

            if (openTasks.OfType<PurchaseInvoiceApproval>().Any())
            {
                openTasks.First().DateClosed = this.Session().Now();
            }

            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.ValidInvoiceItems)
            {
                if (purchaseInvoiceItem.ExistPart)
                {
                    var previousOffering = purchaseInvoiceItem.Part.SupplierOfferingsWherePart.FirstOrDefault(v =>
                        v.Supplier.Equals(this.BilledFrom) && v.FromDate <= this.Session().Now() &&
                        (!v.ExistThroughDate || v.ThroughDate >= this.Session().Now()));

                    if (previousOffering != null)
                    {
                        if (purchaseInvoiceItem.UnitBasePrice != previousOffering.Price)
                        {
                            previousOffering.ThroughDate = this.Session().Now();

                            var newOffering = new SupplierOfferingBuilder(this.Session())
                                .WithSupplier(this.BilledFrom)
                                .WithPart(purchaseInvoiceItem.Part)
                                .WithPrice(purchaseInvoiceItem.UnitBasePrice)
                                .WithFromDate(this.Session().Now())
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
                        new SupplierOfferingBuilder(this.Session())
                            .WithSupplier(this.BilledFrom)
                            .WithPart(purchaseInvoiceItem.Part)
                            .WithUnitOfMeasure(purchaseInvoiceItem.Part?.UnitOfMeasure)
                            .WithPrice(purchaseInvoiceItem.UnitBasePrice)
                            .WithFromDate(this.Session().Now())
                            .Build();
                    }

                    foreach (OrderItemBilling orderItemBilling in purchaseInvoiceItem.OrderItemBillingsWhereInvoiceItem)
                    {
                        foreach (ShipmentReceipt receipt in orderItemBilling.OrderItem.ShipmentReceiptsWhereOrderItem)
                        {
                            receipt.ShipmentItem.InventoryItemTransactionWhereShipmentItem.Cost = purchaseInvoiceItem.UnitBasePrice;
                        }
                    }
                }
            }
        }

        public void AppsRevise(PurchaseInvoiceRevise method)
        {
            this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Session).Revising;
        }

        public void AppsFinishRevising(PurchaseInvoiceFinishRevising method)
        {
            this.PurchaseInvoiceState = new PurchaseInvoiceStates(this.Strategy.Session).Created;
            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.ValidInvoiceItems)
            {
                purchaseInvoiceItem.FinishRevising();
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (OrderAdjustment orderAdjustment in this.OrderAdjustments)
                {
                    orderAdjustment.Delete();
                }

                foreach (PurchaseInvoiceItem invoiceItem in this.PurchaseInvoiceItems)
                {
                    invoiceItem.Delete();
                }

                foreach (SalesTerm salesTerm in this.SalesTerms)
                {
                    salesTerm.Delete();
                }
            }
        }

        public void AppsCreateSalesInvoice(PurchaseInvoiceCreateSalesInvoice method)
        {
            var salesInvoice = new SalesInvoiceBuilder(this.Strategy.Session)
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
                .WithInvoiceDate(this.Session().Now())
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Strategy.Session).SalesInvoice)
                .WithCustomerReference(this.CustomerReference)
                .WithAssignedPaymentMethod(this.DerivedBillToCustomerPaymentMethod)
                .WithComment(this.Comment)
                .WithInternalComment(this.InternalComment)
                .Build();

            foreach (OrderAdjustment orderAdjustment in this.OrderAdjustments)
            {
                OrderAdjustment newAdjustment = null;
                if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                {
                    newAdjustment = new DiscountAdjustmentBuilder(this.Session()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                {
                    newAdjustment = new SurchargeAdjustmentBuilder(this.Session()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                {
                    newAdjustment = new FeeBuilder(this.Session()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                {
                    newAdjustment = new ShippingAndHandlingChargeBuilder(this.Session()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                {
                    newAdjustment = new MiscellaneousChargeBuilder(this.Session()).Build();
                }

                newAdjustment.Amount ??= orderAdjustment.Amount;
                newAdjustment.Percentage ??= orderAdjustment.Percentage;
                salesInvoice.AddOrderAdjustment(newAdjustment);
            }

            foreach (PurchaseInvoiceItem purchaseInvoiceItem in this.PurchaseInvoiceItems)
            {
                var invoiceItem = new SalesInvoiceItemBuilder(this.Strategy.Session)
                    .WithInvoiceItemType(purchaseInvoiceItem.InvoiceItemType)
                    .WithAssignedUnitPrice(purchaseInvoiceItem.AssignedUnitPrice)
                    .WithProduct(purchaseInvoiceItem.Part as UnifiedGood)
                    .WithSerialisedItem(purchaseInvoiceItem.SerialisedItem)
                    .WithNextSerialisedItemAvailability(new SerialisedItemAvailabilities(this.Session()).Sold)
                    .WithQuantity(purchaseInvoiceItem.Quantity)
                    .WithComment(purchaseInvoiceItem.Comment)
                    .WithInternalComment(purchaseInvoiceItem.InternalComment)
                    .Build();

                salesInvoice.AddSalesInvoiceItem(invoiceItem);
            }

            var internalOrganisation = (InternalOrganisation)salesInvoice.BilledFrom;
            if (!internalOrganisation.ActiveCustomers.Contains(salesInvoice.BillToCustomer))
            {
                new CustomerRelationshipBuilder(this.Strategy.Session)
                    .WithCustomer(salesInvoice.BillToCustomer)
                    .WithInternalOrganisation(internalOrganisation)
                    .Build();
            }

            this.AddDeniedPermission(new Permissions(this.Strategy.Session).Get(this.Meta.ObjectType, this.Meta.CreateSalesInvoice));
        }
    }
}

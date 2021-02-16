
// <copyright file="SalesOrder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;
    using System.Linq;

    public partial class SalesOrder
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.SalesOrder, this.M.SalesOrder.SalesOrderState),
            new TransitionalConfiguration(this.M.SalesOrder, this.M.SalesOrder.SalesOrderShipmentState),
            new TransitionalConfiguration(this.M.SalesOrder, this.M.SalesOrder.SalesOrderInvoiceState),
            new TransitionalConfiguration(this.M.SalesOrder, this.M.SalesOrder.SalesOrderPaymentState),
        };

        public int PaymentNetDays
        {
            get
            {
                if (this.ExistSalesTerms)
                {
                    foreach (AgreementTerm term in this.SalesTerms)
                    {
                        if (term.TermType.Equals(new InvoiceTermTypes(this.Strategy.Transaction).PaymentNetDays))
                        {
                            if (int.TryParse(term.TermValue, out var netDays))
                            {
                                return netDays;
                            }
                        }
                    }
                }

                var now = this.Transaction().Now();
                var customerRelationship = this.BillToCustomer.CustomerRelationshipsWhereCustomer
                    .FirstOrDefault(v => Equals(v.InternalOrganisation, this.TakenBy)
                      && v.FromDate <= now
                      && (!v.ExistThroughDate || v.ThroughDate >= now));

                if (customerRelationship?.PaymentNetDays().HasValue == true)
                {
                    return customerRelationship.PaymentNetDays().Value;
                }

                if (this.ExistStore && this.Store.ExistPaymentNetDays)
                {
                    return this.Store.PaymentNetDays;
                }

                return 0;
            }
        }

        public bool IsDeletable =>
            (this.SalesOrderState.Equals(new SalesOrderStates(this.Strategy.Transaction).Provisional)
                || this.SalesOrderState.Equals(new SalesOrderStates(this.Strategy.Transaction).ReadyForPosting)
                || this.SalesOrderState.Equals(new SalesOrderStates(this.Strategy.Transaction).Cancelled)
                || this.SalesOrderState.Equals(new SalesOrderStates(this.Strategy.Transaction).Rejected))
            && !this.ExistQuote
            && this.SalesOrderItems.All(v => v.IsDeletable);

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistSalesOrderState)
            {
                this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).Provisional;
            }

            if (!this.ExistSalesOrderShipmentState)
            {
                this.SalesOrderShipmentState = new SalesOrderShipmentStates(this.Strategy.Transaction).NotShipped;
            }

            if (!this.ExistSalesOrderInvoiceState)
            {
                this.SalesOrderInvoiceState = new SalesOrderInvoiceStates(this.Strategy.Transaction).NotInvoiced;
            }

            if (!this.ExistSalesOrderPaymentState)
            {
                this.SalesOrderPaymentState = new SalesOrderPaymentStates(this.Strategy.Transaction).NotPaid;
            }

            if (!this.ExistOrderDate)
            {
                this.OrderDate = this.Transaction().Now();
            }

            if (!this.ExistEntryDate)
            {
                this.EntryDate = this.Transaction().Now();
            }

            if (!this.ExistPartiallyShip)
            {
                this.PartiallyShip = true;
            }

            if (!this.ExistTakenBy)
            {
                var internalOrganisations = new Organisations(this.Strategy.Transaction).InternalOrganisations();
                if (internalOrganisations.Count() == 1)
                {
                    this.TakenBy = internalOrganisations.First();
                }
            }

            if (!this.ExistStore && this.ExistTakenBy)
            {
                var stores = new Stores(this.Strategy.Transaction).Extent();
                stores.Filter.AddEquals(this.M.Store.InternalOrganisation, this.TakenBy);

                if (stores.Any())
                {
                    this.Store = stores.First;
                }
            }

            if (!this.ExistOriginFacility)
            {
                this.OriginFacility = this.ExistStore ? this.Store.DefaultFacility : this.Strategy.Transaction.GetSingleton().Settings.DefaultFacility;
            }
        }

        public void AppsDelete(SalesOrderDelete method)
        {
            if (this.IsDeletable)
            {
                foreach (OrderAdjustment orderAdjustment in this.OrderAdjustments)
                {
                    orderAdjustment.Delete();
                }

                foreach (SalesOrderItem item in this.SalesOrderItems)
                {
                    item.Delete();
                }

                foreach (SalesTerm salesTerm in this.SalesTerms)
                {
                    salesTerm.Delete();
                }
            }
        }

        public void AppsCancel(OrderCancel method) => this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).Cancelled;

        public void AppsSetReadyForPosting(SalesOrderSetReadyForPosting method)
        {
            var orderThreshold = this.Store.OrderThreshold;
            var partyFinancial = this.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.FirstOrDefault(v => Equals(v.InternalOrganisation, this.TakenBy));

            var amountOverDue = partyFinancial.AmountOverDue;
            var creditLimit = partyFinancial.CreditLimit ?? (this.Store.ExistCreditLimit ? this.Store.CreditLimit : 0);

            if (amountOverDue > creditLimit || this.TotalExVat < orderThreshold) // Theshold is minimum order amount required.
            {
                this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).RequestsApproval;
            }
            else
            {
                this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).ReadyForPosting;
            }
        }

        public void AppsPost(SalesOrderPost method) => this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).AwaitingAcceptance;

        public void AppsReject(OrderReject method) => this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).Rejected;

        public void AppsReopen(OrderReopen method) => this.SalesOrderState = this.PreviousSalesOrderState;

        public void AppsHold(OrderHold method) => this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).OnHold;

        public void AppsApprove(OrderApprove method) => this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).ReadyForPosting;

        public void AppsAccept(SalesOrderAccept method) => this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).InProcess;

        public void AppsRevise(OrderRevise method) => this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).Provisional;

        public void AppsContinue(OrderContinue method) => this.SalesOrderState = this.PreviousSalesOrderState;

        public void AppsComplete(OrderComplete method) => this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).Completed;

        public void AppsShip(SalesOrderShip method)
        {
            if (this.CanShip)
            {
                var addresses = this.ShipToAddresses();
                var shipments = new List<Shipment>();
                if (addresses.Count > 0)
                {
                    foreach (var address in addresses)
                    {
                        var pendingShipment = address.Value.AppsGetPendingCustomerShipmentForStore(address.Key, this.Store, this.DerivedShipmentMethod);

                        if (pendingShipment == null)
                        {
                            pendingShipment = new CustomerShipmentBuilder(this.Strategy.Transaction)
                                .WithShipFromParty(this.TakenBy)
                                .WithShipFromAddress(this.DerivedShipFromAddress)
                                .WithShipToAddress(address.Key)
                                .WithShipToParty(address.Value)
                                .WithStore(this.Store)
                                .WithShipmentMethod(this.DerivedShipmentMethod)
                                .WithPaymentMethod(this.DerivedPaymentMethod)
                                .Build();

                            if (this.Store.AutoGenerateShipmentPackage)
                            {
                                pendingShipment.AddShipmentPackage(new ShipmentPackageBuilder(this.Strategy.Transaction).Build());
                            }
                        }

                        foreach (SalesOrderItem orderItem in this.ValidOrderItems)
                        {
                            var orderItemDerivedRoles = orderItem;

                            if (orderItem.ExistProduct && orderItem.DerivedShipToAddress.Equals(address.Key) && orderItem.QuantityRequestsShipping > 0)
                            {
                                var good = orderItem.Product as Good;
                                var nonUnifiedGood = orderItem.Product as NonUnifiedGood;
                                var unifiedGood = orderItem.Product as UnifiedGood;
                                var inventoryItemKind = unifiedGood?.InventoryItemKind ?? nonUnifiedGood?.Part.InventoryItemKind;
                                var part = unifiedGood ?? nonUnifiedGood?.Part;

                                ShipmentItem shipmentItem = null;
                                foreach (ShipmentItem item in pendingShipment.ShipmentItems)
                                {
                                    if (inventoryItemKind != null
                                        && inventoryItemKind.Equals(new InventoryItemKinds(this.Transaction()).NonSerialised)
                                        && item.Good.Equals(good)
                                        && !item.ItemIssuancesWhereShipmentItem.Any(v => v.PickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(this.Transaction()).Picked)))
                                    {
                                        shipmentItem = item;
                                        break;
                                    }
                                }

                                if (shipmentItem != null)
                                {
                                    shipmentItem.ContentsDescription = $"{shipmentItem.Quantity} * {good.Name}";
                                }
                                else
                                {
                                    shipmentItem = new ShipmentItemBuilder(this.Strategy.Transaction)
                                        .WithGood(good)
                                        .WithContentsDescription($"{orderItem.QuantityRequestsShipping} * {good}")
                                        .Build();

                                    if (orderItem.ExistSerialisedItem)
                                    {
                                        shipmentItem.SerialisedItem = orderItem.SerialisedItem;
                                    }

                                    if (orderItem.ExistNextSerialisedItemAvailability)
                                    {
                                        shipmentItem.NextSerialisedItemAvailability = orderItem.NextSerialisedItemAvailability;
                                    }

                                    if (orderItem.ExistReservedFromNonSerialisedInventoryItem)
                                    {
                                        shipmentItem.AddReservedFromInventoryItem(orderItem.ReservedFromNonSerialisedInventoryItem);
                                    }

                                    if (orderItem.ExistReservedFromSerialisedInventoryItem)
                                    {
                                        shipmentItem.AddReservedFromInventoryItem(orderItem.ReservedFromSerialisedInventoryItem);
                                    }

                                    pendingShipment.AddShipmentItem(shipmentItem);
                                }

                                foreach (SalesOrderItem featureItem in orderItem.OrderedWithFeatures)
                                {
                                    shipmentItem.AddProductFeature(featureItem.ProductFeature);
                                }

                                new OrderShipmentBuilder(this.Strategy.Transaction)
                                    .WithOrderItem(orderItem)
                                    .WithShipmentItem(shipmentItem)
                                    .WithQuantity(orderItem.QuantityRequestsShipping)
                                    .Build();

                                shipmentItem.Quantity = shipmentItem.OrderShipmentsWhereShipmentItem.Sum(v => v.Quantity);

                                orderItemDerivedRoles.QuantityRequestsShipping = 0;

                                orderItemDerivedRoles.CostOfGoodsSold = orderItem.QuantityOrdered * part.PartWeightedAverage.AverageCost;
                            }
                        }

                        shipments.Add(pendingShipment);
                        this.AddDeniedPermission(new Permissions(this.Strategy.Transaction).Get(this.Meta.Class, this.Meta.Ship));
                    }
                }
            }
        }

        public void AppsInvoice(SalesOrderInvoice method)
        {
            if (this.CanInvoice)
            {
                var salesInvoice = new SalesInvoiceBuilder(this.Strategy.Transaction)
                    .WithBilledFrom(this.TakenBy)
                    .WithAssignedBilledFromContactMechanism(this.DerivedTakenByContactMechanism)
                    .WithBilledFromContactPerson(this.TakenByContactPerson)
                    .WithBillToCustomer(this.BillToCustomer)
                    .WithAssignedBillToContactMechanism(this.DerivedBillToContactMechanism)
                    .WithBillToContactPerson(this.BillToContactPerson)
                    .WithBillToEndCustomer(this.BillToEndCustomer)
                    .WithAssignedBillToEndCustomerContactMechanism(this.DerivedBillToEndCustomerContactMechanism)
                    .WithBillToEndCustomerContactPerson(this.BillToEndCustomerContactPerson)
                    .WithShipToCustomer(this.ShipToCustomer)
                    .WithAssignedShipToAddress(this.DerivedShipToAddress)
                    .WithShipToContactPerson(this.ShipToContactPerson)
                    .WithShipToEndCustomer(this.ShipToEndCustomer)
                    .WithAssignedShipToEndCustomerAddress(this.DerivedShipToEndCustomerAddress)
                    .WithShipToEndCustomerContactPerson(this.ShipToEndCustomerContactPerson)
                    .WithDescription(this.Description)
                    .WithStore(this.Store)
                    .WithInvoiceDate(this.Transaction().Now())
                    .WithSalesChannel(this.SalesChannel)
                    .WithSalesInvoiceType(new SalesInvoiceTypes(this.Strategy.Transaction).SalesInvoice)
                    .WithAssignedVatRegime(this.DerivedVatRegime)
                    .WithAssignedIrpfRegime(this.DerivedIrpfRegime)
                    .WithAssignedVatClause(this.DerivedVatClause)
                    .WithCustomerReference(this.CustomerReference)
                    .WithAssignedPaymentMethod(this.DerivedPaymentMethod)
                    .WithAssignedCurrency(this.DerivedCurrency)
                    .WithLocale(this.DerivedLocale)
                    .Build();

                foreach (OrderAdjustment orderAdjustment in this.OrderAdjustments)
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

                foreach (SalesOrderItem orderItem in this.ValidOrderItems)
                {
                    var amountAlreadyInvoiced = orderItem.OrderItemBillingsWhereOrderItem.Sum(v => v.Amount);

                    var leftToInvoice = (orderItem.QuantityOrdered * orderItem.UnitPrice) - amountAlreadyInvoiced;

                    if (leftToInvoice != 0)
                    {
                        var invoiceItem = new SalesInvoiceItemBuilder(this.Strategy.Transaction)
                            .WithInvoiceItemType(orderItem.InvoiceItemType)
                            .WithAssignedUnitPrice(orderItem.UnitPrice)
                            .WithProduct(orderItem.Product)
                            .WithQuantity(orderItem.QuantityOrdered)
                            .WithCostOfGoodsSold(orderItem.CostOfGoodsSold)
                            .WithAssignedVatRegime(orderItem.AssignedVatRegime)
                            .WithAssignedIrpfRegime(orderItem.AssignedIrpfRegime)
                            .WithDescription(orderItem.Description)
                            .WithInternalComment(orderItem.InternalComment)
                            .WithMessage(orderItem.Message)
                            .Build();

                        if (orderItem.ExistSerialisedItem)
                        {
                            invoiceItem.SerialisedItem = orderItem.SerialisedItem;
                            invoiceItem.NextSerialisedItemAvailability = orderItem.NextSerialisedItemAvailability;
                        }

                        salesInvoice.AddSalesInvoiceItem(invoiceItem);

                        new OrderItemBillingBuilder(this.Strategy.Transaction)
                            .WithQuantity(orderItem.QuantityOrdered)
                            .WithAmount(leftToInvoice)
                            .WithOrderItem(orderItem)
                            .WithInvoiceItem(invoiceItem)
                            .Build();
                    }
                }

                foreach (SalesTerm salesTerm in this.SalesTerms)
                {
                    if (salesTerm.GetType().Name == typeof(IncoTerm).Name)
                    {
                        salesInvoice.AddSalesTerm(new IncoTermBuilder(this.Strategy.Transaction)
                            .WithTermType(salesTerm.TermType)
                            .WithTermValue(salesTerm.TermValue)
                            .WithDescription(salesTerm.Description)
                            .Build());
                    }

                    if (salesTerm.GetType().Name == typeof(InvoiceTerm).Name)
                    {
                        salesInvoice.AddSalesTerm(new InvoiceTermBuilder(this.Strategy.Transaction)
                            .WithTermType(salesTerm.TermType)
                            .WithTermValue(salesTerm.TermValue)
                            .WithDescription(salesTerm.Description)
                            .Build());
                    }

                    if (salesTerm.GetType().Name == typeof(OrderTerm).Name)
                    {
                        salesInvoice.AddSalesTerm(new OrderTermBuilder(this.Strategy.Transaction)
                            .WithTermType(salesTerm.TermType)
                            .WithTermValue(salesTerm.TermValue)
                            .WithDescription(salesTerm.Description)
                            .Build());
                    }
                }
            }
        }

        public void AppsPrint(PrintablePrint method)
        {
            if (!method.IsPrinted)
            {
                var singleton = this.Strategy.Transaction.GetSingleton();
                var logo = this.TakenBy?.ExistLogoImage == true ?
                               this.TakenBy.LogoImage.MediaContent.Data :
                               singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]>
                                 {
                                     { "Logo", logo },
                                 };

                if (this.ExistOrderNumber)
                {
                    var transaction = this.Strategy.Transaction;
                    var barcodeService = transaction.Database.Context().BarcodeGenerator;
                    var barcode = barcodeService.Generate(this.OrderNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                var model = new Print.SalesOrderModel.Model(this);
                this.RenderPrintDocument(this.TakenBy?.SalesOrderTemplate, model, images);

                this.PrintDocument.Media.InFileName = $"{this.OrderNumber}.odt";
            }
        }

        private Dictionary<PostalAddress, Party> ShipToAddresses()
        {
            var addresses = new Dictionary<PostalAddress, Party>();
            foreach (SalesOrderItem item in this.ValidOrderItems)
            {
                if (item.QuantityRequestsShipping > 0)
                {
                    if (!addresses.ContainsKey(item.DerivedShipToAddress))
                    {
                        addresses.Add(item.DerivedShipToAddress, item.DerivedShipToParty);
                    }
                }
            }

            return addresses;
        }
    }
}

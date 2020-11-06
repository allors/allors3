// <copyright file="SalesOrderItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;

    public partial class SalesOrderItem
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.SalesOrderItem, this.M.SalesOrderItem.SalesOrderItemState),
            new TransitionalConfiguration(this.M.SalesOrderItem, this.M.SalesOrderItem.SalesOrderItemShipmentState),
            new TransitionalConfiguration(this.M.SalesOrderItem, this.M.SalesOrderItem.SalesOrderItemInvoiceState),
            new TransitionalConfiguration(this.M.SalesOrderItem, this.M.SalesOrderItem.SalesOrderItemPaymentState),
        };

        public bool IsValid => !(this.SalesOrderItemState.IsCancelled || this.SalesOrderItemState.IsRejected);

        public bool WasValid => this.ExistLastObjectStates && !(this.LastSalesOrderItemState.IsCancelled || this.LastSalesOrderItemState.IsRejected);

        internal bool IsDeletable =>
            (this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Session).Provisional)
                || this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Session).ReadyForPosting)
                || this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Session).Cancelled)
                || this.SalesOrderItemState.Equals(new SalesOrderItemStates(this.Strategy.Session).Rejected))
            && !this.ExistOrderItemBillingsWhereOrderItem
            && !this.ExistOrderShipmentsWhereOrderItem
            && !this.ExistOrderRequirementCommitmentsWhereOrderItem
            && !this.ExistWorkEffortsWhereOrderItemFulfillment;

        public Part Part
        {
            get
            {
                if (this.ExistProduct)
                {
                    var nonUnifiedGood = this.Product as NonUnifiedGood;
                    var unifiedGood = this.Product as UnifiedGood;
                    return unifiedGood ?? nonUnifiedGood?.Part;
                }

                return null;
            }
        }

        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.SyncedOrder?.SecurityTokens.ToArray();
            }

            if (method.DeniedPermissions == null)
            {
                method.DeniedPermissions = this.SyncedOrder?.DeniedPermissions.ToArray();
            }
        }

        public void AppsDelete(SalesOrderItemDelete method)
        {
            foreach (SalesTerm salesTerm in this.SalesTerms)
            {
                salesTerm.Delete();
            }

            if (this.ExistSerialisedItem)
            {
                this.SerialisedItem.DerivationTrigger = Guid.NewGuid();
            }
        }

        public void AppsCancel(OrderItemCancel method) => this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Session).Cancelled;

        public void AppsReject(OrderItemReject method) => this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Session).Rejected;

        public void AppsApprove(OrderItemApprove method) => this.SalesOrderItemState = new SalesOrderItemStates(this.Strategy.Session).ReadyForPosting;

        public void AppsReopen(OrderItemReopen method) => this.SalesOrderItemState = this.PreviousSalesOrderItemState;

        //public void SyncPrices(IDerivation derivation, SalesOrder salesOrder) => this.CalculatePrice(derivation, salesOrder, true);

        public void Sync(Order order) => this.SyncedOrder = order;

        //private void CalculatePrice(IDerivation derivation, SalesOrder salesOrder, bool useValueOrdered = false)
        //{
        //    var sameProductItems = salesOrder.SalesOrderItems
        //        .Where(v => v.IsValid && v.ExistProduct && v.Product.Equals(this.Product))
        //        .ToArray();

        //    var quantityOrdered = sameProductItems.Sum(w => w.QuantityOrdered);
        //    var valueOrdered = useValueOrdered ? sameProductItems.Sum(w => w.TotalBasePrice) : 0;

        //    var orderPriceComponents = new PriceComponents(this.Session()).CurrentPriceComponents(salesOrder.OrderDate);
        //    var orderItemPriceComponents = Array.Empty<PriceComponent>();
        //    if (this.ExistProduct)
        //    {
        //        orderItemPriceComponents = this.Product.GetPriceComponents(orderPriceComponents);
        //    }
        //    else if (this.ExistProductFeature)
        //    {
        //        orderItemPriceComponents = this.ProductFeature.GetPriceComponents(this.SalesOrderItemWhereOrderedWithFeature.Product, orderPriceComponents);
        //    }

        //    var priceComponents = orderItemPriceComponents.Where(
        //        v => PriceComponents.AppsIsApplicable(
        //            new PriceComponents.IsApplicable
        //            {
        //                PriceComponent = v,
        //                Customer = salesOrder.BillToCustomer,
        //                Product = this.Product,
        //                SalesOrder = salesOrder,
        //                QuantityOrdered = quantityOrdered,
        //                ValueOrdered = valueOrdered,
        //            })).ToArray();

        //    var unitBasePrice = priceComponents.OfType<BasePrice>().Min(v => v.Price);

        //    // Calculate Unit Price (with Discounts and Surcharges)
        //    if (this.AssignedUnitPrice.HasValue)
        //    {
        //        this.UnitBasePrice = unitBasePrice ?? this.AssignedUnitPrice.Value;
        //        this.UnitDiscount = 0;
        //        this.UnitSurcharge = 0;
        //        this.UnitPrice = this.AssignedUnitPrice.Value;
        //    }
        //    else
        //    {
        //        if (!unitBasePrice.HasValue)
        //        {
        //            derivation.Validation.AddError(this, M.SalesOrderItem.UnitBasePrice, "No BasePrice with a Price");
        //            return;
        //        }

        //        this.UnitBasePrice = unitBasePrice.Value;

        //        this.UnitDiscount = priceComponents.OfType<DiscountComponent>().Sum(
        //            v => v.Percentage.HasValue
        //                ? Math.Round(this.UnitBasePrice * v.Percentage.Value / 100, 2)
        //                : v.Price ?? 0);

        //        this.UnitSurcharge = priceComponents.OfType<SurchargeComponent>().Sum(
        //            v => v.Percentage.HasValue
        //                ? Math.Round(this.UnitBasePrice * v.Percentage.Value / 100, 2)
        //                : v.Price ?? 0);

        //        this.UnitPrice = this.UnitBasePrice - this.UnitDiscount + this.UnitSurcharge;

        //        foreach(OrderAdjustment orderAdjustment in this.DiscountAdjustments)
        //        {
        //            this.UnitDiscount += orderAdjustment.Percentage.HasValue
        //                ? Math.Round(this.UnitPrice * orderAdjustment.Percentage.Value / 100, 2)
        //                : orderAdjustment.Amount ?? 0;
        //        }

        //        foreach (OrderAdjustment orderAdjustment in this.SurchargeAdjustments)
        //        {
        //            this.UnitSurcharge += orderAdjustment.Percentage.HasValue
        //                ? Math.Round(this.UnitPrice * orderAdjustment.Percentage.Value / 100, 2)
        //                : orderAdjustment.Amount ?? 0;
        //        }

        //        this.UnitPrice = this.UnitBasePrice - this.UnitDiscount + this.UnitSurcharge;
        //    }

        //    foreach (SalesOrderItem featureItem in this.OrderedWithFeatures)
        //    {
        //        this.UnitBasePrice += featureItem.UnitBasePrice;
        //        this.UnitPrice += featureItem.UnitPrice;
        //        this.UnitDiscount += featureItem.UnitDiscount;
        //        this.UnitSurcharge += featureItem.UnitSurcharge;
        //    }

        //    this.UnitVat = this.ExistVatRate ? this.UnitPrice * this.VatRate.Rate / 100 : 0;
        //    this.UnitIrpf = this.ExistIrpfRate ? this.UnitPrice * this.IrpfRate.Rate / 100 : 0;

        //    // Calculate Totals
        //    this.TotalBasePrice = this.UnitBasePrice * this.QuantityOrdered;
        //    this.TotalDiscount = this.UnitDiscount * this.QuantityOrdered;
        //    this.TotalSurcharge = this.UnitSurcharge * this.QuantityOrdered;
        //    this.TotalOrderAdjustment = this.TotalSurcharge - this.TotalDiscount;

        //    if (this.TotalBasePrice > 0)
        //    {
        //        this.TotalDiscountAsPercentage = Math.Round(this.TotalDiscount / this.TotalBasePrice * 100, 2);
        //        this.TotalSurchargeAsPercentage = Math.Round(this.TotalSurcharge / this.TotalBasePrice * 100, 2);
        //    }
        //    else
        //    {
        //        this.TotalDiscountAsPercentage = 0;
        //        this.TotalSurchargeAsPercentage = 0;
        //    }

        //this.TotalExVat = this.UnitPrice* this.QuantityOrdered;
        //this.TotalVat = Math.Round(this.UnitVat * this.QuantityOrdered, 2);
        //this.TotalIncVat = this.TotalExVat + this.TotalVat;
        //this.TotalIrpf = Math.Round(this.UnitIrpf * this.QuantityOrdered, 2);
        //this.GrandTotal = this.TotalIncVat - this.TotalIrpf;
        //}
    }
}

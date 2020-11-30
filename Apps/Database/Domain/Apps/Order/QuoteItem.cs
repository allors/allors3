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
                this.QuoteItemState = new QuoteItemStates(this.Strategy.Session).Draft;
            }

            if (this.ExistProduct && !this.ExistInvoiceItemType)
            {
                this.InvoiceItemType = new InvoiceItemTypes(this.Strategy.Session).ProductItem;
            }
        }

        public void AppsOnPostDerive(ObjectOnPostDerive method)
        {
            var deletePermission = new Permissions(this.Strategy.Session).Get(this.Meta.ObjectType, this.Meta.Delete);
            if (this.QuoteWhereQuoteItem.IsDeletable())
            {
                this.RemoveDeniedPermission(deletePermission);
            }
            else
            {
                this.AddDeniedPermission(deletePermission);
            }
        }

        public void AppsDeriveDetails(QuoteItemDeriveDetails method)
        {
            if (!method.Result.HasValue)
            {
                if (this.ExistSerialisedItem)
                {
                    var builder = new StringBuilder();
                    var part = this.SerialisedItem.PartWhereSerialisedItem;

                    if (part != null && part.ExistManufacturedBy)
                    {
                        builder.Append($", Manufacturer: {part.ManufacturedBy.PartyName}");
                    }

                    if (part != null && part.ExistBrand)
                    {
                        builder.Append($", Brand: {part.Brand.Name}");
                    }

                    if (part != null && part.ExistModel)
                    {
                        builder.Append($", Model: {part.Model.Name}");
                    }

                    builder.Append($", SN: {this.SerialisedItem.SerialNumber}");

                    if (this.SerialisedItem.ExistManufacturingYear)
                    {
                        builder.Append($", YOM: {this.SerialisedItem.ManufacturingYear}");
                    }

                    foreach (SerialisedItemCharacteristic characteristic in this.SerialisedItem.SerialisedItemCharacteristics)
                    {
                        if (characteristic.ExistValue)
                        {
                            var characteristicType = characteristic.SerialisedItemCharacteristicType;
                            if (characteristicType.ExistUnitOfMeasure)
                            {
                                var uom = characteristicType.UnitOfMeasure.ExistAbbreviation
                                                ? characteristicType.UnitOfMeasure.Abbreviation
                                                : characteristicType.UnitOfMeasure.Name;
                                builder.Append(
                                    $", {characteristicType.Name}: {characteristic.Value} {uom}");
                            }
                            else
                            {
                                builder.Append($", {characteristicType.Name}: {characteristic.Value}");
                            }
                        }
                    }

                    var details = builder.ToString();

                    if (details.StartsWith(","))
                    {
                        details = details.Substring(2);
                    }

                    this.Details = details;

                }
                else if (this.ExistProduct && this.Product is UnifiedGood unifiedGood)
                {
                    var builder = new StringBuilder();

                    if (unifiedGood != null && unifiedGood.ExistManufacturedBy)
                    {
                        builder.Append($", Manufacturer: {unifiedGood.ManufacturedBy.PartyName}");
                    }

                    if (unifiedGood != null && unifiedGood.ExistBrand)
                    {
                        builder.Append($", Brand: {unifiedGood.Brand.Name}");
                    }

                    if (unifiedGood != null && unifiedGood.ExistModel)
                    {
                        builder.Append($", Model: {unifiedGood.Model.Name}");
                    }

                    foreach (SerialisedItemCharacteristic characteristic in unifiedGood.SerialisedItemCharacteristics)
                    {
                        if (characteristic.ExistValue)
                        {
                            var characteristicType = characteristic.SerialisedItemCharacteristicType;
                            if (characteristicType.ExistUnitOfMeasure)
                            {
                                var uom = characteristicType.UnitOfMeasure.ExistAbbreviation
                                                ? characteristicType.UnitOfMeasure.Abbreviation
                                                : characteristicType.UnitOfMeasure.Name;
                                builder.Append($", {characteristicType.Name}: {characteristic.Value} {uom}");
                            }
                            else
                            {
                                builder.Append($", {characteristicType.Name}: {characteristic.Value}");
                            }
                        }
                    }

                    var details = builder.ToString();

                    if (details.StartsWith(","))
                    {
                        details = details.Substring(2);
                    }

                    this.Details = details;
                }

                method.Result = true;
            }
        }

        public void AppsDelete(QuoteItemDelete method)
        {
            if (this.ExistSerialisedItem)
            {
                this.SerialisedItem.DerivationTrigger = Guid.NewGuid();
            }
        }

        public void AppsSend(QuoteItemSend method) => this.QuoteItemState = new QuoteItemStates(this.Strategy.Session).AwaitingAcceptance;

        public void AppsCancel(QuoteItemCancel method) => this.QuoteItemState = new QuoteItemStates(this.Strategy.Session).Cancelled;

        public void AppsReject(QuoteItemReject method) => this.QuoteItemState = new QuoteItemStates(this.Strategy.Session).Rejected;

        public void AppsOrder(QuoteItemOrder method) => this.QuoteItemState = new QuoteItemStates(this.Strategy.Session).Ordered;

        public void AppsSubmit(QuoteItemSubmit method) => this.QuoteItemState = new QuoteItemStates(this.Strategy.Session).Submitted;

        public void Sync(Quote quote) => this.SyncedQuote = quote;
    }
}

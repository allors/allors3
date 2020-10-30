// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public class SalesInvoiceItemDerivation : DomainDerivation
    {
        public SalesInvoiceItemDerivation(M m) : base(m, new Guid("37C0910B-7C48-46B5-8F7A-F6B2E70BE05C")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.SalesInvoiceItem.Class),
                new ChangedPattern(m.SalesInvoice.SalesInvoiceState) { Steps =  new IPropertyType[] {this.M.SalesInvoice.SalesInvoiceItems} },
                new ChangedPattern(m.PaymentApplication.AmountApplied) { Steps =  new IPropertyType[] {this.M.PaymentApplication.InvoiceItem} },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                var salesInvoice = @this.SalesInvoiceWhereSalesInvoiceItem;
                var salesInvoiceItemStates = new SalesInvoiceItemStates(session);

                validation.AssertExistsAtMostOne(@this, this.M.SalesInvoiceItem.Product, this.M.SalesInvoiceItem.ProductFeatures, this.M.SalesInvoiceItem.Part);
                validation.AssertExistsAtMostOne(@this, this.M.SalesInvoiceItem.SerialisedItem, this.M.SalesInvoiceItem.ProductFeatures, this.M.SalesInvoiceItem.Part);

                if (!@this.ExistDerivationTrigger)
                {
                    @this.DerivationTrigger = Guid.NewGuid();
                }

                if (@this.ExistSerialisedItem && !@this.ExistNextSerialisedItemAvailability && salesInvoice.SalesInvoiceType.Equals(new SalesInvoiceTypes(@this.Session()).SalesInvoice))
                {
                    validation.AssertExists(@this, @this.Meta.NextSerialisedItemAvailability);
                }

                if (@this.Part != null && @this.Part.InventoryItemKind.IsSerialised && @this.Quantity != 1)
                {
                    validation.AddError($"{@this}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }

                if (@this.Part != null && @this.Part.InventoryItemKind.IsNonSerialised && @this.Quantity == 0)
                {
                    validation.AddError($"{@this}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }

                if (@this.ExistInvoiceItemType && @this.InvoiceItemType.MaxQuantity.HasValue && @this.Quantity > @this.InvoiceItemType.MaxQuantity.Value)
                {
                    validation.AddError($"{@this}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }

                @this.VatRegime = @this.ExistAssignedVatRegime ? @this.AssignedVatRegime : @this.SalesInvoiceWhereSalesInvoiceItem?.VatRegime;
                @this.VatRate = @this.VatRegime?.VatRate;

                @this.IrpfRegime = @this.ExistAssignedIrpfRegime ? @this.AssignedIrpfRegime : @this.SalesInvoiceWhereSalesInvoiceItem?.IrpfRegime;
                @this.IrpfRate = @this.IrpfRegime?.IrpfRate;

                if (@this.ExistInvoiceItemType && @this.IsSubTotalItem().Result == true && @this.Quantity <= 0)
                {
                    validation.AssertExists(@this, @this.Meta.Quantity);
                }

                @this.AmountPaid = 0;
                foreach (PaymentApplication paymentApplication in @this.PaymentApplicationsWhereInvoiceItem)
                {
                    @this.AmountPaid += paymentApplication.AmountApplied;
                }

                if (salesInvoice != null
                    && salesInvoice.ExistSalesInvoiceState
                    && salesInvoice.SalesInvoiceState.IsReadyForPosting
                    && @this.SalesInvoiceItemState.IsCancelledByInvoice)
                {
                    @this.SalesInvoiceItemState = salesInvoiceItemStates.ReadyForPosting;
                }

                // SalesInvoiceItem States
                if (salesInvoice != null
                    && salesInvoice.ExistSalesInvoiceState
                    && @this.IsValid)
                {
                    if (salesInvoice.SalesInvoiceState.IsWrittenOff)
                    {
                        @this.SalesInvoiceItemState = salesInvoiceItemStates.WrittenOff;
                    }

                    if (salesInvoice.SalesInvoiceState.IsCancelled)
                    {
                        @this.SalesInvoiceItemState = salesInvoiceItemStates.CancelledByInvoice;
                    }
                }

                // TODO: Move to Custom
                if (changeSet.IsCreated(@this) && !@this.ExistDescription)
                {
                    if (@this.ExistSerialisedItem)
                    {
                        var builder = new StringBuilder();
                        var part = @this.SerialisedItem.PartWhereSerialisedItem;

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

                        builder.Append($", SN: {@this.SerialisedItem.SerialNumber}");

                        if (@this.SerialisedItem.ExistManufacturingYear)
                        {
                            builder.Append($", YOM: {@this.SerialisedItem.ManufacturingYear}");
                        }

                        foreach (SerialisedItemCharacteristic characteristic in @this.SerialisedItem.SerialisedItemCharacteristics)
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

                        @this.Description = details;

                    }
                    else if (@this.ExistProduct && @this.Product is UnifiedGood unifiedGood)
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

                        @this.Description = details;
                    }
                }
                var deletePermission = new Permissions(@this.Strategy.Session).Get(@this.Meta.ObjectType, @this.Meta.Delete);
                if (@this.IsDeletable)
                {
                    @this.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    @this.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}

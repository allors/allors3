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
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.SalesInvoiceItem.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var session = cycle.Session;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var SalesInvoiceItem in matches.Cast<SalesInvoiceItem>())
            {
                var salesInvoice = SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem;
                var salesInvoiceItemStates = new SalesInvoiceItemStates(session);

                validation.AssertExistsAtMostOne(SalesInvoiceItem, this.M.SalesInvoiceItem.Product, this.M.SalesInvoiceItem.ProductFeatures, this.M.SalesInvoiceItem.Part);
                validation.AssertExistsAtMostOne(SalesInvoiceItem, this.M.SalesInvoiceItem.SerialisedItem, this.M.SalesInvoiceItem.ProductFeatures, this.M.SalesInvoiceItem.Part);

                if (!SalesInvoiceItem.ExistDerivationTrigger)
                {
                    SalesInvoiceItem.DerivationTrigger = Guid.NewGuid();
                }

                if (SalesInvoiceItem.ExistSerialisedItem && !SalesInvoiceItem.ExistNextSerialisedItemAvailability && salesInvoice.SalesInvoiceType.Equals(new SalesInvoiceTypes(SalesInvoiceItem.Session()).SalesInvoice))
                {
                    validation.AssertExists(SalesInvoiceItem, SalesInvoiceItem.Meta.NextSerialisedItemAvailability);
                }

                if (SalesInvoiceItem.Part != null && SalesInvoiceItem.Part.InventoryItemKind.IsSerialised && SalesInvoiceItem.Quantity != 1)
                {
                    validation.AddError($"{SalesInvoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }

                if (SalesInvoiceItem.Part != null && SalesInvoiceItem.Part.InventoryItemKind.IsNonSerialised && SalesInvoiceItem.Quantity == 0)
                {
                    validation.AddError($"{SalesInvoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }

                if (SalesInvoiceItem.ExistInvoiceItemType && SalesInvoiceItem.InvoiceItemType.MaxQuantity.HasValue && SalesInvoiceItem.Quantity > SalesInvoiceItem.InvoiceItemType.MaxQuantity.Value)
                {
                    validation.AddError($"{SalesInvoiceItem}, {this.M.SalesInvoiceItem.Quantity},{ ErrorMessages.InvalidQuantity}");
                }

                SalesInvoiceItem.VatRegime = SalesInvoiceItem.ExistAssignedVatRegime ? SalesInvoiceItem.AssignedVatRegime : SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem?.VatRegime;
                SalesInvoiceItem.VatRate = SalesInvoiceItem.VatRegime?.VatRate;

                SalesInvoiceItem.IrpfRegime = SalesInvoiceItem.ExistAssignedIrpfRegime ? SalesInvoiceItem.AssignedIrpfRegime : SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem?.IrpfRegime;
                SalesInvoiceItem.IrpfRate = SalesInvoiceItem.IrpfRegime?.IrpfRate;

                if (SalesInvoiceItem.ExistInvoiceItemType && SalesInvoiceItem.IsSubTotalItem().Result == true && SalesInvoiceItem.Quantity <= 0)
                {
                    validation.AssertExists(SalesInvoiceItem, SalesInvoiceItem.Meta.Quantity);
                }

                SalesInvoiceItem.AmountPaid = 0;
                foreach (PaymentApplication paymentApplication in SalesInvoiceItem.PaymentApplicationsWhereInvoiceItem)
                {
                    SalesInvoiceItem.AmountPaid += paymentApplication.AmountApplied;
                }

                if (salesInvoice != null
                    && salesInvoice.ExistSalesInvoiceState
                    && salesInvoice.SalesInvoiceState.IsReadyForPosting
                    && SalesInvoiceItem.SalesInvoiceItemState.IsCancelledByInvoice)
                {
                    SalesInvoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.ReadyForPosting;
                }

                // SalesInvoiceItem States
                if (salesInvoice != null
                    && salesInvoice.ExistSalesInvoiceState
                    && SalesInvoiceItem.IsValid)
                {
                    if (salesInvoice.SalesInvoiceState.IsWrittenOff)
                    {
                        SalesInvoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.WrittenOff;
                    }

                    if (salesInvoice.SalesInvoiceState.IsCancelled)
                    {
                        SalesInvoiceItem.SalesInvoiceItemState = salesInvoiceItemStates.CancelledByInvoice;
                    }
                }

                // TODO: Move to Custom
                if (changeSet.IsCreated(SalesInvoiceItem) && !SalesInvoiceItem.ExistDescription)
                {
                    if (SalesInvoiceItem.ExistSerialisedItem)
                    {
                        var builder = new StringBuilder();
                        var part = SalesInvoiceItem.SerialisedItem.PartWhereSerialisedItem;

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

                        builder.Append($", SN: {SalesInvoiceItem.SerialisedItem.SerialNumber}");

                        if (SalesInvoiceItem.SerialisedItem.ExistManufacturingYear)
                        {
                            builder.Append($", YOM: {SalesInvoiceItem.SerialisedItem.ManufacturingYear}");
                        }

                        foreach (SerialisedItemCharacteristic characteristic in SalesInvoiceItem.SerialisedItem.SerialisedItemCharacteristics)
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

                        SalesInvoiceItem.Description = details;

                    }
                    else if (SalesInvoiceItem.ExistProduct && SalesInvoiceItem.Product is UnifiedGood unifiedGood)
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

                        SalesInvoiceItem.Description = details;
                    }
                }
                var deletePermission = new Permissions(SalesInvoiceItem.Strategy.Session).Get(SalesInvoiceItem.Meta.ObjectType, SalesInvoiceItem.Meta.Delete);
                if (SalesInvoiceItem.IsDeletable)
                {
                    SalesInvoiceItem.RemoveDeniedPermission(deletePermission);
                }
                else
                {
                    SalesInvoiceItem.AddDeniedPermission(deletePermission);
                }
            }
        }
    }
}

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

    public class SerialisedItemDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("A871B4BB-3285-418F-9E10-5A786A6284DA");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.SerialisedItem.Class),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var serialisedItem in matches.Cast<SerialisedItem>())
            {
                validation.AssertExistsAtMostOne(serialisedItem, serialisedItem.Meta.AcquiredDate, serialisedItem.Meta.AcquisitionYear);

                if (!serialisedItem.ExistName && serialisedItem.ExistPartWhereSerialisedItem)
                {
                    serialisedItem.Name = serialisedItem.PartWhereSerialisedItem.Name;
                }

                serialisedItem.DerivePurchaseOrder();
                serialisedItem.DerivePurchaseInvoice();
                serialisedItem.DerivePurchasePrice();

                serialisedItem.SuppliedBy = serialisedItem.AssignedSuppliedBy ??
                    serialisedItem.PurchaseOrder?.TakenViaSupplier ??
                    serialisedItem.PartWhereSerialisedItem?.SupplierOfferingsWherePart?.FirstOrDefault()?.Supplier;

                serialisedItem.SuppliedByPartyName = serialisedItem.ExistSuppliedBy ? serialisedItem.SuppliedBy.PartyName : string.Empty;
                serialisedItem.OwnedByPartyName = serialisedItem.ExistOwnedBy ? serialisedItem.OwnedBy.PartyName : string.Empty;
                serialisedItem.RentedByPartyName = serialisedItem.ExistRentedBy ? serialisedItem.RentedBy.PartyName : string.Empty;
                serialisedItem.OwnershipByOwnershipName = serialisedItem.ExistOwnership ? serialisedItem.Ownership.Name : string.Empty;
                serialisedItem.SerialisedItemAvailabilityName = serialisedItem.ExistSerialisedItemAvailability ? serialisedItem.SerialisedItemAvailability.Name : string.Empty;

                var doubles = serialisedItem.PartWhereSerialisedItem?.SerialisedItems.Where(v =>
                    !string.IsNullOrEmpty(v.SerialNumber)
                    && v.SerialNumber != "."
                    && v.SerialNumber.Equals(serialisedItem.SerialNumber)).ToArray();
                if (doubles?.Length > 1)
                {
                    validation.AddError($"{serialisedItem} {serialisedItem.Meta.SerialNumber} {ErrorMessages.SameSerialNumber}");
                }

                serialisedItem.OnQuote = serialisedItem.QuoteItemsWhereSerialisedItem.Any(v => v.QuoteItemState.IsDraft
                            || v.QuoteItemState.IsSubmitted || v.QuoteItemState.IsApproved
                            || v.QuoteItemState.IsAwaitingAcceptance || v.QuoteItemState.IsAccepted);

                serialisedItem.OnSalesOrder = serialisedItem.SalesOrderItemsWhereSerialisedItem.Any(v => v.SalesOrderItemState.IsProvisional
                            || v.SalesOrderItemState.IsReadyForPosting || v.SalesOrderItemState.IsRequestsApproval
                            || v.SalesOrderItemState.IsAwaitingAcceptance || v.SalesOrderItemState.IsOnHold || v.SalesOrderItemState.IsInProcess);

                serialisedItem.OnWorkEffort = serialisedItem.WorkEffortFixedAssetAssignmentsWhereFixedAsset.Any(v => v.Assignment.WorkEffortState.IsCreated
                            || v.Assignment.WorkEffortState.IsInProgress);

                this.DeriveProductCharacteristics(serialisedItem);
                
                serialisedItem.DeriveDisplayProductCategories();
            }
        }

        void DeriveProductCharacteristics(SerialisedItem serialisedItem)
        {
            var characteristicsToDelete = serialisedItem.SerialisedItemCharacteristics.ToList();
            var part = serialisedItem.PartWhereSerialisedItem;

            if (serialisedItem.ExistPartWhereSerialisedItem && part.ExistProductType)
            {
                foreach (SerialisedItemCharacteristicType characteristicType in part.ProductType.SerialisedItemCharacteristicTypes)
                {
                    var characteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => Equals(v.SerialisedItemCharacteristicType, characteristicType));
                    if (characteristic == null)
                    {
                        var newCharacteristic = new SerialisedItemCharacteristicBuilder(serialisedItem.Strategy.Session)
                            .WithSerialisedItemCharacteristicType(characteristicType).Build();

                        serialisedItem.AddSerialisedItemCharacteristic(newCharacteristic);

                        var partCharacteristics = part.SerialisedItemCharacteristics;
                        partCharacteristics.Filter.AddEquals(M.SerialisedItemCharacteristic.SerialisedItemCharacteristicType, characteristicType);
                        var fromPart = partCharacteristics.FirstOrDefault();

                        if (fromPart != null)
                        {
                            newCharacteristic.Value = fromPart.Value;
                        }
                    }
                    else
                    {
                        characteristicsToDelete.Remove(characteristic);
                    }
                }
            }

            foreach (var characteristic in characteristicsToDelete)
            {
                serialisedItem.RemoveSerialisedItemCharacteristic(characteristic);
            }

            var deletePermission = new Permissions(serialisedItem.Strategy.Session).Get(serialisedItem.Meta.ObjectType, serialisedItem.Meta.Delete, Operations.Execute);
            if (IsDeletable(serialisedItem))
            {
                serialisedItem.RemoveDeniedPermission(deletePermission);
            }
            else
            {
                serialisedItem.AddDeniedPermission(deletePermission);
            }

            var builder = new StringBuilder();

            builder.Append(serialisedItem.ItemNumber);
            builder.Append(string.Join(" ", serialisedItem.SerialNumber));
            builder.Append(string.Join(" ", serialisedItem.Name));

            if (serialisedItem.ExistOwnedBy)
            {
                builder.Append(string.Join(" ", serialisedItem.OwnedBy.PartyName));
            }

            if (serialisedItem.ExistBuyer)
            {
                builder.Append(string.Join(" ", serialisedItem.Buyer.PartyName));
            }

            if (serialisedItem.ExistSeller)
            {
                builder.Append(string.Join(" ", serialisedItem.Seller.PartyName));
            }

            if (serialisedItem.ExistPartWhereSerialisedItem)
            {
                builder.Append(string.Join(" ", serialisedItem.PartWhereSerialisedItem?.Brand?.Name));
                builder.Append(string.Join(" ", serialisedItem.PartWhereSerialisedItem?.Model?.Name));
            }

            builder.Append(string.Join(" ", serialisedItem.Keywords));

            serialisedItem.SearchString = builder.ToString();
        }

        bool IsDeletable(SerialisedItem serialised) =>
            !serialised.ExistInventoryItemTransactionsWhereSerialisedItem
            && !serialised.ExistPurchaseInvoiceItemsWhereSerialisedItem
            && !serialised.ExistPurchaseOrderItemsWhereSerialisedItem
            && !serialised.ExistQuoteItemsWhereSerialisedItem
            && !serialised.ExistSalesInvoiceItemsWhereSerialisedItem
            && !serialised.ExistSalesOrderItemsWhereSerialisedItem
            && !serialised.ExistSerialisedInventoryItemsWhereSerialisedItem
            && !serialised.ExistShipmentItemsWhereSerialisedItem;
    }
}

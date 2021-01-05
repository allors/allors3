// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SerialisedItemDerivation : DomainDerivation
    {
        public SerialisedItemDerivation(M m) : base(m, new Guid("A871B4BB-3285-418F-9E10-5A786A6284DA")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.SerialisedItem.AcquiredDate),
                new ChangedPattern(this.M.SerialisedItem.AssignedSuppliedBy),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                validation.AssertExistsAtMostOne(@this, @this.Meta.AcquiredDate, @this.Meta.AcquisitionYear);

                if (!@this.ExistName && @this.ExistPartWhereSerialisedItem)
                {
                    @this.Name = @this.PartWhereSerialisedItem.Name;
                }

                @this.SuppliedBy = @this.AssignedSuppliedBy ??
                    @this.PurchaseOrder?.TakenViaSupplier ??
                    @this.PartWhereSerialisedItem?.SupplierOfferingsWherePart?.FirstOrDefault()?.Supplier;

                @this.SuppliedByPartyName = @this.ExistSuppliedBy ? @this.SuppliedBy.PartyName : string.Empty;
                @this.OwnedByPartyName = @this.ExistOwnedBy ? @this.OwnedBy.PartyName : string.Empty;
                @this.RentedByPartyName = @this.ExistRentedBy ? @this.RentedBy.PartyName : string.Empty;
                @this.OwnershipByOwnershipName = @this.ExistOwnership ? @this.Ownership.Name : string.Empty;
                @this.SerialisedItemAvailabilityName = @this.ExistSerialisedItemAvailability ? @this.SerialisedItemAvailability.Name : string.Empty;

                var doubles = @this.PartWhereSerialisedItem?.SerialisedItems.Where(v =>
                    !string.IsNullOrEmpty(v.SerialNumber)
                    && v.SerialNumber != "."
                    && v.SerialNumber.Equals(@this.SerialNumber)).ToArray();
                if (doubles?.Length > 1)
                {
                    validation.AddError($"{@this} {@this.Meta.SerialNumber} {ErrorMessages.SameSerialNumber}");
                }

                @this.OnQuote = @this.QuoteItemsWhereSerialisedItem.Any(v => v.QuoteItemState.IsDraft
                            || v.QuoteItemState.IsSubmitted || v.QuoteItemState.IsApproved
                            || v.QuoteItemState.IsAwaitingAcceptance || v.QuoteItemState.IsAccepted);

                @this.OnSalesOrder = @this.SalesOrderItemsWhereSerialisedItem.Any(v => v.SalesOrderItemState.IsProvisional
                            || v.SalesOrderItemState.IsReadyForPosting || v.SalesOrderItemState.IsRequestsApproval
                            || v.SalesOrderItemState.IsAwaitingAcceptance || v.SalesOrderItemState.IsOnHold || v.SalesOrderItemState.IsInProcess);

                @this.OnWorkEffort = @this.WorkEffortFixedAssetAssignmentsWhereFixedAsset.Any(v => v.Assignment.WorkEffortState.IsCreated
                            || v.Assignment.WorkEffortState.IsInProgress);

                this.DeriveProductCharacteristics(@this);

                @this.DeriveDisplayProductCategories();
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
                        partCharacteristics.Filter.AddEquals(this.M.SerialisedItemCharacteristic.SerialisedItemCharacteristicType, characteristicType);
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
    }
}

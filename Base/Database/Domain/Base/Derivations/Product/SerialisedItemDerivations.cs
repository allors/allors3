// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class SerialisedItemCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdSerialisedItem = changeSet.Created.Select(v=>v.GetObject()).OfType<SerialisedItem>();

                foreach(var serialisedItem in createdSerialisedItem)
                {
                    validation.AssertExistsAtMostOne(serialisedItem, serialisedItem.Meta.AcquiredDate, serialisedItem.Meta.AcquisitionYear);

                    if (!serialisedItem.ExistName && serialisedItem.ExistPartWhereSerialisedItem)
                    {
                        serialisedItem.Name = serialisedItem.PartWhereSerialisedItem.Name;
                    }

                    serialisedItem.PurchasePrice = serialisedItem.AssignedPurchasePrice ?? serialisedItem.PurchaseOrderItemsWhereSerialisedItem.LastOrDefault()?.UnitPrice ?? 0M;
                    serialisedItem.SuppliedBy = serialisedItem.AssignedSuppliedBy ?? serialisedItem.PartWhereSerialisedItem?.SupplierOfferingsWherePart?.FirstOrDefault()?.Supplier;

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

                    DeriveProductCharacteristics(serialisedItem);
                    serialisedItem.DeriveDisplayProductCategories();
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
                }
            }
        }

        public static void SerialisedItemRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("10c0c93b-8dd2-49ec-a5a6-dc5de8e8cf88")] = new SerialisedItemCreationDerivation();
        }
    }
}

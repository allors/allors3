// <copyright file="ShipmentItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class ShipmentItem
    {
        // TODO: Cache
        public TransitionalConfiguration[] TransitionalConfigurations => new[] {
            new TransitionalConfiguration(this.M.ShipmentItem, this.M.ShipmentItem.ShipmentItemState),
        };

        public void AppsDelegateAccess(DelegatedAccessControlledObjectDelegateAccess method)
        {
            if (method.SecurityTokens == null)
            {
                method.SecurityTokens = this.SyncedShipment?.SecurityTokens.ToArray();
            }

            if (method.DeniedPermissions == null)
            {
                method.DeniedPermissions = this.SyncedShipment?.DeniedPermissions.ToArray();
            }
        }

        public void AppsDelete(DeletableDelete method)
        {
            if (this.ExistItemIssuancesWhereShipmentItem)
            {
                foreach (ItemIssuance itemIssuance in this.ItemIssuancesWhereShipmentItem)
                {
                    itemIssuance.Delete();
                }
            }
        }

        public void AppsOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistShipmentItemState)
            {
                this.ShipmentItemState = new ShipmentItemStates(this.Strategy.Session).Created;
            }
        }

        public void AppsOnPreDerive(ObjectOnPreDerive method)
        {
            //var (iteration, changeSet, derivedObjects) = method;

            //if (iteration.IsMarked(this) || changeSet.IsCreated(this) || changeSet.HasChangedRoles(this))
            //{
            //    iteration.AddDependency(this.ShipmentWhereShipmentItem, this);
            //    iteration.Mark(this.ShipmentWhereShipmentItem, this);
            //}
        }

        public void AppsOnDerive(ObjectOnDerive method)
        {
            //var derivation = method.Derivation;

            //if (this.ExistSerialisedItem && !this.ExistNextSerialisedItemAvailability)
            //{
            //    derivation.Validation.AssertExists(this, this.Meta.NextSerialisedItemAvailability);
            //}

            //if (this.ExistSerialisedItem && this.Quantity != 1)
            //{
            //    derivation.Validation.AddError(this, this.Meta.Quantity, ErrorMessages.SerializedItemQuantity);
            //}

            //this.AppsOnDeriveCustomerShipmentItem(derivation);

            //this.AppsOnDerivePurchaseShipmentItem(derivation);
        }

        //public void AppsOnDerivePurchaseShipmentItem(IDerivation derivation)
        //{
        //    if (this.ExistShipmentWhereShipmentItem
        //        && this.ShipmentWhereShipmentItem is PurchaseShipment
        //        && this.ExistPart
        //        && this.Part.InventoryItemKind.IsNonSerialised
        //        && !this.ExistUnitPurchasePrice)
        //    {
        //        derivation.Validation.AssertExists(this, this.Meta.UnitPurchasePrice);
        //    }

        //    if (this.ExistShipmentWhereShipmentItem
        //        && this.ShipmentWhereShipmentItem is PurchaseShipment
        //        && !this.ExistStoredInFacility
        //        && this.ShipmentWhereShipmentItem.ExistShipToFacility)
        //    {
        //        this.StoredInFacility = this.ShipmentWhereShipmentItem.ShipToFacility;
        //    }

        //    if (this.ExistShipmentWhereShipmentItem
        //        && this.ShipmentWhereShipmentItem is PurchaseShipment
        //        && this.ExistShipmentReceiptWhereShipmentItem)
        //    {
        //        this.Quantity = 0;
        //        var shipmentReceipt = this.ShipmentReceiptWhereShipmentItem;
        //        this.Quantity += shipmentReceipt.QuantityAccepted + shipmentReceipt.QuantityRejected;
        //    }
        //}

        //public void AppsOnDeriveCustomerShipmentItem(IDerivation derivation)
        //{
        //    if (this.ShipmentWhereShipmentItem is CustomerShipment)
        //    {
        //        this.QuantityPicked = 0;
        //        foreach (ItemIssuance itemIssuance in this.ItemIssuancesWhereShipmentItem)
        //        {
        //            if (itemIssuance.PickListItem.PickListWherePickListItem.PickListState.Equals(new PickListStates(this.Strategy.Session).Picked))
        //            {
        //                this.QuantityPicked += itemIssuance.Quantity;
        //            }
        //        }

        //        if (Equals(this.ShipmentWhereShipmentItem.ShipmentState, new ShipmentStates(this.Strategy.Session).Shipped))
        //        {
        //            this.QuantityShipped = 0;
        //            foreach (ItemIssuance itemIssuance in this.ItemIssuancesWhereShipmentItem)
        //            {
        //                this.QuantityShipped += itemIssuance.Quantity;
        //            }
        //        }
        //    }
        //}

        public void Sync(Shipment shipment) => this.SyncedShipment = shipment;
    }
}

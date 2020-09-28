// <copyright file="ShipmentItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;

    #region Allors
    [Id("d35c33c3-ca15-4b70-b20d-c51ed068626a")]
    #endregion
    public partial class ShipmentItem : Transitional, Deletable, DelegatedAccessControlledObject
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public ObjectState[] PreviousObjectStates { get; set; }

        public ObjectState[] LastObjectStates { get; set; }

        public ObjectState[] ObjectStates { get; set; }

        #endregion

        #region ShipmentItemState
        #region Allors
        [Id("016778E4-B815-47B8-AC7D-1C950B3A11FB")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public ShipmentItemState PreviousShipmentItemState { get; set; }

        #region Allors
        [Id("C14771CD-0DF1-4FBF-BCF0-B10485311C61")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        public ShipmentItemState LastShipmentItemState { get; set; }

        #region Allors
        [Id("D1C46A96-F960-4C1D-A925-34FCCE175B40")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public ShipmentItemState ShipmentItemState { get; set; }
        #endregion

        #region Allors
        [Id("082e7e0d-190c-463f-89c8-af8e2c57c68d")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("91C23443-1666-4123-ABEE-53913D26FA3C")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Derived]
        [Workspace]
        public decimal QuantityShipped { get; set; }

        #region Allors
        [Id("A239DCBA-121D-4446-9193-517EBFC7B60F")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Derived]
        [Workspace]
        public decimal QuantityPicked { get; set; }

        #region Allors
        [Id("158f104b-fa5c-425e-8b55-ee4e866820ec")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Part Part { get; set; }

        #region Allors
        [Id("19c691ae-f849-451e-ac7e-ea84f4a9b51a")]
        #endregion
        [Size(-1)]
        [Workspace]
        public string ContentsDescription { get; set; }

        #region Allors
        [Id("6b3ab563-a19b-4d92-be3a-ddf3046d5b18")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        public Document[] Documents { get; set; }

        #region Allors
        [Id("b5d35e87-f741-4600-9838-4419b127681d")]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Indexed]
        [Workspace]
        public ShipmentItem[] InResponseToShipmentItems { get; set; }

        #region Allors
        [Id("4B2F0349-ECCE-4D24-B8A7-BB917B86F0F8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("b8ca6fae-0866-4806-9ffd-64d5d2b978f9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public InventoryItem[] ReservedFromInventoryItems { get; set; }

        #region Allors
        [Id("669a3209-1fc9-45ad-8331-7602f5c16ce0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Facility StoredInFacility { get; set; }

        #region Allors
        [Id("b9bfaea8-e5f0-4b0e-955f-df28ed63e8e3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace]
        public ProductFeature[] ProductFeatures { get; set; }

        #region Allors
        [Id("f5c0279e-5ce4-4f09-bb93-ecaeb4825bcf")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public Good Good { get; set; }

        #region Allors
        [Id("B5E17B64-5C6D-4318-8E45-3E34D8DAE731")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace]
        public SerialisedItemAvailability NextSerialisedItemAvailability { get; set; }

        #region Allors
        [Id("312FC32F-3EB9-492A-AC69-AEC608A48AF4")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Synced]
        [Workspace]
        public Shipment SyncedShipment { get; set; }

        /// <summary>
        /// Gets or Sets the Purchase price in case PurchaseShipment is not coming from PurchaseOrder.
        /// We need a price for calculating the weighted average cost for inventory.
        /// </summary>
        #region Allors
        [Id("d6778107-7d74-4217-b7b3-a1cd78c1e764")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal UnitPurchasePrice { get; set; }

        #region Allors
        [Id("f02f4cb4-1050-4304-9f9b-182a123d3dd2")]
        #endregion
        [Required]
        [Workspace]
        public Guid DerivationTrigger { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        public void DelegateAccess() { }

        #endregion
    }
}

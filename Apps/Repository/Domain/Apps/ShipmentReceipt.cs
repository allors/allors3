// <copyright file="ShipmentReceipt.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("48d14522-5fa8-44a8-ba4c-e2ddfc18e069")]
    #endregion
    public partial class ShipmentReceipt : Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("0c4eee66-ff66-49fa-9a06-4ce3848a6d3c")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string ItemDescription { get; set; }

        #region Allors
        [Id("2bbc4476-7a06-4c36-9985-68a60b72eacd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public InventoryItem InventoryItem { get; set; }

        // Set this when invent does not exist. On deriving it will create an InventoryItemTransaction for this Facility.
        // The InventoryItemTransaction will then create the InventoryItem
        #region Allors
        [Id("a74aced0-2c7b-4947-b5a8-dace618e5286")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Facility Facility { get; set; }

        #region Allors
        [Id("87f84720-1233-4779-be9d-4b0a12ba19cd")]
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string RejectionReason { get; set; }

        #region Allors
        [Id("9a76f8ba-ae96-4040-81ce-59330392e77a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public OrderItem OrderItem { get; set; }

        #region Allors
        [Id("9a9cce59-f45c-4da0-adb6-9583a1694921")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityRejected { get; set; }

        #region Allors
        [Id("ccd41d3d-2be8-47ca-8217-4e2aa1d1c03b")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public ShipmentItem ShipmentItem { get; set; }

        #region Allors
        [Id("ecdd6b27-3bcf-4f61-8e21-f829503aeeb0")]
        #endregion
        [Required]
        [Workspace(Default)]
        public DateTime ReceivedDateTime { get; set; }

        #region Allors
        [Id("f057b89e-3688-4172-9efa-102298c7e0e4")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal QuantityAccepted { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        #endregion

    }
}

// <copyright file="ItemIssuance.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("441f6007-022d-4d77-bc2d-04c7a876e1bd")]
    #endregion
    public partial class ItemIssuance : Deletable, Object
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("60089b34-e9aa-4b09-9a5c-4523ce60152f")]
        #endregion
        public DateTime IssuanceDateTime { get; set; }

        #region Allors
        [Id("83de0bfa-98ca-4299-a529-f8ba8a02cb90")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public ShipmentItem ShipmentItem { get; set; }

        #region Allors
        [Id("6d0e1669-1583-4004-a0dd-6481faaa4803")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public InventoryItem InventoryItem { get; set; }

        #region Allors
        [Id("af4fbe17-bbdc-4f05-bf2e-398ee18598a5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public PickListItem PickListItem { get; set; }

        #region Allors
        [Id("72872b29-69e3-4408-ad61-80201c46421b")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("8DCB7F3A-C47C-4276-9107-2DB60609EE4A")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Workspace(Default)]
        public InventoryItemTransaction[] InventoryItemTransactions { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}

// <copyright file="PickListItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("7fec090e-3d4a-4ec7-895f-4b30d01f59bb")]
    #endregion
    public partial class PickListItem : DelegatedAccessControlledObject, Deletable
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("8a8ad2c2-e301-40be-8c7e-3c8291c3bbe9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public InventoryItem InventoryItem { get; set; }

        #region Allors
        [Id("6e89daf6-f07f-4a7d-8032-cc3c08d443c2")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Quantity { get; set; }

        #region Allors
        [Id("f32d100b-a6e8-4cb2-98b4-c06264789c76")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Required]
        [Workspace(Default)]
        public decimal QuantityPicked { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        public void DelegateAccess() { }
        #endregion
    }
}

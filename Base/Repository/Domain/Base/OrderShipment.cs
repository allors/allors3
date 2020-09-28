// <copyright file="OrderShipment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("00be6409-1ca0-491e-b0a1-3d53e17005f6")]
    #endregion
    public partial class OrderShipment : Deletable
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("1494758b-f763-48e5-a5a9-cd5c83a8af95")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public OrderItem OrderItem { get; set; }

        #region Allors
        [Id("b55bbdb8-af05-4008-a6a7-b4eea78096bd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public ShipmentItem ShipmentItem { get; set; }

        #region Allors
        [Id("d4725e9c-b72c-4cdf-95f9-70f9c4b57b11")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Quantity { get; set; }

        #region inherited methods

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }
        #endregion
    }
}

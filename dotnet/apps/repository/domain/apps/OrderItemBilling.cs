// <copyright file="OrderItemBilling.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("1f14fdb3-9e0f-4cea-b7c7-3ca2ab898f56")]
    #endregion
    public partial class OrderItemBilling : Deletable, Object
    {
        #region inherited properties

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }
        #endregion

        #region Allors
        [Id("214988fc-b5a2-4944-9c83-93a645a96853")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public OrderItem OrderItem { get; set; }

        #region Allors
        [Id("23a0d52d-3ec7-4ddf-a300-c0ee46edf41a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public InvoiceItem InvoiceItem { get; set; }

        #region Allors
        [Id("2f75bdee-46f9-4dd0-b349-00a497462fdb")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Amount { get; set; }

        #region Allors
        [Id("cfff23f0-1f3c-48a1-b4a7-85bc2254dbff")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Quantity { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void Delete() { }

        public void OnPostDerive() { }

        #endregion
    }
}

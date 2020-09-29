// <copyright file="ShipmentItemBilling.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("F54CE592-6935-401C-B341-198FD2E7888D")]
    #endregion
    public partial class ShipmentItemBilling : Object, Deletable
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("46852AB7-2B72-433E-B029-404607A603CE")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public ShipmentItem ShipmentItem { get; set; }

        #region Allors
        [Id("331685BD-1903-419F-A964-DF7A1F725B69")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        public InvoiceItem InvoiceItem { get; set; }

        #region Allors
        [Id("A1FB2C5B-7F15-4C4F-A790-752552114C0E")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        public decimal Amount { get; set; }

        #region Allors
        [Id("016E7DBF-73FA-4F3E-96CD-677A34968CE6")]
        #endregion
        [Precision(19)]
        [Scale(2)]
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

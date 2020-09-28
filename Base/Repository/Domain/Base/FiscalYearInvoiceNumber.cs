// <copyright file="FiscalYearInvoiceNumber.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("341fa885-0161-406b-89e6-08b1c92cd3b3")]
    #endregion
    public partial class FiscalYearInvoiceNumber : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("14f064a8-461c-4726-93c4-91bc34c9c443")]
        #endregion
        [Derived]
        [Required]
        public int NextSalesInvoiceNumber { get; set; }

        #region Allors
        [Id("C349F8A9-82D8-406B-B026-AFBE67DCD375")]
        #endregion
        [Derived]
        [Required]
        public int NextCreditNoteNumber { get; set; }

        #region Allors
        [Id("c1b0dcb6-8627-4a47-86d0-2866344da3f1")]
        #endregion
        [Required]
        public int FiscalYear { get; set; }

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

        #endregion
    }
}

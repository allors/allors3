// <copyright file="WorkEffortBilling.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("15c8c72b-f551-41b0-86c8-80f02424ec4c")]
    #endregion
    public partial class WorkEffortBilling : Object, Deletable
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("3c83ca1d-b20e-4e8c-aa23-3bb03f421ba7")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public WorkEffort WorkEffort { get; set; }

        #region Allors
        [Id("91d38ce9-bf06-4272-bdd8-13401084223d")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal Percentage { get; set; }

        #region Allors
        [Id("c6ed6a42-6889-4ad9-b76a-22bd45e02e75")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public InvoiceItem InvoiceItem { get; set; }

        #region inherited methods

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

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

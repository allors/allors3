// <copyright file="InternalOrganisationInventorySettings.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("17b982f7-5184-4590-b15f-849ccd65695a")]
    #endregion
    [Plural("InternalOrganisationInventorySettingses")]
    public partial class InternalOrganisationInventorySettings : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("7957426a-582f-4160-b330-eb12d9d743da")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public InventoryTransactionReason InventoryTransactionReason { get; set; }

        #region Allors
        [Id("b3e0675e-0937-4f3c-bb5e-67f9e3aa6afd")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount GeneralLedgerAccount { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}

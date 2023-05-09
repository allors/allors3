// <copyright file="InternalOrganisationIrpfRegimeSettings.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("c8b6ee7b-18ab-4921-96a0-01dfa76b577c")]
    #endregion
    [Plural("InternalOrganisationIrpfRateSettingses")]
    public partial class InternalOrganisationIrpfRateSettings : Deletable
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("7a75a987-6442-4fd1-868f-18aad12208d9")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public IrpfRate IrpfRate { get; set; }

        #region Allors
        [Id("9fa1006d-720f-4b95-9059-fd04c119df32")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount IrpfPayableAccount { get; set; }

        #region Allors
        [Id("fe8ebc0c-8e9c-4c39-8495-319d03e130b1")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount IrpfReceivableAccount { get; set; }

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

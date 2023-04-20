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
    [Plural("InternalOrganisationIrpfRegimeSettingses")]
    public partial class InternalOrganisationIrpfRegimeSettings : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("9318df80-9760-4b7d-b304-b633211f19a3")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public IrpfRegime IrpfRegime { get; set; }

        #region Allors
        [Id("9fa1006d-720f-4b95-9059-fd04c119df32")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
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

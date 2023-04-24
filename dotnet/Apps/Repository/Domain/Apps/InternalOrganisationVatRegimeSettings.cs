// <copyright file="InternalOrganisationVatRegimeSettings.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("88a49ec6-3c06-432a-8894-b28fe76bb41f")]
    #endregion
    [Plural("InternalOrganisationVatRegimeSettingses")]
    public partial class InternalOrganisationVatRegimeSettings : Deletable
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("b189679d-553e-4430-9b72-91543dd195e6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public VatRegime VatRegime { get; set; }

        #region Allors
        [Id("eee70b9b-9b97-4404-8f5e-2a416bb9373b")]
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

        public void Delete() { }

        #endregion
    }
}

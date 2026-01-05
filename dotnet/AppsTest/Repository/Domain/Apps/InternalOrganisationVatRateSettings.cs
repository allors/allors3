// <copyright file="InternalOrganisationVatRegimeSettings.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("88a49ec6-3c06-432a-8894-b28fe76bb41f")]
    #endregion
    [Plural("InternalOrganisationVatRateSettingses")]
    public partial class InternalOrganisationVatRateSettings : Deletable
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("5f4fd544-fdb4-486c-9376-7b6803eb2619")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public VatRate VatRate { get; set; }

        #region Allors
        [Id("eee70b9b-9b97-4404-8f5e-2a416bb9373b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount VatPayableAccount { get; set; }

        #region Allors
        [Id("eb562cec-d1b4-45f9-9d79-7182a9936ebb")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount VatReceivableAccount { get; set; }

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

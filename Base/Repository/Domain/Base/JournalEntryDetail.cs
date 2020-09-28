// <copyright file="JournalEntryDetail.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("9ffd634a-27b9-46a5-bf77-4ae25a9b9ebf")]
    #endregion
    public partial class JournalEntryDetail : DelegatedAccessControlledObject
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("9e273a44-b68f-4379-b2cd-f6ac1d524c4a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public OrganisationGlAccount GeneralLedgerAccount { get; set; }

        #region Allors
        [Id("b51ddcf7-ae36-4fbc-b8b5-3b2befa4a720")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal Amount { get; set; }

        #region Allors
        [Id("bc59e72d-935c-46fd-a595-4de24369fc12")]
        #endregion

        public bool Debit { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void DelegateAccess() { }

        #endregion

    }
}

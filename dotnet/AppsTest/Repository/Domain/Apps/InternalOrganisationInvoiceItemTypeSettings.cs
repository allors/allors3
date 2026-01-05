// <copyright file="InternalOrganisationInvoiceSettings.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("9a45a01f-2899-4162-bc01-6a0ac6ce8903")]
    #endregion
    [Plural("InternalOrganisationInvoiceInvoiceItemTypeSettingses")]
    public partial class InternalOrganisationInvoiceItemTypeSettings : Object
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        #endregion

        #region Allors
        [Id("536aacbc-b5c4-42fa-9da7-ac36f6f3ec6c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public InvoiceItemType InvoiceItemType { get; set; }

        #region Allors
        [Id("58089202-3fda-4ef4-8fba-dcc79ab86aa2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount SalesGeneralLedgerAccount { get; set; }

        #region Allors
        [Id("5895baca-c71b-42f9-843a-a6a151fa3dda")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public GeneralLedgerAccount PurchaseGeneralLedgerAccount { get; set; }

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

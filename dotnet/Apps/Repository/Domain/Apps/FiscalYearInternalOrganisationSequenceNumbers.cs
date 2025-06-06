// <copyright file="FiscalYearOrganisationSequenceNumbers.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("0519ac3d-898d-4880-9019-7d47eb650a88")]
    #endregion
    [Plural("FiscalYearsInternalOrganisationSequenceNumbers")]

    public partial class FiscalYearInternalOrganisationSequenceNumbers : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("05b4f4a5-2e87-47e8-a665-47449c56cc05")]
        #endregion
        [Required]
        public int FiscalYear { get; set; }

        #region Allors
        [Id("26e27fd4-b5ce-45ac-9643-52e0537b6601")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string PurchaseInvoiceNumberPrefix { get; set; }

        #region Allors
        [Id("fdabc29e-ee35-44c0-a9bf-8d789d06ab46")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter PurchaseInvoiceNumberCounter { get; set; }

        #region Allors
        [Id("4a3909c6-3443-425b-89f9-18e3df46d7dd")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string PurchaseOrderNumberPrefix { get; set; }

        #region Allors
        [Id("d241e5f1-7dd9-442f-90c1-67707b92bcb6")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter PurchaseOrderNumberCounter { get; set; }

        #region Allors
        [Id("2afe1843-a604-427d-aaf0-486153fe7a96")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string RequestNumberPrefix { get; set; }

        #region Allors
        [Id("7fd7a805-651a-48e0-93d8-ce595dcc4411")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter RequestNumberCounter { get; set; }

        #region Allors
        [Id("91c177da-eb89-4d6f-8d51-d8e98e855faa")]
        #endregion
        [Workspace(Default)]
        [Size(256)]
        public string PurchaseShipmentNumberPrefix { get; set; }

        #region Allors
        [Id("96201883-7ccb-4fea-9958-de7ef27fabd0")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter PurchaseShipmentNumberCounter { get; set; }

        #region Allors
        [Id("158d714e-17db-4368-a405-b52f86b1cbf8")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string PurchaseReturnNumberPrefix { get; set; }

        #region Allors
        [Id("9ef91df0-0be0-4e58-9173-de0cf3011c4b")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        public Counter PurchaseReturnNumberCounter { get; set; }

        #region Allors
        [Id("9762a55f-08f0-45a5-93af-9998921994e7")]
        #endregion
        [Workspace(Default)]
        [Size(256)]
        public string CustomerReturnNumberPrefix { get; set; }

        #region Allors
        [Id("fad093f8-a46c-464a-800a-7d99b214e2c0")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter CustomerReturnNumberCounter { get; set; }

        #region Allors
        [Id("1486b1ff-ffa4-4bdc-914b-c7b4d86ca020")]
        #endregion
        [Workspace(Default)]
        [Size(256)]
        public string IncomingTransferNumberPrefix { get; set; }

        #region Allors
        [Id("b3713ae3-abf0-408f-a680-c2d1930f2cfe")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter IncomingTransferNumberCounter { get; set; }

        #region Allors
        [Id("c54b04d9-e38d-410b-9572-a039446fccd0")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string ProductQuoteNumberPrefix { get; set; }

        #region Allors
        [Id("5bbfbf49-59ab-49f2-b00f-799cb0568e35")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter ProductQuoteNumberCounter { get; set; }

        #region Allors
        [Id("85ac5b50-0eeb-404f-b459-6778bb892119")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string StatementOfWorkNumberPrefix { get; set; }

        #region Allors
        [Id("233f35a7-e35d-486e-b211-17a72148d0ca")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter StatementOfWorkNumberCounter { get; set; }

        #region Allors
        [Id("9de2d2aa-c787-4a3c-9e77-49cd60fb2e27")]
        #endregion
        [Size(256)]
        [Workspace(Default)]
        public string WorkEffortNumberPrefix { get; set; }

        #region Allors
        [Id("af0cfafd-6b54-40bd-bdf0-258bf868d202")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter WorkEffortNumberCounter { get; set; }

        #region Allors
        [Id("db2c040c-c6b2-4eaa-a4aa-2a68abc5bd37")]
        #endregion
        [Workspace(Default)]
        public string RequirementNumberPrefix { get; set; }

        #region Allors
        [Id("8311c00d-a724-49b6-b0b9-932d96e99fa5")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public Counter RequirementNumberCounter { get; set; }

        #region inherited methods

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        #endregion
    }
}

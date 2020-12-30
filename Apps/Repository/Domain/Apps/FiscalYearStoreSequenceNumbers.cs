// <copyright file="FiscalYearStoreSequenceNumbers.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Allors.Repository.Attributes;

    #region Allors
    [Id("341fa885-0161-406b-89e6-08b1c92cd3b3")]
    #endregion
    [Plural("FiscalYearsStoreSequenceNumbers")]

    public partial class FiscalYearStoreSequenceNumbers : Object
    {
        #region inherited properties
        #endregion

        #region Allors
        [Id("c1b0dcb6-8627-4a47-86d0-2866344da3f1")]
        #endregion
        [Required]
        public int FiscalYear { get; set; }

        #region Allors
        [Id("e5c02d81-e0de-412b-91ad-5da3342a749c")]
        #endregion
        [Size(256)]
        [Workspace]
        public string SalesInvoiceNumberPrefix { get; set; }

        #region Allors
        [Id("14f064a8-461c-4726-93c4-91bc34c9c443")]
        #endregion
        [Derived]
        [Required]
        public int ObsoleteNextSalesInvoiceNumber { get; set; }

        #region Allors
        [Id("c21f533a-cf7a-4c15-8af4-40d87d4ad162")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public Counter SalesInvoiceNumberCounter { get; set; }

        #region Allors
        [Id("fe09d5c7-c750-47e1-8430-ee57bc30ae40")]
        #endregion
        [Size(256)]
        [Workspace]
        public string CreditNoteNumberPrefix { get; set; }

        #region Allors
        [Id("C349F8A9-82D8-406B-B026-AFBE67DCD375")]
        #endregion
        [Derived]
        [Required]
        public int ObsoleteNextCreditNoteNumber { get; set; }

        #region Allors
        [Id("a1f93578-bde7-427b-abf2-6eaffb9d9e99")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public Counter CreditNoteNumberCounter { get; set; }

        #region Allors
        [Id("74fd1e41-11c6-4d40-9108-554314b53d12")]
        #endregion
        [Size(256)]
        [Workspace]
        public string OutgoingShipmentNumberPrefix { get; set; }

        #region Allors
        [Id("2fe9d593-f56f-4be0-87a3-72b0c02862dc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public Counter OutgoingShipmentNumberCounter { get; set; }

        #region Allors
        [Id("4657bfc5-c276-44fa-a8ac-dbff2d046701")]
        #endregion
        [Size(256)]
        [Workspace]
        public string SalesOrderNumberPrefix { get; set; }

        #region Allors
        [Id("6c1c7ee3-a14c-46aa-9b92-6463485c53f5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        public Counter SalesOrderNumberCounter { get; set; }

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

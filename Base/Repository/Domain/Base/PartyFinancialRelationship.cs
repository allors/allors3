// <copyright file="PartyFinancialRelationship.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Allors.Repository.Attributes;

    #region Allors
    [Id("8F6C4557-AED7-4063-B05F-16573424FC51")]
    #endregion
    public partial class PartyFinancialRelationship : PartyRelationship, UniquelyIdentifiable
    {
        #region inherited properties

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Party[] Parties { get; set; }

        public Agreement[] Agreements { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("1A71A148-7F3D-4695-9546-63D282DA77D0")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace]
        public Party FinancialParty { get; set; }

        #region Allors
        [Id("AB000DAF-93D2-43EE-8820-924575FEB098")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region Allors
        [Id("01771db8-e79c-4ce4-9d81-db3675e8708a")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal YTDRevenue { get; set; }

        #region Allors
        [Id("04bc4912-cd23-4b2e-973c-76bbf2f2de8d")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal LastYearsRevenue { get; set; }

        #region Allors
        [Id("52863081-34b7-48e2-a7ff-c6bd67172350")]
        #endregion
        [Workspace]
        public bool ExcludeFromDunning { get; set; }

        #region Allors
        [Id("d05ee314-57be-4852-a3b5-62710df4d4b7")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal OpenOrderAmount { get; set; }

        #region Allors
        [Id("d97ab83b-85dc-4877-8b49-1e552489bcb0")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public PaymentMethod DefaultPaymentMethod { get; set; }

        #region Allors
        [Id("B66127AF-F490-4BF0-B8F5-6EBA878B1314")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        public decimal SimpleMovingAverage { get; set; }

        #region Allors
        [Id("42e3b2c4-376d-4e8b-bb49-2af031881ed0")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal AmountOverDue { get; set; }

        #region Allors
        [Id("76b46019-c145-403d-9f99-cd8e1001c968")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace]
        public DunningType DunningType { get; set; }

        #region Allors
        [Id("894f4ff2-9c41-4201-ad36-ac10dafd65dd")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal AmountDue { get; set; }

        #region Allors
        [Id("af50ade8-5964-4963-819d-c87689c6434e")]
        #endregion
        [Workspace]
        public DateTime LastReminderDate { get; set; }

        #region Allors
        [Id("dd59ed76-b6da-49a3-8c3e-1edf4d1d0900")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace]
        public decimal CreditLimit { get; set; }

        #region Allors
        [Id("e3a06a1c-998a-4871-8f0e-2f166eac6c7b")]
        #endregion
        [Required]
        [Workspace]
        public int SubAccountNumber { get; set; }

        #region Allors
        [Id("ee871786-8840-404d-9b41-932a9f59be13")]
        #endregion
        [Workspace]
        public DateTime BlockedForDunning { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPreDerive() { }

        public void OnDerive() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}

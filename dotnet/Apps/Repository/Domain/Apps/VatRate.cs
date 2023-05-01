// <copyright file="VatRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("a5e29ca1-80de-4de4-9085-b69f21550b5a")]
    #endregion
    public partial class VatRate : UniquelyIdentifiable, Period, Deletable
    {
        #region inherited properties
        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("3f1ca41a-8443-4d81-a112-48fa1e28728b")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Rate { get; set; }

        #region Allors
        [Id("5418fdea-366c-4e0b-b2e0-d49cfb12cbe5")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Organisation TaxAuthority { get; set; }

        #region Allors
        [Id("5551f4ce-858f-4f29-9e92-3c2c893bb44b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public VatRateUsage VatRateUsage { get; set; }

        #region Allors
        [Id("821df580-26d4-415c-b2ea-3e96a08c2f62")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public VatRatePurchaseKind VatRatePurchaseKind { get; set; }

        #region Allors
        [Id("958c1fda-0126-4b0a-8967-5d9df3ba50dc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public TimeFrequency PaymentFrequency { get; set; }

        #region Allors
        [Id("b628964e-5139-4c32-a2c1-239deaff70e8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public EuSalesListType EuSalesListType { get; set; }

        #region Allors
        [Id("46cf5d68-cceb-4b49-933c-875e9614eb8b")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public OrganisationGlAccount[] VatPayableAccounts { get; set; }

        #region Allors
        [Id("b2aa3989-8e65-4fdb-9654-46ae615fd73a")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public OrganisationGlAccount[] VatToPayAccounts { get; set; }

        #region Allors
        [Id("cf879781-9f52-438c-b0e0-fd23f336bead")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public OrganisationGlAccount[] VatReceivableAccounts { get; set; }

        #region Allors
        [Id("cbd85372-08d1-4c6d-81a9-02d76c874c46")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public OrganisationGlAccount[] VatToReceiveAccounts { get; set; }

        #region Allors
        [Id("e6242c51-98f9-408d-9dd8-07e3c639c82e")]
        #endregion
        [Workspace(Default)]
        public bool ReverseCharge { get; set; }

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

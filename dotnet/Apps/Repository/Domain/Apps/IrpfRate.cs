// <copyright file="IrpfRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("a8fdc91c-492b-4916-b272-04669ae8afe2")]
    #endregion

    /// <summary>
    /// Impuesto sobre la renta de las personas fí­sicas.
    /// It is the personal Income Tax in Spain, a direct tax levied on the income of individuals.
    /// if you are selling to a Spanish business you will have to indicate the level of IRPF that should be withheld by the customer.
    /// Essentially this is withheld by your customer and paid to the tax office by them on your behalf.
    /// </summary>
    public partial class IrpfRate : UniquelyIdentifiable, Period, Deletable
    {
        #region inherited properties
        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        #endregion

        #region Allors
        [Id("449f0627-3e4c-4fa3-aeaa-8a7b3c15ef12")]
        #endregion
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal Rate { get; set; }

        #region Allors
        [Id("fc8c5d16-482a-4f99-a553-2380f30117f4")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Organisation TaxAuthority { get; set; }

        #region Allors
        [Id("c512b3be-af24-4865-866c-03f38bcac4fc")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public TimeFrequency PaymentFrequency { get; set; }

        #region Allors
        [Id("841FE2F6-85E9-495E-9043-D5F228304E22")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public OrganisationGlAccount[] IrpfPayableAccounts { get; set; }

        #region Allors
        [Id("748AB6BD-DC32-45EE-83E4-88CF7BB3845A")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public OrganisationGlAccount[] IrpfToPayAccounts { get; set; }

        #region Allors
        [Id("D0981213-985A-43FF-9DC5-AD571ADFB8B8")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public OrganisationGlAccount[] IrpfReceivableAccounts { get; set; }

        #region Allors
        [Id("1AD7B882-B4A8-43E0-8777-C044721D3710")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Indexed]
        [Workspace(Default)]
        public OrganisationGlAccount[] IrpfToReceiveAccounts { get; set; }

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

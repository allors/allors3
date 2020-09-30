// <copyright file="IrpfRate.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
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
    public partial class IrpfRate : UniquelyIdentifiable
    {
        #region inherited properties
        public Permission[] DeniedPermissions { get; set; }

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

        #region inherited methods

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

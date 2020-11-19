// <copyright file="PaymentMethod.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("f34d5b9b-b940-4885-9744-754dd0eae08d")]
    #endregion
    public partial interface PaymentMethod : UniquelyIdentifiable, Object
    {
        #region Allors
        [Id("0b16fdbc-c535-45a5-8be9-7b1d2c12337a")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        decimal BalanceLimit { get; set; }

        #region Allors
        [Id("2e5e9d24-4697-4811-8636-1ebf9d86b9c2")]
        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        decimal CurrentBalance { get; set; }

        #region Allors
        [Id("36559f29-1182-42d1-831d-587103456ce6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        Journal Journal { get; set; }

        #region Allors
        [Id("386c301e-8f0f-48fc-8bec-10ac0df6be9d")]
        #endregion
        [Size(-1)]
        string Description { get; set; }

        #region Allors
        [Id("59da5fc4-e861-4c7d-aa96-c15cebbb63f2")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        OrganisationGlAccount GlPaymentInTransit { get; set; }

        #region Allors
        [Id("6e61f71f-77a1-4795-b876-ba5d74ebdc3e")]
        #endregion
        [Size(-1)]
        string Remarks { get; set; }

        #region Allors
        [Id("8b11feda-09c8-4f8d-a21d-dddd87531d5b")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        OrganisationGlAccount GeneralLedgerAccount { get; set; }

        #region Allors
        [Id("c32243ac-8810-478b-b0f4-11a1fe4773bd")]
        #endregion
        [Required]
        bool IsActive { get; set; }
    }
}

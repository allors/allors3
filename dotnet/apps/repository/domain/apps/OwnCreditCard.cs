// <copyright file="OwnCreditCard.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("23848955-69ae-40ce-b973-0d416ae80c78")]
    #endregion
    public partial class OwnCreditCard : PaymentMethod, FinancialAccount
    {
        #region inherited properties

        public decimal BalanceLimit { get; set; }

        public decimal CurrentBalance { get; set; }

        public Journal Journal { get; set; }

        public string Description { get; set; }

        public OrganisationGlAccount GlPaymentInTransit { get; set; }

        public string Remarks { get; set; }

        public OrganisationGlAccount GeneralLedgerAccount { get; set; }

        public bool IsActive { get; set; }

        public Permission[] DeniedPermissions { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public FinancialAccountTransaction[] FinancialAccountTransactions { get; set; }

        #endregion

        #region Allors
        [Id("7ca9a38c-4318-4bb6-8bc6-50e5dfe9c701")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]

        public Person Owner { get; set; }

        #region Allors
        [Id("e2514c8b-5980-4e58-a75f-20890ed79516")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public CreditCard CreditCard { get; set; }

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

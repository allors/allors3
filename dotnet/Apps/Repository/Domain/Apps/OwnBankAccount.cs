// <copyright file="OwnBankAccount.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;

    #region Allors
    [Id("ca008b8d-584e-4aa5-a759-895b634defc5")]
    #endregion
    public partial class OwnBankAccount : PaymentMethod, FinancialAccount
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

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public FinancialAccountTransaction[] FinancialAccountTransactions { get; set; }

        #endregion

        #region Allors
        [Id("d83ca1e3-4137-4e92-a61d-0b8a1b8f7085")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]

        public BankAccount BankAccount { get; set; }

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

// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;
    using System.Diagnostics;

    public class AccountingTransactionDerivedTotalAmountRule : Rule
    {
        public AccountingTransactionDerivedTotalAmountRule(MetaPopulation m) : base(m, new Guid("237a3b79-9202-4f3f-9632-980bb564f987")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingTransaction.RolePattern(v => v.AccountingTransactionDetails),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransaction>())
            {
                @this.DeriveAccountingTransactionDerivedTotalAmount(validation);
            }
        }
    }

    public static class AccountingTransactionDerivedTotalAmountRuleExtensions
    {
        public static void DeriveAccountingTransactionDerivedTotalAmount(this AccountingTransaction @this, IValidation validation) => @this.DerivedTotalAmount = @this.AccountingTransactionDetails.Where(v => v.ExistBalanceSide && v.BalanceSide.Equals(new BalanceSides(@this.Transaction()).Debit)).Sum(v => v.Amount);
    }
}

// <copyright file="AccountingTransactionRule.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class AccountingTransactionTransactionNumberRule : Rule
    {
        public AccountingTransactionTransactionNumberRule(MetaPopulation m) : base(m, new Guid("ae7c43c6-258a-4f6f-b558-8cb8af81660c")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingTransaction.RolePattern(v => v.AccountingTransactionType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransaction>().Where(v => v.Strategy.IsNewInTransaction))
            {
                @this.DeriveAccountingTransactionTransactionNumber(validation);
            }
        }
    }

    public static class AccountingTransactionTransactionNumberRuleExtensions
    {
        public static void DeriveAccountingTransactionTransactionNumber(this AccountingTransaction @this, IValidation validation)
        {
            @this.TransactionNumber = @this.Strategy.Transaction.GetSingleton().Settings.NextTransactionNumber();
        }
    }
}

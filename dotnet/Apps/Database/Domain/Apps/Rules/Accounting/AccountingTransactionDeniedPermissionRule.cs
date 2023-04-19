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

    public class AccountingTransactionDeniedPermissionRule : Rule
    {
        public AccountingTransactionDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("75d5b503-4564-4545-9f98-1bdac52dc1b1")) =>
            this.Patterns = new Pattern[]
        {
            m.AccountingTransaction.RolePattern(v => v.Exported),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransaction>())
            {
                @this.DeriveAccountingTransactionDeniedPermission(validation);
            }
        }
    }

    public static class AccountingTransactionDeniedPermissionRuleExtensions
    {
        public static void DeriveAccountingTransactionDeniedPermission(this AccountingTransaction @this, IValidation validation)
        {
            var deleteRevocation = new Revocations(@this.Strategy.Transaction).AccountingTransactionDeleteRevocation;

            if (@this.IsDeletable)
            {
                @this.RemoveRevocation(deleteRevocation);
            }
            else
            {
                @this.AddRevocation(deleteRevocation);
            }
        }
    }
}

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

    public class AccountingTransactionWorkEffortNumberRule : Rule
    {
        public AccountingTransactionWorkEffortNumberRule(MetaPopulation m) : base(m, new Guid("143243b9-1fc3-46c7-b5e6-d2fb43ebd3fb")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingTransaction.RolePattern(v => v.WorkEffort),
                m.WorkEffort.RolePattern(v => v.WorkEffortNumber, v => v.AccountingTransactionsWhereWorkEffort.AccountingTransaction),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransaction>())
            {
                @this.DeriveAccountingTransactionWorkEffortNumber(validation);
            }
        }
    }

    public static class AccountingTransactionWorkEffortNumberRuleExtensions
    {
        public static void DeriveAccountingTransactionWorkEffortNumber(this AccountingTransaction @this, IValidation validation) => @this.WorkEffortNumber = @this.WorkEffort.WorkEffortNumber;
    }
}

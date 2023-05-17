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

    public class AccountingTransactionAccountingTransactionTypeNameRule : Rule
    {
        public AccountingTransactionAccountingTransactionTypeNameRule(MetaPopulation m) : base(m, new Guid("26b9db34-2532-43fa-b9d8-aa7401887a8e")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingTransaction.RolePattern(v => v.AccountingTransactionDetails),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransaction>())
            {
                @this.DeriveAccountingTransactionAccountingTransactionTypeName(validation);
            }
        }
    }

    public static class AccountingTransactionAccountingTransactionTypeNameRuleExtensions
    {
        public static void DeriveAccountingTransactionAccountingTransactionTypeName(this AccountingTransaction @this, IValidation validation) => @this.AccountingTransactionTypeName = @this.AccountingTransactionType.Name;
    }
}

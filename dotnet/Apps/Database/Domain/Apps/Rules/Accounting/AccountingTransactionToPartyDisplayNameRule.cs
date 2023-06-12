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

    public class AccountingTransactionToPartyDisplayNameRule : Rule
    {
        public AccountingTransactionToPartyDisplayNameRule(MetaPopulation m) : base(m, new Guid("ea76ea0e-445f-4a96-88e9-543a314372cd")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingTransaction.RolePattern(v => v.ToParty),
                m.Party.RolePattern(v => v.DisplayName, v => v.AccountingTransactionsWhereToParty.ObjectType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransaction>())
            {
                @this.DeriveAccountingTransactionToPartyDisplayName(validation);
            }
        }
    }

    public static class AccountingTransactionToPartyDisplayNameRuleExtensions
    {
        public static void DeriveAccountingTransactionToPartyDisplayName(this AccountingTransaction @this, IValidation validation) => @this.ToPartyDisplayName = @this.ToParty.DisplayName;
    }
}

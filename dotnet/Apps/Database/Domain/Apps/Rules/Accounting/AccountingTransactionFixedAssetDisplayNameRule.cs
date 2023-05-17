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

    public class AccountingTransactionFixedAssetDisplayNameRule : Rule
    {
        public AccountingTransactionFixedAssetDisplayNameRule(MetaPopulation m) : base(m, new Guid("9efda11b-ab7e-463e-ba3e-e0a9794a0e0e")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingTransaction.RolePattern(v => v.FixedAsset),
                m.FixedAsset.RolePattern(v => v.DisplayName, v => v.AccountingTransactionsWhereFixedAsset.AccountingTransaction),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransaction>())
            {
                @this.DeriveAccountingTransactionFixedAssetDisplayName(validation);
            }
        }
    }

    public static class AccountingTransactionFixedAssetDisplayNameRuleExtensions
    {
        public static void DeriveAccountingTransactionFixedAssetDisplayName(this AccountingTransaction @this, IValidation validation) => @this.FixedAssetDisplayName = @this.FixedAsset.DisplayName;
    }
}

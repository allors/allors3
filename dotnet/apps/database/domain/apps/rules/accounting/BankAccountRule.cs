// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Derivations;

    public class BankAccountRule : Rule
    {
        public BankAccountRule(MetaPopulation m) : base(m, new Guid("633f58cd-ca1b-4a2e-8f6e-e1642466a9f7")) =>
            this.Patterns = new Pattern[]
            {
                m.BankAccount.AssociationPattern(v => v.OwnBankAccountsWhereBankAccount),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<BankAccount>())
            {
                if (@this.ExistOwnBankAccountsWhereBankAccount)
                {
                    validation.AssertExists(@this, @this.Meta.Bank);
                    validation.AssertExists(@this, @this.Meta.Currency);
                    validation.AssertExists(@this, @this.Meta.NameOnAccount);
                }
            }
        }
    }
}

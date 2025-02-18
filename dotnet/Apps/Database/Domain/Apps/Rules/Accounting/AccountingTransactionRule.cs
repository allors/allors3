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

    public class AccountingTransactionRule : Rule
    {
        public AccountingTransactionRule(MetaPopulation m) : base(m, new Guid("321bfc64-6430-435b-959b-c5198f332046")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingTransaction.RolePattern(v => v.FromParty),
                m.AccountingTransaction.RolePattern(v => v.ToParty),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingTransaction>())
            {
                if (!@this.ExistInternalOrganisation)
                {
                    if (@this.ExistFromParty && @this.FromParty.GetType().Name.Equals(nameof(InternalOrganisation)))
                    {
                        @this.InternalOrganisation = (InternalOrganisation)@this.FromParty;
                    }
                    else if (@this.ExistToParty && @this.ToParty.GetType().Name.Equals(nameof(InternalOrganisation)))
                    {
                        @this.InternalOrganisation = (InternalOrganisation)@this.ToParty;
                    }
                }
            }
        }
    }
}


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

    public class OwnCreditCardRule : Rule
    {
        public OwnCreditCardRule(MetaPopulation m) : base(m, new Guid("838dbea6-9123-4cfe-acfe-1c6347ec7ff2")) =>
            this.Patterns = new Pattern[]
            {
                m.OwnCreditCard.RolePattern(v => v.CreditCard),
                m.CreditCard.RolePattern(v => v.ExpirationYear, v => v.OwnCreditCardsWhereCreditCard),
                m.CreditCard.RolePattern(v => v.ExpirationMonth, v => v.OwnCreditCardsWhereCreditCard),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<OwnCreditCard>())
            {
                if (@this.ExistCreditCard)
                {
                    if (@this.CreditCard.ExpirationYear <= @this.Transaction().Now().Year && @this.CreditCard.ExpirationMonth <= @this.Transaction().Now().Month)
                    {
                        @this.IsActive = false;
                    }
                }
            }
        }
    }
}

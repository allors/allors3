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

    public class QuoteCreatedDeriveCurrencyRule : Rule
    {
        public QuoteCreatedDeriveCurrencyRule(MetaPopulation m) : base(m, new Guid("383cdf87-ac0e-4ab6-998d-9f8cef6fcd83")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Quote, m.Quote.QuoteState),
                new RolePattern(m.Quote, m.Quote.Issuer),
                new RolePattern(m.Quote, m.Quote.Receiver),
                new RolePattern(m.Quote, m.Quote.AssignedCurrency),
                new RolePattern(m.Party, m.Party.PreferredCurrency) { Steps = new IPropertyType[] { m.Party.QuotesWhereReceiver }},
                new RolePattern(m.Organisation, m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { m.Organisation.QuotesWhereReceiver }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Quote>().Where(v => v.QuoteState.IsCreated))
            {
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.Receiver?.PreferredCurrency ?? @this.Receiver?.Locale?.Country?.Currency ?? @this.Issuer?.PreferredCurrency;
            }
        }
    }
}

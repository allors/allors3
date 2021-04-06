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

    public class QuoteCreatedDeriveLocalRule : Rule
    {
        public QuoteCreatedDeriveLocalRule(MetaPopulation m) : base(m, new Guid("3a31edf7-6973-49f3-bdc7-55b857c2b213")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.Quote, m.Quote.QuoteState),
                new RolePattern(m.Quote, m.Quote.Issuer),
                new RolePattern(m.Quote, m.Quote.Receiver),
                new RolePattern(m.Quote, m.Quote.Locale),
                new RolePattern(m.Organisation, m.Organisation.Locale) { Steps = new IPropertyType[] { m.Organisation.QuotesWhereIssuer }},
                new RolePattern(m.Party, m.Party.Locale) { Steps = new IPropertyType[] { m.Party.QuotesWhereReceiver }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Quote>().Where(v => v.QuoteState.IsCreated))
            {
                @this.DerivedLocale = @this.Locale ?? @this.Receiver?.Locale ?? @this.Issuer?.Locale;
            }
        }
    }
}

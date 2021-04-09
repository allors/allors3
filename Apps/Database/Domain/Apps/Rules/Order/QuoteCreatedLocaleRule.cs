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

    public class QuoteCreatedLocalRule : Rule
    {
        public QuoteCreatedLocalRule(MetaPopulation m) : base(m, new Guid("3a31edf7-6973-49f3-bdc7-55b857c2b213")) =>
            this.Patterns = new Pattern[]
            {
                m.Quote.RolePattern(v => v.QuoteState),
                m.Quote.RolePattern(v => v.Issuer),
                m.Quote.RolePattern(v => v.Receiver),
                m.Quote.RolePattern(v => v.Locale),
                m.Organisation.RolePattern(v => v.Locale, v => v.QuotesWhereIssuer),
                m.Party.RolePattern(v => v.Locale, v => v.QuotesWhereReceiver),
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

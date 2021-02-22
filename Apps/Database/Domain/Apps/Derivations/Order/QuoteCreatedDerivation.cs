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

    public class QuoteCreatedDerivation : DomainDerivation
    {
        public QuoteCreatedDerivation(M m) : base(m, new Guid("b66c0721-4aa5-4ca7-91a0-534f6cfc6718")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.Quote.QuoteState),
                new ChangedPattern(m.Quote.Issuer),
                new ChangedPattern(m.Quote.Receiver),
                new ChangedPattern(m.Quote.Locale),
                new ChangedPattern(m.Quote.AssignedVatRegime),
                new ChangedPattern(m.Quote.AssignedIrpfRegime),
                new ChangedPattern(m.Quote.AssignedCurrency),
                new ChangedPattern(m.Organisation.Locale) { Steps = new IPropertyType[] { m.Organisation.QuotesWhereIssuer }},
                new ChangedPattern(m.Party.Locale) { Steps = new IPropertyType[] { m.Party.QuotesWhereReceiver }},
                new ChangedPattern(m.Party.PreferredCurrency) { Steps = new IPropertyType[] { m.Party.QuotesWhereReceiver }},
                new ChangedPattern(m.Party.VatRegime) { Steps = new IPropertyType[] { m.Party.QuotesWhereReceiver }},
                new ChangedPattern(m.Party.IrpfRegime) { Steps = new IPropertyType[] { m.Party.QuotesWhereReceiver }},
                new ChangedPattern(m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { m.Organisation.QuotesWhereReceiver }},
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Quote>().Where(v => v.QuoteState.IsCreated))
            {
                @this.DerivedLocale = @this.Locale ?? @this.Receiver?.Locale ?? @this.Issuer?.Locale;
                @this.DerivedCurrency = @this.AssignedCurrency ?? @this.Receiver?.PreferredCurrency ?? @this.Receiver?.Locale?.Country?.Currency ?? @this.Issuer?.PreferredCurrency;
                @this.DerivedVatRegime = @this.AssignedVatRegime ?? @this.Receiver?.VatRegime;
                @this.DerivedIrpfRegime = @this.AssignedIrpfRegime ?? @this.Receiver?.IrpfRegime;
            }
        }
    }
}

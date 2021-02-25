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
                new AssociationPattern(m.Quote.QuoteState),
                new AssociationPattern(m.Quote.Issuer),
                new AssociationPattern(m.Quote.Receiver),
                new AssociationPattern(m.Quote.Locale),
                new AssociationPattern(m.Quote.AssignedVatRegime),
                new AssociationPattern(m.Quote.AssignedIrpfRegime),
                new AssociationPattern(m.Quote.AssignedCurrency),
                new AssociationPattern(m.Organisation.Locale) { Steps = new IPropertyType[] { m.Organisation.QuotesWhereIssuer }},
                new AssociationPattern(m.Party.Locale) { Steps = new IPropertyType[] { m.Party.QuotesWhereReceiver }},
                new AssociationPattern(m.Party.PreferredCurrency) { Steps = new IPropertyType[] { m.Party.QuotesWhereReceiver }},
                new AssociationPattern(m.Party.VatRegime) { Steps = new IPropertyType[] { m.Party.QuotesWhereReceiver }},
                new AssociationPattern(m.Party.IrpfRegime) { Steps = new IPropertyType[] { m.Party.QuotesWhereReceiver }},
                new AssociationPattern(m.Organisation.PreferredCurrency) { Steps = new IPropertyType[] { m.Organisation.QuotesWhereReceiver }},
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

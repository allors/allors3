// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class QuoteDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("B2464D89-5370-44D7-BB6B-7E6FA48EEF0B");

        public IEnumerable<Pattern> Patterns { get; } = new Pattern[]
        {
            new CreatedPattern(M.Quote.Interface),
        };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var Quote in matches.Cast<Quote>())
            {
                if (!Quote.ExistIssuer)
                {
                    var internalOrganisations = new Organisations(cycle.Session).InternalOrganisations();

                    if (internalOrganisations.Count() == 1)
                    {
                        Quote.Issuer = internalOrganisations.First();
                    }
                }

                if (!Quote.ExistQuoteNumber && Quote.ExistIssuer)
                {
                    Quote.QuoteNumber = Quote.Issuer.NextQuoteNumber(cycle.Session.Now().Year);
                    (Quote).SortableQuoteNumber = Quote.Session().GetSingleton().SortableNumber(Quote.Issuer.QuoteNumberPrefix, Quote.QuoteNumber, Quote.IssueDate.Year.ToString());
                }

                Quote.Currency ??= Quote.Receiver?.PreferredCurrency ?? Quote.Issuer?.PreferredCurrency;

                foreach (QuoteItem quoteItem in Quote.QuoteItems)
                {
                    var quoteItemDerivedRoles = quoteItem;

                    quoteItemDerivedRoles.VatRegime = quoteItem.AssignedVatRegime ?? Quote.VatRegime;
                    quoteItemDerivedRoles.VatRate = quoteItem.VatRegime?.VatRate;

                    quoteItemDerivedRoles.IrpfRegime = quoteItem.AssignedIrpfRegime ?? Quote.IrpfRegime;
                    quoteItemDerivedRoles.IrpfRate = quoteItem.IrpfRegime?.IrpfRate;
                }

                Quote.AddSecurityToken(new SecurityTokens(cycle.Session).DefaultSecurityToken);
            }
        }
    }
}

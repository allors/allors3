// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;
    using Resources;

    public static partial class DabaseExtensions
    {
        public class QuoteExtensionsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdQuote = changeSet.Created.Select(session.Instantiate).OfType<Quote>();

                foreach (var Quote in createdQuote)
                {
                    if (!Quote.ExistIssuer)
                    {
                        var internalOrganisations = new Organisations(session).InternalOrganisations();

                        if (internalOrganisations.Count() == 1)
                        {
                            Quote.Issuer = internalOrganisations.First();
                        }
                    }

                    if (!Quote.ExistQuoteNumber && Quote.ExistIssuer)
                    {
                        Quote.QuoteNumber = Quote.Issuer.NextQuoteNumber(session.Now().Year);
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

                    Quote.AddSecurityToken(new SecurityTokens(session).DefaultSecurityToken);

                    Sync(Quote);
                }

                void Sync(Quote quoteExtensions)
                {
                    var QuoteDerivedRoles = quoteExtensions;
                }
            }
        }

        public static void QuoteExtensionsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("d11b6c61-d19a-4197-b474-846c5fd6c528")] = new QuoteExtensionsCreationDerivation();
        }
    }
}

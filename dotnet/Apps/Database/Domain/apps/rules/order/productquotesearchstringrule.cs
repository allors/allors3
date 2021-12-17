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
    using Derivations.Rules;

    public class ProductQuoteSearchStringRule : Rule
    {
        public ProductQuoteSearchStringRule(MetaPopulation m) : base(m, new Guid("25e985a3-dd04-403a-ba85-f9b16fd4b474")) =>
            this.Patterns = new Pattern[]
            {
                m.Quote.RolePattern(v => v.QuoteNumber, m.ProductQuote),
                m.Quote.RolePattern(v => v.QuoteState, m.ProductQuote),
                m.Quote.RolePattern(v => v.Issuer, m.ProductQuote),
                m.Quote.RolePattern(v => v.DerivedCurrency, m.ProductQuote),
                m.Quote.RolePattern(v => v.InternalComment, m.ProductQuote),
                m.Quote.RolePattern(v => v.Description, m.ProductQuote),
                m.Quote.RolePattern(v => v.DerivedIrpfRegime, m.ProductQuote),
                m.Quote.RolePattern(v => v.DerivedVatRegime, m.ProductQuote),
                m.Quote.RolePattern(v => v.DerivedVatClause, m.ProductQuote),
                m.Quote.RolePattern(v => v.Request, m.ProductQuote),
                m.Quote.RolePattern(v => v.ContactPerson, m.ProductQuote),
                m.Person.RolePattern(v => v.DisplayName, v => v.QuotesWhereContactPerson.Quote, m.ProductQuote),
                m.Quote.RolePattern(v => v.QuoteTerms, m.ProductQuote),
                m.QuoteTerm.RolePattern(v => v.TermValue, v => v.QuotesWhereQuoteTerm.Quote, m.ProductQuote),
                m.QuoteTerm.RolePattern(v => v.TermType, v => v.QuotesWhereQuoteTerm.Quote, m.ProductQuote),
                m.QuoteTerm.RolePattern(v => v.Description, v => v.QuotesWhereQuoteTerm.Quote, m.ProductQuote),
                m.Quote.RolePattern(v => v.Receiver, m.ProductQuote),
                m.Party.RolePattern(v => v.DisplayName, v => v.QuotesWhereReceiver.Quote, m.ProductQuote),
                m.Quote.RolePattern(v => v.FullfillContactMechanism, m.ProductQuote),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.QuotesWhereFullfillContactMechanism.Quote, m.ProductQuote),

                m.QuoteItem.RolePattern(v => v.QuoteItemState, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.DerivedIrpfRegime, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.DerivedVatRegime, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.InvoiceItemType, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.InternalComment, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.Authorizer, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.Party.RolePattern(v => v.DisplayName, v => v.QuoteItemsWhereAuthorizer.QuoteItem.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.Product, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.ProductFeature, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.SerialisedItem, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.WorkEffort, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.QuoteTerms, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteTerm.RolePattern(v => v.TermValue, v => v.QuoteItemWhereQuoteTerm.QuoteItem.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteTerm.RolePattern(v => v.TermType, v => v.QuoteItemWhereQuoteTerm.QuoteItem.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteTerm.RolePattern(v => v.Description, v => v.QuoteItemWhereQuoteTerm.QuoteItem.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.RequestItem, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.Details, v => v.QuoteWhereQuoteItem.Quote, m.ProductQuote),

                m.ProductQuote.AssociationPattern(v => v.SalesOrderWhereQuote),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProductQuote>())
            {
                var array = new string[] {
                    @this.QuoteNumber,
                    @this.QuoteState?.Name,
                    @this.Issuer?.DisplayName,
                    @this.DerivedCurrency?.Abbreviation,
                    @this.DerivedCurrency?.Name,
                    @this.InternalComment,
                    @this.Description,
                    @this.DerivedIrpfRegime?.Name,
                    @this.DerivedVatRegime?.Name,
                    @this.DerivedVatClause?.Name,
                    @this.Request?.RequestNumber,
                    @this.ContactPerson?.DisplayName,
                    @this.ExistQuoteTerms ? string.Join(" ", @this.QuoteTerms?.Select(v => v.TermValue)) : null,
                    @this.ExistQuoteTerms ? string.Join(" ", @this.QuoteTerms?.Select(v => v.TermType?.Name)) : null,
                    @this.ExistQuoteTerms ? string.Join(" ", @this.QuoteTerms?.Select(v => v.Description)) : null,
                    @this.Receiver?.DisplayName,
                    @this.FullfillContactMechanism?.DisplayName,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.QuoteItemState?.Name)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.DerivedIrpfRegime?.Name)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.DerivedVatRegime?.Name)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.InvoiceItemType?.Name)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.InternalComment)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.Authorizer?.DisplayName)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.Product?.DisplayName)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.ProductFeature?.Description)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.SerialisedItem?.DisplayName)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.WorkEffort?.WorkEffortNumber)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.QuoteTerms?.Select(v => v.TermValue))) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.QuoteTerms?.Select(v => v.TermType?.Name))) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.QuoteTerms?.Select(v => v.Description))) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.RequestItem?.RequestWhereRequestItem?.RequestNumber)) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.Details)) : null,
                    @this.ExistSalesOrderWhereQuote ? string.Join(" ", @this.SalesOrderWhereQuote?.OrderNumber) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}

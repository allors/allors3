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

    public class StatementOfWorkSearchStringRule : Rule
    {
        public StatementOfWorkSearchStringRule(MetaPopulation m) : base(m, new Guid("ecb0d109-7cdc-4672-9703-62639158b6c0")) =>
            this.Patterns = new Pattern[]
            {
                m.Quote.RolePattern(v => v.QuoteNumber, m.StatementOfWork),
                m.Quote.RolePattern(v => v.QuoteState, m.StatementOfWork),
                m.Quote.RolePattern(v => v.Issuer, m.StatementOfWork),
                m.Quote.RolePattern(v => v.DerivedCurrency, m.StatementOfWork),
                m.Quote.RolePattern(v => v.InternalComment, m.StatementOfWork),
                m.Quote.RolePattern(v => v.Description, m.StatementOfWork),
                m.Quote.RolePattern(v => v.DerivedIrpfRegime, m.StatementOfWork),
                m.Quote.RolePattern(v => v.DerivedVatRegime, m.StatementOfWork),
                m.Quote.RolePattern(v => v.DerivedVatClause, m.StatementOfWork),
                m.Quote.RolePattern(v => v.Request, m.StatementOfWork),
                m.Quote.RolePattern(v => v.ContactPerson, m.StatementOfWork),
                m.Person.RolePattern(v => v.DisplayName, v => v.QuotesWhereContactPerson.Quote, m.StatementOfWork),
                m.Quote.RolePattern(v => v.QuoteTerms, m.StatementOfWork),
                m.QuoteTerm.RolePattern(v => v.TermValue, v => v.QuotesWhereQuoteTerm.Quote, m.StatementOfWork),
                m.QuoteTerm.RolePattern(v => v.TermType, v => v.QuotesWhereQuoteTerm.Quote, m.StatementOfWork),
                m.QuoteTerm.RolePattern(v => v.Description, v => v.QuotesWhereQuoteTerm.Quote, m.StatementOfWork),
                m.Quote.RolePattern(v => v.Receiver, m.StatementOfWork),
                m.Party.RolePattern(v => v.DisplayName, v => v.QuotesWhereReceiver.Quote, m.StatementOfWork),
                m.Quote.RolePattern(v => v.FullfillContactMechanism, m.StatementOfWork),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.QuotesWhereFullfillContactMechanism.Quote, m.StatementOfWork),

                m.QuoteItem.RolePattern(v => v.QuoteItemState, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.DerivedIrpfRegime, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.DerivedVatRegime, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.InvoiceItemType, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.InternalComment, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.Authorizer, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.Party.RolePattern(v => v.DisplayName, v => v.QuoteItemsWhereAuthorizer.QuoteItem.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.Product, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.ProductFeature, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.SerialisedItem, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.QuoteItemsWhereSerialisedItem.QuoteItem.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.WorkEffort, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.QuoteTerms, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteTerm.RolePattern(v => v.TermValue, v => v.QuoteItemWhereQuoteTerm.QuoteItem.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteTerm.RolePattern(v => v.TermType, v => v.QuoteItemWhereQuoteTerm.QuoteItem.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteTerm.RolePattern(v => v.Description, v => v.QuoteItemWhereQuoteTerm.QuoteItem.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.RequestItem, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
                m.QuoteItem.RolePattern(v => v.Details, v => v.QuoteWhereQuoteItem.Quote, m.StatementOfWork),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<StatementOfWork>())
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
                    @this.ExistQuoteTerms ? string.Join(" ", @this.QuoteTerms?.Select(v => v.TermValue ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteTerms ? string.Join(" ", @this.QuoteTerms?.Select(v => v.TermType?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteTerms ? string.Join(" ", @this.QuoteTerms?.Select(v => v.Description ?? string.Empty).ToArray()) : null,
                    @this.Receiver?.DisplayName,
                    @this.FullfillContactMechanism?.DisplayName,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.QuoteItemState?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.DerivedIrpfRegime?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.DerivedVatRegime?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.InvoiceItemType?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.InternalComment ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.Authorizer?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.Product?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.ProductFeature?.Description ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.SerialisedItem?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.WorkEffort?.WorkEffortNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.SelectMany(v => v.QuoteTerms?.Select(v => v.TermValue ?? string.Empty)).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.SelectMany(v => v.QuoteTerms?.Select(v => v.TermType?.Name ?? string.Empty)).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.SelectMany(v => v.QuoteTerms?.Select(v => v.Description ?? string.Empty)).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.RequestItem?.RequestWhereRequestItem?.RequestNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.Details ?? string.Empty).ToArray()) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}

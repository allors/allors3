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
                m.Person.RolePattern(v => v.DisplayName, v => v.QuotesWhereContactPerson.ObjectType, m.ProductQuote),
                m.Quote.RolePattern(v => v.SalesTerms, m.ProductQuote),
                m.SalesTerm.RolePattern(v => v.TermValue, v => v.QuoteWhereSalesTerm.ObjectType, m.ProductQuote),
                m.SalesTerm.RolePattern(v => v.TermType, v => v.QuoteWhereSalesTerm.ObjectType, m.ProductQuote),
                m.SalesTerm.RolePattern(v => v.Description, v => v.QuoteWhereSalesTerm.ObjectType, m.ProductQuote),
                m.Quote.RolePattern(v => v.Receiver, m.ProductQuote),
                m.Party.RolePattern(v => v.DisplayName, v => v.QuotesWhereReceiver.ObjectType, m.ProductQuote),
                m.Quote.RolePattern(v => v.FullfillContactMechanism, m.ProductQuote),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.QuotesWhereFullfillContactMechanism.ObjectType, m.ProductQuote),

                m.QuoteItem.RolePattern(v => v.QuoteItemState, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.DerivedIrpfRegime, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.DerivedVatRegime, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.InvoiceItemType, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.InternalComment, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.Authorizer, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.Party.RolePattern(v => v.DisplayName, v => v.QuoteItemsWhereAuthorizer.ObjectType.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.Product, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.ProductFeature, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.SerialisedItem, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.QuoteItemsWhereSerialisedItem.ObjectType.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.WorkEffort, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.SalesTerms, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteTerm.RolePattern(v => v.TermValue, v => v.QuoteItemWhereSalesTerm.ObjectType.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteTerm.RolePattern(v => v.TermType, v => v.QuoteItemWhereSalesTerm.ObjectType.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteTerm.RolePattern(v => v.Description, v => v.QuoteItemWhereSalesTerm.ObjectType.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.RequestItem, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),
                m.QuoteItem.RolePattern(v => v.Details, v => v.QuoteWhereQuoteItem.ObjectType, m.ProductQuote),

                m.ProductQuote.AssociationPattern(v => v.SalesOrderWhereQuote),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductQuote>())
            {
                @this.DeriveProductQuoteSearchString(validation);
            }
        }
    }

    public static class ProductQuoteSearchStringRuleExtensions
    {
        public static void DeriveProductQuoteSearchString(this ProductQuote @this, IValidation validation)
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
                    @this.ExistSalesTerms ? string.Join(" ", @this.SalesTerms?.Select(v => v.TermValue ?? string.Empty).ToArray()) : null,
                    @this.ExistSalesTerms ? string.Join(" ", @this.SalesTerms?.Select(v => v.TermType?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistSalesTerms ? string.Join(" ", @this.SalesTerms?.Select(v => v.Description ?? string.Empty).ToArray()) : null,
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
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.SelectMany(v => v.SalesTerms?.Select(v => v.TermValue ?? string.Empty)).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.SelectMany(v => v.SalesTerms?.Select(v => v.TermType?.Name ?? string.Empty)).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.SelectMany(v => v.SalesTerms?.Select(v => v.Description ?? string.Empty)).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.RequestItem?.RequestWhereRequestItem?.RequestNumber ?? string.Empty).ToArray()) : null,
                    @this.ExistQuoteItems ? string.Join(" ", @this.QuoteItems?.Select(v => v.Details ?? string.Empty).ToArray()) : null,
                    @this.SalesOrderWhereQuote?.OrderNumber,
                };

            if (array.Any(s => !string.IsNullOrEmpty(s)))
            {
                @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
            }
            else
            {
                @this.RemoveSearchString();
            }
        }
    }
}

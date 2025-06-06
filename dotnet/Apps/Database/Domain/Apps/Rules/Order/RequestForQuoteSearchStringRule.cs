// <copyright file="Domain.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
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

    public class RequestForQuoteSearchStringRule : Rule
    {
        public RequestForQuoteSearchStringRule(MetaPopulation m) : base(m, new Guid("755df94a-d000-49f6-a024-34acaf4b99a1")) =>
            this.Patterns = new Pattern[]
            {
                m.Request.RolePattern(v => v.RequestNumber, m.RequestForQuote),
                m.Request.RolePattern(v => v.RequestState, m.RequestForQuote),
                m.Request.RolePattern(v => v.Recipient, m.RequestForQuote),
                m.Request.RolePattern(v => v.DerivedCurrency, m.RequestForQuote),
                m.Request.RolePattern(v => v.InternalComment, m.RequestForQuote),
                m.Request.RolePattern(v => v.Description, m.RequestForQuote),
                m.Request.RolePattern(v => v.EmailAddress, m.RequestForQuote),
                m.Request.RolePattern(v => v.TelephoneNumber, m.RequestForQuote),
                m.Request.RolePattern(v => v.ContactPerson, m.RequestForQuote),
                m.Person.RolePattern(v => v.DisplayName, v => v.RequestsWhereContactPerson, m.RequestForQuote),
                m.Request.RolePattern(v => v.Originator, m.RequestForQuote),
                m.Party.RolePattern(v => v.DisplayName, v => v.RequestsWhereOriginator, m.RequestForQuote),
                m.Request.RolePattern(v => v.FullfillContactMechanism, m.RequestForQuote),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.QuotesWhereFullfillContactMechanism, m.RequestForQuote),

                m.RequestItem.RolePattern(v => v.RequestItemState, v => v.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.RequestItem.RolePattern(v => v.Description, v => v.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.RequestItem.RolePattern(v => v.Comment, v => v.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.RequestItem.RolePattern(v => v.InternalComment, v => v.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.RequestItem.RolePattern(v => v.Product, v => v.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.RequestItem.RolePattern(v => v.ProductFeature, v => v.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.RequestItem.RolePattern(v => v.SerialisedItem, v => v.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.RequestItemsWhereSerialisedItem.ObjectType.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.RequestItem.RolePattern(v => v.Requirements, v => v.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.RequestItem.RolePattern(v => v.Deliverable, v => v.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.Deliverable.RolePattern(v => v.Name, v => v.RequestItemsWhereDeliverable.ObjectType.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.RequestItem.RolePattern(v => v.NeededSkill, v => v.RequestWhereRequestItem.ObjectType, m.RequestForQuote),
                m.Skill.RolePattern(v => v.Name, v => v.NeededSkillsWhereSkill.ObjectType.RequestItemsWhereNeededSkill.ObjectType.RequestWhereRequestItem.ObjectType, m.RequestForQuote),

                m.Request.AssociationPattern(v => v.QuoteWhereRequest, m.RequestForQuote),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<RequestForQuote>())
            {
                @this.DeriveRequestForQuoteSearchString(validation);
            }
        }
    }

    public static class RequestForQuoteSearchStringRuleExtensions
    {
        public static void DeriveRequestForQuoteSearchString(this RequestForQuote @this, IValidation validation)
        {
            var array = new string[] {
                    @this.RequestNumber,
                    @this.RequestState?.Name,
                    @this.Recipient?.DisplayName,
                    @this.DerivedCurrency?.Abbreviation,
                    @this.DerivedCurrency?.Name,
                    @this.InternalComment,
                    @this.Description,
                    @this.EmailAddress,
                    @this.TelephoneNumber,
                    @this.ContactPerson?.DisplayName,
                    @this.Originator?.DisplayName,
                    @this.FullfillContactMechanism?.DisplayName,
                    @this.QuoteWhereRequest?.QuoteNumber,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.RequestItemState?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.Description ?? string.Empty).ToArray()) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.Comment ?? string.Empty).ToArray()) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.InternalComment ?? string.Empty).ToArray()) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.Product?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.ProductFeature?.Description ?? string.Empty).ToArray()) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.SerialisedItem?.DisplayName ?? string.Empty).ToArray()) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.SelectMany(v => v.Requirements?.Select(v => v.RequirementNumber ?? string.Empty)).ToArray()) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.Deliverable?.Name ?? string.Empty).ToArray()) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.NeededSkill?.Skill?.Name ?? string.Empty).ToArray()) : null,
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

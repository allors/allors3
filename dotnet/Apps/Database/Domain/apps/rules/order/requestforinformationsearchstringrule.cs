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

    public class RequestForInformationSearchStringRule : Rule
    {
        public RequestForInformationSearchStringRule(MetaPopulation m) : base(m, new Guid("278c8b7b-96d3-48d6-bde0-1442aa466ff5")) =>
            this.Patterns = new Pattern[]
            {
                m.Request.RolePattern(v => v.RequestNumber, m.RequestForInformation),
                m.Request.RolePattern(v => v.RequestState, m.RequestForInformation),
                m.Request.RolePattern(v => v.Recipient, m.RequestForInformation),
                m.Request.RolePattern(v => v.DerivedCurrency, m.RequestForInformation),
                m.Request.RolePattern(v => v.InternalComment, m.RequestForInformation),
                m.Request.RolePattern(v => v.Description, m.RequestForInformation),
                m.Request.RolePattern(v => v.EmailAddress, m.RequestForInformation),
                m.Request.RolePattern(v => v.TelephoneNumber, m.RequestForInformation),
                m.Request.RolePattern(v => v.ContactPerson, m.RequestForInformation),
                m.Person.RolePattern(v => v.DisplayName, v => v.RequestsWhereContactPerson, m.RequestForInformation),
                m.Request.RolePattern(v => v.Originator, m.RequestForInformation),
                m.Party.RolePattern(v => v.DisplayName, v => v.RequestsWhereOriginator, m.RequestForInformation),
                m.Request.RolePattern(v => v.FullfillContactMechanism, m.RequestForInformation),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.QuotesWhereFullfillContactMechanism, m.RequestForInformation),

                m.RequestItem.RolePattern(v => v.RequestItemState, v => v.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.RequestItem.RolePattern(v => v.Description, v => v.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.RequestItem.RolePattern(v => v.Comment, v => v.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.RequestItem.RolePattern(v => v.InternalComment, v => v.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.RequestItem.RolePattern(v => v.Product, v => v.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.RequestItem.RolePattern(v => v.ProductFeature, v => v.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.RequestItem.RolePattern(v => v.SerialisedItem, v => v.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.RequestItemsWhereSerialisedItem.RequestItem.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.RequestItem.RolePattern(v => v.Requirements, v => v.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.RequestItem.RolePattern(v => v.Deliverable, v => v.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.Deliverable.RolePattern(v => v.Name, v => v.RequestItemsWhereDeliverable.RequestItem.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.RequestItem.RolePattern(v => v.NeededSkill, v => v.RequestWhereRequestItem.Request, m.RequestForInformation),
                m.Skill.RolePattern(v => v.Name, v => v.NeededSkillsWhereSkill.NeededSkill.RequestItemsWhereNeededSkill.RequestItem.RequestWhereRequestItem.Request, m.RequestForInformation),

                m.Request.AssociationPattern(v => v.QuoteWhereRequest, m.RequestForInformation),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<RequestForInformation>())
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
            }
        }
    }
}

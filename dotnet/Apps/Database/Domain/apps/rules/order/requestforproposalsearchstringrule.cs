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

    public class RequestForProposalSearchStringRule : Rule
    {
        public RequestForProposalSearchStringRule(MetaPopulation m) : base(m, new Guid("dcb8cc06-30ce-4e32-a09d-e7ca39919800")) =>
            this.Patterns = new Pattern[]
            {
                m.Request.RolePattern(v => v.RequestNumber, m.RequestForProposal),
                m.Request.RolePattern(v => v.RequestState, m.RequestForProposal),
                m.Request.RolePattern(v => v.Recipient, m.RequestForProposal),
                m.Request.RolePattern(v => v.DerivedCurrency, m.RequestForProposal),
                m.Request.RolePattern(v => v.InternalComment, m.RequestForProposal),
                m.Request.RolePattern(v => v.Description, m.RequestForProposal),
                m.Request.RolePattern(v => v.EmailAddress, m.RequestForProposal),
                m.Request.RolePattern(v => v.TelephoneNumber, m.RequestForProposal),
                m.Request.RolePattern(v => v.ContactPerson, m.RequestForProposal),
                m.Person.RolePattern(v => v.DisplayName, v => v.RequestsWhereContactPerson, m.RequestForProposal),
                m.Request.RolePattern(v => v.Originator, m.RequestForProposal),
                m.Party.RolePattern(v => v.DisplayName, v => v.RequestsWhereOriginator, m.RequestForProposal),
                m.Request.RolePattern(v => v.FullfillContactMechanism, m.RequestForProposal),
                m.ContactMechanism.RolePattern(v => v.DisplayName, v => v.QuotesWhereFullfillContactMechanism, m.RequestForProposal),

                m.RequestItem.RolePattern(v => v.RequestItemState, v => v.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.RequestItem.RolePattern(v => v.Description, v => v.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.RequestItem.RolePattern(v => v.Comment, v => v.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.RequestItem.RolePattern(v => v.InternalComment, v => v.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.RequestItem.RolePattern(v => v.Product, v => v.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.RequestItem.RolePattern(v => v.ProductFeature, v => v.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.RequestItem.RolePattern(v => v.SerialisedItem, v => v.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.SerialisedItem.RolePattern(v => v.DisplayName, v => v.RequestItemsWhereSerialisedItem.RequestItem.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.RequestItem.RolePattern(v => v.Requirements, v => v.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.RequestItem.RolePattern(v => v.Deliverable, v => v.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.Deliverable.RolePattern(v => v.Name, v => v.RequestItemsWhereDeliverable.RequestItem.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.RequestItem.RolePattern(v => v.NeededSkill, v => v.RequestWhereRequestItem.Request, m.RequestForProposal),
                m.Skill.RolePattern(v => v.Name, v => v.NeededSkillsWhereSkill.NeededSkill.RequestItemsWhereNeededSkill.RequestItem.RequestWhereRequestItem.Request, m.RequestForProposal),

                m.Request.AssociationPattern(v => v.QuoteWhereRequest, m.RequestForProposal),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<RequestForProposal>())
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
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.RequestItemState?.Name)) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.Description)) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.Comment)) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.InternalComment)) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.Product?.DisplayName)) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.ProductFeature?.Description)) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.SerialisedItem?.DisplayName)) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.Requirements?.Select(v => v.RequirementNumber))) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.Deliverable?.Name)) : null,
                    @this.ExistRequestItems ? string.Join(" ", @this.RequestItems?.Select(v => v.NeededSkill?.Skill?.Name)) : null,
                };

                if (array.Any(s => !string.IsNullOrEmpty(s)))
                {
                    @this.SearchString = string.Join(" ", array.Where(s => !string.IsNullOrEmpty(s)));
                }
            }
        }
    }
}

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

    public class WorkRequirementSearchStringRule : Rule
    {
        public WorkRequirementSearchStringRule(MetaPopulation m) : base(m, new Guid("111314b9-bcab-4ea8-a3e8-4b608a571bb6")) =>
            this.Patterns = new Pattern[]
            {
                m.Requirement.RolePattern(v => v.RequirementState, m.WorkRequirement),
                m.Requirement.RolePattern(v => v.RequirementNumber, m.WorkRequirement),
                m.Requirement.RolePattern(v => v.RequirementType, m.WorkRequirement),
                m.Requirement.RolePattern(v => v.Description, m.WorkRequirement),
                m.Requirement.RolePattern(v => v.Reason, m.WorkRequirement),
                m.Requirement.RolePattern(v => v.Authorizer, m.WorkRequirement),
                m.Party.RolePattern(v => v.DisplayName, v => v.RequirementsWhereAuthorizer.ObjectType, m.WorkRequirement),
                m.Requirement.RolePattern(v => v.NeededFor, m.WorkRequirement),
                m.Party.RolePattern(v => v.DisplayName, v => v.RequirementsWhereNeededFor.ObjectType, m.WorkRequirement),
                m.Requirement.RolePattern(v => v.Originator, m.WorkRequirement),
                m.Party.RolePattern(v => v.DisplayName, v => v.RequirementsWhereOriginator.ObjectType, m.WorkRequirement),
                m.Requirement.RolePattern(v => v.ServicedBy, m.WorkRequirement),
                m.Requirement.RolePattern(v => v.Priority, m.WorkRequirement),

                m.WorkRequirement.RolePattern(v => v.Location),
                m.WorkRequirement.RolePattern(v => v.FixedAsset),
                m.FixedAsset.RolePattern(v => v.DisplayName, v => v.WorkRequirementsWhereFixedAsset.ObjectType),

                m.WorkRequirement.AssociationPattern(v => v.WorkRequirementFulfillmentWhereFullfilledBy),
                m.Requirement.AssociationPattern(v => v.RequirementCommunicationsWhereRequirement, m.WorkRequirement),
                m.Requirement.AssociationPattern(v => v.RequirementBudgetAllocationsWhereRequirement, m.WorkRequirement),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkRequirement>())
            {
                @this.DeriveWorkRequirementSearchString(validation);
            }
        }
    }

    public static class WorkRequirementSearchStringRuleExtensions
    {
        public static void DeriveWorkRequirementSearchString(this WorkRequirement @this, IValidation validation)
        {
            var array = new string[] {
                    @this.RequirementState?.Name,
                    @this.RequirementNumber,
                    @this.RequirementType?.Name,
                    @this.Description,
                    @this.Reason,
                    @this.Authorizer?.DisplayName,
                    @this.NeededFor?.DisplayName,
                    @this.Originator?.DisplayName,
                    @this.ServicedBy?.DisplayName,
                    @this.Priority?.Name,
                    @this.Location,
                    @this.FixedAsset?.DisplayName,
                    @this.WorkRequirementFulfillmentWhereFullfilledBy?.FullfillmentOf.WorkEffortNumber,
                    @this.ExistRequirementCommunicationsWhereRequirement ? string.Join(" ", @this.RequirementCommunicationsWhereRequirement?.SelectMany(v => v.CommunicationEvent?.InvolvedParties?.Select(v => v.DisplayName ?? string.Empty)).ToArray()) : null,
                    @this.ExistRequirementBudgetAllocationsWhereRequirement ? string.Join(" ", @this.RequirementBudgetAllocationsWhereRequirement?.Select(v => v.BudgetItem?.BudgetWhereBudgetItem?.BudgetNumber ?? string.Empty).ToArray()) : null,
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

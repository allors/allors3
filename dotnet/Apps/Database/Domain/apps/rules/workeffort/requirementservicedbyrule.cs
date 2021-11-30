
// <copyright file="RequirementServicedByRule.cs" company="Allors bvba">
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
    using Resources;

    public class RequirementServicedByRule : Rule
    {
        public RequirementServicedByRule(MetaPopulation m) : base(m, new Guid("77a37bb7-a664-4351-b264-271e583a2194")) =>
            this.Patterns = new Pattern[]
        {
            m.Requirement.RolePattern(v => v.ServicedBy),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Requirement>())
            {
                if (@this.ExistCurrentVersion
                    && @this.CurrentVersion.ExistServicedBy
                    && @this.ServicedBy != @this.CurrentVersion.ServicedBy)
                {
                    validation.AddError(@this, this.M.Requirement.ServicedBy, ErrorMessages.InternalOrganisationChanged);
                }

                if (!@this.ExistRequirementNumber && @this.ExistServicedBy)
                {
                    var year = @this.Transaction().Now().Year;
                    @this.RequirementNumber = @this.ServicedBy.NextRequirementNumber(year);

                    var fiscalYearInternalOrganisationSequenceNumbers = @this.ServicedBy.FiscalYearsInternalOrganisationSequenceNumbers.FirstOrDefault(v => v.FiscalYear == year);
                    var prefix = @this.ServicedBy.RequirementSequence.IsEnforcedSequence ? @this.ServicedBy.RequirementNumberPrefix : fiscalYearInternalOrganisationSequenceNumbers.RequirementNumberPrefix;
                    @this.SortableRequirementNumber = @this.Transaction().GetSingleton().SortableNumber(prefix, @this.RequirementNumber, year.ToString());
                }
            }
        }
    }
}

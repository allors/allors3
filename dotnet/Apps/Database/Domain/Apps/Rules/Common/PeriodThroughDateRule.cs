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
    using Resources;

    public class PeriodThroughDateRule : Rule
    {
        public PeriodThroughDateRule(MetaPopulation m) : base(m, new Guid("379fcf77-93fc-46ee-a219-0c95270960ab")) =>
        this.Patterns = new Pattern[]
        {
            m.Period.RolePattern(v => v.ThroughDate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Period>())
            {
                @this.DerivePeriodThroughDate(validation);
            }
        }
    }

    public static class PeriodThroughDateRuleExtensions
    {
        public static void DerivePeriodThroughDate(this Period @this, IValidation validation)
        {
            if (@this.ExistThroughDate && @this.ThroughDate < @this.FromDate)
            {
                {
                    validation.AddError(ErrorMessages.ThroughDateInvalid);
                }
            }
        }
    }
}

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
    using Resources;

    public class EmploymentFromDateRule : Rule
    {
        public EmploymentFromDateRule(MetaPopulation m) : base(m, new Guid("9d557c8a-0df0-42c8-acda-953927bd2591")) =>
        this.Patterns = new Pattern[]
        {
            m.Employment.RolePattern(v => v.FromDate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Employment>())
            {
                @this.DeriveEmploymentFromDate(validation);
            }
        }
    }

    public static class EmploymentFromDateRuleExtensions
    {
        public static void DeriveEmploymentFromDate(this Employment @this, IValidation validation)
        {
            if (@this.ExistEmployee && @this.ExistEmployer)
            {
                if (@this.Employee.EmploymentsWhereEmployee.Except(new[] { @this })
                    .FirstOrDefault(v => v.Employer.Equals(@this.Employer)
                                        && v.FromDate.Date <= @this.FromDate.Date
                                        && (!v.ExistThroughDate || v.ThroughDate.Value.Date >= @this.FromDate.Date))
                    != null)
                {
                    validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
                }

                if (@this.Employee.EmploymentsWhereEmployee.Except(new[] { @this })
                    .FirstOrDefault(v => v.Employer.Equals(@this.Employer)
                                        && @this.FromDate.Date <= v.FromDate.Date
                                        && (!@this.ExistThroughDate || @this.ThroughDate.Value.Date >= v.FromDate.Date))
                    != null)
                {
                    validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
                }
            }
        }
    }
}

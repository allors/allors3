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

    public class IrpfRateFromDateRule : Rule
    {
        public IrpfRateFromDateRule(MetaPopulation m) : base(m, new Guid("a89173d3-e745-4eea-8c41-b57738731671")) =>
        this.Patterns = new Pattern[]
        {
            m.IrpfRate.RolePattern(v => v.FromDate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<IrpfRate>())
            {
                @this.DeriveIrpfRateFromDate(validation);
            }
        }
    }

    public static class IrpfRateFromDateRuleExtensions
    {
        public static void DeriveIrpfRateFromDate(this IrpfRate @this, IValidation validation)
        {
            if (@this.ExistIrpfRegimeWhereIrpfRate)
            {
                if (@this.IrpfRegimeWhereIrpfRate.IrpfRates.Except(new[] { @this })
                    .FirstOrDefault(v => v.FromDate.Date <= @this.FromDate.Date
                                        && (!v.ExistThroughDate || v.ThroughDate.Value.Date >= @this.FromDate.Date))
                    != null)
                {
                    validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
                }

                if (@this.IrpfRegimeWhereIrpfRate.IrpfRates.Except(new[] { @this })
                    .FirstOrDefault(v => @this.FromDate.Date <= v.FromDate.Date
                                        && (!@this.ExistThroughDate || @this.ThroughDate.Value.Date >= v.FromDate.Date))
                    != null)
                {
                    validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
                }
            }
        }
    }
}

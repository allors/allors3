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

    public class VatRateFromDateRule : Rule
    {
        public VatRateFromDateRule(MetaPopulation m) : base(m, new Guid("a1cb7255-27d1-4b07-ab3f-035f303f4a5b")) =>
        this.Patterns = new Pattern[]
        {
            m.VatRate.RolePattern(v => v.FromDate),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<VatRate>())
            {
                @this.DeriveVatRateFromDate(validation);
            }
        }
    }

    public static class VatRateFromDateRuleExtensions
    {
        public static void DeriveVatRateFromDate(this VatRate @this, IValidation validation)
        {
            if (@this.ExistVatRegimeWhereVatRate)
            {
                if (@this.VatRegimeWhereVatRate.VatRates.Except(new[] { @this })
                    .FirstOrDefault(v => v.FromDate.Date <= @this.FromDate.Date
                                        && (!v.ExistThroughDate || v.ThroughDate.Value.Date >= @this.FromDate.Date))
                    != null)
                {
                    validation.AddError(@this, @this.Meta.FromDate, ErrorMessages.PeriodActive);
                }

                if (@this.VatRegimeWhereVatRate.VatRates.Except(new[] { @this })
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

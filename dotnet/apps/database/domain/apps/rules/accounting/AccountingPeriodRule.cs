// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class AccountingPeriodRule : Rule
    {
        public AccountingPeriodRule(MetaPopulation m) : base(m, new Guid("3d328335-a10a-44b7-b001-ac0d98f89c64")) =>
            this.Patterns = new Pattern[]
            {
                m.AccountingPeriod.RolePattern(v => v.FromDate),
                m.AccountingPeriod.RolePattern(v => v.ThroughDate),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<AccountingPeriod>())
            {
                var stringBuilder = new StringBuilder();
                if (@this.ExistFromDate)
                {
                    stringBuilder.AppendFormat("{0:d}", @this.FromDate);
                }

                if (@this.ExistThroughDate)
                {
                    if (stringBuilder.Length > 0)
                    {
                        stringBuilder.Append(" through ");
                    }

                    stringBuilder.AppendFormat("{0:d}", @this.ThroughDate);
                }

                @this.Description = stringBuilder.ToString();
            }
        }
    }
}

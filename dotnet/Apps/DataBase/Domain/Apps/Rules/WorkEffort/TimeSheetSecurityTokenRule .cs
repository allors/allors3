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
    using Derivations.Rules;
    using Meta;

    public class TimeSheetSecurityTokenRule : Rule
    {
        public TimeSheetSecurityTokenRule(MetaPopulation m) : base(m, new Guid("07a5c765-3686-4d25-9cf8-6f99e4910497")) =>
            this.Patterns = new Pattern[]
        {
            m.TimeSheet.RolePattern(v => v.Worker),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<TimeSheet>())
            {
                var defaultSecurityToken = new SecurityTokens(@this.Transaction()).DefaultSecurityToken;
                @this.SecurityTokens = new[] { defaultSecurityToken, @this.Worker.OwnerSecurityToken };
            }
        }
    }
}

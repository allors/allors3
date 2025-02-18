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

    public class AutomatedAgentDisplayNameRule : Rule
    {
        public AutomatedAgentDisplayNameRule(MetaPopulation m) : base(m, new Guid("0735d5f4-62c4-489f-96f4-eadbb4237719")) =>
            this.Patterns = new Pattern[]
            {
                m.AutomatedAgent.RolePattern(v => v.UserName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<AutomatedAgent>())
            {
                @this.DisplayName = @this.UserName ?? "N/A";
            }
        }
    }
}

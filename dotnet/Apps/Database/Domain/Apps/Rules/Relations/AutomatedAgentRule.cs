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

    public class AutomatedAgentRule : Rule
    {
        public AutomatedAgentRule(MetaPopulation m) : base(m, new Guid("98237B56-E163-4FFC-84E7-2BB8E60BBEB8")) =>
            this.Patterns = new Pattern[]
            {
                m.AutomatedAgent.RolePattern(v => v.Name),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<AutomatedAgent>())
            {
                @this.DisplayName = @this.Name;
            }
        }
    }
}

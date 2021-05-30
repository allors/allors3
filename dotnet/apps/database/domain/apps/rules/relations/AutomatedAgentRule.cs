// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Derivations.Rules;

    public class AutomatedAgentRule : Rule
    {
        public AutomatedAgentRule(MetaPopulation m) : base(m, new Guid("98237B56-E163-4FFC-84E7-2BB8E60BBEB8")) =>
            this.Patterns = new Pattern[]
            {
                m.AutomatedAgent.RolePattern(v => v.Name),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<AutomatedAgent>())
            {
                @this.PartyName = @this.Name;
            }
        }
    }
}

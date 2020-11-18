// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class AutomatedAgentDerivation : DomainDerivation
    {
        public AutomatedAgentDerivation(M m) : base(m, new Guid("98237B56-E163-4FFC-84E7-2BB8E60BBEB8")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.AutomatedAgent.Name),
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

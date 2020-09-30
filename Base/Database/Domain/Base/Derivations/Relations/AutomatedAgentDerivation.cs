// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;

    public class AutomatedAgentDerivation : DomainDerivation
    {
        public AutomatedAgentDerivation(M m) : base(m, new Guid("98237B56-E163-4FFC-84E7-2BB8E60BBEB8")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(M.AutomatedAgent.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var automatedAgent in matches.Cast<AutomatedAgent>())
            {
                automatedAgent.PartyName = automatedAgent.Name;
            }
        }
    }
}

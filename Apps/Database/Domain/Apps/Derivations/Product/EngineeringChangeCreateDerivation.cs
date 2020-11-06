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

    public class EngineeringChangeCreateDerivation : DomainDerivation
    {
        public EngineeringChangeCreateDerivation(M m) : base(m, new Guid("7d2404fe-990c-48c4-b370-203087fc3ade")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.EngineeringChange.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<EngineeringChange>())
            {
                @this.AddPreviousObjectState(new EngineeringChangeObjectStates(@this.Strategy.Session).Requested);
                @this.CurrentObjectState ??= new EngineeringChangeObjectStates(@this.Strategy.Session).Noticed;
            }
        }
    }
}

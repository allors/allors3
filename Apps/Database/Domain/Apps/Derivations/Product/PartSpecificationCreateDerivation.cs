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

    public class PartSpecificationCreateDerivation : DomainDerivation
    {
        public PartSpecificationCreateDerivation(M m) : base(m, new Guid("1448521c-1753-4f9c-a5fb-af53794d5ae1")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.PartSpecification.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<PartSpecification>())
            {
                if (!@this.ExistPartSpecificationState)
                {
                    @this.PartSpecificationState = new PartSpecificationStates(@this.Strategy.Session).Created;
                }
            }
        }
    }
}

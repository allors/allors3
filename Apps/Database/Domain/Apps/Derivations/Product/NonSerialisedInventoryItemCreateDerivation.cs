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

    public class NonSerialisedInventoryItemCreateDerivation : DomainDerivation
    {
        public NonSerialisedInventoryItemCreateDerivation(M m) : base(m, new Guid("9b93ef45-7741-44cb-8f2a-863deb92baf7")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.NonSerialisedInventoryItem.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                if (!@this.ExistNonSerialisedInventoryItemState)
                {
                    @this.NonSerialisedInventoryItemState = new NonSerialisedInventoryItemStates(@this.Strategy.Session).Good;
                }
            }
        }
    }
}

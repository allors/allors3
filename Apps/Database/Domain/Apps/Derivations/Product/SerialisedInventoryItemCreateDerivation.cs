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

    public class SerialisedInventoryItemCreateDerivation : DomainDerivation
    {
        public SerialisedInventoryItemCreateDerivation(M m) : base(m, new Guid("3e22e75e-974d-4cd6-8cf1-da13366b52e7")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.SerialisedInventoryItem.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SerialisedInventoryItem>())
            {
                if (!@this.ExistSerialisedInventoryItemState)
                {
                    @this.SerialisedInventoryItemState = new SerialisedInventoryItemStates(@this.Strategy.Session).Good;
                }
            }
        }
    }
}

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

    public class SerialisedItemCreateDerivation : DomainDerivation
    {
        public SerialisedItemCreateDerivation(M m) : base(m, new Guid("31150c93-c109-4048-b723-ea0a67192d28")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(m.SerialisedItem.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var session = cycle.Session;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                if (!@this.ExistSerialisedItemState)
                {
                    @this.SerialisedItemState = new SerialisedItemStates(@this.Strategy.Session).NA;
                }

                if (!@this.ExistItemNumber)
                {
                    @this.ItemNumber = @this.Strategy.Session.GetSingleton().Settings.NextSerialisedItemNumber();
                }

                if (!@this.ExistDerivationTrigger)
                {
                    @this.DerivationTrigger = Guid.NewGuid();
                }
            }
        }
    }
}

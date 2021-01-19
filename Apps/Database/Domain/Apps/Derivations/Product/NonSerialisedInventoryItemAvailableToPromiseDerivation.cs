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
    using Database.Derivations;

    public class NonSerialisedInventoryItemAvailableToPromiseDerivation : DomainDerivation
    {
        public NonSerialisedInventoryItemAvailableToPromiseDerivation(M m) : base(m, new Guid("284d0316-0b9c-43e3-90b5-e0ecf8daa88e")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(m.NonSerialisedInventoryItem.QuantityOnHand),
                new ChangedPattern(m.NonSerialisedInventoryItem.QuantityCommittedOut),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                var availableToPromise = @this.QuantityOnHand - @this.QuantityCommittedOut;

                if (availableToPromise < 0)
                {
                    availableToPromise = 0;
                }

                if (availableToPromise != @this.AvailableToPromise)
                {
                    @this.AvailableToPromise = availableToPromise;
                }
            }
        }
    }
}

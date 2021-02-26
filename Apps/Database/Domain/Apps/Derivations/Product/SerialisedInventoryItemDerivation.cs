// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;

    public class SerialisedInventoryItemDerivation : DomainDerivation
    {
        public SerialisedInventoryItemDerivation(M m) : base(m, new Guid("29B3C9B5-7BB2-4851-A424-F984E7AE348B")) =>
            this.Patterns = new Pattern[]
            {
                new AssociationPattern(m.SerialisedInventoryItem.Part),
                new AssociationPattern(m.SerialisedInventoryItem.Facility),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedInventoryItem>())
            {
                if (!@this.ExistName)
                {
                    @this.Name = $"{@this.Part?.Name} at {@this.Facility?.Name} with state {@this.SerialisedInventoryItemState?.Name}";
                }
            }
        }
    }
}

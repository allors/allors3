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

    public class SerialisedItemSerialisedItemAvailabilityNameRule : Rule
    {
        public SerialisedItemSerialisedItemAvailabilityNameRule(MetaPopulation m) : base(m, new Guid("8c692f21-bfe7-41b7-b9de-ace2ddb45d7e")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.SerialisedItemAvailability),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.SerialisedItemAvailabilityName = @this.ExistSerialisedItemAvailability ? @this.SerialisedItemAvailability.Name : string.Empty;
            }
        }
    }
}

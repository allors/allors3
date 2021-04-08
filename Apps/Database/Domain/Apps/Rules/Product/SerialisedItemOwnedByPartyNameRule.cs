// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Derivations;
    using Meta;
    using Database.Derivations;
    using Resources;

    public class SerialisedItemOwnedByPartyNameRule : Rule
    {
        public SerialisedItemOwnedByPartyNameRule(MetaPopulation m) : base(m, new Guid("ad743eab-992e-4e9b-a6b6-f55ee748317b")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.OwnedBy),
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.OwnedByPartyName = @this.ExistOwnedBy ? @this.OwnedBy.PartyName : string.Empty;
            }
        }
    }
}

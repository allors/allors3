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

    public class SerialisedItemOwnershipByOwnershipNameRule : Rule
    {
        public SerialisedItemOwnershipByOwnershipNameRule(MetaPopulation m) : base(m, new Guid("457d5eb8-43c3-4b41-bd55-323ee83ffeeb")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.Ownership),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.OwnershipByOwnershipName = @this.ExistOwnership ? @this.Ownership.Name : string.Empty;
            }
        }
    }
}

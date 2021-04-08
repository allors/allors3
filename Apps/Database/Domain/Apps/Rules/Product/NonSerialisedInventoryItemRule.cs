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

    public class NonSerialisedInventoryItemRule : Rule
    {
        public NonSerialisedInventoryItemRule(MetaPopulation m) : base(m, new Guid("DDB383AD-3B4C-43BE-8F30-7E3A8D16F6BE")) =>
            this.Patterns = new Pattern[]
            {
                m.NonSerialisedInventoryItem.RolePattern(v => v.Part),
                m.NonSerialisedInventoryItem.RolePattern(v => v.Facility),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                if (!@this.ExistName)
                {
                    @this.Name = $"{@this.Part?.Name} at {@this.Facility?.Name} with state {@this.NonSerialisedInventoryItemState?.Name}";
                }
            }
        }
    }
}

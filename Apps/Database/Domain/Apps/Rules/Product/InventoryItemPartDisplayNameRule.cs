// <copyright file="InventoryItemPartDisplayNameDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Meta;
    using Database.Derivations;

    public class InventoryItemPartDisplayNameRule : Rule
    {
        public InventoryItemPartDisplayNameRule(MetaPopulation m) : base(m, new Guid("9bd924bb-3625-423b-b12d-b90d70f491af")) =>
            this.Patterns = new[]
            {
                m.InventoryItem.RolePattern(v => v.Part),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InventoryItem>())
            {
                if (!@this.ExistPartDisplayName)
                {
                    @this.PartDisplayName = @this.Part.DisplayName;
                }
            }
        }
    }
}

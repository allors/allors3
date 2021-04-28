// <copyright file="InventoryItemSearchStringDerivation.cs" company="Allors bvba">
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

    public class InventoryItemSearchStringRule : Rule
    {
        public InventoryItemSearchStringRule(MetaPopulation m) : base(m, new Guid("5d253bff-4537-4b1b-96e4-74b4a16dbf48")) =>
            this.Patterns = new[]
            {
               m.InventoryItem.RolePattern(v => v.Part),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<InventoryItem>())
            {
                if (!@this.ExistSearchString)
                {
                    @this.SearchString = @this.Part.SearchString;
                }
            }
        }
    }
}

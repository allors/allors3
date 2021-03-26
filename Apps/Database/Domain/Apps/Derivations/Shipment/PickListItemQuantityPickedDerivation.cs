// <copyright file="PickListItemQuantityPickedDerivation.cs" company="Allors bvba">
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

    public class PickListItemQuantityPickedDerivation : DomainDerivation
    {
        public PickListItemQuantityPickedDerivation(M m) : base(m, new Guid("c07f30af-09c4-409a-bfdc-fffcac2082fd")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.PickList, m.PickList.PickListState) { Steps = new IPropertyType[] { m.PickList.PickListItems } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PickListItem>())
            {
                if (@this.ExistPickListWherePickListItem
                    && @this.PickListWherePickListItem.PickListState.IsPicked
                    && @this.QuantityPicked == 0)
                {
                    @this.QuantityPicked = @this.Quantity;
                }
            }
        }
    }
}

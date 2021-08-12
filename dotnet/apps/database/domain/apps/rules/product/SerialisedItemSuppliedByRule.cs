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
    using Derivations.Rules;

    public class SerialisedItemSuppliedByRule : Rule
    {
        public SerialisedItemSuppliedByRule(MetaPopulation m) : base(m, new Guid("444245c3-7e2a-4c94-877d-c7a1490b2c6e")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.SerialNumber),
                m.SerialisedItem.RolePattern(v => v.AssignedSuppliedBy),
                m.SerialisedItem.RolePattern(v => v.PurchaseOrder),
                m.Part.AssociationPattern(v => v.SupplierOfferingsWherePart, v => v.SerialisedItems),
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.SuppliedBy = @this.AssignedSuppliedBy ??
                    @this.PurchaseOrder?.TakenViaSupplier ??
                    @this.PartWhereSerialisedItem?.SupplierOfferingsWherePart?.FirstOrDefault()?.Supplier;
            }
        }
    }
}

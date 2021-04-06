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

    public class SerialisedItemSuppliedByRule : Rule
    {
        public SerialisedItemSuppliedByRule(MetaPopulation m) : base(m, new Guid("444245c3-7e2a-4c94-877d-c7a1490b2c6e")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SerialisedItem, m.SerialisedItem.AssignedSuppliedBy),
                new RolePattern(m.SerialisedItem, m.SerialisedItem.PurchaseOrder),
                new AssociationPattern(m.SupplierOffering.Part) { Steps = new IPropertyType[] { m.Part.SerialisedItems } },
                new AssociationPattern(m.Part.SerialisedItems),
                new RolePattern(m.SerialisedItem, m.SerialisedItem.SerialNumber),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
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

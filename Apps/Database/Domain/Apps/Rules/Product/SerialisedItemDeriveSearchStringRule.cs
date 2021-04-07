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

    public class SerialisedItemDeriveSearchStringRule : Rule
    {
        public SerialisedItemDeriveSearchStringRule(MetaPopulation m) : base(m, new Guid("9d4316ae-3abf-4b6a-839e-1acbcea8995f")) =>
            this.Patterns = new Pattern[]
            {
                new RolePattern(m.SerialisedItem, m.SerialisedItem.Name),
                new RolePattern(m.SerialisedItem, m.SerialisedItem.ItemNumber),
                new RolePattern(m.SerialisedItem, m.SerialisedItem.SerialNumber),
                new RolePattern(m.SerialisedItem, m.SerialisedItem.OwnedBy),
                new RolePattern(m.SerialisedItem, m.SerialisedItem.Buyer),
                new RolePattern(m.SerialisedItem, m.SerialisedItem.Seller),
                new AssociationPattern(m.Part.SerialisedItems),
                new RolePattern(m.Brand, m.Brand.Name) { Steps = new IPropertyType[] { m.Brand.PartsWhereBrand, m.Part.SerialisedItems } },
                new RolePattern(m.Model, m.Model.Name) { Steps = new IPropertyType[] { m.Model.PartsWhereModel, m.Part.SerialisedItems } },
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                var builder = new StringBuilder();

                builder.Append(@this.ItemNumber);
                builder.Append(string.Join(" ", @this.SerialNumber));
                builder.Append(string.Join(" ", @this.Name));

                if (@this.ExistOwnedBy)
                {
                    builder.Append(string.Join(" ", @this.OwnedBy.PartyName));
                }

                if (@this.ExistBuyer)
                {
                    builder.Append(string.Join(" ", @this.Buyer.PartyName));
                }

                if (@this.ExistSeller)
                {
                    builder.Append(string.Join(" ", @this.Seller.PartyName));
                }

                if (@this.ExistPartWhereSerialisedItem)
                {
                    builder.Append(string.Join(" ", @this.PartWhereSerialisedItem?.Brand?.Name));
                    builder.Append(string.Join(" ", @this.PartWhereSerialisedItem?.Model?.Name));
                }

                builder.Append(string.Join(" ", @this.Keywords));
                @this.SearchString = builder.ToString();
            }
        }
    }
}

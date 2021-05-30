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
    using Meta;
    using Derivations.Rules;

    public class SerialisedItemSearchStringRule : Rule
    {
        public SerialisedItemSearchStringRule(MetaPopulation m) : base(m, new Guid("9d4316ae-3abf-4b6a-839e-1acbcea8995f")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.Name),
                m.SerialisedItem.RolePattern(v => v.ItemNumber),
                m.SerialisedItem.RolePattern(v => v.SerialNumber),
                m.SerialisedItem.RolePattern(v => v.OwnedBy),
                m.SerialisedItem.RolePattern(v => v.Buyer),
                m.SerialisedItem.RolePattern(v => v.Seller),
                m.Brand.RolePattern(v => v.Name, v => v.PartsWhereBrand.Part.SerialisedItems),
                m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.Part.SerialisedItems),
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
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

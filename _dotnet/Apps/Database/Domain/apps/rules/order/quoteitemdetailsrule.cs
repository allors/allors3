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
    using Derivations.Rules;
    using System.Text;
    using Database.Derivations;
    using Derivations;

    public class QuoteItemDetailsRule : Rule
    {
        public QuoteItemDetailsRule(MetaPopulation m) : base(m, new Guid("9f1ff793-4ad5-4694-8e15-a918954727f1")) =>
            this.Patterns = new Pattern[]
            {
                m.QuoteItem.RolePattern(v => v.SerialisedItem),
                m.QuoteItem.RolePattern(v => v.Product),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<QuoteItem>())
            {
                if (cycle.ChangeSet.Created.Contains(@this) && !@this.ExistDetails)
                {
                    if (@this.ExistSerialisedItem)
                    {
                        var builder = new StringBuilder();
                        var part = @this.SerialisedItem.PartWhereSerialisedItem;

                        if (part != null && part.ExistManufacturedBy)
                        {
                            builder.Append($", Manufacturer: {part.ManufacturedBy.PartyName}");
                        }

                        if (part != null && part.ExistBrand)
                        {
                            builder.Append($", Brand: {part.Brand.Name}");
                        }

                        if (part != null && part.ExistModel)
                        {
                            builder.Append($", Model: {part.Model.Name}");
                        }

                        builder.Append($", SN: {@this.SerialisedItem.SerialNumber}");

                        var details = builder.ToString();

                        if (details.StartsWith(","))
                        {
                            details = details.Substring(2);
                        }

                        @this.Details = details;

                    }
                    else if (@this.ExistProduct && @this.Product is UnifiedGood unifiedGood)
                    {
                        var builder = new StringBuilder();

                        if (unifiedGood != null && unifiedGood.ExistManufacturedBy)
                        {
                            builder.Append($", Manufacturer: {unifiedGood.ManufacturedBy.PartyName}");
                        }

                        if (unifiedGood != null && unifiedGood.ExistBrand)
                        {
                            builder.Append($", Brand: {unifiedGood.Brand.Name}");
                        }

                        if (unifiedGood != null && unifiedGood.ExistModel)
                        {
                            builder.Append($", Model: {unifiedGood.Model.Name}");
                        }

                        var details = builder.ToString();

                        if (details.StartsWith(","))
                        {
                            details = details.Substring(2);
                        }

                        @this.Details = details;
                    }
                }
            }
        }
    }
}

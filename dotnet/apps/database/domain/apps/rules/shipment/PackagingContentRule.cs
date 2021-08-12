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
    using Resources;

    public class PackagingContentRule : Rule
    {
        public PackagingContentRule(MetaPopulation m) : base(m, new Guid("E6D43FBC-8501-4BEA-83D3-4034657E0D3A")) =>
            this.Patterns = new Pattern[]
        {
            m.PackagingContent.RolePattern(v => v.ShipmentItem),
            m.ShipmentItem.RolePattern(v => v.Quantity, v => v.PackagingContentsWhereShipmentItem),
            m.ShipmentItem.RolePattern(v => v.QuantityShipped, v => v.PackagingContentsWhereShipmentItem),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<PackagingContent>())
            {
                if (@this.ExistQuantity
                    && @this.ExistShipmentItem
                    && @this.ShipmentItem.ExistShipmentWhereShipmentItem
                    && !@this.ShipmentItem.ShipmentWhereShipmentItem.ShipmentState.IsShipped)
                {
                    var maxQuantity = @this.ShipmentItem.Quantity - @this.ShipmentItem.QuantityShipped;
                    if (@this.Quantity == 0 || @this.Quantity > maxQuantity)
                    {
                        validation.AddError(@this, this.M.PackagingContent.Quantity, ErrorMessages.PackagingContentMaximum);
                    }
                }
            }
        }
    }
}

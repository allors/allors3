// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Meta;
    using Resources;

    public class PackagingContentDerivation : DomainDerivation
    {
        public PackagingContentDerivation(M m) : base(m, new Guid("E6D43FBC-8501-4BEA-83D3-4034657E0D3A")) =>
            this.Patterns = new Pattern[]
        {
            new CreatedPattern(M.PackagingContent.Class),
        };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var packagingContent in matches.Cast<PackagingContent>())
            {
                if (packagingContent.ExistQuantity && packagingContent.ExistShipmentItem)
                {
                    var maxQuantity = packagingContent.ShipmentItem.Quantity - packagingContent.ShipmentItem.QuantityShipped;
                    if (packagingContent.Quantity == 0 || packagingContent.Quantity > maxQuantity)
                    {
                        validation.AddError($"{packagingContent} {M.PackagingContent.Quantity} {ErrorMessages.PackagingContentMaximum}");
                    }
                }
            }
        }
    }
}

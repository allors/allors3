// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public class OrderAdjustmentDerivation : DomainDerivation
    {
        public OrderAdjustmentDerivation(M m) : base(m, new Guid("324777D9-18B4-4601-A64E-66C87947A751")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.OrderAdjustment.Interface)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var orderAdjustment in matches.Cast<OrderAdjustment>())
            {
                cycle.Validation.AssertAtLeastOne(orderAdjustment, this.M.OrderAdjustment.Amount, this.M.ShippingAndHandlingCharge.Percentage);
                cycle.Validation.AssertExistsAtMostOne(orderAdjustment, this.M.OrderAdjustment.Amount, this.M.ShippingAndHandlingCharge.Percentage);
            }
        }
    }
}

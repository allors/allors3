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

    public class OrderQuantityBreakDerivation : DomainDerivation
    {
        public OrderQuantityBreakDerivation(M m) : base(m, new Guid("CFEBA3D7-4B3F-4E56-80CA-E84228DAE2E9")) =>
            this.Patterns = new Pattern[]
            {
                new CreatedPattern(this.M.OrderQuantityBreak.Class),
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<OrderQuantityBreak>())
            {
                cycle.Validation.AssertAtLeastOne(@this, this.M.OrderQuantityBreak.FromAmount, this.M.OrderQuantityBreak.ThroughAmount);
            }
        }
    }
}

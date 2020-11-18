// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Domain.Derivations;
    using Allors.Database.Meta;
    using Database.Derivations;

    public class OrderQuantityBreakDerivation : DomainDerivation
    {
        public OrderQuantityBreakDerivation(M m) : base(m, new Guid("CFEBA3D7-4B3F-4E56-80CA-E84228DAE2E9")) =>
            this.Patterns = new Pattern[]
            {
                new ChangedPattern(this.M.OrderQuantityBreak.FromAmount),
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

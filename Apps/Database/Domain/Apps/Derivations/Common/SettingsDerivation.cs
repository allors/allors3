// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public class SettingsDerivation : DomainDerivation
    {
        public SettingsDerivation(M m) : base(m, new Guid("48BC48B2-1614-4E79-984C-E1BFC14C0C22")) =>
            this.Patterns = new[]
            {
                new CreatedPattern(this.M.Settings.Class)
            };

        public override void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Settings>())
            {
                if (!@this.ExistSkuCounter)
                {
                    @this.SkuCounter = new CounterBuilder(@this.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }

                if (!@this.ExistSerialisedItemCounter)
                {
                    @this.SerialisedItemCounter = new CounterBuilder(@this.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }

                if (!@this.ExistProductNumberCounter)
                {
                    @this.ProductNumberCounter = new CounterBuilder(@this.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }

                if (!@this.ExistPartNumberCounter)
                {
                    @this.PartNumberCounter = new CounterBuilder(@this.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }
            }
        }
    }
}

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

    public class SettingsDerivation : IDomainDerivation
    {
        public Guid Id => new Guid("5195dc97-6005-4a2c-b6ae-041f46969d3b");

        public IEnumerable<Pattern> Patterns { get; } = new[] { new CreatedPattern(M.Settings.Class) };

        public void Derive(IDomainDerivationCycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var settings in matches.Cast<Settings>())
            {
                if (!settings.ExistSkuCounter)
                {
                    settings.SkuCounter = new CounterBuilder(settings.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }

                if (!settings.ExistSerialisedItemCounter)
                {
                    settings.SerialisedItemCounter = new CounterBuilder(settings.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }

                if (!settings.ExistProductNumberCounter)
                {
                    settings.ProductNumberCounter = new CounterBuilder(settings.Strategy.Session).WithUniqueId(Guid.NewGuid()).WithValue(0).Build();
                }
            }
        }
    }
}

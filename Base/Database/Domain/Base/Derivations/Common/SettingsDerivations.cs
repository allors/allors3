// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;
    using System.Linq;
    using Allors.Domain.Derivations;
    using Allors.Meta;

    public static partial class DabaseExtensions
    {
        public class SettingsCreationDerivation : IDomainDerivation
        {
            public void Derive(ISession session, IChangeSet changeSet, IDomainValidation validation)
            {
                var createdSettings = changeSet.Created.Select(v=>v.GetObject()).OfType<Settings>();

                foreach(var settings in createdSettings)
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

        public static void SettingsRegisterDerivations(this IDatabase @this)
        {
            @this.DomainDerivationById[new Guid("5195dc97-6005-4a2c-b6ae-041f46969d3b")] = new SettingsCreationDerivation();
        }
    }
}

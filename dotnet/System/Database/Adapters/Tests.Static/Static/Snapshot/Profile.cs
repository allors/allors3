// <copyright file="Profile.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using System.Collections.Generic;
    using Adapters;
    using Meta;
    using C1 = Domain.C1;

    public class Profile : Adapters.Profile
    {
        public override Action[] Markers
        {
            get
            {
                var markers = new List<Action>
                {
                    () => { },
                    () => this.Transaction.Commit(),
                };

                if (Settings.ExtraMarkers)
                {
                    markers.Add(
                        () =>
                        {
                            this.Transaction.Checkpoint();
                        });
                }

                return markers.ToArray();
            }
        }

        public override IDatabase CreateDatabase()
        {
            var metaPopulation = new MetaBuilder().Build();
            var scope = new DefaultDomainDatabaseServices();
            return new Database(scope, new Configuration
            {
                ObjectFactory = new ObjectFactory(metaPopulation, typeof(C1)),
            });
        }
    }
}

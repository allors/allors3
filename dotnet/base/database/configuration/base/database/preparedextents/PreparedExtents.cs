// <copyright file="PreparedExtents.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using Data;
    using Domain;

    public class PreparedExtents : IPreparedExtents
    {
        private readonly ConcurrentDictionary<Guid, IExtent> extentById;

        public PreparedExtents(IDomainDatabaseServices domainDatabaseServices)
        {
            this.DomainDatabaseServices = domainDatabaseServices;
            this.extentById = new ConcurrentDictionary<Guid, IExtent>();
        }

        public IDomainDatabaseServices DomainDatabaseServices { get; }

        public IExtent Get(Guid id)
        {
            if (!this.extentById.TryGetValue(id, out var extent))
            {
                var transaction = this.DomainDatabaseServices.Database.CreateTransaction();
                try
                {
                    var m = transaction.Database.Services().M;

                    var filter = new Extent(m.PersistentPreparedExtent)
                    {
                        Predicate = new Equals(m.PersistentPreparedExtent.UniqueId) { Value = id },
                    };

                    var preparedExtent = (PersistentPreparedExtent)filter.Build(transaction).First;
                    if (preparedExtent != null)
                    {
                        extent = preparedExtent.Extent;
                        this.extentById[id] = extent;
                    }
                }
                finally
                {
                    if (this.DomainDatabaseServices.Database.IsShared)
                    {
                        transaction.Dispose();
                    }
                }
            }

            return extent;
        }
    }
}

// <copyright file="PreparedExtentCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using System.Collections.Concurrent;
    using Data;
    using Domain;

    public class PreparedExtentCache : IPreparedExtentCache
    {
        private readonly ConcurrentDictionary<Guid, IExtent> extentById;

        public PreparedExtentCache(IDatabaseInstance databaseInstance)
        {
            this.DatabaseInstance = databaseInstance;
            this.extentById = new ConcurrentDictionary<Guid, IExtent>();
        }

        public IDatabaseInstance DatabaseInstance { get; }

        public IExtent Get(Guid id)
        {
            if (!this.extentById.TryGetValue(id, out var extent))
            {
                using var session = this.DatabaseInstance.Database.CreateSession();
                var m = session.Database.State().M;

                var filter = new Extent(m.PreparedExtent.Class)
                {
                    Predicate = new Equals(m.PreparedExtent.UniqueId) { Value = id },
                };

                var preparedExtent = (PreparedExtent)filter.Build(session).First;
                if (preparedExtent != null)
                {
                    extent = preparedExtent.Extent;
                    this.extentById[id] = extent;
                }
            }

            return extent;
        }
    }
}

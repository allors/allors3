// <copyright file="PreparedExtentCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.State
{
    using System;
    using System.Collections.Concurrent;
    using Data;
    using Domain;

    public class PreparedExtentCache : IPreparedExtentCache
    {
        private readonly ConcurrentDictionary<Guid, IExtent> extentById;

        public PreparedExtentCache(IDatabaseState databaseState)
        {
            this.DatabaseState = databaseState;
            this.extentById = new ConcurrentDictionary<Guid, IExtent>();
        }

        public IDatabaseState DatabaseState { get; }

        public IExtent Get(Guid id)
        {
            if (!this.extentById.TryGetValue(id, out var extent))
            {
                using var session = this.DatabaseState.Database.CreateSession();
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

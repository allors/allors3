// <copyright file="PersistentPreparedFetches.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Concurrent;
    using Database.Data;
    using Domain;

    public class PreparedExtents : IPreparedExtents
    {
        private readonly ConcurrentDictionary<Guid, IExtent> extentById;

        public PreparedExtents(IDatabaseState databaseState)
        {
            this.DatabaseState = databaseState;
            this.extentById = new ConcurrentDictionary<Guid, IExtent>();
        }

        public IDatabaseState DatabaseState { get; }

        public IExtent Get(Guid id)
        {
            if (!this.extentById.TryGetValue(id, out var extent))
            {
                var session = this.DatabaseState.Database.CreateSession();
                try
                {
                    var m = session.Database.State().M;

                    var filter = new Extent(m.PersistentPreparedExtent.Class)
                    {
                        Predicate = new Equals(m.PersistentPreparedExtent.UniqueId) { Value = id },
                    };

                    var preparedExtent = (PersistentPreparedExtent)filter.Build(session).First;
                    if (preparedExtent != null)
                    {
                        extent = preparedExtent.Extent;
                        this.extentById[id] = extent;
                    }
                }
                finally
                {
                    if (this.DatabaseState.Database.IsShared)
                    {
                        session.Dispose();
                    }
                }
            }

            return extent;
        }
    }
}

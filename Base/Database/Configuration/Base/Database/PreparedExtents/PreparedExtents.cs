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

        public PreparedExtents(IDatabaseContext databaseContext)
        {
            this.DatabaseContext = databaseContext;
            this.extentById = new ConcurrentDictionary<Guid, IExtent>();
        }

        public IDatabaseContext DatabaseContext { get; }

        public IExtent Get(Guid id)
        {
            if (!this.extentById.TryGetValue(id, out var extent))
            {
                var transaction = this.DatabaseContext.Database.CreateTransaction();
                try
                {
                    var m = transaction.Database.Context().M;

                    var filter = new Extent(m.PersistentPreparedExtent.Class)
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
                    if (this.DatabaseContext.Database.IsShared)
                    {
                        transaction.Dispose();
                    }
                }
            }

            return extent;
        }
    }
}

// <copyright file="ExtentService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using System.Collections.Concurrent;
    using Data;
    using Domain;

    public class ExtentService : IExtentService
    {
        private readonly IDatabase database;
        private readonly ConcurrentDictionary<Guid, IExtent> extentById;

        public ExtentService(IDatabase database)
        {
            this.database = database;
            this.extentById = new ConcurrentDictionary<Guid, IExtent>();
        }

        public IExtent Get(Guid id)
        {
            if (!this.extentById.TryGetValue(id, out var extent))
            {
                using var session = this.database.CreateSession();
                var m = session.Database.Scope().M;

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

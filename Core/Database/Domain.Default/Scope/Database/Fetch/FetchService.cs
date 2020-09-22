// <copyright file="FetchService.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Services
{
    using System;
    using System.Collections.Concurrent;

    using Allors.Data;
    using Allors.Domain;

    public class FetchService : IFetchService
    {
        private readonly IDatabase database;

        private readonly ConcurrentDictionary<Guid, Fetch> fetchById;

        public FetchService(IDatabase database)
        {
            this.database = database;
            this.fetchById = new ConcurrentDictionary<Guid, Fetch>();
        }

        public Fetch Get(Guid id)
        {
            if (!this.fetchById.TryGetValue(id, out var fetch))
            {
                using (var session = this.database.CreateSession())
                {
                    var m = ((IDatabaseScope) session.Database.Scope()).M;

                    var filter = new Extent(m.PreparedFetch.Class)
                    {
                        Predicate = new Equals(m.PreparedFetch.UniqueId) { Value = id },
                    };

                    var preparedFetch = (PreparedFetch)filter.Build(session).First;
                    if (preparedFetch != null)
                    {
                        fetch = preparedFetch.Fetch;
                        this.fetchById[id] = fetch;
                    }
                }
            }

            return fetch;
        }
    }
}

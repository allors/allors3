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

    public class PreparedFetches : IPreparedFetches
   {
       private readonly ConcurrentDictionary<Guid, Fetch> fetchById;

        public PreparedFetches(IDatabaseState databaseState)
        {
            this.DatabaseState = databaseState;
            this.fetchById = new ConcurrentDictionary<Guid, Fetch>();
        }

        public IDatabaseState DatabaseState { get; }

        public Fetch Get(Guid id)
        {
            if (!this.fetchById.TryGetValue(id, out var fetch))
            {
                var session = this.DatabaseState.Database.CreateSession();
                try
                {
                    var m = session.Database.State().M;

                    var filter = new Extent(m.PersistentPreparedFetch.Class)
                    {
                        Predicate = new Equals(m.PersistentPreparedFetch.UniqueId) { Value = id },
                    };

                    var preparedFetch = (PersistentPreparedFetch)filter.Build(session).First;
                    if (preparedFetch != null)
                    {
                        fetch = preparedFetch.Fetch;
                        this.fetchById[id] = fetch;
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

            return fetch;
        }
    }
}

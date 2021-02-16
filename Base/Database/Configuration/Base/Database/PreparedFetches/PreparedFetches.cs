// <copyright file="PersistentPreparedFetches.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using Data;
    using Domain;

    public class PreparedFetches : IPreparedFetches
   {
       private readonly ConcurrentDictionary<Guid, Fetch> fetchById;

        public PreparedFetches(IDatabaseContext databaseContext)
        {
            this.DatabaseContext = databaseContext;
            this.fetchById = new ConcurrentDictionary<Guid, Fetch>();
        }

        public IDatabaseContext DatabaseContext { get; }

        public Fetch Get(Guid id)
        {
            if (!this.fetchById.TryGetValue(id, out var fetch))
            {
                var transaction = this.DatabaseContext.Database.CreateTransaction();
                try
                {
                    var m = transaction.Database.Context().M;

                    var filter = new Extent(m.PersistentPreparedFetch.Class)
                    {
                        Predicate = new Equals(m.PersistentPreparedFetch.UniqueId) { Value = id },
                    };

                    var preparedFetch = (PersistentPreparedFetch)filter.Build(transaction).First;
                    if (preparedFetch != null)
                    {
                        fetch = preparedFetch.Fetch;
                        this.fetchById[id] = fetch;
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

            return fetch;
        }
    }
}

// <copyright file="PreparedFetches.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Concurrent;
    using Database.Data;
    using Domain;
    using Meta;

    public class PreparedFetches : IPreparedFetches
   {
       public PreparedFetches(M m)
       {
           this.M = m;
           this.FetchById = new ConcurrentDictionary<Guid, Fetch>();
       }

       public M M { get; }

        public ConcurrentDictionary<Guid, Fetch> FetchById { get; }

        public Fetch Get(Guid id)
        {
            this.FetchById.TryGetValue(id, out var fetch);
            return fetch;
        }
    }
}

// <copyright file="Caches.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Domain;

    public class Caches : ICaches
    {
        private readonly ConcurrentDictionary<string, object> caches;

        public Caches() => this.caches = new ConcurrentDictionary<string, object>();

        public IDictionary<T, long> Get<T>(string key)
        {
            if (this.caches.TryGetValue(key, out var cache))
            {
                return (IDictionary<T, long>)cache;
            }

            return null;
        }

        public void Set<T>(string key, IDictionary<T, long> value) => this.caches[key] = value;
    }
}

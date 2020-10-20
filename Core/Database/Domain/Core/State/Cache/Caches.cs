// <copyright file="Caches.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.State
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public class Caches : ICaches
    {
        private readonly ConcurrentDictionary<string, object> stickies;

        public Caches() => this.stickies = new ConcurrentDictionary<string, object>();

        public IDictionary<T, long> Get<T>(string key)
        {
            if (this.stickies.TryGetValue(key, out var cache))
            {
                return (IDictionary<T, long>)cache;
            }

            return null;
        }

        public void Set<T>(string key, IDictionary<T, long> value) => this.stickies[key] = value;
    }
}

// <copyright file="TransactionExtension.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using Meta;

    public static partial class TransactionExtension
    {
        public static IDictionary<T, long> GetCache<T>(this ITransaction @this, Type type, IRoleType roleType)
        {
            var key = $"{type}.{roleType}";

            var caches = @this.Database.Services.Get<ICaches>();
            var cache = caches.Get<T>(key);
            if (cache == null)
            {
                cache = new ConcurrentDictionary<T, long>();
                caches.Set(key, cache);
            }

            return cache;
        }
    }
}

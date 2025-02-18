// <copyright file="ICaches.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Generic;

    public interface ICaches
    {
        IDictionary<T, long> Get<T>(string key);

        void Set<T>(string key, IDictionary<T, long> value);
    }
}

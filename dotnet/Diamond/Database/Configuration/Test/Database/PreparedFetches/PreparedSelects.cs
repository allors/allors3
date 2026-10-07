// <copyright file="PreparedSelects.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using Data;

    public class PreparedSelects : IPreparedSelects
    {
        public ConcurrentDictionary<Guid, Select> SelectById { get; } = new ConcurrentDictionary<Guid, Select>();

        public Select Get(Guid id)
        {
            this.SelectById.TryGetValue(id, out var @select);
            return @select;
        }
    }
}

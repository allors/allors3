// <copyright file="PreparedSelects.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using Data;
    using Meta;

    public class PreparedSelects : IPreparedSelects
    {
        public PreparedSelects(MetaPopulation m)
        {
            this.M = m;
            this.SelectById = new ConcurrentDictionary<Guid, Select>();
        }

        public MetaPopulation M { get; }

        public ConcurrentDictionary<Guid, Select> SelectById { get; }

        public Select Get(Guid id)
        {
            this.SelectById.TryGetValue(id, out var @select);
            return @select;
        }
    }
}

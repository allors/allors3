// <copyright file="PreparedExtents.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
    using System.Collections.Concurrent;
    using Data;
    using Meta;
    using Services;

    public class PreparedExtents : IPreparedExtents
    {
        public PreparedExtents(MetaPopulation m)
        {
            this.M = m;
            this.ExtentById = new ConcurrentDictionary<Guid, IExtent>();
        }

        public MetaPopulation M { get; }

        public ConcurrentDictionary<Guid, IExtent> ExtentById { get; }

        public IExtent Get(Guid id)
        {
            this.ExtentById.TryGetValue(id, out var extent);
            return extent;
        }
    }
}

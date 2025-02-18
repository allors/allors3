// <copyright file="PersistentPreparedExtents.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{


    public partial class PersistentPreparedExtents
    {
        private UniquelyIdentifiableCache<PersistentPreparedExtent> cache;

        public UniquelyIdentifiableCache<PersistentPreparedExtent> Cache => this.cache ??= new UniquelyIdentifiableCache<PersistentPreparedExtent>(this.Transaction);
    }
}

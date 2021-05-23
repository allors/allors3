// <copyright file="PersistentPreparedExtents.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
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

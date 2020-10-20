// <copyright file="PersistentPreparedExtents.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class PersistentPreparedExtents
    {
        private UniquelyIdentifiableSticky<PersistentPreparedExtent> cache;

        public UniquelyIdentifiableSticky<PersistentPreparedExtent> Cache => this.cache ??= new UniquelyIdentifiableSticky<PersistentPreparedExtent>(this.Session);
    }
}

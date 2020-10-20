// <copyright file="PersistentPreparedFetches.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class PersistentPreparedFetches
    {
        private UniquelyIdentifiableCache<PersistentPreparedFetch> cache;

        public UniquelyIdentifiableCache<PersistentPreparedFetch> Cache => this.cache ??= new UniquelyIdentifiableCache<PersistentPreparedFetch>(this.Session);
    }
}

// <copyright file="Counters.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    public partial class Counters
    {
        private UniquelyIdentifiableCache<Counter> cache;

        private UniquelyIdentifiableCache<Counter> Cache => this.cache ??= new UniquelyIdentifiableCache<Counter>(this.Session);
    }
}
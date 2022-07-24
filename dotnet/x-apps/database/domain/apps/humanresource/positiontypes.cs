// <copyright file="PositionTypes.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class PositionTypes
    {
        private UniquelyIdentifiableCache<PositionType> cache;

        private UniquelyIdentifiableCache<PositionType> Cache => this.cache ??= new UniquelyIdentifiableCache<PositionType>(this.Transaction);
    }
}

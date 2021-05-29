// <copyright file="UniquelyIdentifiableExtension.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public static partial class UniquelyIdentifiableExtensions
    {
        public static void CoreOnBuild(this UniquelyIdentifiable @this, ObjectOnBuild method)
        {
            if (!@this.ExistUniqueId)
            {
                @this.Strategy.SetUnitRole(@this.Transaction().Database.Services().M.UniquelyIdentifiable.UniqueId, Guid.NewGuid());
            }
        }
    }
}

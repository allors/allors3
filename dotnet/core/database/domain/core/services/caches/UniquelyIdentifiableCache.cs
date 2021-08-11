// <copyright file="UniquelyIdentifiableCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using Meta;

    public class UniquelyIdentifiableCache<TObject> : Cache<Guid, TObject>
        where TObject : class, UniquelyIdentifiable
    {
        public UniquelyIdentifiableCache(ITransaction transaction)
            : base(transaction, transaction.Database.Services.Get<MetaPopulation>().UniquelyIdentifiable.UniqueId)
        {
        }
    }
}

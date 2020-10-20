// <copyright file="UniquelyIdentifiableCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Domain
{
    using System;

    using Allors;

    public class UniquelyIdentifiableCache<TObject> : Cache<Guid, TObject>
        where TObject : class, UniquelyIdentifiable
    {
        public UniquelyIdentifiableCache(ISession session)
            : base(session, session.Database.State().M.UniquelyIdentifiable.UniqueId)
        {
        }
    }
}

// <copyright file="ICaches.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Collections.Concurrent;
    using Meta;

    public class ClassById : IClassById
    {
        private readonly ConcurrentDictionary<long, IClass> classById;

        public ClassById() => this.classById = new ConcurrentDictionary<long, IClass>();

        public IClass Get(long id)
        {
            this.classById.TryGetValue(id, out var @class);
            return @class;
        }

        public void Set(long id, IClass @class) => this.classById[id] = @class;
    }
}

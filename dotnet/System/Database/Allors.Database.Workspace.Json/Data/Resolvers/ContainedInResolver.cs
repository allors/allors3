// <copyright file="FromJson.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;

    public class ContainedInResolver : IResolver
    {
        private readonly ContainedIn containedIn;
        private readonly long[] objectIds;

        public ContainedInResolver(ContainedIn containedIn, long[] objectIds)
        {
            this.containedIn = containedIn;
            this.objectIds = objectIds;
        }

        public void Prepare(HashSet<long> objectIds) => objectIds.UnionWith(this.objectIds);

        public void Resolve(Dictionary<long, IObject> objectById) => this.containedIn.Objects = this.objectIds.Select(v => objectById[v]).ToArray();
    }
}

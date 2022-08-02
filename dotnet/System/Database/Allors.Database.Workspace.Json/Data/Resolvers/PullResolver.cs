// <copyright file="FromJson.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using Data;

    public class PullResolver : IResolver
    {
        private readonly Pull pull;
        private readonly long objectId;

        public PullResolver(Pull pull, long objectId)
        {
            this.pull = pull;
            this.objectId = objectId;
        }

        public void Prepare(HashSet<long> objectIds) => objectIds.Add(this.objectId);

        public void Resolve(Dictionary<long, IObject> objectById) => this.pull.Object = objectById[this.objectId];
    }
}

// <copyright file="FromJson.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using Data;

    public class EqualsResolver : IResolver
    {
        private readonly Equals @equals;
        private readonly long objectId;

        public EqualsResolver(Equals equals, long objectId)
        {
            this.@equals = @equals;
            this.objectId = objectId;
        }

        public void Prepare(HashSet<long> objectIds) => objectIds.Add(this.objectId);

        public void Resolve(Dictionary<long, IObject> objectById) => this.@equals.Object = objectById[this.objectId];
    }
}

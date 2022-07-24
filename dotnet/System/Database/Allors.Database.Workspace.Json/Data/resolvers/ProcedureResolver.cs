// <copyright file="FromJson.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Data;

    public class ProcedureResolver : IResolver
    {
        private readonly Procedure procedure;
        private readonly IDictionary<string, long[]> collections;
        private readonly IDictionary<string, long> objects;
        private readonly long[][] pool;

        public ProcedureResolver(Procedure procedure, IDictionary<string, long[]> collections, IDictionary<string, long> objects, long[][] pool)
        {
            this.procedure = procedure;
            this.collections = collections;
            this.objects = objects;
            this.pool = pool;
        }

        public void Prepare(HashSet<long> objectIds)
        {
            if (this.collections != null)
            {
                objectIds.UnionWith(this.collections.Values.SelectMany(v => v));
            }

            if (this.objects != null)
            {
                objectIds.UnionWith(this.objects.Values);
            }

            if (this.pool != null)
            {
                objectIds.UnionWith(this.pool.Select(v => v[0]));
            }
        }

        public void Resolve(Dictionary<long, IObject> objectById)
        {
            this.procedure.Collections = this.collections?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Select(v => objectById[v]).ToArray());
            this.procedure.Objects = this.objects?.ToDictionary(kvp => kvp.Key, kvp => objectById[kvp.Value]);
            this.procedure.Pool = this.pool?.ToDictionary(v => objectById[v[0]], v => v[1]);
        }
    }
}

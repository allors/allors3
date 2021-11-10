// <copyright file="FromJson.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json;
    using Data;
    using Meta;

    public class FromJson
    {
        private readonly ITransaction transaction;
        private readonly IList<IResolver> resolvers;

        public IMetaPopulation MetaPopulation { get; }

        public IUnitConvert UnitConvert { get; }

        public FromJson(ITransaction transaction, IUnitConvert unitConvert)
        {
            this.transaction = transaction;
            this.MetaPopulation = this.transaction.Database.MetaPopulation;
            this.UnitConvert = unitConvert;

            this.resolvers = new List<IResolver>();
        }

        public void Resolve(Contains contains, long objectId) => this.resolvers.Add(new ContainsResolver(contains, objectId));

        public void Resolve(ContainedIn containedIn, long[] objectIds) => this.resolvers.Add(new ContainedInResolver(containedIn, objectIds));

        public void Resolve(Equals equals, long objectId) => this.resolvers.Add(new EqualsResolver(equals, objectId));

        public void Resolve(Pull pull, long objectId) => this.resolvers.Add(new PullResolver(pull, objectId));

        public void Resolve(Procedure procedure, IDictionary<string, long[]> collections, IDictionary<string, long> objects, long[][] pool) => this.resolvers.Add(new ProcedureResolver(procedure, collections, objects, pool));

        public void Resolve()
        {
            var objectIds = new HashSet<long>();
            foreach (var resolver in this.resolvers)
            {
                resolver.Prepare(objectIds);
            }

            var objectById = this.transaction.Instantiate(objectIds).ToDictionary(v => v.Id);

            foreach (var resolver in this.resolvers)
            {
                resolver.Resolve(objectById);
            }
        }
    }
}

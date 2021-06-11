// <copyright file="PullExtent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Invoke;
    using Allors.Protocol.Json.Api.Pull;
    using Allors.Protocol.Json.Api.Push;
    using Allors.Protocol.Json.Api.Security;
    using Allors.Protocol.Json.Api.Sync;
    using Allors.Protocol.Json.SystemTextJson;
    using Data;
    using Derivations;
    using Domain;
    using Meta;
    using User = Domain.User;

    public class Api
    {
        public Api(ITransaction transaction, string workspaceName)
        {
            this.Transaction = transaction;

            var transactionContext = transaction.Services();
            var databaseContext = transaction.Database.Services();
            var metaCache = databaseContext.MetaCache;

            this.User = transactionContext.User;
            this.AccessControlLists = new WorkspaceAccessControlLists(workspaceName, this.User);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(workspaceName);
            this.M = databaseContext.M;
            this.MetaPopulation = databaseContext.M;
            this.PreparedSelects = databaseContext.PreparedSelects;
            this.PreparedExtents = databaseContext.PreparedExtents;
            this.Build = @class => (IObject)DefaultObjectBuilder.Build(transaction, @class);
            this.Derive = () => this.Transaction.Derive(false);

            this.UnitConvert = new UnitConvert();
        }

        public ITransaction Transaction { get; }

        public User User { get; }

        public WorkspaceAccessControlLists AccessControlLists { get; }

        public ISet<IClass> AllowedClasses { get; }

        public MetaPopulation M { get; }

        public MetaPopulation MetaPopulation { get; }

        public IPreparedSelects PreparedSelects { get; }

        public IPreparedExtents PreparedExtents { get; }

        public Func<IClass, IObject> Build { get; }

        public Func<IValidation> Derive { get; }

        public UnitConvert UnitConvert { get; }

        public InvokeResponse Invoke(InvokeRequest invokeRequest)
        {
            var invokeResponseBuilder = new InvokeResponseBuilder(this.Transaction, this.Derive, this.AccessControlLists, this.AllowedClasses);
            return invokeResponseBuilder.Build(invokeRequest);
        }

        public PullResponse Pull(PullRequest pullRequest)
        {
            var response = new PullResponseBuilder(this.Transaction, this.AccessControlLists, this.AllowedClasses, this.PreparedSelects, this.PreparedExtents, this.UnitConvert);
            return response.Build(pullRequest);
        }

        public PushResponse Push(PushRequest pushRequest)
        {
            var responseBuilder = new PushResponseBuilder(this.Transaction, this.Derive, this.MetaPopulation, this.AccessControlLists, this.AllowedClasses, this.Build, this.UnitConvert);
            return responseBuilder.Build(pushRequest);
        }

        public SyncResponse Sync(SyncRequest syncRequest)
        {
            void Prefetch(IEnumerable<IObject> objects)
            {
                // Prefetch
                foreach (var groupBy in objects.GroupBy(v => v.Strategy.Class, v => v))
                {
                    var prefetchClass = groupBy.Key;
                    var prefetchObjects = groupBy;

                    var prefetchPolicyBuilder = new PrefetchPolicyBuilder();
                    prefetchPolicyBuilder.WithWorkspaceRules(prefetchClass);
                    prefetchPolicyBuilder.WithSecurityRules((Class)prefetchClass, this.M);

                    var prefetcher = prefetchPolicyBuilder.Build();

                    this.Transaction.Prefetch(prefetcher, prefetchObjects);
                }
            }

            var responseBuilder = new SyncResponseBuilder(this.Transaction, this.AccessControlLists, this.AllowedClasses, Prefetch, this.UnitConvert);
            return responseBuilder.Build(syncRequest);
        }

        public SecurityResponse Security(SecurityRequest securityRequest)
        {
            var responseBuilder = new SecurityResponseBuilder(this.Transaction, this.AccessControlLists, this.AllowedClasses);
            return responseBuilder.Build(securityRequest);
        }

        // TODO: Delete
        public PullResponseBuilder CreatePullResponseBuilder()
        {
            var response = new PullResponseBuilder(this.Transaction, this.AccessControlLists, this.AllowedClasses, this.PreparedSelects, this.PreparedExtents, this.UnitConvert);
            return response;
        }
    }
}

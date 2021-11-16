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
    using Ranges;
    using Services;
    using Tracing;
    using User = Domain.User;

    public class Api
    {
        public Api(ITransaction transaction, string workspaceName)
        {
            this.Transaction = transaction;
            this.Sink = transaction.Database.Sink;

            var transactionServices = transaction.Services;
            var databaseServices = transaction.Database.Services;
            var metaCache = databaseServices.Get<IMetaCache>();

            this.Ranges = databaseServices.Get<IRanges<long>>();
            this.User = transactionServices.Get<IUserService>().User;
            this.AccessControl = new WorkspaceAccessControl(workspaceName, this.User);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(workspaceName);
            this.M = databaseServices.Get<MetaPopulation>();
            this.MetaPopulation = this.M;
            this.PreparedSelects = databaseServices.Get<IPreparedSelects>();
            this.PreparedExtents = databaseServices.Get<IPreparedExtents>();
            this.Build = @class => DefaultObjectBuilder.Build(transaction, @class);
            this.Derive = () => this.Transaction.Derive(false);

            this.UnitConvert = new UnitConvert();
        }

        public ITransaction Transaction { get; }

        public ISink Sink { get; }

        public IRanges<long> Ranges { get; }

        public User User { get; }

        public WorkspaceAccessControl AccessControl { get; }

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
            using var @event = this.Sink?.OnInvoke(this.Transaction, invokeRequest);

            var invokeResponseBuilder = new InvokeResponseBuilder(this.Transaction, this.Derive, this.AccessControl, this.AllowedClasses);
            return invokeResponseBuilder.Build(invokeRequest);
        }

        public PullResponse Pull(PullRequest pullRequest)
        {
            using var @event = this.Sink?.OnPull(this.Transaction, pullRequest);

            var pullResponsePrefetcher = new PullResponsePrefetcher(this.Transaction, this.M);
            var dependencies = this.ToDependencies(pullRequest.d);
            var response = new PullResponseBuilder(this.Transaction, this.AccessControl, this.AllowedClasses, this.PreparedSelects, this.PreparedExtents, this.UnitConvert, this.Ranges, dependencies, pullResponsePrefetcher);
            return response.Build(pullRequest);
        }

        public PushResponse Push(PushRequest pushRequest)
        {
            using var @event = this.Sink?.OnPush(this.Transaction, pushRequest);

            var responseBuilder = new PushResponseBuilder(this.Transaction, this.Derive, this.MetaPopulation, this.AccessControl, this.AllowedClasses, this.Build, this.UnitConvert);
            return responseBuilder.Build(pushRequest);
        }

        public SyncResponse Sync(SyncRequest syncRequest)
        {
            using var @event = this.Sink?.OnSync(this.Transaction, syncRequest);

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

            var responseBuilder = new SyncResponseBuilder(this.Transaction, this.AccessControl, this.AllowedClasses, Prefetch, this.UnitConvert, this.Ranges);
            return responseBuilder.Build(syncRequest);
        }

        public AccessResponse Access(AccessRequest accessRequest)
        {
            var responseBuilder = new AccessResponseBuilder(this.Transaction, this.AccessControl);
            return responseBuilder.Build(accessRequest);
        }

        public PermissionResponse Permission(PermissionRequest permissionRequest)
        {
            var responseBuilder = new PermissionResponseBuilder(this.Transaction, this.AllowedClasses);
            return responseBuilder.Build(permissionRequest);
        }

        // TODO: Delete
        public PullResponseBuilder CreatePullResponseBuilder(string dependencyId = null)
        {

            // TODO: Dependencies
            var pullResponsePrefetcher = new PullResponsePrefetcher(this.Transaction, this.M);
            return new PullResponseBuilder(this.Transaction, this.AccessControl, this.AllowedClasses, this.PreparedSelects, this.PreparedExtents, this.UnitConvert, this.Ranges, null, pullResponsePrefetcher);
        }

        private IDictionary<IClass, ISet<IPropertyType>> ToDependencies(PullDependency[] pullDependencies)
        {
            if (pullDependencies == null)
            {
                return null;
            }

            var classDependencies = new Dictionary<IClass, ISet<IPropertyType>>();

            foreach (var pullDependency in pullDependencies)
            {
                var objectType = (IComposite)this.M.FindByTag(pullDependency.o);
                IPropertyType propertyType;
                if (pullDependency.a != null)
                {
                    propertyType = ((IRelationType)this.M.FindByTag(pullDependency.a)).AssociationType;
                }
                else
                {
                    propertyType = ((IRelationType)this.M.FindByTag(pullDependency.r)).RoleType;
                }

                foreach (var @class in objectType.Classes)
                {
                    if (!classDependencies.TryGetValue(@class, out var classDependency))
                    {
                        classDependency = new HashSet<IPropertyType>();
                        classDependencies.Add(@class, classDependency);
                    }

                    classDependency.Add(propertyType);
                }
            }

            return classDependencies;
        }
    }
}

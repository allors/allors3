// <copyright file="PullExtent.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the ISessionExtension type.</summary>

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
    using Data;
    using Derivations;
    using Domain;
    using Meta;
    
    public class Api
    {
        public Api(ISession session, string workspaceName)
        {
            this.Session = session;

            var sessionContext = session.Context();
            var databaseContext = session.Database.Context();
            var metaCache = databaseContext.MetaCache;

            this.User = sessionContext.User;
            this.AccessControlLists = new WorkspaceAccessControlLists(workspaceName, this.User);
            this.AllowedClasses = metaCache.GetWorkspaceClasses(workspaceName);
            this.M = databaseContext.M;
            this.MetaPopulation = databaseContext.MetaPopulation;
            this.PreparedFetches = databaseContext.PreparedFetches;
            this.PreparedExtents = databaseContext.PreparedExtents;
            this.Build = @class => (IObject)DefaultObjectBuilder.Build(session, @class);
            this.Derive = () => this.Session.Derive(false);
        }

        public ISession Session { get; }

        public User User { get; }

        public WorkspaceAccessControlLists AccessControlLists { get; }

        public ISet<IClass> AllowedClasses { get; }

        public M M { get; }

        public MetaPopulation MetaPopulation { get; }

        public IPreparedFetches PreparedFetches { get; }

        public IPreparedExtents PreparedExtents { get; }

        public Func<IClass, IObject> Build { get; }

        public Func<IDerivationResult> Derive { get; }

        public PullResponseBuilder CreatePullResponseBuilder() => new PullResponseBuilder(this.Session, this.AccessControlLists, this.AllowedClasses, this.PreparedFetches, this.PreparedExtents);

        public InvokeResponse Invoke(InvokeRequest invokeRequest)
        {
            var invokeResponseBuilder = new InvokeResponseBuilder(this.Session, this.Derive, this.AccessControlLists, this.AllowedClasses);
            return invokeResponseBuilder.Build(invokeRequest);
        }

        public PullResponse Pull(PullRequest pullRequest)
        {
            var response = this.CreatePullResponseBuilder();
            return response.Build(pullRequest);
        }

        public PushResponse Push(PushRequest pushRequest)
        {
            var responseBuilder = new PushResponseBuilder(this.Session, this.Derive, this.MetaPopulation, this.AccessControlLists, this.AllowedClasses, this.Build);
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

                    this.Session.Prefetch(prefetcher, prefetchObjects);
                }
            }

            var responseBuilder = new SyncResponseBuilder(this.Session, this.AccessControlLists, this.AllowedClasses, Prefetch);
            return responseBuilder.Build(syncRequest);
        }

        public SecurityResponse Security(SecurityRequest securityRequest)
        {
            var responseBuilder = new SecurityResponseBuilder(this.Session, this.AccessControlLists, this.AllowedClasses);
            return responseBuilder.Build(securityRequest);
        }
    }
}

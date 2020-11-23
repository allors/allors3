// <copyright file="Session.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Protocol.Direct.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database;
    using Data;
    using Database.Data;
    using Database.Derivations;
    using Database.Domain;
    using Database.Meta;
    using Pull;

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
            this.ClassIdById = databaseContext.ClassById;
            this.VersionedIdByStrategy = databaseContext.VersionedIdByStrategy;
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

        public IClassById ClassIdById { get; }

        public IVersionedIdByStrategy VersionedIdByStrategy { get; }
        
        public IPreparedFetches PreparedFetches { get; }

        public IPreparedExtents PreparedExtents { get; }

        public Func<IClass, IObject> Build { get; }

        public Func<IDerivationResult> Derive { get; }

        public PullResponseBuilder CreatePullResponseBuilder() => new PullResponseBuilder(this);

        public PullResponse Pull(PullRequest request)
        {
            var builder = this.CreatePullResponseBuilder();

            var pulls = request.Pulls.Select(v =>
            {
                var visitor = new ToDatabaseVisitor(this.Session);
                v.Accept(visitor);
                return visitor.Pull;
            });

            foreach (var pull in pulls)
            {
                builder.With(pull);
            }

            return builder.Build();
        }
    }
}

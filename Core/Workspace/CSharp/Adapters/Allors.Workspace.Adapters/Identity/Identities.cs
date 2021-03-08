// <copyright file="Id.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections.Generic;

    public sealed class Identities
    {
        private readonly object identityByIdLock = new object();

        private readonly IDictionary<long, Identity> identityById;

        private long sessionAndWorkspaceIdCounter;

        public Identities()
        {
            this.identityById = new Dictionary<long, Identity>();
            this.sessionAndWorkspaceIdCounter = 0;
        }

        public Identity Get(long id)
        {
            lock (this.identityByIdLock)
            {
                this.identityById.TryGetValue(id, out var identity);
                return identity;
            }
        }

        public Identity GetOrCreate(long id)
        {
            lock (this.identityByIdLock)
            {
                if (this.identityById.TryGetValue(id, out var identity))
                {
                    return identity;
                }

                if (id <= 0)
                {
                    return null;
                }

                identity = new DatabaseIdentity { DatabaseId = id };
                this.identityById.Add(id, identity);
                return identity;
            }
        }

        public Identity GetAndUpdate(long workspaceId, long databaseId)
        {
            lock (this.identityByIdLock)
            {
                if (!this.identityById.TryGetValue(workspaceId, out var identity))
                {
                    throw new ArgumentException("workspaceId not present");
                }

                var databaseIdentity = (DatabaseIdentity)identity;
                databaseIdentity.DatabaseId = databaseId;
                this.identityById.Add(databaseId, identity);

                return identity;
            }
        }

        public DatabaseIdentity NextDatabaseIdentity()
        {
            lock (this.identityByIdLock)
            {
                var workspaceId = this.NextWorkspaceId();
                var identity = new DatabaseIdentity { WorkspaceId = workspaceId };
                this.identityById.Add(workspaceId, identity);
                return identity;
            }
        }

        public WorkspaceIdentity NextWorkspaceIdentity()
        {
            lock (this.identityByIdLock)
            {
                var workspaceId = this.NextWorkspaceId();
                var identity = new WorkspaceIdentity(--this.sessionAndWorkspaceIdCounter);
                this.identityById.Add(workspaceId, identity);
                return identity;
            }
        }

        public SessionIdentity NextSessionIdentity()
        {
            lock (this.identityByIdLock)
            {
                var workspaceId = this.NextWorkspaceId();
                var identity = new SessionIdentity(--this.sessionAndWorkspaceIdCounter);
                this.identityById.Add(workspaceId, identity);
                return identity;
            }
        }

        private long NextWorkspaceId() => --this.sessionAndWorkspaceIdCounter;
    }
}

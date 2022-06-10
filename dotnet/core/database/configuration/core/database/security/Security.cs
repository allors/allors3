// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Security;
    using Domain;
    using Meta;
    using Ranges;
    using Services;
    using Grant = Domain.Grant;
    using Revocation = Domain.Revocation;

    public class Security : ISecurity
    {
        private readonly ConcurrentDictionary<long, IVersionedPermissions> databasePermissionsById;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, IVersionedPermissions>> permissionsByIdByWorkspace;

        private readonly IRanges<long> ranges;
        private readonly IMetaCache metaCache;

        private readonly PrefetchPolicy grantPrefetchPolicy;
        private readonly PrefetchPolicy revocationPrefetchPolicy;
        private readonly Dictionary<string, HashSet<long>> permissionIdsByWorkspaceName;

        public Security(DatabaseServices databaseServices)
        {
            var database = databaseServices.Database;
            var m = database.Services.Get<MetaPopulation>();

            this.ranges = databaseServices.Get<IRanges<long>>();
            this.metaCache = databaseServices.Get<IMetaCache>();

            this.databasePermissionsById = new ConcurrentDictionary<long, IVersionedPermissions>();
            this.permissionsByIdByWorkspace = new ConcurrentDictionary<string, ConcurrentDictionary<long, IVersionedPermissions>>();

            this.grantPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.Grant.EffectivePermissions)
                .Build();
            this.revocationPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.Revocation.DeniedPermissions)
                .Build();

            this.permissionIdsByWorkspaceName = m.WorkspaceNames
                .ToDictionary(v => v, v => new HashSet<long>(this.metaCache.GetWorkspaceClasses(v).SelectMany(w =>
                {
                    var @class = (Class)w;
                    var permissionIds = new HashSet<long>();
                    permissionIds.Add(@class.CreatePermissionId);

                    foreach (var relationType in @class.DatabaseRoleTypes.Select(v => v.RelationType).Where(w => w.WorkspaceNames.Contains(v)))
                    {
                        permissionIds.Add(@class.ReadPermissionIdByRelationTypeId[relationType.Id]);
                        permissionIds.Add(@class.WritePermissionIdByRelationTypeId[relationType.Id]);
                    }

                    foreach (var methodType in @class.MethodTypes.Where(w => w.WorkspaceNames.Contains(v)))
                    {
                        permissionIds.Add(@class.ExecutePermissionIdByMethodTypeId[methodType.Id]);
                    }

                    return permissionIds;
                })));
        }

        public IDictionary<IGrant, IVersionedPermissions> GetGrantPermissions(ITransaction transaction, IEnumerable<IGrant> grants)
        {
            ISet<IGrant> missingGrants = null;
            IDictionary<IGrant, IVersionedPermissions> permissionsByGrant = new Dictionary<IGrant, IVersionedPermissions>();

            foreach (var grant in grants)
            {
                if (this.databasePermissionsById.TryGetValue(grant.Strategy.ObjectId, out var permissions) && permissions.Version == grant.Strategy.ObjectVersion)
                {
                    permissionsByGrant[grant] = permissions;
                }
                else
                {
                    missingGrants ??= new HashSet<IGrant>();
                    missingGrants.Add(grant);
                }
            }

            if (missingGrants != null)
            {
                transaction.Prefetch(this.grantPrefetchPolicy, missingGrants.Select(v => v.Strategy));

                foreach (var grant in missingGrants)
                {
                    var permissions = new VersionedPermissions(this.ranges, grant.Strategy.ObjectVersion, ((Grant)grant).EffectivePermissions);
                    this.databasePermissionsById[grant.Strategy.ObjectId] = permissions;
                    permissionsByGrant[grant] = permissions;
                }
            }

            return permissionsByGrant;
        }

        public IDictionary<IGrant, IVersionedPermissions> GetGrantPermissions(ITransaction transaction, IEnumerable<IGrant> grants, string workspaceName)
        {
            if (!this.permissionsByIdByWorkspace.TryGetValue(workspaceName, out var permissionsById))
            {
                permissionsById = new ConcurrentDictionary<long, IVersionedPermissions>();
                this.permissionsByIdByWorkspace[workspaceName] = permissionsById;
            }

            ISet<IGrant> missingGrants = null;
            IDictionary<IGrant, IVersionedPermissions> permissionsByGrant = new Dictionary<IGrant, IVersionedPermissions>();

            foreach (var grant in grants)
            {
                if (permissionsById.TryGetValue(grant.Strategy.ObjectId, out var permissions) && permissions.Version == grant.Strategy.ObjectVersion)
                {
                    permissionsByGrant[grant] = permissions;
                }
                else
                {
                    missingGrants ??= new HashSet<IGrant>();
                    missingGrants.Add(grant);
                }
            }

            if (missingGrants != null)
            {
                transaction.Prefetch(this.grantPrefetchPolicy, missingGrants.Select(v => v.Strategy));

                var workspacePermissionIds = this.permissionIdsByWorkspaceName[workspaceName];

                foreach (var grant in missingGrants)
                {
                    var permissions = new VersionedPermissions(this.ranges, grant.Strategy.ObjectVersion, ((Grant)grant).EffectivePermissions.Where(v => workspacePermissionIds.Contains(v.Id)));
                    permissionsById[grant.Strategy.ObjectId] = permissions;
                    permissionsByGrant[grant] = permissions;
                }
            }

            return permissionsByGrant;
        }

        public IDictionary<IRevocation, IVersionedPermissions> GetRevocationPermissions(ITransaction transaction, IEnumerable<IRevocation> revocations)
        {
            ISet<IRevocation> missingRevocations = null;
            IDictionary<IRevocation, IVersionedPermissions> permissionsByRevocation = new Dictionary<IRevocation, IVersionedPermissions>();

            foreach (var revocation in revocations)
            {
                if (this.databasePermissionsById.TryGetValue(revocation.Strategy.ObjectId, out var permissions) && permissions.Version == revocation.Strategy.ObjectVersion)
                {
                    permissionsByRevocation[revocation] = permissions;
                }
                else
                {
                    missingRevocations ??= new HashSet<IRevocation>();
                    missingRevocations.Add(revocation);
                }
            }

            if (missingRevocations != null)
            {
                transaction.Prefetch(this.revocationPrefetchPolicy, missingRevocations.Select(v => v.Strategy));

                foreach (var revocation in missingRevocations)
                {
                    var permissions = new VersionedPermissions(this.ranges, revocation.Strategy.ObjectVersion, ((Revocation)revocation).DeniedPermissions);
                    this.databasePermissionsById[revocation.Strategy.ObjectId] = permissions;
                    permissionsByRevocation[revocation] = permissions;
                }
            }

            return permissionsByRevocation;
        }

        public IDictionary<IRevocation, IVersionedPermissions> GetRevocationPermissions(ITransaction transaction, IEnumerable<IRevocation> revocations, string workspaceName)
        {
            if (!this.permissionsByIdByWorkspace.TryGetValue(workspaceName, out var permissionsById))
            {
                permissionsById = new ConcurrentDictionary<long, IVersionedPermissions>();
                this.permissionsByIdByWorkspace[workspaceName] = permissionsById;
            }

            ISet<IRevocation> missingRevocations = null;
            IDictionary<IRevocation, IVersionedPermissions> permissionsByRevocation = new Dictionary<IRevocation, IVersionedPermissions>();

            foreach (var revocation in revocations)
            {
                if (permissionsById.TryGetValue(revocation.Strategy.ObjectId, out var permissions) && permissions.Version == revocation.Strategy.ObjectVersion)
                {
                    permissionsByRevocation[revocation] = permissions;
                }
                else
                {
                    missingRevocations ??= new HashSet<IRevocation>();
                    missingRevocations.Add(revocation);
                }
            }

            if (missingRevocations != null)
            {
                transaction.Prefetch(this.revocationPrefetchPolicy, missingRevocations.Select(v => v.Strategy));

                var x = this.permissionsByIdByWorkspace[workspaceName];

                foreach (var revocation in missingRevocations)
                {
                    var workspacePermissionIds = x[revocation.Strategy.ObjectId];

                    var permissions = new VersionedPermissions(this.ranges, revocation.Strategy.ObjectVersion, ((Revocation)revocation).DeniedPermissions.Where(v => workspacePermissionIds.Set.Contains(v.Id)));
                    permissionsById[revocation.Strategy.ObjectId] = permissions;
                    permissionsByRevocation[revocation] = permissions;
                }
            }

            return permissionsByRevocation;
        }
    }
}

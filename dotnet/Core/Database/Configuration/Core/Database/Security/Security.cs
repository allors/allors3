// <copyright file="IBarcodeGenerator.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using System.Linq;
    using Database.Security;
    using Domain;
    using Ranges;
    using Services;
    using Class = Meta.Class;
    using MetaPopulation = Meta.MetaPopulation;
    using Revocation = Domain.Revocation;

    public class Security : ISecurity
    {
        private readonly Dictionary<string, HashSet<long>> permissionIdsByWorkspaceName;

        public Security(DatabaseServices databaseServices)
        {
            var database = databaseServices.Database;
            var m = database.Services.Get<MetaPopulation>();
            var metaCache = databaseServices.Get<IMetaCache>();

            this.Ranges = databaseServices.Get<IRanges<long>>();

            this.SecurityTokenPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.SecurityToken.Grants)
                .Build();

            this.GrantPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.Grant.EffectiveUsers)
                .WithRule(m.Grant.EffectivePermissions)
                .Build();

            this.RevocationPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.Revocation.DeniedPermissions)
                .Build();

            this.permissionIdsByWorkspaceName = m.WorkspaceNames
                .ToDictionary(v => v, v => new HashSet<long>(metaCache.GetWorkspaceClasses(v).SelectMany(w =>
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

        public PrefetchPolicy SecurityTokenPrefetchPolicy { get; }

        public PrefetchPolicy GrantPrefetchPolicy { get; }

        public PrefetchPolicy RevocationPrefetchPolicy { get; }

        public IRanges<long> Ranges { get; }

        // Uncommitted roles retain their committed object version. Recompute derived
        // security bodies in the caller's transaction instead of sharing them by version.
        public IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, ISecurityToken[] securityTokens)
        {
            var result = new Dictionary<long, IVersionedGrant>();
            var grantIds = this.GetGrantIds(transaction, securityTokens).Distinct().ToArray();

            transaction.Prefetch(this.GrantPrefetchPolicy, grantIds);
            var grants = transaction.Instantiate(grantIds).Cast<Grant>();

            foreach (var grant in grants)
            {
                var versionedGrant = new VersionedGrant(this.Ranges, grant.Id, grant.Strategy.ObjectVersion, new HashSet<long>(grant.EffectiveUsers.Select(v => v.Id)), grant.EffectivePermissions.Select(v => v.Id));
                if (user != null && versionedGrant.UserSet.Contains(user.Id))
                {
                    result.Add(versionedGrant.Id, versionedGrant);
                }
            }

            return result.Values.ToArray();
        }

        public IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, ISecurityToken[] securityTokens, string workspaceName) =>
            this.GetVersionedGrants(transaction, user, this.GetGrantIds(transaction, securityTokens), workspaceName);

        public IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, IGrant[] grants, string workspaceName) => this.GetVersionedGrants(transaction, user, grants.Select(v => v.Id), workspaceName);

        public IVersionedRevocation[] GetVersionedRevocations(ITransaction transaction, IUser user, IRevocation[] revocations)
        {
            var result = new Dictionary<long, IVersionedRevocation>();
            transaction.Prefetch(this.RevocationPrefetchPolicy, revocations);

            foreach (var revocation in revocations)
            {
                var versionedRevocation = new VersionedRevocation(this.Ranges, revocation.Id, revocation.Strategy.ObjectVersion, ((Revocation)revocation).DeniedPermissions.Select(v => v.Id));
                result.Add(versionedRevocation.Id, versionedRevocation);
            }

            return result.Values.ToArray();
        }

        public IVersionedRevocation[] GetVersionedRevocations(ITransaction transaction, IUser user, IRevocation[] revocations, string workspaceName)
        {
            var result = new Dictionary<long, IVersionedRevocation>();

            if (revocations.Length > 0)
            {
                transaction.Prefetch(this.RevocationPrefetchPolicy, revocations.Select(v => v.Strategy));
                var workspacePermissionIds = this.permissionIdsByWorkspaceName[workspaceName];

                foreach (var revocation in revocations)
                {
                    var versionedRevocation = new VersionedRevocation(this.Ranges, revocation.Id, revocation.Strategy.ObjectVersion, ((Revocation)revocation).DeniedPermissions.Select(v => v.Id).Where(v => workspacePermissionIds.Contains(v)));
                    result.Add(versionedRevocation.Id, versionedRevocation);
                }
            }

            return result.Values.ToArray();
        }

        private IEnumerable<long> GetGrantIds(ITransaction transaction, ISecurityToken[] securityTokens)
        {
            transaction.Prefetch(this.SecurityTokenPrefetchPolicy, securityTokens);
            return securityTokens.SelectMany(v => v.Grants).Select(v => v.Id);
        }

        private IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, IEnumerable<long> grantIds, string workspaceName)
        {
            var result = new List<IVersionedGrant>();
            var ids = grantIds.ToArray();

            if (ids.Length > 0)
            {
                transaction.Prefetch(this.GrantPrefetchPolicy, ids);
                var grants = transaction.Instantiate(ids).Cast<Grant>();
                var workspacePermissionIds = this.permissionIdsByWorkspaceName[workspaceName];

                foreach (var grant in grants)
                {
                    var versionedGrant = new VersionedGrant(this.Ranges, grant.Id, grant.Strategy.ObjectVersion, new HashSet<long>(grant.EffectiveUsers.Select(v => v.Id)), grant.EffectivePermissions.Where(v => workspacePermissionIds.Contains(v.Id)).Select(v => v.Id));
                    if (user != null && versionedGrant.UserSet.Contains(user.Id))
                    {
                        result.Add(versionedGrant);
                    }
                }
            }

            return result.ToArray();
        }
    }
}

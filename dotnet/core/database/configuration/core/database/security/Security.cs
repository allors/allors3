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
    using Ranges;
    using Services;
    using Class = Meta.Class;
    using MetaPopulation = Meta.MetaPopulation;
    using Revocation = Domain.Revocation;

    public class Security : ISecurity
    {
        private readonly ConcurrentDictionary<long, IVersionedSecurityToken> databaseVersionedSecurityTokenById;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, IVersionedSecurityToken>> versionedSecurityTokenByIdByWorkspace;

        private readonly ConcurrentDictionary<long, IVersionedGrant> databaseVersionedGrantById;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, IVersionedGrant>> versionedGrantByIdByWorkspace;

        private readonly ConcurrentDictionary<long, IVersionedRevocation> databaseVersionedRevocationById;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<long, IVersionedRevocation>> versionedRevocationByIdByWorkspace;

        private readonly IRanges<long> ranges;

        private readonly PrefetchPolicy securityTokenPrefetchPolicy;
        private readonly PrefetchPolicy grantPrefetchPolicy;
        private readonly PrefetchPolicy revocationPrefetchPolicy;

        private readonly Dictionary<string, HashSet<long>> permissionIdsByWorkspaceName;

        public Security(DatabaseServices databaseServices)
        {
            var database = databaseServices.Database;
            var m = database.Services.Get<MetaPopulation>();
            var metaCache = databaseServices.Get<IMetaCache>();

            this.ranges = databaseServices.Get<IRanges<long>>();

            this.databaseVersionedSecurityTokenById = new ConcurrentDictionary<long, IVersionedSecurityToken>();
            this.versionedSecurityTokenByIdByWorkspace = new ConcurrentDictionary<string, ConcurrentDictionary<long, IVersionedSecurityToken>>();

            this.databaseVersionedGrantById = new ConcurrentDictionary<long, IVersionedGrant>();
            this.versionedGrantByIdByWorkspace = new ConcurrentDictionary<string, ConcurrentDictionary<long, IVersionedGrant>>();

            this.databaseVersionedRevocationById = new ConcurrentDictionary<long, IVersionedRevocation>();
            this.versionedRevocationByIdByWorkspace = new ConcurrentDictionary<string, ConcurrentDictionary<long, IVersionedRevocation>>();

            this.securityTokenPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.SecurityToken.Grants)
                .Build();

            this.grantPrefetchPolicy = new PrefetchPolicyBuilder()
                .WithRule(m.Grant.EffectiveUsers)
                .WithRule(m.Grant.EffectivePermissions)
                .Build();

            this.revocationPrefetchPolicy = new PrefetchPolicyBuilder()
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

        public IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, ISecurityToken[] securityTokens)
        {
            var result = new List<IVersionedGrant>();

            var versionedSecurityTokens = this.GetVersionedSecurityTokens(transaction, user, securityTokens);

            IList<long> missingIds = null;
            foreach (var kvp in versionedSecurityTokens.SelectMany(v => v.VersionByGrant))
            {
                var grantId = kvp.Key;
                var grantVersion = kvp.Value;

                if (this.databaseVersionedGrantById.TryGetValue(grantId, out var versionedGrant) && versionedGrant.Version == grantVersion)
                {
                    if (versionedGrant.UserSet.Contains(user.Id))
                    {
                        result.Add(versionedGrant);
                    }
                }
                else
                {
                    missingIds ??= new List<long>();
                    missingIds.Add(grantId);
                }
            }

            if (missingIds != null)
            {
                transaction.Prefetch(this.grantPrefetchPolicy, missingIds);
                var missing = transaction.Instantiate(missingIds).Cast<Grant>();

                foreach (var grant in missing)
                {
                    var versionedGrant = new VersionedGrant(this.ranges, grant.Id, grant.Strategy.ObjectVersion, new HashSet<long>(grant.EffectiveUsers.Select(v => v.Id)), grant.EffectivePermissions.Select(v => v.Id));
                    this.databaseVersionedGrantById[grant.Id] = versionedGrant;
                    if (versionedGrant.UserSet.Contains(user.Id))
                    {
                        result.Add(versionedGrant);
                    }
                }
            }

            return result.ToArray();
        }

        public IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, ISecurityToken[] securityTokens, string workspaceName)
        {
            var result = new List<IVersionedGrant>();

            var versionedSecurityTokens = this.GetVersionedSecurityTokens(transaction, user, securityTokens, workspaceName);

            if (!this.versionedGrantByIdByWorkspace.TryGetValue(workspaceName, out var versionedGrantById))
            {
                versionedGrantById = new ConcurrentDictionary<long, IVersionedGrant>();
                this.versionedGrantByIdByWorkspace[workspaceName] = versionedGrantById;
            }

            IList<long> missingIds = null;
            foreach (var kvp in versionedSecurityTokens.SelectMany(v => v.VersionByGrant))
            {
                var grantId = kvp.Key;
                var grantVersion = kvp.Value;

                if (versionedGrantById.TryGetValue(grantId, out var versionedGrant) && versionedGrant.Version == grantVersion)
                {
                    if (versionedGrant.UserSet.Contains(user.Id))
                    {
                        result.Add(versionedGrant);
                    }
                }
                else
                {
                    missingIds ??= new List<long>();
                    missingIds.Add(grantId);
                }
            }

            if (missingIds != null)
            {
                transaction.Prefetch(this.grantPrefetchPolicy, missingIds);
                var missing = transaction.Instantiate(missingIds).Cast<Grant>();

                var workspacePermissionIds = this.permissionIdsByWorkspaceName[workspaceName];

                foreach (var grant in missing)
                {
                    var versionedGrant = new VersionedGrant(this.ranges, grant.Id, grant.Strategy.ObjectVersion, new HashSet<long>(grant.EffectiveUsers.Select(v => v.Id)), grant.EffectivePermissions.Where(v => workspacePermissionIds.Contains(v.Id)).Select(v => v.Id));
                    versionedGrantById[grant.Id] = versionedGrant;
                    if (versionedGrant.UserSet.Contains(user.Id))
                    {
                        result.Add(versionedGrant);
                    }
                }
            }

            return result.ToArray();
        }

        public IVersionedRevocation[] GetVersionedRevocations(ITransaction transaction, IUser user, IRevocation[] revocations)
        {
            var result = new List<IVersionedRevocation>(revocations.Length);

            IList<IRevocation> missing = null;
            foreach (var revocation in revocations)
            {
                if (this.databaseVersionedRevocationById.TryGetValue(revocation.Strategy.ObjectId, out var versionedRevocation) && versionedRevocation.Version == revocation.Strategy.ObjectVersion)
                {
                    result.Add(versionedRevocation);
                }
                else
                {
                    missing ??= new List<IRevocation>(revocations.Length);
                    missing.Add(revocation);
                }
            }

            if (missing != null)
            {
                transaction.Prefetch(this.revocationPrefetchPolicy, missing);

                foreach (var revocation in missing)
                {
                    var versionedRevocation = new VersionedRevocation(this.ranges, revocation.Strategy.ObjectId, revocation.Strategy.ObjectVersion, ((Revocation)revocation).DeniedPermissions.Select(v => v.Id));
                    this.databaseVersionedRevocationById[revocation.Strategy.ObjectId] = versionedRevocation;
                    result.Add(versionedRevocation);
                }
            }

            return result.ToArray();
        }

        public IVersionedRevocation[] GetVersionedRevocations(ITransaction transaction, IUser user, IRevocation[] revocations, string workspaceName)
        {
            var result = new List<IVersionedRevocation>(revocations.Length);

            if (!this.versionedRevocationByIdByWorkspace.TryGetValue(workspaceName, out var versionedRevocationById))
            {
                versionedRevocationById = new ConcurrentDictionary<long, IVersionedRevocation>();
                this.versionedRevocationByIdByWorkspace[workspaceName] = versionedRevocationById;
            }

            IList<IRevocation> missing = null;
            foreach (var revocation in revocations)
            {
                if (versionedRevocationById.TryGetValue(revocation.Strategy.ObjectId, out var versionedRevocation) && versionedRevocation.Version == revocation.Strategy.ObjectVersion)
                {
                    result.Add(versionedRevocation);
                }
                else
                {
                    missing ??= new List<IRevocation>(revocations.Length);
                    missing.Add(revocation);
                }
            }

            if (missing != null)
            {
                transaction.Prefetch(this.revocationPrefetchPolicy, missing.Select(v => v.Strategy));

                var workspacePermissionIds = this.permissionIdsByWorkspaceName[workspaceName];

                foreach (var revocation in missing)
                {
                    var versionedRevocation = new VersionedRevocation(this.ranges, revocation.Id, revocation.Strategy.ObjectVersion, ((Revocation)revocation).DeniedPermissions.Select(v => v.Id).Where(v => workspacePermissionIds.Contains(v)));
                    versionedRevocationById[revocation.Strategy.ObjectId] = versionedRevocation;
                    result.Add(versionedRevocation);
                }
            }

            return result.ToArray();
        }

        private IList<IVersionedSecurityToken> GetVersionedSecurityTokens(ITransaction transaction, IUser user, ISecurityToken[] securityTokens)
        {
            var versionedSecurityTokens = new List<IVersionedSecurityToken>(securityTokens.Length);

            IList<ISecurityToken> missing = null;
            foreach (var securityToken in securityTokens)
            {
                if (this.databaseVersionedSecurityTokenById.TryGetValue(securityToken.Strategy.ObjectId, out var versionedSecurityToken) && versionedSecurityToken.Version == securityToken.Strategy.ObjectVersion)
                {
                    versionedSecurityTokens.Add(versionedSecurityToken);
                }
                else
                {
                    missing ??= new List<ISecurityToken>(securityTokens.Length);
                    missing.Add(securityToken);
                }
            }

            if (missing != null)
            {
                transaction.Prefetch(this.securityTokenPrefetchPolicy, missing);

                foreach (var securityToken in missing)
                {
                    var versionedSecurityToken = new VersionedSecurityToken(this.ranges, securityToken.Id, securityToken.Strategy.ObjectVersion, securityToken.Grants.ToDictionary(v => v.Id, v => v.Strategy.ObjectVersion));
                    this.databaseVersionedSecurityTokenById[securityToken.Id] = versionedSecurityToken;
                    versionedSecurityTokens.Add(versionedSecurityToken);
                }
            }

            return versionedSecurityTokens;
        }

        private IList<IVersionedSecurityToken> GetVersionedSecurityTokens(ITransaction transaction, IUser user, ISecurityToken[] securityTokens, string workspaceName)
        {
            var result = new List<IVersionedSecurityToken>(securityTokens.Length);

            if (!this.versionedSecurityTokenByIdByWorkspace.TryGetValue(workspaceName, out var versionedSecurityTokenById))
            {
                versionedSecurityTokenById = new ConcurrentDictionary<long, IVersionedSecurityToken>();
                this.versionedSecurityTokenByIdByWorkspace[workspaceName] = versionedSecurityTokenById;
            }

            IList<ISecurityToken> missing = null;
            foreach (var securityToken in securityTokens)
            {
                if (versionedSecurityTokenById.TryGetValue(securityToken.Strategy.ObjectId, out var versionedSecurityToken) && versionedSecurityToken.Version == securityToken.Strategy.ObjectVersion)
                {
                    result.Add(versionedSecurityToken);
                }
                else
                {
                    missing ??= new List<ISecurityToken>(securityTokens.Length);
                    missing.Add(securityToken);
                }
            }

            if (missing != null)
            {
                transaction.Prefetch(this.securityTokenPrefetchPolicy, missing);

                foreach (var securityToken in missing)
                {
                    var versionedSecurityToken = new VersionedSecurityToken(this.ranges, securityToken.Id, securityToken.Strategy.ObjectVersion, securityToken.Grants.ToDictionary(v => v.Id, v => v.Strategy.ObjectVersion));
                    versionedSecurityTokenById[securityToken.Strategy.ObjectId] = versionedSecurityToken;
                    result.Add(versionedSecurityToken);
                }
            }

            return result;
        }
    }
}

// <copyright file="IBarcodeGenerator.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System;
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
        private readonly ConcurrentDictionary<long, IVersionedSecurityToken> databaseVersionedSecurityTokens;
        private readonly ConcurrentDictionary<long, IVersionedGrant> databaseVersionedGrants;
        private readonly ConcurrentDictionary<long, IVersionedRevocation> databaseVersionedRevocations;

        private readonly ConcurrentDictionaryByWorkspace<IVersionedSecurityToken> versionedSecurityTokensByWorkspace;
        private readonly ConcurrentDictionaryByWorkspace<IVersionedGrant> versionedGrantsByWorkspace;
        private readonly ConcurrentDictionaryByWorkspace<IVersionedRevocation> versionedRevocationsByWorkspace;

        private readonly Dictionary<string, HashSet<long>> permissionIdsByWorkspaceName;

        public Security(DatabaseServices databaseServices)
        {
            var database = databaseServices.Database;
            var m = database.Services.Get<MetaPopulation>();
            var metaCache = databaseServices.Get<IMetaCache>();

            this.Ranges = databaseServices.Get<IRanges<long>>();

            this.databaseVersionedSecurityTokens = new ConcurrentDictionary<long, IVersionedSecurityToken>();
            this.databaseVersionedGrants = new ConcurrentDictionary<long, IVersionedGrant>();
            this.databaseVersionedRevocations = new ConcurrentDictionary<long, IVersionedRevocation>();

            this.versionedSecurityTokensByWorkspace = new ConcurrentDictionaryByWorkspace<IVersionedSecurityToken>();
            this.versionedGrantsByWorkspace = new ConcurrentDictionaryByWorkspace<IVersionedGrant>();
            this.versionedRevocationsByWorkspace = new ConcurrentDictionaryByWorkspace<IVersionedRevocation>();

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

        public IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, ISecurityToken[] securityTokens)
        {
            var result = new Dictionary<long, IVersionedGrant>();

            var versionedSecurityTokens = this.GetVersionedSecurityTokens(transaction, securityTokens);

            ISet<long> missingGrantIds = null;
            foreach (var kvp in versionedSecurityTokens.SelectMany(v => v.VersionByGrant))
            {
                var grantId = kvp.Key;
                var grantVersion = kvp.Value;

                if (result.ContainsKey(grantId) || missingGrantIds?.Contains(grantId) == true)
                {
                    continue;
                }

                if (this.databaseVersionedGrants.TryGetValue(grantId, out var versionedGrant) && versionedGrant.Version == grantVersion)
                {
                    if (versionedGrant.UserSet.Contains(user.Id))
                    {
                        result.Add(versionedGrant.Id, versionedGrant);
                    }
                }
                else
                {
                    missingGrantIds ??= new HashSet<long>();
                    missingGrantIds.Add(grantId);
                }
            }

            if (missingGrantIds != null)
            {
                transaction.Prefetch(this.GrantPrefetchPolicy, missingGrantIds);
                var missing = transaction.Instantiate(missingGrantIds).Cast<Grant>();

                foreach (var grant in missing)
                {
                    var versionedGrant = new VersionedGrant(this.Ranges, grant.Id, grant.Strategy.ObjectVersion, new HashSet<long>(grant.EffectiveUsers.Select(v => v.Id)), grant.EffectivePermissions.Select(v => v.Id));
                    this.databaseVersionedGrants[grant.Id] = versionedGrant;
                    if (versionedGrant.UserSet.Contains(user.Id))
                    {
                        result.Add(versionedGrant.Id, versionedGrant);
                    }
                }
            }

            return result.Values.ToArray();
        }

        public IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, ISecurityToken[] securityTokens, string workspaceName)
        {
            var versionedSecurityTokens = this.GetVersionedSecurityTokens(transaction, user, securityTokens, workspaceName);
            return this.GetVersionedGrants(transaction, user, versionedSecurityTokens.SelectMany(v => v.VersionByGrant), workspaceName);
        }

        public IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, IGrant[] grants, string workspaceName) => this.GetVersionedGrants(transaction, user, grants.Select(v => new KeyValuePair<long, long>(v.Id, v.Strategy.ObjectVersion)), workspaceName);

        public IVersionedRevocation[] GetVersionedRevocations(ITransaction transaction, IUser user, IRevocation[] revocations)
        {
            var result = new Dictionary<long, IVersionedRevocation>();

            IList<IRevocation> missing = null;
            foreach (var revocation in revocations)
            {
                if (this.databaseVersionedRevocations.TryGetValue(revocation.Strategy.ObjectId, out var versionedRevocation) && versionedRevocation.Version == revocation.Strategy.ObjectVersion)
                {
                    result.Add(versionedRevocation.Id, versionedRevocation);
                }
                else
                {
                    missing ??= new List<IRevocation>(revocations.Length);
                    missing.Add(revocation);
                }
            }

            if (missing != null)
            {
                transaction.Prefetch(this.RevocationPrefetchPolicy, missing);

                foreach (var revocation in missing)
                {
                    var versionedRevocation = new VersionedRevocation(this.Ranges, revocation.Strategy.ObjectId, revocation.Strategy.ObjectVersion, ((Revocation)revocation).DeniedPermissions.Select(v => v.Id));
                    this.databaseVersionedRevocations[revocation.Strategy.ObjectId] = versionedRevocation;
                    result.Add(versionedRevocation.Id, versionedRevocation);
                }
            }

            return result.Values.ToArray();
        }

        public IVersionedRevocation[] GetVersionedRevocations(ITransaction transaction, IUser user, IRevocation[] revocations, string workspaceName)
        {
            var result = new Dictionary<long, IVersionedRevocation>();

            var versionedRevocations = this.versionedRevocationsByWorkspace[workspaceName];

            IList<IRevocation> missing = null;
            foreach (var revocation in revocations)
            {
                if (versionedRevocations.TryGetValue(revocation.Strategy.ObjectId, out var versionedRevocation) && versionedRevocation.Version == revocation.Strategy.ObjectVersion)
                {
                    result.Add(versionedRevocation.Id, versionedRevocation);
                }
                else
                {
                    missing ??= new List<IRevocation>(revocations.Length);
                    missing.Add(revocation);
                }
            }

            if (missing != null)
            {
                transaction.Prefetch(this.RevocationPrefetchPolicy, missing.Select(v => v.Strategy));

                var workspacePermissionIds = this.permissionIdsByWorkspaceName[workspaceName];

                foreach (var revocation in missing)
                {
                    var versionedRevocation = new VersionedRevocation(this.Ranges, revocation.Id, revocation.Strategy.ObjectVersion, ((Revocation)revocation).DeniedPermissions.Select(v => v.Id).Where(v => workspacePermissionIds.Contains(v)));
                    versionedRevocations[revocation.Strategy.ObjectId] = versionedRevocation;
                    result.Add(versionedRevocation.Id, versionedRevocation);
                }
            }

            return result.Values.ToArray();
        }

        private IList<IVersionedSecurityToken> GetVersionedSecurityTokens(ITransaction transaction, ISecurityToken[] securityTokens)
        {
            var versionedSecurityTokens = new List<IVersionedSecurityToken>(securityTokens.Length);

            IList<ISecurityToken> missing = null;
            foreach (var securityToken in securityTokens)
            {
                if (this.databaseVersionedSecurityTokens.TryGetValue(securityToken.Strategy.ObjectId, out var versionedSecurityToken) && versionedSecurityToken.Version == securityToken.Strategy.ObjectVersion)
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
                transaction.Prefetch(this.SecurityTokenPrefetchPolicy, missing);

                foreach (var securityToken in missing)
                {
                    var versionedSecurityToken = new VersionedSecurityToken(this.Ranges, securityToken.Id, securityToken.Strategy.ObjectVersion, securityToken.Grants.ToDictionary(v => v.Id, v => v.Strategy.ObjectVersion));
                    this.databaseVersionedSecurityTokens[securityToken.Id] = versionedSecurityToken;
                    versionedSecurityTokens.Add(versionedSecurityToken);
                }
            }

            return versionedSecurityTokens;
        }

        private IList<IVersionedSecurityToken> GetVersionedSecurityTokens(ITransaction transaction, IUser user, ISecurityToken[] securityTokens, string workspaceName)
        {
            var result = new List<IVersionedSecurityToken>(securityTokens.Length);

            var versionedSecurityTokens = this.versionedSecurityTokensByWorkspace[workspaceName];

            IList<ISecurityToken> missing = null;
            foreach (var securityToken in securityTokens)
            {
                if (versionedSecurityTokens.TryGetValue(securityToken.Strategy.ObjectId, out var versionedSecurityToken) && versionedSecurityToken.Version == securityToken.Strategy.ObjectVersion)
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
                transaction.Prefetch(this.SecurityTokenPrefetchPolicy, missing);

                foreach (var securityToken in missing)
                {
                    var versionedSecurityToken = new VersionedSecurityToken(this.Ranges, securityToken.Id, securityToken.Strategy.ObjectVersion, securityToken.Grants.ToDictionary(v => v.Id, v => v.Strategy.ObjectVersion));
                    versionedSecurityTokens[securityToken.Strategy.ObjectId] = versionedSecurityToken;
                    result.Add(versionedSecurityToken);
                }
            }

            return result;
        }

        private IVersionedGrant[] GetVersionedGrants(ITransaction transaction, IUser user, IEnumerable<KeyValuePair<long, long>> grants, string workspaceName)
        {
            var result = new List<IVersionedGrant>();

            var versionedGrantById = this.versionedGrantsByWorkspace[workspaceName];

            IList<long> missingIds = null;
            foreach (var kvp in grants)
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
                transaction.Prefetch(this.GrantPrefetchPolicy, missingIds);
                var missing = transaction.Instantiate(missingIds).Cast<Grant>();

                var workspacePermissionIds = this.permissionIdsByWorkspaceName[workspaceName];

                foreach (var grant in missing)
                {
                    var versionedGrant = new VersionedGrant(this.Ranges, grant.Id, grant.Strategy.ObjectVersion, new HashSet<long>(grant.EffectiveUsers.Select(v => v.Id)), grant.EffectivePermissions.Where(v => workspacePermissionIds.Contains(v.Id)).Select(v => v.Id));
                    versionedGrantById[grant.Id] = versionedGrant;
                    if (versionedGrant.UserSet.Contains(user.Id))
                    {
                        result.Add(versionedGrant);
                    }
                }
            }

            return result.ToArray();
        }
    }
}

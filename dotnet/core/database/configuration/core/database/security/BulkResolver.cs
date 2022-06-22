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

    public class BulkResolver
    {
        private readonly Security security;
        private readonly Dictionary<IObject, IAccessControlList> cache;
        private readonly Func<IObject, IVersionedGrant[], IVersionedRevocation[], IAccessControlList> create;
        private readonly ConcurrentDictionary<long, IVersionedSecurityToken> cachedSecurityTokens;
        private readonly ConcurrentDictionary<long, IVersionedGrant> cachedGrants;
        private readonly ConcurrentDictionary<long, IVersionedRevocation> cachedRevocations;
        private readonly HashSet<long> allowedPermissionIds;

        private IList<Domain.Object> missingObjects;

        private IDictionary<long, IVersionedSecurityToken> versionedSecurityTokens;
        private IDictionary<long, IVersionedGrant> versionedGrants;
        private IDictionary<long, IVersionedRevocation> versionedRevocations;

        private ISet<ISecurityToken> missingSecurityTokens;
        private ISet<long> missingGrants;
        private ISet<IRevocation> missingRevocations;

        public BulkResolver(Security security, ITransaction transaction, IEnumerable<IObject> objects, IUser user,
            Dictionary<IObject, IAccessControlList> cache,
            Func<IObject, IVersionedGrant[], IVersionedRevocation[], IAccessControlList> create,
            ConcurrentDictionary<long, IVersionedSecurityToken> cachedSecurityTokens,
            ConcurrentDictionary<long, IVersionedGrant> cachedGrants,
            ConcurrentDictionary<long, IVersionedRevocation> cachedRevocations,
            HashSet<long> allowedPermissionIds)
        {
            this.security = security;
            this.cache = cache;
            this.create = create;
            this.cachedSecurityTokens = cachedSecurityTokens;
            this.cachedGrants = cachedGrants;
            this.cachedRevocations = cachedRevocations;
            this.allowedPermissionIds = allowedPermissionIds;

            this.Result = new Dictionary<IObject, IAccessControlList>();

            this.FromCache(objects);

            if (this.missingObjects != null)
            {
                this.versionedSecurityTokens = new Dictionary<long, IVersionedSecurityToken>();
                this.versionedGrants = new Dictionary<long, IVersionedGrant>();
                this.versionedRevocations = new Dictionary<long, IVersionedRevocation>();

                var lookup = new SecurityTokens(transaction);
                var initialSecurityToken = lookup.InitialSecurityToken;
                var defaultSecurityToken = lookup.DefaultSecurityToken;

                this.FromCache(initialSecurityToken);
                this.FromCache(defaultSecurityToken);

                foreach (var missingObject in this.missingObjects)
                {
                    foreach (var securityToken in this.GetDefinedSecurityTokens(missingObject))
                    {
                        this.FromCache(securityToken);
                    }

                    foreach (var revocation in missingObject.Revocations)
                    {
                        this.FromCache(revocation);
                    }
                }

                if (this.missingSecurityTokens != null)
                {
                    transaction.Prefetch(this.security.SecurityTokenPrefetchPolicy, this.missingSecurityTokens);

                    foreach (var securityToken in this.missingSecurityTokens)
                    {
                        var versionedSecurityToken = new VersionedSecurityToken(this.security.Ranges, securityToken.Id, securityToken.Strategy.ObjectVersion, securityToken.Grants.ToDictionary(v => v.Id, v => v.Strategy.ObjectVersion));
                        cachedSecurityTokens[securityToken.Strategy.ObjectId] = versionedSecurityToken;
                        this.versionedSecurityTokens[securityToken.Strategy.ObjectId] = versionedSecurityToken;
                        foreach (var grant in versionedSecurityToken.VersionByGrant)
                        {
                            this.FromCache(grant);
                        }
                    }
                }

                if (this.missingRevocations != null)
                {
                    transaction.Prefetch(this.security.RevocationPrefetchPolicy, this.missingRevocations.Select(v => v.Strategy));

                    foreach (var revocation in this.missingRevocations)
                    {
                        var permissions = ((Revocation)revocation).DeniedPermissions.Select(v => v.Id);
                        if (this.allowedPermissionIds != null)
                        {
                            permissions = permissions.Where(v => this.allowedPermissionIds.Contains(v));
                        }

                        var versionedRevocation = new VersionedRevocation(this.security.Ranges, revocation.Id, revocation.Strategy.ObjectVersion, permissions);
                        cachedRevocations[revocation.Strategy.ObjectId] = versionedRevocation;
                        this.versionedRevocations[revocation.Strategy.ObjectId] = versionedRevocation;
                    }
                }

                if (this.missingGrants != null)
                {
                    transaction.Prefetch(this.security.GrantPrefetchPolicy, this.missingGrants);
                    var missing = transaction.Instantiate(this.missingGrants).Cast<Grant>();

                    foreach (var grant in missing)
                    {
                        var permissions = grant.EffectivePermissions;
                        if (this.allowedPermissionIds != null)
                        {
                            permissions = permissions.Where(v => this.allowedPermissionIds.Contains(v.Id));
                        }

                        var versionedGrant = new VersionedGrant(this.security.Ranges, grant.Id, grant.Strategy.ObjectVersion, new HashSet<long>(grant.EffectiveUsers.Select(v => v.Id)), permissions.Select(v => v.Id));
                        this.versionedGrants[grant.Id] = versionedGrant;
                    }
                }

                foreach (var @object in this.missingObjects)
                {
                    var tokens = this.GetDefinedSecurityTokens(@object).ToArray();
                    if (tokens.Length == 0)
                    {
                        tokens = @object.Strategy.IsNewInTransaction
                            ? new ISecurityToken[] { initialSecurityToken ?? defaultSecurityToken }
                            : new ISecurityToken[] { defaultSecurityToken };
                    }

                    var grants = tokens.SelectMany(v => this.versionedSecurityTokens[v.Id].VersionByGrant.Keys
                            .Select(w => this.versionedGrants[w]))
                            .Where(v => v.UserSet.Contains(user.Id))
                            .Distinct()
                            .ToArray();

                    var revocations = @object.Revocations.Select(v => this.versionedRevocations[v.Id]).Where(v => v.PermissionSet.Any()).ToArray();
                    var acl = this.create(@object, grants, revocations);

                    this.cache[@object] = acl;
                    this.Result[@object] = acl;
                }
            }
        }

        public Dictionary<IObject, IAccessControlList> Result { get; }

        private void FromCache(IEnumerable<IObject> objects)
        {
            foreach (var @object in objects)
            {
                if (this.cache.TryGetValue(@object, out var acl))
                {
                    this.Result[@object] = acl;
                }
                else
                {
                    this.missingObjects ??= new List<Domain.Object>();
                    this.missingObjects.Add((Domain.Object)@object);
                }
            }
        }

        private void FromCache(ISecurityToken securityToken)
        {
            if (this.cachedSecurityTokens.TryGetValue(securityToken.Strategy.ObjectId, out var versionedSecurityToken) && versionedSecurityToken.Version == securityToken.Strategy.ObjectVersion)
            {
                this.versionedSecurityTokens[securityToken.Id] = versionedSecurityToken;
                foreach (var grant in versionedSecurityToken.VersionByGrant)
                {
                    this.FromCache(grant);
                }
            }
            else
            {
                this.missingSecurityTokens ??= new HashSet<ISecurityToken>();
                this.missingSecurityTokens.Add(securityToken);
            }
        }

        private void FromCache(KeyValuePair<long, long> grant)
        {
            if (this.cachedGrants.TryGetValue(grant.Key, out var versionedGrant) && versionedGrant.Version == grant.Value)
            {
                this.versionedGrants[versionedGrant.Id] = versionedGrant;
            }
            else
            {
                this.missingGrants ??= new HashSet<long>();
                this.missingGrants.Add(grant.Key);
            }
        }

        private void FromCache(IRevocation revocation)
        {
            if (this.cachedRevocations.TryGetValue(revocation.Strategy.ObjectId, out var versionedRevocation) && versionedRevocation.Version != revocation.Strategy.ObjectVersion)
            {
                this.versionedRevocations[versionedRevocation.Id] = versionedRevocation;
            }
            else
            {
                this.missingRevocations ??= new HashSet<IRevocation>();
                this.missingRevocations.Add(revocation);
            }
        }

        private IEnumerable<ISecurityToken> GetDefinedSecurityTokens(Domain.Object @object)
        {
            if (@object is DelegatedAccessObject { ExistSecurityTokens: true } delegated)
            {
                return @object.ExistSecurityTokens ? @object.SecurityTokens.Concat(delegated.SecurityTokens) : delegated.SecurityTokens;
            }

            return @object.SecurityTokens;
        }
    }
}

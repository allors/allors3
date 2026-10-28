// <copyright file="SyncResponseBuilder.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Protocol.Json
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Security;
    using Meta;
    using Allors.Protocol.Json.Api.Sync;
    using Ranges;
    using Security;

    public class SyncResponseBuilder
    {
        private readonly IUnitConvert unitConvert;
        private readonly IRanges<long> ranges;

        private readonly ITransaction transaction;
        private readonly ISet<IClass> allowedClasses;
        private readonly IDictionary<IClass, ISet<IRoleType>> roleTypesByClass;
        private readonly IDictionary<IClass, PrefetchPolicy> prefetchPolicyByClass;

        public SyncResponseBuilder(ITransaction transaction,
            IAccessControl accessControl,
            ISet<IClass> allowedClasses,
            IDictionary<IClass, ISet<IRoleType>> roleTypesByClass,
            IDictionary<IClass, PrefetchPolicy> prefetchPolicyByClass,
            IUnitConvert unitConvert,
            IRanges<long> ranges)
        {
            this.transaction = transaction;
            this.allowedClasses = allowedClasses;
            this.roleTypesByClass = roleTypesByClass;
            this.prefetchPolicyByClass = prefetchPolicyByClass;
            this.unitConvert = unitConvert;
            this.ranges = ranges;

            this.AccessControl = accessControl;
        }

        public IAccessControl AccessControl { get; }

        public SyncResponse Build(SyncRequest syncRequest)
        {
            var knownGrants = ReadKnownVersions(syncRequest.g, nameof(syncRequest.g));
            var knownRevocations = ReadKnownVersions(syncRequest.r, nameof(syncRequest.r));
            var grants = new Dictionary<long, AccessResponseGrant>();
            var revocations = new Dictionary<long, AccessResponseRevocation>();
            var permissionIds = new HashSet<long>();
            var requestObjects = this.transaction.Instantiate(syncRequest.o);

            foreach (var groupBy in requestObjects.GroupBy(v => v.Strategy.Class, v => v))
            {
                var prefetchClass = groupBy.Key;
                var prefetchObjects = groupBy;
                if (this.prefetchPolicyByClass.TryGetValue(prefetchClass, out var prefetchPolicy))
                {
                    this.transaction.Prefetch(prefetchPolicy, prefetchObjects);
                }
            }

            var objects = requestObjects.Where(this.Include).Select(v =>
            {
                var @class = v.Strategy.Class;
                var acl = this.AccessControl[v];
                var roleTypes = this.roleTypesByClass[@class];

                foreach (var grant in acl.Grants)
                {
                    if (grants.TryGetValue(grant.Id, out var existing))
                    {
                        if (existing.v != grant.Version)
                        {
                            throw new InvalidOperationException("Sync cannot bind one grant ID to different versions in the same response.");
                        }

                        continue;
                    }

                    var entry = new AccessResponseGrant
                    {
                        i = grant.Id,
                        v = grant.Version,
                        p = knownGrants.Contains((grant.Id, grant.Version)) ? null : grant.PermissionRange.ToArray(),
                    };
                    grants.Add(entry.i, entry);
                    if (entry.p != null)
                    {
                        permissionIds.UnionWith(entry.p);
                    }
                }

                foreach (var revocation in acl.Revocations)
                {
                    if (revocations.TryGetValue(revocation.Id, out var existing))
                    {
                        if (existing.v != revocation.Version)
                        {
                            throw new InvalidOperationException("Sync cannot bind one revocation ID to different versions in the same response.");
                        }

                        continue;
                    }

                    var entry = new AccessResponseRevocation
                    {
                        i = revocation.Id,
                        v = revocation.Version,
                        p = knownRevocations.Contains((revocation.Id, revocation.Version)) ? null : revocation.PermissionRange.ToArray(),
                    };
                    revocations.Add(entry.i, entry);
                    if (entry.p != null)
                    {
                        permissionIds.UnionWith(entry.p);
                    }
                }

                return new SyncResponseObject
                {
                    i = v.Id,
                    v = v.Strategy.ObjectVersion,
                    c = v.Strategy.Class.Tag,
                    ro = roleTypes
                        .Where(w => acl.CanRead(w) && v.Strategy.ExistRole(w))
                        .Select(w => this.CreateSyncResponseRole(v, w, this.unitConvert))
                        .ToArray(),
                    g = this.ranges.Import(acl.Grants.Select(w => w.Id)).Save(),
                    r = this.ranges.Import(acl.Revocations.Select(w => w.Id)).Save(),
                };
            }).ToArray();

            var permissions = new PermissionResponseBuilder(this.transaction, this.allowedClasses)
                .Build(new PermissionRequest { p = permissionIds.OrderBy(v => v).ToArray() }).p ?? Array.Empty<PermissionResponsePermission>();
            if (permissions.Length != permissionIds.Count || !permissionIds.SetEquals(permissions.Select(v => v.i)))
            {
                throw new InvalidOperationException("Sync cannot return complete permission definitions for the selected access entries.");
            }

            return new SyncResponse
            {
                o = objects,
                g = grants.Values.OrderBy(v => v.i).ToArray(),
                r = revocations.Values.OrderBy(v => v.i).ToArray(),
                p = permissions,
            };
        }

        private static HashSet<(long Id, long Version)> ReadKnownVersions(long[][] entries, string parameterName)
        {
            var versions = new HashSet<(long Id, long Version)>();
            if (entries == null)
            {
                return versions;
            }

            foreach (var entry in entries)
            {
                if (entry == null || entry.Length != 2 || entry[0] <= 0 || entry[1] < 0)
                {
                    throw new ArgumentException("Sync cache entries must be [id, version] pairs with a positive ID and a non-negative version.", parameterName);
                }

                versions.Add((entry[0], entry[1]));
            }

            return versions;
        }

        private SyncResponseRole CreateSyncResponseRole(IObject @object, IRoleType roleType, IUnitConvert unitConvert)
        {
            var syncResponseRole = new SyncResponseRole { t = roleType.RelationType.Tag };

            if (roleType.ObjectType.IsUnit)
            {
                syncResponseRole.v = unitConvert.ToJson(@object.Strategy.GetUnitRole(roleType));
            }
            else if (roleType.IsOne)
            {
                var role = @object.Strategy.GetCompositeRole(roleType);
                if (this.Include(role))
                {
                    syncResponseRole.o = role.Id;
                }
            }
            else
            {
                var roles = @object.Strategy.GetCompositesRole<IObject>(roleType).Where(this.Include);
                syncResponseRole.c = this.ranges.Import(roles.Select(roleObject => roleObject.Id)).ToArray();
            }

            return syncResponseRole;
        }

        public bool Include(IObject @object) => @object != null && this.allowedClasses?.Contains(@object.Strategy.Class) == true;
    }
}

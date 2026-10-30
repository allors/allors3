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
    using Allors.Protocol.Json.Api.Pull;
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

        public static void ValidateRequest(SyncRequest request)
        {
            if (request?.pull == null)
            {
                throw new ArgumentException("Sync requires the original Pull request, including its arguments and dependencies.", nameof(request));
            }

            var grants = ReadKnownVersions(request.g, nameof(request.g));
            var revocations = ReadKnownVersions(request.r, nameof(request.r));
            ReadKnownObjects(request.o, grants, revocations);
        }

        public SyncResponse Build(SyncRequest syncRequest, PullResponse finalSelection)
        {
            var knownGrants = ReadKnownVersions(syncRequest.g, nameof(syncRequest.g));
            var knownRevocations = ReadKnownVersions(syncRequest.r, nameof(syncRequest.r));
            var knownObjects = ReadKnownObjects(syncRequest.o, knownGrants, knownRevocations);
            var grants = new Dictionary<long, AccessResponseGrant>();
            var revocations = new Dictionary<long, AccessResponseRevocation>();
            var permissionIds = new HashSet<long>();
            var requestObjects = this.transaction.Instantiate(finalSelection.p.Select(v => v.i));

            if (requestObjects.Length != finalSelection.p.Length)
            {
                throw new InvalidOperationException("Sync could not complete its selected object pool in the current transaction.");
            }

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
                var grantIds = this.ranges.Import(acl.Grants.Select(w => w.Id)).Save();
                var revocationIds = this.ranges.Import(acl.Revocations.Select(w => w.Id)).Save();

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
                        p = knownGrants.TryGetValue(grant.Id, out var version) && version == grant.Version ? null : grant.PermissionRange.ToArray(),
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
                        p = knownRevocations.TryGetValue(revocation.Id, out var version) && version == revocation.Version ? null : revocation.PermissionRange.ToArray(),
                    };
                    revocations.Add(entry.i, entry);
                    if (entry.p != null)
                    {
                        permissionIds.UnionWith(entry.p);
                    }
                }

                var reuse = knownObjects.TryGetValue(v.Id, out var known) && known.v == v.Strategy.ObjectVersion &&
                    (known.g ?? Array.Empty<long>()).SequenceEqual(grantIds ?? Array.Empty<long>()) &&
                    (known.r ?? Array.Empty<long>()).SequenceEqual(revocationIds ?? Array.Empty<long>()) &&
                    acl.Grants.All(w => knownGrants.TryGetValue(w.Id, out var version) && version == w.Version) &&
                    acl.Revocations.All(w => knownRevocations.TryGetValue(w.Id, out var version) && version == w.Version);

                return new SyncResponseObject
                {
                    i = v.Id,
                    v = v.Strategy.ObjectVersion,
                    c = v.Strategy.Class.Tag,
                    ro = reuse ? null : roleTypes
                        .Where(w => acl.CanRead(w) && v.Strategy.ExistRole(w))
                        .Select(w => this.CreateSyncResponseRole(v, w, this.unitConvert))
                        .ToArray(),
                    g = grantIds,
                    r = revocationIds,
                };
            }).ToArray();

            var permissions = new PermissionResponseBuilder(this.transaction, this.allowedClasses)
                .Build(new PermissionRequest { p = permissionIds.OrderBy(v => v).ToArray() }).p ?? Array.Empty<PermissionResponsePermission>();
            if (permissions.Length != permissionIds.Count || !permissionIds.SetEquals(permissions.Select(v => v.i)))
            {
                throw new InvalidOperationException("Sync could not provide complete permission definitions for its security contents.");
            }

            return new SyncResponse
            {
                result = new SyncResponseResult { o = finalSelection.o, c = finalSelection.c, v = finalSelection.v },
                o = objects,
                g = grants.Values.OrderBy(v => v.i).ToArray(),
                r = revocations.Values.OrderBy(v => v.i).ToArray(),
                p = permissions,
            };
        }

        private static Dictionary<long, long> ReadKnownVersions(long[][] entries, string parameterName)
        {
            var versions = new Dictionary<long, long>();
            foreach (var entry in entries ?? Array.Empty<long[]>())
            {
                if (entry == null || entry.Length != 2 || entry[0] <= 0 || entry[1] < 0 || !versions.TryAdd(entry[0], entry[1]))
                {
                    throw new ArgumentException("Sync cache entries must be [id, version] pairs with a positive ID, a non-negative version and one entry per ID.", parameterName);
                }
            }

            return versions;
        }

        private static Dictionary<long, PullResponseObject> ReadKnownObjects(PullResponseObject[] entries, Dictionary<long, long> grants, Dictionary<long, long> revocations)
        {
            var objects = new Dictionary<long, PullResponseObject>();
            foreach (var entry in entries ?? Array.Empty<PullResponseObject>())
            {
                if (entry == null || entry.i <= 0 || entry.v < 0 || !objects.TryAdd(entry.i, entry))
                {
                    throw new ArgumentException("Sync object cache entries must have a positive ID, a non-negative version and one entry per ID.", nameof(entries));
                }

                ValidateSecurityIds(entry.g, grants);
                ValidateSecurityIds(entry.r, revocations);
            }

            return objects;
        }

        private static void ValidateSecurityIds(long[] ids, Dictionary<long, long> versions)
        {
            var previous = 0L;
            foreach (var id in ids ?? Array.Empty<long>())
            {
                if (id <= previous || !versions.ContainsKey(id))
                {
                    throw new ArgumentException("Sync object security IDs must be sorted, unique and declared in the corresponding security cache table.", nameof(ids));
                }

                previous = id;
            }
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

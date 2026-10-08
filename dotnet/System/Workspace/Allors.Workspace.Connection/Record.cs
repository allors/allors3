// <copyright file="Record.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Connection
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json;
    using Allors.Protocol.Json.Api.Sync;
    using Meta;
    using Ranges;

    /// <summary>
    /// A record as a sync response delivered it, converted from the wire when it is built and
    /// immutable after that, so that a cache may hand it to any thread. The permissions are
    /// answered against the grants and revocations the cache holds at the time of asking.
    /// </summary>
    internal sealed class Record : IRecord
    {
        private readonly ICache cache;
        private readonly IReadOnlyDictionary<IRelationType, object> roleByRelationType;

        internal Record(ICache cache, IMetaPopulation metaPopulation, IUnitConvert unitConvert, IRanges<long> ranges, ResponseContext ctx, SyncResponseObject syncResponseObject)
        {
            this.cache = cache;

            this.Class = (IClass)metaPopulation.FindByTag(syncResponseObject.c);
            this.Id = syncResponseObject.i;
            this.Version = syncResponseObject.v;
            this.GrantIds = ranges.Load(ctx.CheckForMissingGrants(syncResponseObject.g));
            this.RevocationIds = ranges.Load(ctx.CheckForMissingRevocations(syncResponseObject.r));

            var roleByRelationType = new Dictionary<IRelationType, object>();
            if (syncResponseObject.ro != null)
            {
                foreach (var syncResponseRole in syncResponseObject.ro)
                {
                    var relationType = (IRelationType)metaPopulation.FindByTag(syncResponseRole.t);
                    var roleType = relationType.RoleType;
                    var objectType = roleType.ObjectType;

                    object role;
                    if (objectType.IsUnit)
                    {
                        role = unitConvert.UnitFromJson(objectType.Tag, syncResponseRole.v);
                    }
                    else if (roleType.IsOne)
                    {
                        role = syncResponseRole.o;
                    }
                    else
                    {
                        role = ranges.Load(syncResponseRole.c);
                    }

                    roleByRelationType[relationType] = role;
                }
            }

            this.roleByRelationType = roleByRelationType;
        }

        public IClass Class { get; }

        public long Id { get; }

        public long Version { get; }

        public IRange<long> GrantIds { get; }

        public IRange<long> RevocationIds { get; }

        public object GetRole(IRoleType roleType)
        {
            this.roleByRelationType.TryGetValue(roleType.RelationType, out var role);
            return role;
        }

        public bool IsPermitted(long permission)
        {
            // A grant or revocation the cache no longer holds grants nothing and denies nothing.
            if (this.RevocationIds.Any(v => this.cache.GetRevocation(v)?.PermissionIds.Contains(permission) == true))
            {
                return false;
            }

            return this.GrantIds.Any(v => this.cache.GetGrant(v)?.PermissionIds.Contains(permission) == true);
        }
    }
}

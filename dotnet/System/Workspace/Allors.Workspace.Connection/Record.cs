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
    /// A record as a sync response delivered it. The roles are converted from the wire on first
    /// use; the permissions are answered against the grants and revocations of the cache.
    /// </summary>
    internal sealed class Record : IRecord
    {
        private readonly ICache cache;
        private readonly IMetaPopulation metaPopulation;
        private readonly IUnitConvert unitConvert;
        private readonly IRanges<long> ranges;

        private Dictionary<IRelationType, object> roleByRelationType;
        private SyncResponseRole[] syncResponseRoles;

        internal Record(ICache cache, IMetaPopulation metaPopulation, IUnitConvert unitConvert, IRanges<long> ranges, ResponseContext ctx, SyncResponseObject syncResponseObject)
        {
            this.cache = cache;
            this.metaPopulation = metaPopulation;
            this.unitConvert = unitConvert;
            this.ranges = ranges;

            this.Class = (IClass)metaPopulation.FindByTag(syncResponseObject.c);
            this.Id = syncResponseObject.i;
            this.Version = syncResponseObject.v;
            this.syncResponseRoles = syncResponseObject.ro;
            this.GrantIds = ranges.Load(ctx.CheckForMissingGrants(syncResponseObject.g));
            this.RevocationIds = ranges.Load(ctx.CheckForMissingRevocations(syncResponseObject.r));
        }

        public IClass Class { get; }

        public long Id { get; }

        public long Version { get; }

        internal IRange<long> GrantIds { get; }

        internal IRange<long> RevocationIds { get; }

        private Dictionary<IRelationType, object> RoleByRelationType
        {
            get
            {
                if (this.syncResponseRoles != null)
                {
                    this.roleByRelationType = this.syncResponseRoles.ToDictionary(
                        v => (IRelationType)this.metaPopulation.FindByTag(v.t),
                        v =>
                        {
                            var roleType = ((IRelationType)this.metaPopulation.FindByTag(v.t)).RoleType;
                            var objectType = roleType.ObjectType;

                            if (objectType.IsUnit)
                            {
                                return this.unitConvert.UnitFromJson(objectType.Tag, v.v);
                            }

                            if (roleType.IsOne)
                            {
                                return v.o;
                            }

                            return this.ranges.Load(v.c);
                        });

                    this.syncResponseRoles = null;
                }

                return this.roleByRelationType;
            }
        }

        public object GetRole(IRoleType roleType)
        {
            object @object = null;
            this.RoleByRelationType?.TryGetValue(roleType.RelationType, out @object);
            return @object;
        }

        public bool IsPermitted(long permission)
        {
            if (this.GrantIds == null)
            {
                return false;
            }

            return !this.RevocationIds.Any(v => this.cache.GetRevocation(v).PermissionIds.Contains(permission)) && this.GrantIds.Any(v => this.cache.GetGrant(v).PermissionIds.Contains(permission));
        }
    }
}

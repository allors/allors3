// <copyright file="RemoteDatabaseObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Sync;
    using Meta;
    using Ranges;

    internal class DatabaseRecord : Adapters.DatabaseRecord
    {
        private readonly DatabaseConnection database;

        private Dictionary<IRelationType, object> roleByRelationType;
        private SyncResponseRole[] syncResponseRoles;

        internal DatabaseRecord(DatabaseConnection database, IClass @class, long id) : base(@class, id, 0) => this.database = database;

        internal DatabaseRecord(DatabaseConnection database, ResponseContext ctx, SyncResponseObject syncResponseObject)
            : base((IClass)database.Configuration.MetaPopulation.FindByTag(syncResponseObject.t), syncResponseObject.i, syncResponseObject.v)
        {
            this.database = database;
            this.syncResponseRoles = syncResponseObject.r;

            var ranges = database.Ranges;

            this.AccessControlIds = ranges.Load(ctx.CheckForMissingAccessControls(syncResponseObject.a));
            this.DeniedPermissions = ranges.Load(ctx.CheckForMissingPermissions(syncResponseObject.d));
        }

        internal IRange AccessControlIds { get; }

        internal IRange DeniedPermissions { get; }

        private Dictionary<IRelationType, object> RoleByRelationType
        {
            get
            {
                if (this.syncResponseRoles != null)
                {
                    var meta = this.database.Configuration.MetaPopulation;
                    var ranges = this.database.Ranges;

                    var metaPopulation = this.database.Configuration.MetaPopulation;
                    this.roleByRelationType = this.syncResponseRoles.ToDictionary(
                        v => (IRelationType)meta.FindByTag(v.t),
                        v =>
                        {
                            var roleType = ((IRelationType)metaPopulation.FindByTag(v.t)).RoleType;

                            var objectType = roleType.ObjectType;
                            if (objectType.IsUnit)
                            {
                                return this.database.UnitConvert.FromJson(roleType.ObjectType.Tag, v.v);
                            }

                            if (roleType.IsOne)
                            {
                                return v.o;
                            }

                            return ranges.Load(v.c);
                        });

                    this.syncResponseRoles = null;
                }

                return this.roleByRelationType;
            }
        }

        public override object GetRole(IRoleType roleType)
        {
            object @object = null;
            this.RoleByRelationType?.TryGetValue(roleType.RelationType, out @object);
            return @object;
        }

        public override bool IsPermitted(long permission)
        {
            if (this.AccessControlIds == null)
            {
                return false;
            }

            return !this.DeniedPermissions.Contains(permission) && this.AccessControlIds.Any(v => this.database.AccessControlById[v].PermissionIds.Any(w => w == permission));
        }
    }
}

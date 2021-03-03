// <copyright file="RemoteDatabaseRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using Meta;

    public class RemoteWorkspaceRoles
    {
        private readonly IReadOnlyDictionary<Guid, object> roleByRelationTypeId;

        internal RemoteWorkspaceRoles(RemoteDatabase database, Identity identity, IClass @class, long version, IReadOnlyDictionary<Guid, object> roleByRelationTypeId)
        {
            this.Database = database;
            this.Identity = identity;
            this.Class = @class;
            this.Version = version;
            this.roleByRelationTypeId = roleByRelationTypeId;
        }

        public RemoteDatabase Database { get; }

        public IClass Class { get; }

        public Identity Identity { get; }

        public long Version { get; private set; }

        public object GetRole(IRoleType roleType)
        {
            object @object = null;
            this.roleByRelationTypeId?.TryGetValue(roleType.RelationType.Id, out @object);
            return @object;
        }

        internal RemoteWorkspaceRoles Update(Dictionary<Guid, object> changedRoleByRoleType)
        {
            var newRoleByRelationTypeId = new Dictionary<Guid, object>();
            foreach (var roleType in this.Class.WorkspaceRoleTypes)
            {
                var id = roleType.RelationType.Id;
                if (changedRoleByRoleType.TryGetValue(id, out var role))
                {
                    newRoleByRelationTypeId[id] = role;
                }
                else if (this.roleByRelationTypeId.TryGetValue(id, out role))
                {
                    newRoleByRelationTypeId[id] = role;
                }
            }

            return new RemoteWorkspaceRoles(this.Database, this.Identity, this.Class, ++this.Version, newRoleByRelationTypeId);
        }
    }
}

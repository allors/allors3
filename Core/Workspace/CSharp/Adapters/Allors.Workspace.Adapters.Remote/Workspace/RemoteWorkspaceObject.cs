// <copyright file="RemoteDatabaseRoles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using Meta;

    internal class RemoteWorkspaceObject
    {
        private readonly IReadOnlyDictionary<IRelationType, object> roleByRelationType;

        internal RemoteWorkspaceObject(RemoteDatabase database, Identity identity, IClass @class, long version, IReadOnlyDictionary<IRelationType, object> roleByRelationType)
        {
            this.Database = database;
            this.Identity = identity;
            this.Class = @class;
            this.Version = version;
            this.roleByRelationType = roleByRelationType;
        }

        internal RemoteDatabase Database { get; }

        internal IClass Class { get; }

        internal Identity Identity { get; }

        internal long Version { get; private set; }

        internal object GetRole(IRoleType roleType)
        {
            object @object = null;
            this.roleByRelationType?.TryGetValue(roleType.RelationType, out @object);
            return @object;
        }

        internal RemoteWorkspaceObject Update(Dictionary<IRelationType, object> changedRoleByRoleType)
        {
            var newRoleByRelationTypeId = new Dictionary<IRelationType, object>();
            foreach (var roleType in this.Class.WorkspaceRoleTypes)
            {
                if (changedRoleByRoleType.TryGetValue(roleType.RelationType, out var role))
                {
                    newRoleByRelationTypeId[roleType.RelationType] = role;
                }
                else if (this.roleByRelationType.TryGetValue(roleType.RelationType, out role))
                {
                    newRoleByRelationTypeId[roleType.RelationType] = role;
                }
            }

            return new RemoteWorkspaceObject(this.Database, this.Identity, this.Class, ++this.Version, newRoleByRelationTypeId);
        }
    }
}

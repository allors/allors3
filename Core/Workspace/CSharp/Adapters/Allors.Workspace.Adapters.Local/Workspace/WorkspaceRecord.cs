// <copyright file="LocalWorkspaceObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    internal class WorkspaceRecord : IRecord
    {
        private readonly IReadOnlyDictionary<IRelationType, object> roleByRelationType;

        internal WorkspaceRecord(DatabaseAdapter databaseAdapter, long identity, IClass @class, long version, IReadOnlyDictionary<IRelationType, object> roleByRelationType)
        {
            this.DatabaseAdapter = databaseAdapter;
            this.Identity = identity;
            this.Class = @class;
            this.Version = version;

            this.roleByRelationType = this.Import(roleByRelationType).ToDictionary(v => v.Key, v => v.Value);
        }

        public WorkspaceRecord(WorkspaceRecord originalWorkspaceRecord, IReadOnlyDictionary<IRelationType, object> changedRoleByRoleType)
        {
            this.DatabaseAdapter = originalWorkspaceRecord.DatabaseAdapter;
            this.Identity = originalWorkspaceRecord.Identity;
            this.Class = originalWorkspaceRecord.Class;
            this.Version = ++originalWorkspaceRecord.Version;


            this.roleByRelationType = this.Import(changedRoleByRoleType, originalWorkspaceRecord.roleByRelationType).ToDictionary(v => v.Key, v => v.Value);
        }

        internal DatabaseAdapter DatabaseAdapter { get; }

        internal IClass Class { get; }

        internal long Identity { get; }

        public long Version { get; private set; }

        public object GetRole(IRoleType roleType)
        {
            object @object = null;
            _ = (this.roleByRelationType?.TryGetValue(roleType.RelationType, out @object));
            return @object;
        }

        private IEnumerable<KeyValuePair<IRelationType, object>> Import(IReadOnlyDictionary<IRelationType, object> changedRoleByRoleType, IReadOnlyDictionary<IRelationType, object> originalRoleByRoleType = null)
        {
            foreach (var roleType in this.Class.WorkspaceRoleTypes)
            {
                var relationType = roleType.RelationType;

                if (changedRoleByRoleType.TryGetValue(relationType, out var role))
                {
                    if (role != null)
                    {
                        yield return new KeyValuePair<IRelationType, object>(relationType, role);
                    }
                }
                else if (originalRoleByRoleType?.TryGetValue(roleType.RelationType, out role) == true)
                {
                    yield return new KeyValuePair<IRelationType, object>(relationType, role);
                }
            }
        }
    }
}

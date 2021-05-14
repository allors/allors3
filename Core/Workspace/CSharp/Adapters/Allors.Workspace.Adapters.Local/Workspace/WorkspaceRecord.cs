// <copyright file="WorkspaceRecord.cs" company="Allors bvba">
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
        private readonly DatabaseAdapter databaseAdapter;
        private readonly IClass @class;
        private readonly long id;
        private readonly IReadOnlyDictionary<IRelationType, object> roleByRelationType;

        internal WorkspaceRecord(DatabaseAdapter databaseAdapter, long id, IClass @class, long version, IReadOnlyDictionary<IRelationType, object> roleByRelationType)
        {
            this.databaseAdapter = databaseAdapter;
            this.id = id;
            this.@class = @class;
            this.Version = version;
            this.roleByRelationType = this.Import(roleByRelationType).ToDictionary(v => v.Key, v => v.Value);
        }

        public WorkspaceRecord(WorkspaceRecord originalRecord, IReadOnlyDictionary<IRelationType, object> changedRoleByRoleType)
        {
            this.databaseAdapter = originalRecord.databaseAdapter;
            this.id = originalRecord.id;
            this.@class = originalRecord.@class;
            this.Version = ++originalRecord.Version;
            this.roleByRelationType = this.Import(changedRoleByRoleType, originalRecord.roleByRelationType).ToDictionary(v => v.Key, v => v.Value);
        }

        public long Version { get; private set; }

        public object GetRole(IRoleType roleType)
        {
            object @object = null;
            _ = (this.roleByRelationType?.TryGetValue(roleType.RelationType, out @object));
            return @object;
        }

        private IEnumerable<KeyValuePair<IRelationType, object>> Import(IReadOnlyDictionary<IRelationType, object> changedRoleByRoleType, IReadOnlyDictionary<IRelationType, object> originalRoleByRoleType = null)
        {
            foreach (var roleType in this.@class.WorkspaceOriginRoleTypes)
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

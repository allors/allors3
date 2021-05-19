// <copyright file="RemoteWorkspaceObject.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    internal class WorkspaceRecord : IRecord
    {
        private readonly Database database;
        private readonly IClass @class;
        private readonly long id;
        private readonly IReadOnlyDictionary<IRelationType, object> roleByRelationType;

        internal WorkspaceRecord(Database database, long id, IClass @class, long version, IReadOnlyDictionary<IRelationType, object> roleByRelationType)
        {
            this.database = database;
            this.id = id;
            this.@class = @class;
            this.Version = version;
            this.roleByRelationType = this.Import(roleByRelationType).ToDictionary(v => v.Key, v => v.Value);
        }

        public WorkspaceRecord(WorkspaceRecord originalWorkspaceRecord, IReadOnlyDictionary<IRelationType, object> changedRoleByRoleType)
        {
            this.database = originalWorkspaceRecord.database;
            this.id = originalWorkspaceRecord.id;
            this.@class = originalWorkspaceRecord.@class;
            this.Version = ++originalWorkspaceRecord.Version;

            this.roleByRelationType = this.Import(changedRoleByRoleType, originalWorkspaceRecord.roleByRelationType).ToDictionary(v => v.Key, v => v.Value);
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
                        if (roleType.ObjectType.IsUnit)
                        {
                            yield return new KeyValuePair<IRelationType, object>(relationType, role);
                        }
                        else
                        {
                            if (roleType.IsOne)
                            {
                                yield return new KeyValuePair<IRelationType, object>(relationType, ((Strategy)role).Id);

                            }
                            else
                            {
                                var roles = (Strategy[])role;
                                if (roles.Length > 0)
                                {
                                    yield return new KeyValuePair<IRelationType, object>(relationType, roles.Select(v => v.Id).ToArray());
                                }
                            }
                        }
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

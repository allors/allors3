// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Push;
    using Meta;

    public sealed class RemoteStrategy : IStrategy
    {
        private IObject @object;

        private Dictionary<IRoleType, object> changedRoleByRoleType;

        private Dictionary<IRoleType, object> roleByRoleType = new Dictionary<IRoleType, object>();

        private RemoteStrategy(RemoteSession session, long workspaceId, IClass @class)
        {
            this.Session = session;
            this.WorkspaceId = workspaceId;
            this.Class = @class;
        }

        public RemoteStrategy(RemoteSession session, IClass @class, long workspaceId) : this(session, workspaceId, @class) { }

        public RemoteStrategy(RemoteSession session, RemoteDatabaseRoles databaseRoles, long workspaceId) : this(session, workspaceId, databaseRoles.Class) => this.DatabaseRoles = databaseRoles;

        ISession IStrategy.Session => this.Session;

        public RemoteSession Session { get; }

        public IObject Object
        {
            get
            {
                this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);
                return this.@object;
            }
        }

        public IClass Class { get; }

        public long WorkspaceId { get; }

        public long? DatabaseId => this.DatabaseRoles?.DatabaseId;

        public long? Version => this.DatabaseRoles?.Version;

        private RemoteDatabaseRoles DatabaseRoles { get; set; }

        internal bool HasDatabaseChanges
        {
            get
            {
                if (this.Class.HasDatabaseOrigin && !this.ExistDatabaseObject)
                {
                    return true;
                }

                return this.changedRoleByRoleType != null;
            }
        }

        private bool ExistDatabaseObject => this.DatabaseRoles != null;

        public bool Exist(IRoleType roleType)
        {
            var value = this.Get(roleType);

            if (roleType.ObjectType.IsComposite && roleType.IsMany)
            {
                return ((IEnumerable<IObject>)value).Any();
            }

            return value != null;
        }

        public object Get(IRoleType roleType)
        {
            if (roleType.Origin != Origin.Database)
            {
                var population = this.GetPopulation(roleType.Origin);
                population.GetRole(this.WorkspaceId, roleType, out var role);
                if (roleType.ObjectType.IsUnit)
                {
                    return role;
                }

                if (roleType.IsOne)
                {
                    var id = (long?)role;
                    return id.HasValue ? this.Session.Instantiate(id.Value) : null;
                }

                var ids = (IEnumerable<long>)role;
                return ids?.Select(v => this.Session.Instantiate(v)).ToArray() ?? this.Session.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
            }

            if (!this.roleByRoleType.TryGetValue(roleType, out var value))
            {
                if (this.ExistDatabaseObject)
                {
                    var databaseRole = this.DatabaseRoles.GetRole(roleType);
                    if (databaseRole != null)
                    {
                        if (roleType.ObjectType.IsUnit)
                        {
                            value = databaseRole;
                        }
                        else
                        {
                            if (roleType.IsOne)
                            {
                                value = this.Session.Instantiate((long)databaseRole);
                            }
                            else
                            {
                                var ids = (long[])databaseRole;
                                var array = Array.CreateInstance(roleType.ObjectType.ClrType, ids.Length);
                                for (var i = 0; i < ids.Length; i++)
                                {
                                    array.SetValue(this.Session.Instantiate(ids[i]), i);
                                }

                                value = array;
                            }
                        }
                    }
                }

                if (value == null && roleType.IsMany)
                {
                    value = this.Session.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
                }

                this.roleByRoleType[roleType] = value;
            }

            return value;
        }

        public void Set(IRoleType roleType, object value)
        {
            if (roleType.Origin != Origin.Database)
            {
                var population = this.GetPopulation(roleType.Origin);
                population.SetRole(this.WorkspaceId, roleType, value);
                return;
            }

            var current = this.Get(roleType);
            if (roleType.ObjectType.IsUnit || roleType.IsOne)
            {
                if (Equals(current, value))
                {
                    return;
                }
            }
            else
            {
                value ??= Array.Empty<IStrategy>();

                var currentCollection = (IList<object>)current;
                var valueCollection = (IList<object>)value;
                if (currentCollection.Count == valueCollection.Count &&
                    !currentCollection.Except(valueCollection).Any())
                {
                    return;
                }
            }

            this.changedRoleByRoleType ??= new Dictionary<IRoleType, object>();

            if (roleType.ObjectType.IsComposite && roleType.IsMany)
            {
                // TODO: Optimize
                value = new ArrayList((Array)value).ToArray(roleType.ObjectType.ClrType);
            }

            this.roleByRoleType[roleType] = value;
            this.changedRoleByRoleType[roleType] = value;
        }

        public void Add(IRoleType roleType, IObject value)
        {
            var roles = (IObject[])this.Get(roleType);
            if (!roles.Contains(value))
            {
                roles = new List<IObject>(roles) { value }.ToArray();
            }

            this.Set(roleType, roles);
        }

        public void Remove(IRoleType roleType, IObject value)
        {
            var roles = (IStrategy[])this.Get(roleType);
            if (roles.Contains(value.Strategy))
            {
                var newRoles = new List<IStrategy>(roles);
                newRoles.Remove(value.Strategy);
                roles = newRoles.ToArray();
            }

            this.Set(roleType, roles);
        }

        public IObject GetAssociation(IAssociationType associationType)
        {
            if (associationType.Origin != Origin.Database)
            {
                var population = this.GetPopulation(associationType.Origin);
                population.GetAssociation(this.WorkspaceId, associationType, out var association);
                var id = (long?)association;
                return id.HasValue ? this.Session.Instantiate(id.Value) : null;
            }

            return this.Session.GetAssociation(this.Object, associationType).FirstOrDefault();
        }

        public IEnumerable<IObject> GetAssociations(IAssociationType associationType)
        {
            if (associationType.Origin != Origin.Database)
            {
                var population = this.GetPopulation(associationType.Origin);
                population.GetAssociation(this.WorkspaceId, associationType, out var association);
                var ids = (IEnumerable<long>)association;
                return ids?.Select(v => this.Session.Instantiate(v)).ToArray() ?? Array.Empty<IObject>();
            }

            return this.Session.GetAssociation(this.Object, associationType);
        }

        public bool CanRead(IRoleType roleType)
        {
            if (!this.ExistDatabaseObject)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Read);
            return this.DatabaseRoles.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (!this.ExistDatabaseObject)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Write);
            return this.DatabaseRoles.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistDatabaseObject)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, methodType, Operations.Execute);
            return this.DatabaseRoles.IsPermitted(permission);
        }

        internal State GetPopulation(Origin origin) =>
            origin switch
            {
                Origin.Workspace => this.Session.Workspace.State,
                Origin.Session => this.Session.State,
                _ => throw new Exception($"Unsupported origin: {origin}")
            };

        internal PushRequestNewObject SaveNew() => new PushRequestNewObject
        {
            NewWorkspaceId = this.WorkspaceId.ToString(),
            ObjectType = this.Class.IdAsString,
            Roles = this.SaveRoles(),
        };

        internal PushRequestObject SaveExisting() => new PushRequestObject
        {
            DatabaseId = this.DatabaseId?.ToString(),
            Version = this.Version.ToString(),
            Roles = this.SaveRoles(),
        };

        internal void Reset()
        {
            if (this.DatabaseRoles != null)
            {
                this.DatabaseRoles = this.Session.Workspace.Database.Get(this.DatabaseId.Value);
            }

            this.changedRoleByRoleType = null;

            this.roleByRoleType = new Dictionary<IRoleType, object>();
        }

        internal void PushResponse(RemoteDatabaseRoles databaseRoles) => this.DatabaseRoles = databaseRoles;

        internal void Refresh(bool merge = false)
        {
            if (!this.HasDatabaseChanges)
            {
                this.Reset();
            }
            else
            {
                if (merge)
                {
                    if (this.DatabaseRoles != null)
                    {
                        this.DatabaseRoles = this.Session.Workspace.Database.Get(this.DatabaseId.Value);
                    }
                }
            }
        }

        internal object GetDatabase(IRoleType roleType)
        {
            if (!this.roleByRoleType.TryGetValue(roleType, out var value))
            {
                if (this.ExistDatabaseObject)
                {
                    var databaseRole = this.DatabaseRoles.GetRole(roleType);
                    if (databaseRole != null)
                    {
                        if (roleType.ObjectType.IsUnit)
                        {
                            value = databaseRole;
                        }
                        else
                        {
                            if (roleType.IsOne)
                            {
                                value = this.Session.GetForAssociation((long)databaseRole);
                            }
                            else
                            {
                                var ids = (long[])databaseRole;
                                value = ids.Select(v => this.Session.GetForAssociation(v))
                                    .Where(v => v != null)
                                    .ToArray();
                            }
                        }
                    }
                }

                if (value == null && roleType.IsMany)
                {
                    value = this.Session.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
                }
            }

            return value;
        }

        private PushRequestRole[] SaveRoles()
        {
            if (this.changedRoleByRoleType?.Count > 0)
            {
                var saveRoles = new List<PushRequestRole>();

                foreach (var keyValuePair in this.changedRoleByRoleType)
                {
                    var roleType = keyValuePair.Key;
                    var roleValue = keyValuePair.Value;

                    var pushRequestRole = new PushRequestRole { RelationType = roleType.RelationType.IdAsString };

                    if (roleType.ObjectType.IsUnit)
                    {
                        pushRequestRole.SetRole = UnitConvert.ToString(roleValue);
                    }
                    else
                    {
                        if (roleType.IsOne)
                        {
                            var sessionRole = (IObject)roleValue;
                            pushRequestRole.SetRole = sessionRole?.DatabaseId?.ToString() ??
                                                sessionRole?.WorkspaceId.ToString();
                        }
                        else
                        {
                            var sessionRoles = (IObject[])roleValue;
                            var roleIds = sessionRoles
                                .Select(item => item.DatabaseId?.ToString() ?? item.WorkspaceId.ToString()).ToArray();
                            if (!this.ExistDatabaseObject)
                            {
                                pushRequestRole.AddRole = roleIds;
                            }
                            else
                            {
                                var databaseRole = this.DatabaseRoles.GetRole(roleType);
                                if (databaseRole == null)
                                {
                                    pushRequestRole.AddRole = roleIds;
                                }
                                else
                                {
                                    var originalRoleIds = ((IEnumerable<long>)databaseRole)
                                        .Select(v => v.ToString())
                                        .ToArray();
                                    pushRequestRole.AddRole = roleIds.Except(originalRoleIds).ToArray();
                                    pushRequestRole.RemoveRole = originalRoleIds.Except(roleIds).ToArray();
                                }
                            }
                        }
                    }

                    saveRoles.Add(pushRequestRole);
                }

                return saveRoles.ToArray();
            }

            return null;
        }
    }
}

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
    using Allors.Workspace.Meta;
    using Protocol.Data;
    using Protocol.Database.Push;

    public class ExistingDatabaseStrategy : IDatabaseStrategy
    {
        private Dictionary<IRoleType, object> changedRoleByRoleType;

        private Dictionary<IRoleType, object> roleByRoleType = new Dictionary<IRoleType, object>();

        private IObject @object;

        public ExistingDatabaseStrategy(Session session, DatabaseObject databaseObject, long workspaceId)
        {
            this.Session = session;
            this.DatabaseObject = databaseObject;
            this.Class = databaseObject.Class;
            this.WorkspaceId = workspaceId;
        }

        public IObject Object => this.@object ??= this.Session.Object(this.WorkspaceId);

        public DatabaseObject DatabaseObject { get; private set; }

        public IClass Class { get; set; }

        public long WorkspaceId { get; set; }

        public long? DatabaseId => this.DatabaseObject?.Id;

        public long? Version => this.DatabaseObject?.Version;

        public bool HasDatabaseChanges
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

        ISession IStrategy.Session => this.Session;

        public Session Session { get; }

        private bool ExistDatabaseObject => this.DatabaseObject != null;

        public bool CanRead(IRoleType roleType)
        {
            if (!this.ExistDatabaseObject)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Read);
            return this.DatabaseObject.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (!this.ExistDatabaseObject)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Write);
            return this.DatabaseObject.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistDatabaseObject)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, methodType, Operations.Execute);
            return this.DatabaseObject.IsPermitted(permission);
        }

        public bool Exist(IRoleType roleType)
        {
            var value = this.Get(roleType);
            
            if (roleType.ObjectType.IsComposite && roleType.IsMany)
            {
                return ((IEnumerable<IObject>)value).Any();
            }

            return value != null;
        }

        public object Get(IRoleType roleType) => roleType.Origin switch
        {
            Origin.Database => this.GetForDatabase(roleType),
            Origin.Workspace => this.GetForWorkspace(roleType),
            Origin.Session => this.GetForSession(roleType),
            _ => throw new Exception($"Unsupported origin: {roleType.Origin}"),
        };
     
        public void Set(IRoleType roleType, object value)
        {
            switch (roleType.Origin)
            {
                case Origin.Database:
                    this.SetForDatabase(roleType, value);
                    break;
                case Origin.Workspace:
                    this.Session.Workspace.Population.SetRole(this.WorkspaceId, roleType, value);
                    break;
                case Origin.Session:
                    this.Session.Population.SetRole(this.WorkspaceId, roleType, value);
                    break;
                default:
                    throw new Exception($"Unsupported origin: {roleType.Origin}");
            }
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

        public object GetAssociation(IAssociationType associationType) =>
            this.Session.GetAssociation(this.Object, associationType).FirstOrDefault();

        public IEnumerable<IObject> GetAssociations(IAssociationType associationType) =>
            this.Session.GetAssociation(this.Object, associationType);

        internal PushRequestNewObject SaveNew() => new PushRequestNewObject
        {
            NI = this.WorkspaceId.ToString(),
            T = this.Class.IdAsString,
            Roles = this.SaveRoles(),
        };

        public PushRequestObject SaveExisting() => new PushRequestObject
        {
            I = this.DatabaseId?.ToString(),
            V = this.Version.ToString(),
            Roles = this.SaveRoles(),
        };

        public void Reset()
        {
            if (this.DatabaseObject != null)
            {
                this.DatabaseObject = this.Session.Workspace.Database.Get(this.DatabaseId.Value);
            }

            this.changedRoleByRoleType = null;

            this.roleByRoleType = new Dictionary<IRoleType, object>();
        }

        public void Refresh(bool merge = false)
        {
            if (!this.HasDatabaseChanges)
            {
                this.Reset();
            }
            else
            {
                if (merge)
                {
                    if (this.DatabaseObject != null)
                    {
                        this.DatabaseObject = this.Session.Workspace.Database.Get(this.DatabaseId.Value);
                    }
                }
            }
        }
        
        private object GetForDatabase(IRoleType roleType)
        {
            if (!this.roleByRoleType.TryGetValue(roleType, out var value))
            {
                if (this.ExistDatabaseObject)
                {
                    var databaseRole = this.DatabaseObject.Roles?.FirstOrDefault(v => Equals(v.RoleType, roleType));
                    if (databaseRole?.Value != null)
                    {
                        if (roleType.ObjectType.IsUnit)
                        {
                            value = databaseRole.Value;
                        }
                        else
                        {
                            if (roleType.IsOne)
                            {
                                value = this.Session.Instantiate((long)databaseRole.Value);
                            }
                            else
                            {
                                var ids = (long[])databaseRole.Value;
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

        private object GetForWorkspace(IRoleType roleType)
        {
            this.Session.Workspace.Population.GetRole(this.WorkspaceId, roleType, out var role);
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

        private object GetForSession(IRoleType roleType)
        {
            this.Session.Population.GetRole(this.WorkspaceId, roleType, out var role);
            return role;
        }

        private void SetForDatabase(IRoleType roleType, object value)
        {
            var current = this.Get(roleType);
            if (roleType.ObjectType.IsUnit || roleType.IsOne)
            {
                if (object.Equals(current, value))
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

        public object GetAssociationForDatabase(IRoleType roleType)
        {
            if (!this.roleByRoleType.TryGetValue(roleType, out var value))
            {
                if (this.ExistDatabaseObject)
                {
                    var databaseRole = this.DatabaseObject.Roles?.FirstOrDefault(v => Equals(v.RoleType, roleType));
                    if (databaseRole?.Value != null)
                    {
                        if (roleType.ObjectType.IsUnit)
                        {
                            value = databaseRole.Value;
                        }
                        else
                        {
                            if (roleType.IsOne)
                            {
                                value = this.Session.GetForAssociation((long)databaseRole.Value);
                            }
                            else
                            {
                                var ids = (long[])databaseRole.Value;
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

                    var pushRequestRole = new PushRequestRole { T = roleType.RelationType.IdAsString };

                    if (roleType.ObjectType.IsUnit)
                    {
                        pushRequestRole.S = UnitConvert.ToString(roleValue);
                    }
                    else
                    {
                        if (roleType.IsOne)
                        {
                            var sessionRole = (IObject)roleValue;
                            pushRequestRole.S = sessionRole?.DatabaseId?.ToString() ??
                                                sessionRole?.WorkspaceId.ToString();
                        }
                        else
                        {
                            var sessionRoles = (IObject[])roleValue;
                            var roleIds = sessionRoles
                                .Select(item => item.DatabaseId?.ToString() ?? item.WorkspaceId.ToString()).ToArray();
                            if (!this.ExistDatabaseObject)
                            {
                                pushRequestRole.A = roleIds;
                            }
                            else
                            {
                                var databaseRole =
                                    this.DatabaseObject.Roles.FirstOrDefault(v => Equals(v.RoleType, roleType));
                                if (databaseRole?.Value == null)
                                {
                                    pushRequestRole.A = roleIds;
                                }
                                else
                                {
                                    if (databaseRole.Value != null)
                                    {
                                        var originalRoleIds = ((IEnumerable<long>)databaseRole.Value)
                                            .Select(v => v.ToString())
                                            .ToArray();
                                        pushRequestRole.A = roleIds.Except(originalRoleIds).ToArray();
                                        pushRequestRole.R = originalRoleIds.Except(roleIds).ToArray();
                                    }
                                    else
                                    {
                                        pushRequestRole.A = roleIds.ToArray();
                                    }
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

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

    public sealed class RemoteDatabaseStrategy : RemoteStrategy
    {
        private IObject @object;

        private Dictionary<IRoleType, object> changedRoleByRoleType;

        private Dictionary<IRoleType, object> roleByRoleType = new Dictionary<IRoleType, object>();

        private RemoteDatabaseStrategy(RemoteSession session, Identity identity, IClass @class)
        {
            this.Session = session;
            this.Identity = identity;
            this.Class = @class;
        }

        public RemoteDatabaseStrategy(RemoteSession session, IClass @class, Identity identity) : this(session, identity, @class) { }

        public RemoteDatabaseStrategy(RemoteSession session, RemoteDatabaseRoles databaseRoles, Identity identity) : this(session, identity, databaseRoles.Class) => this.DatabaseRoles = databaseRoles;

        public override RemoteSession Session { get; }

        public override IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        public override IClass Class { get; }

        public override Identity Identity { get; }

        public long? Version => this.DatabaseRoles?.Version;

        private RemoteDatabaseRoles DatabaseRoles { get; set; }

        internal bool HasDatabaseChanges
        {
            get
            {
                if (this.Class.HasDatabaseOrigin && !this.ExistDatabaseRoles)
                {
                    return true;
                }

                return this.changedRoleByRoleType != null;
            }
        }

        private bool ExistDatabaseRoles => this.DatabaseRoles != null;

        public override bool Exist(IRoleType roleType)
        {
            var value = this.Get(roleType);

            if (roleType.ObjectType.IsComposite && roleType.IsMany)
            {
                return ((IEnumerable<IObject>)value).Any();
            }

            return value != null;
        }

        public override object Get(IRoleType roleType)
        {
            if (roleType.Origin == Origin.Session)
            {
                var state = this.Session.State;
                state.GetRole(this.Identity, roleType, out var role);
                if (roleType.ObjectType.IsUnit)
                {
                    return role;
                }

                if (roleType.IsOne)
                {
                    return this.Session.Instantiate<IObject>((Identity)role);
                }

                var ids = (IEnumerable<Identity>)role;
                return ids?.Select(v => this.Session.Instantiate<IObject>(v)).ToArray() ?? this.Session.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
            }

            if (!this.roleByRoleType.TryGetValue(roleType, out var value))
            {
                if (this.ExistDatabaseRoles)
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
                                value = this.Session.Instantiate<IObject>((Identity)databaseRole);
                            }
                            else
                            {
                                var ids = (Identity[])databaseRole;
                                var array = Array.CreateInstance(roleType.ObjectType.ClrType, ids.Length);
                                for (var i = 0; i < ids.Length; i++)
                                {
                                    array.SetValue(this.Session.Instantiate<IObject>(ids[i]), i);
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

        public override void Set(IRoleType roleType, object value)
        {
            if (roleType.Origin == Origin.Session)
            {
                var population = this.Session.State;
                population.SetRole(this.Identity, roleType, value);
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

        public override void Add(IRoleType roleType, IObject value)
        {
            var roles = (IObject[])this.Get(roleType);
            if (!roles.Contains(value))
            {
                roles = new List<IObject>(roles) { value }.ToArray();
            }

            this.Set(roleType, roles);
        }

        public override void Remove(IRoleType roleType, IObject value)
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

        public override IObject GetAssociation(IAssociationType associationType)
        {
            if (associationType.Origin == Origin.Session)
            {
                var population = this.Session.State;
                population.GetAssociation(this.Identity, associationType, out var association);
                var id = (Identity)association;
                return id != null ? this.Session.Instantiate<IObject>(id) : null;
            }

            return this.Session.GetAssociation(this.Object, associationType).FirstOrDefault();
        }

        public override IEnumerable<IObject> GetAssociations(IAssociationType associationType)
        {
            if (associationType.Origin == Origin.Session)
            {
                var population = this.Session.State;
                population.GetAssociation(this.Identity, associationType, out var association);
                var ids = (IEnumerable<Identity>)association;
                return ids?.Select(v => this.Session.Instantiate<IObject>(v)).ToArray() ?? Array.Empty<IObject>();
            }

            return this.Session.GetAssociation(this.Object, associationType);
        }

        public override bool CanRead(IRoleType roleType)
        {
            if (!this.ExistDatabaseRoles)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Read);
            return this.DatabaseRoles.IsPermitted(permission);
        }

        public override bool CanWrite(IRoleType roleType)
        {
            if (!this.ExistDatabaseRoles)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Write);
            return this.DatabaseRoles.IsPermitted(permission);
        }

        public override bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistDatabaseRoles)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, methodType, Operations.Execute);
            return this.DatabaseRoles.IsPermitted(permission);
        }

        internal PushRequestNewObject SaveNew() => new PushRequestNewObject
        {
            NewWorkspaceId = ((DatabaseIdentity)this.Identity)?.WorkspaceId.ToString(),
            ObjectType = this.Class.IdAsString,
            Roles = this.SaveRoles(),
        };

        internal PushRequestObject SaveExisting() => new PushRequestObject
        {
            DatabaseId = ((DatabaseIdentity)this.Identity).DatabaseId.ToString(),
            Version = this.Version.ToString(),
            Roles = this.SaveRoles(),
        };

        internal void Reset()
        {
            if (this.DatabaseRoles != null)
            {
                this.DatabaseRoles = this.Session.Workspace.Database.Get(this.Identity);
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
                        this.DatabaseRoles = this.Session.Workspace.Database.Get(this.Identity);
                    }
                }
            }
        }

        internal object GetDatabase(IRoleType roleType)
        {
            if (!this.roleByRoleType.TryGetValue(roleType, out var value))
            {
                if (this.ExistDatabaseRoles)
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
                                value = this.Session.GetForAssociation((Identity)databaseRole);
                            }
                            else
                            {
                                var ids = (Identity[])databaseRole;
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
                            var identity = (DatabaseIdentity)sessionRole?.Identity;
                            pushRequestRole.SetRole = identity?.DatabaseId?.ToString() ?? identity?.WorkspaceId.ToString();
                        }
                        else
                        {
                            var sessionRoles = (IObject[])roleValue;
                            var roleIds = sessionRoles
                                .Select(item =>
                                {
                                    var identity = (DatabaseIdentity)item?.Identity;
                                    return identity.DatabaseId?.ToString() ?? identity.WorkspaceId.ToString();
                                }).ToArray();
                            if (!this.ExistDatabaseRoles)
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
                                    var originalRoleIds = ((IEnumerable<Identity>)databaseRole)
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

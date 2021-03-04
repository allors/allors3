// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Protocol.Json.Api.Push;
    using Workspace;
    using Remote;
    using Workspace.Meta;

    internal sealed class RemoteDatabaseState
    {
        private readonly RemoteStrategy strategy;

        private Dictionary<IRoleType, object> changedRoleByRoleType;

        private RemoteDatabaseObject databaseObject;

        private RemoteWorkspaceObject changeSetDatabaseObject;

        internal RemoteDatabaseState(RemoteStrategy strategy, RemoteDatabaseObject databaseObject = null)
        {
            this.strategy = strategy;
            this.databaseObject = databaseObject ?? this.Database.Get(this.Identity);
        }

        internal bool HasDatabaseChanges => this.databaseObject == null || this.changedRoleByRoleType != null;

        private bool ExistDatabaseRoles => this.databaseObject != null;

        internal long Version => this.databaseObject?.Version ?? 0;

        private Identity Identity => this.strategy.Identity;

        private IClass Class => this.strategy.Class;

        private RemoteSession Session => this.strategy.Session;

        private RemoteDatabase Database => this.Session.Database;

        public bool CanRead(IRoleType roleType)
        {
            if (!this.ExistDatabaseRoles)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Read);
            return this.databaseObject.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (!this.ExistDatabaseRoles)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Write);
            return this.databaseObject.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistDatabaseRoles)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, methodType, Operations.Execute);
            return this.databaseObject.IsPermitted(permission);
        }


        internal object GetRole(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {

                if (this.changedRoleByRoleType == null || !this.changedRoleByRoleType.TryGetValue(roleType, out var unit))
                {
                    unit = this.databaseObject?.GetRole(roleType);
                }

                return unit;
            }
            else
            {
                if (roleType.IsOne)
                {
                    if (this.changedRoleByRoleType == null || !this.changedRoleByRoleType.TryGetValue(roleType, out var workspaceRole))
                    {
                        workspaceRole = (Identity)this.databaseObject?.GetRole(roleType);
                    }

                    return this.Session.Instantiate<IObject>((Identity)workspaceRole);
                }

                if (this.changedRoleByRoleType == null || !this.changedRoleByRoleType.TryGetValue(roleType, out var identities))
                {
                    identities = (Identity[])this.databaseObject?.GetRole(roleType);
                }

                var ids = (Identity[])identities;

                if (ids == null)
                {
                    return this.Session.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
                }

                var array = Array.CreateInstance(roleType.ObjectType.ClrType, ids.Length);
                for (var i = 0; i < ids.Length; i++)
                {
                    array.SetValue(this.Session.Instantiate<IObject>(ids[i]), i);
                }

                return array;
            }
        }

        internal void SetRole(IRoleType roleType, object value)
        {
            var current = this.GetRole(roleType);
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

            if (roleType.ObjectType.IsUnit)
            {
                this.changedRoleByRoleType[roleType] = value;
            }
            else
            {
                if (roleType.IsOne)
                {
                    this.changedRoleByRoleType[roleType] = ((IObject)value)?.Identity;
                }
                else
                {
                    this.changedRoleByRoleType[roleType] = ((IEnumerable<object>)value).Select(v => ((IObject)v).Identity).ToArray();
                }
            }

            this.Session.OnChange(this);
        }

        internal void Reset()
        {
            this.databaseObject = this.Database.Get(this.Identity);
            this.changedRoleByRoleType = null;
        }

        internal void PushResponse(RemoteDatabaseObject databaseObject) => this.databaseObject = databaseObject;

        internal PushRequestNewObject SaveNew() => new PushRequestNewObject
        {
            NewWorkspaceId = this.Identity.Id.ToString(),
            ObjectType = this.Class.IdAsString,
            Roles = this.SaveRoles(),
        };

        internal PushRequestObject SaveExisting() => new PushRequestObject
        {
            DatabaseId = this.Identity.Id.ToString(),
            Version = this.Version.ToString(),
            Roles = this.SaveRoles(),
        };

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
                            var identity = (Identity)roleValue;
                            pushRequestRole.SetRole = identity?.Id.ToString();
                        }
                        else
                        {
                            var sessionRoles = (Identity[])roleValue;
                            var roleIds = sessionRoles.Select(v => v.Id.ToString()).ToArray();
                            if (!this.ExistDatabaseRoles)
                            {
                                pushRequestRole.AddRole = roleIds;
                            }
                            else
                            {
                                var databaseRole = (Identity[])this.databaseObject.GetRole(roleType);
                                if (databaseRole == null)
                                {
                                    pushRequestRole.AddRole = roleIds;
                                }
                                else
                                {
                                    var originalRoleIds = databaseRole
                                        .Select(v => v.Id.ToString())
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

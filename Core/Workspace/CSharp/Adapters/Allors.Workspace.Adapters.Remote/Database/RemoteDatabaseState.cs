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

        private RemoteDatabaseObject databaseObject;
        private Dictionary<IRelationType, object> changedRoleByRelationType;

        private RemoteDatabaseObject previousDatabaseObject;
        private Dictionary<IRelationType, object> previousChangedRoleByRelationType;

        internal RemoteDatabaseState(RemoteStrategy strategy, RemoteDatabaseObject databaseObject = null)
        {
            this.strategy = strategy;
            this.databaseObject = databaseObject ?? this.Database.Get(this.Identity);
            this.previousDatabaseObject = this.databaseObject;
        }

        internal bool HasDatabaseChanges => this.databaseObject == null || this.changedRoleByRelationType != null;

        private bool ExistDatabaseObjects => this.databaseObject != null;

        internal long Version => this.databaseObject?.Version ?? 0;

        private long Identity => this.strategy.Identity;

        private IClass Class => this.strategy.Class;

        private RemoteSession Session => this.strategy.Session;

        private RemoteDatabase Database => this.Session.Database;

        public bool CanRead(IRoleType roleType)
        {
            if (!this.ExistDatabaseObjects)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Read);
            return this.databaseObject.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (!this.ExistDatabaseObjects)
            {
                return true;
            }

            var permission = this.Session.Workspace.Database.GetPermission(this.Class, roleType, Operations.Write);
            return this.databaseObject.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistDatabaseObjects)
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

                if (this.changedRoleByRelationType == null || !this.changedRoleByRelationType.TryGetValue(roleType.RelationType, out var unit))
                {
                    unit = this.databaseObject?.GetRole(roleType);
                }

                return unit;
            }

            if (roleType.IsOne)
            {
                if (this.changedRoleByRelationType != null &&
                    this.changedRoleByRelationType.TryGet<RemoteStrategy>(roleType.RelationType, out var workspaceRole))
                {
                    return workspaceRole?.Object;
                }

                var identity = (long?)this.databaseObject?.GetRole(roleType);
                workspaceRole = this.Session.GetStrategy(identity);

                return workspaceRole?.Object;
            }

            if (this.changedRoleByRelationType != null &&
                this.changedRoleByRelationType.TryGet<RemoteStrategy[]>(roleType.RelationType, out var workspaceRoles))
            {
                return workspaceRoles != null ? workspaceRoles.Select(v => v.Object).ToArray() : Array.Empty<IObject>();
            }

            var identities = (long[])this.databaseObject?.GetRole(roleType);
            return identities == null ? Array.Empty<IObject>() : identities.Select(v => this.Session.Get<IObject>(v)).ToArray();
        }

        internal void SetRole(IRoleType roleType, object value)
        {
            if (roleType.ObjectType.IsUnit)
            {
                this.SetUnitRole(roleType, value);
            }
            else
            {
                if (roleType.IsOne)
                {
                    this.SetCompositeRole(roleType, value);
                }
                else
                {
                    this.SetCompositesRole(roleType, value);
                }
            }
        }

        internal void Reset()
        {
            this.databaseObject = this.Database.Get(this.Identity);
            this.changedRoleByRelationType = null;
        }

        internal void Merge() => this.databaseObject = this.Database.Get(this.Identity);

        internal void Checkpoint(RemoteChangeSet changeSet)
        {
            // Same workspace object
            if (this.databaseObject.Identity == this.previousDatabaseObject.Identity)
            {
                // No previous changed roles
                if (this.previousChangedRoleByRelationType == null)
                {
                    if (this.changedRoleByRelationType != null)
                    {
                        // Changed roles
                        foreach (var kvp in this.changedRoleByRelationType)
                        {
                            var relationType = kvp.Key;
                            var cooked = kvp.Value;
                            var raw = this.databaseObject.GetRole(relationType.RoleType);

                            changeSet.DiffCookedWithRaw(this.strategy, relationType, cooked, raw);
                        }
                    }
                }
                // Previous changed roles
                else
                {
                    foreach (var kvp in this.changedRoleByRelationType)
                    {
                        var relationType = kvp.Key;
                        var role = kvp.Value;

                        this.previousChangedRoleByRelationType.TryGetValue(relationType, out var previousRole);
                        changeSet.DiffCookedWithCooked(this.strategy, relationType, role, previousRole);
                    }
                }
            }
            // Different workspace objects
            else
            {
                var hasPreviousCooked = this.previousChangedRoleByRelationType != null;
                var hasCooked = this.changedRoleByRelationType != null;

                foreach (var roleType in this.Class.WorkspaceRoleTypes)
                {
                    var relationType = roleType.RelationType;

                    if (hasPreviousCooked && this.previousChangedRoleByRelationType.TryGetValue(relationType, out var previousCooked))
                    {
                        if (hasCooked && this.changedRoleByRelationType.TryGetValue(relationType, out var cooked))
                        {
                            changeSet.DiffCookedWithCooked(this.strategy, relationType, cooked, previousCooked);
                        }
                        else
                        {
                            var raw = this.databaseObject.GetRole(roleType);
                            changeSet.DiffRawWithCooked(this.strategy, relationType, raw, previousCooked);
                        }
                    }
                    else
                    {
                        var previousRaw = this.previousDatabaseObject?.GetRole(roleType);
                        if (hasCooked && this.changedRoleByRelationType.TryGetValue(relationType, out var cooked) == true)
                        {
                            changeSet.DiffCookedWithRaw(this.strategy, relationType, cooked, previousRaw);
                        }
                        else
                        {
                            var raw = this.databaseObject.GetRole(roleType);
                            changeSet.DiffRawWithRaw(this.strategy, relationType, raw, previousRaw);
                        }
                    }
                }
            }

            this.previousDatabaseObject = this.databaseObject;
            this.previousChangedRoleByRelationType = this.changedRoleByRelationType;
        }



        internal void PushResponse(RemoteDatabaseObject newDatabaseObject) => this.databaseObject = newDatabaseObject;

        internal PushRequestNewObject SaveNew() => new PushRequestNewObject
        {
            NewWorkspaceId = this.Identity.ToString(),
            ObjectType = this.Class.IdAsString,
            Roles = this.SaveRoles(),
        };

        internal PushRequestObject SaveExisting() => new PushRequestObject
        {
            DatabaseId = this.Identity.ToString(),
            Version = this.Version.ToString(),
            Roles = this.SaveRoles(),
        };

        private PushRequestRole[] SaveRoles()
        {
            if (this.changedRoleByRelationType?.Count > 0)
            {
                var saveRoles = new List<PushRequestRole>();

                foreach (var keyValuePair in this.changedRoleByRelationType)
                {
                    var relationType = keyValuePair.Key;
                    var roleValue = keyValuePair.Value;

                    var pushRequestRole = new PushRequestRole { RelationType = relationType.IdAsString };

                    if (relationType.RoleType.ObjectType.IsUnit)
                    {
                        pushRequestRole.SetRole = UnitConvert.ToString(roleValue);
                    }
                    else
                    {
                        if (relationType.RoleType.IsOne)
                        {
                            pushRequestRole.SetRole = ((RemoteStrategy)roleValue)?.Identity.ToString();
                        }
                        else
                        {
                            var roleIds = ((RemoteStrategy[])roleValue).Select(v => v.Identity.ToString()).ToArray();
                            if (!this.ExistDatabaseObjects)
                            {
                                pushRequestRole.AddRole = roleIds;
                            }
                            else
                            {
                                var databaseRole = (long[])this.databaseObject.GetRole(relationType.RoleType);
                                if (databaseRole == null)
                                {
                                    pushRequestRole.AddRole = roleIds;
                                }
                                else
                                {
                                    var originalRoleIds = databaseRole
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

        private void SetUnitRole(IRoleType roleType, object role)
        {
            var previousRole = this.GetRole(roleType);
            if (Equals(previousRole, role))
            {
                return;
            }

            this.changedRoleByRelationType ??= new Dictionary<IRelationType, object>();
            this.changedRoleByRelationType[roleType.RelationType] = role ;

            this.Session.OnChange(this);
        }

        private void SetCompositeRole(IRoleType roleType, object value)
        {
            var role = (IObject)value;
            var previousRole = (IObject)this.GetRole(roleType);
            if (Equals(previousRole, role))
            {
                return;
            }

            // OneToOne
            if (previousRole != null)
            {
                var associationType = roleType.AssociationType;
                if (associationType.IsOne)
                {
                    var previousAssociationObject = this.Session.GetAssociation((RemoteStrategy)previousRole.Strategy, associationType).FirstOrDefault();
                    previousAssociationObject?.Strategy.Set(roleType, null);
                }
            }

            this.changedRoleByRelationType ??= new Dictionary<IRelationType, object>();
            this.changedRoleByRelationType[roleType.RelationType] = role?.Strategy;

            this.Session.OnChange(this);
        }

        private void SetCompositesRole(IRoleType roleType, object value)
        {
            var previousRole = ((IObject[])this.GetRole(roleType));

            var role = Array.Empty<IObject>();
            if (value != null)
            {
                role = ((IEnumerable<IObject>)value).ToArray();
            }

            var addedRoles = role.Except(previousRole).ToArray();
            var removedRoles = previousRole.Except(role).ToArray();

            if (addedRoles.Length == 0 && removedRoles.Length == 0)
            {
                return;
            }

            // OneToMany
            if (previousRole.Length > 0)
            {
                var associationType = roleType.AssociationType;
                if (associationType.IsOne)
                {
                    var addedObjects = this.Session.Get<IObject>(addedRoles);
                    foreach (var addedObject in addedObjects)
                    {
                        var previousAssociationObject = this.Session.GetAssociation((RemoteStrategy)addedObject.Strategy, associationType).FirstOrDefault();
                        previousAssociationObject?.Strategy.Remove(roleType, addedObject);
                    }
                }
            }

            this.changedRoleByRelationType ??= new Dictionary<IRelationType, object>();
            this.changedRoleByRelationType[roleType.RelationType] = role.Select(v => (RemoteStrategy)v.Strategy).ToArray();

            this.Session.OnChange(this);
        }

        public bool IsAssociationForRole(IRoleType roleType, RemoteStrategy forRole)
        {
            if (roleType.ObjectType.IsUnit)
            {
               return false;
            }

            if (roleType.IsOne)
            {
                if (this.changedRoleByRelationType != null &&
                    this.changedRoleByRelationType.TryGet<RemoteStrategy>(roleType.RelationType, out var workspaceRole))
                {
                    return workspaceRole?.Equals(forRole) == true;
                }

                var identity = (long?)this.databaseObject?.GetRole(roleType);
                return identity?.Equals(forRole.Identity) == true;
            }

            if (this.changedRoleByRelationType != null &&
                this.changedRoleByRelationType.TryGet<RemoteStrategy[]>(roleType.RelationType, out var workspaceRoles))
            {
                return workspaceRoles?.Contains(forRole) == true;
            }

            var identities = (long[])this.databaseObject?.GetRole(roleType);
            return identities?.Contains(forRole.Identity) == true;
        }
    }
}

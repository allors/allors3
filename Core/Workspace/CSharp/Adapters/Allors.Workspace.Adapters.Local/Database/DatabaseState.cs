// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    internal sealed class DatabaseState
    {
        private readonly Strategy strategy;

        private DatabaseObject databaseObject;

        private DatabaseObject previousDatabaseObject;
        private Dictionary<IRelationType, object> previousChangedRoleByRelationType;

        internal DatabaseState(Strategy strategy, DatabaseObject databaseObject = null)
        {
            this.strategy = strategy;
            this.databaseObject = databaseObject ?? this.DatabaseAdapter.Get(this.Identity);
            this.previousDatabaseObject = this.databaseObject;
        }

        internal bool HasDatabaseChanges => this.databaseObject == null || this.ChangedRoleByRelationType != null;

        internal Dictionary<IRelationType, object> ChangedRoleByRelationType { get; private set; }

        private bool ExistDatabaseObjects => this.databaseObject != null;

        internal long Version => this.databaseObject?.Version ?? 0;

        private long Identity => this.strategy.Id;

        private IClass Class => this.strategy.Class;

        private Session Session => this.strategy.Session;

        private DatabaseAdapter DatabaseAdapter => this.Session.DatabaseAdapter;

        public bool CanRead(IRoleType roleType)
        {
            if (!this.ExistDatabaseObjects)
            {
                return true;
            }

            var permission = this.Session.Workspace.DatabaseAdapter.GetPermission(this.Class, roleType, Operations.Read);
            return this.databaseObject.IsPermitted(permission);
        }

        public bool CanWrite(IRoleType roleType)
        {
            if (!this.ExistDatabaseObjects)
            {
                return true;
            }

            var permission = this.Session.Workspace.DatabaseAdapter.GetPermission(this.Class, roleType, Operations.Write);
            return this.databaseObject.IsPermitted(permission);
        }

        public bool CanExecute(IMethodType methodType)
        {
            if (!this.ExistDatabaseObjects)
            {
                return true;
            }

            var permission = this.Session.Workspace.DatabaseAdapter.GetPermission(this.Class, methodType, Operations.Execute);
            return this.databaseObject.IsPermitted(permission);
        }

        internal object GetRole(IRoleType roleType)
        {
            if (!this.CanRead(roleType))
            {
                return null;
            }

            if (roleType.ObjectType.IsUnit)
            {

                if (this.ChangedRoleByRelationType == null || !this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var unit))
                {
                    unit = this.databaseObject?.GetRole(roleType);
                }

                return unit;
            }

            if (roleType.IsOne)
            {
                if (this.ChangedRoleByRelationType != null &&
                    this.ChangedRoleByRelationType.TryGet<Strategy>(roleType.RelationType, out var workspaceRole))
                {
                    return workspaceRole?.Object;
                }

                var identity = (long?)this.databaseObject?.GetRole(roleType);
                workspaceRole = this.Session.GetStrategy(identity);

                return workspaceRole?.Object;
            }

            if (this.ChangedRoleByRelationType != null &&
                this.ChangedRoleByRelationType.TryGet<Strategy[]>(roleType.RelationType, out var workspaceRoles))
            {
                return workspaceRoles != null ? workspaceRoles.Select(v => v.Object).ToArray() : Array.Empty<IObject>();
            }

            var identities = (long[])this.databaseObject?.GetRole(roleType);
            return identities == null ? Array.Empty<IObject>() : identities.Select(v => this.Session.Get<IObject>(v)).ToArray();
        }

        internal void SetUnitRole(IRoleType roleType, object role)
        {
            var previousRole = this.GetRole(roleType);
            if (Equals(previousRole, role))
            {
                return;
            }

            this.ChangedRoleByRelationType ??= new Dictionary<IRelationType, object>();
            this.ChangedRoleByRelationType[roleType.RelationType] = role;

            this.Session.OnChange(this);
        }

        internal void SetCompositeRole(IRoleType roleType, object value)
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
                    var previousAssociationObject = this.Session.GetAssociation<IObject>((Strategy)previousRole.Strategy, associationType).FirstOrDefault();
                    previousAssociationObject?.Strategy.Set(roleType, null);
                }
            }

            this.ChangedRoleByRelationType ??= new Dictionary<IRelationType, object>();
            this.ChangedRoleByRelationType[roleType.RelationType] = role?.Strategy;

            this.Session.OnChange(this);
        }

        internal void SetCompositesRole(IRoleType roleType, object value)
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
                        var previousAssociationObject = this.Session.GetAssociation<IObject>((Strategy)addedObject.Strategy, associationType).FirstOrDefault();
                        previousAssociationObject?.Strategy.Remove(roleType, addedObject);
                    }
                }
            }

            this.ChangedRoleByRelationType ??= new Dictionary<IRelationType, object>();
            this.ChangedRoleByRelationType[roleType.RelationType] = role.Select(v => (Strategy)v.Strategy).ToArray();

            this.Session.OnChange(this);
        }

        internal void Reset()
        {
            this.databaseObject = this.DatabaseAdapter.Get(this.Identity);
            this.ChangedRoleByRelationType = null;
        }

        internal void Checkpoint(ChangeSet changeSet)
        {
            // Same workspace object
            if (this.databaseObject.Version == this.previousDatabaseObject.Version)
            {
                // No previous changed roles
                if (this.previousChangedRoleByRelationType == null)
                {
                    if (this.ChangedRoleByRelationType != null)
                    {
                        // Changed roles
                        foreach (var kvp in this.ChangedRoleByRelationType)
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
                    foreach (var kvp in this.ChangedRoleByRelationType)
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
                var hasCooked = this.ChangedRoleByRelationType != null;

                foreach (var roleType in this.Class.WorkspaceRoleTypes)
                {
                    var relationType = roleType.RelationType;

                    if (hasPreviousCooked && this.previousChangedRoleByRelationType.TryGetValue(relationType, out var previousCooked))
                    {
                        if (hasCooked && this.ChangedRoleByRelationType.TryGetValue(relationType, out var cooked))
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
                        if (hasCooked && this.ChangedRoleByRelationType.TryGetValue(relationType, out var cooked))
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
            this.previousChangedRoleByRelationType = this.ChangedRoleByRelationType;
        }

        internal void PushResponse(DatabaseObject newDatabaseObject) => this.databaseObject = newDatabaseObject;

        internal bool IsAssociationForRole(IRoleType roleType, Strategy forRole)
        {
            if (roleType.ObjectType.IsUnit)
            {
                return false;
            }

            if (roleType.IsOne)
            {
                if (this.ChangedRoleByRelationType != null &&
                    this.ChangedRoleByRelationType.TryGet<Strategy>(roleType.RelationType, out var workspaceRole))
                {
                    return workspaceRole?.Equals(forRole) == true;
                }

                var identity = (long?)this.databaseObject?.GetRole(roleType);
                return identity?.Equals(forRole.Id) == true;
            }

            if (this.ChangedRoleByRelationType != null &&
                this.ChangedRoleByRelationType.TryGet<Strategy[]>(roleType.RelationType, out var workspaceRoles))
            {
                return workspaceRoles?.Contains(forRole) == true;
            }

            var identities = (long[])this.databaseObject?.GetRole(roleType);
            return identities?.Contains(forRole.Id) == true;
        }

        internal IEnumerable<IRelationType> Diff()
        {
            if (this.ChangedRoleByRelationType == null)
            {
                yield break;
            }

            foreach (var kvp in this.ChangedRoleByRelationType)
            {
                yield return kvp.Key;
            }
        }
    }
}

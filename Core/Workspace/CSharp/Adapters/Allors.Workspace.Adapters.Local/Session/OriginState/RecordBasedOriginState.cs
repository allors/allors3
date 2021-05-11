// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Numbers;

    internal abstract class RecordBasedOriginState
    {

        protected RecordBasedOriginState(Strategy strategy, IRecord record)
        {
            this.Strategy = strategy;
            this.Record = record;
            this.PreviousRecord = this.Record;
        }

        protected Strategy Strategy { get; }

        protected IRecord Record { get; set; }

        protected IRecord PreviousRecord { get; set; }

        protected internal Dictionary<IRelationType, object> ChangedRoleByRelationType { get; set; }

        protected Dictionary<IRelationType, object> PreviousChangedRoleByRelationType { get; set; }

        public abstract bool HasChanges { get; }

        protected long Id => this.Strategy.Id;

        protected IClass Class => this.Strategy.Class;

        protected Session Session => this.Strategy.Session;

        protected Workspace Workspace => this.Session.Workspace;

        internal abstract void Reset();

        internal object GetRole(IRoleType roleType)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var role))
            {
                return role;
            }

            return this.Record?.GetRole(roleType);
        }

        internal void SetUnitRole(IRoleType roleType, object role) => this.SetChangedRole(roleType, role);

        internal void SetCompositeRole(IRoleType roleType, long? role)
        {
            var previousRole = (long?)this.GetRole(roleType);

            if (previousRole == role)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            if (associationType.IsOne && role.HasValue)
            {
                var previousAssociationObject = this.Session.GetAssociation<IObject>(role.Value, associationType).FirstOrDefault();

                this.SetChangedRole(roleType, role);

                if (associationType.IsOne && previousAssociationObject != null)
                {
                    // OneToOne
                    previousAssociationObject?.Strategy.Set(roleType, null);
                }
            }
            else
            {
                this.SetChangedRole(roleType, role);
            }
        }

        internal void SetCompositesRole(IRoleType roleType, object role)
        {
            var previousRole = this.GetRole(roleType);

            this.SetChangedRole(roleType, role);

            var associationType = roleType.AssociationType;
            if (associationType.IsMany)
            {
                return;
            }

            // OneToMany
            var addedRoles = this.Numbers.Except(role, previousRole);
            foreach (var addedRole in this.Numbers.Enumerate(addedRoles))
            {
                var previousAssociationObject = this.Session.GetAssociation<IObject>(addedRole, associationType).FirstOrDefault();
                previousAssociationObject?.Strategy.Set(roleType, null);
            }
        }

        internal void AddRole(IRoleType roleType, long roleToAdd)
        {
            var previousRole = this.GetRole(roleType);

            if (this.Numbers.Contains(previousRole, roleToAdd))
            {
                return;
            }

            var role = this.Numbers.Add(previousRole, roleToAdd);

            this.SetChangedRole(roleType, role);

            var associationType = roleType.AssociationType;
            if (associationType.IsMany)
            {
                return;
            }

            // OneToMany
            var previousAssociationObject = this.Session.GetAssociation<IObject>(roleToAdd, associationType).FirstOrDefault();
            previousAssociationObject?.Strategy.Set(roleType, null);
        }

        internal void RemoveRole(IRoleType roleType, long roleToRemove)
        {
            var previousRole = this.GetRole(roleType);

            if (!this.Numbers.Contains(previousRole, roleToRemove))
            {
                return;
            }

            var role = this.Numbers.Remove(previousRole, roleToRemove);

            this.SetChangedRole(roleType, role);
        }

        protected INumbers Numbers => this.Strategy.Numbers;

        internal void Push()
        {
            if (this.HasChanges)
            {
                this.Workspace.Push(this.Id, this.Class, this.Record?.Version ?? 0, this.ChangedRoleByRelationType);
            }

            this.Reset();
        }

        internal void Checkpoint(ChangeSet changeSet)
        {
            // Same workspace object
            if (this.Record.Version == this.PreviousRecord.Version)
            {
                // No previous changed roles
                if (this.PreviousChangedRoleByRelationType == null)
                {
                    if (this.ChangedRoleByRelationType != null)
                    {
                        // Changed roles
                        foreach (var kvp in this.ChangedRoleByRelationType)
                        {
                            var relationType = kvp.Key;
                            var cooked = kvp.Value;
                            var raw = this.Record.GetRole(relationType.RoleType);

                            changeSet.DiffCookedWithRaw(this.Strategy, relationType, cooked, raw);
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

                        _ = this.PreviousChangedRoleByRelationType.TryGetValue(relationType, out var previousRole);
                        changeSet.DiffCookedWithCooked(this.Strategy, relationType, role, previousRole);
                    }
                }
            }
            // Different workspace objects
            else
            {
                var hasPreviousCooked = this.PreviousChangedRoleByRelationType != null;
                var hasCooked = this.ChangedRoleByRelationType != null;

                foreach (var roleType in this.Class.WorkspaceRoleTypes)
                {
                    var relationType = roleType.RelationType;

                    if (hasPreviousCooked && this.PreviousChangedRoleByRelationType.TryGetValue(relationType, out var previousCooked))
                    {
                        if (hasCooked && this.ChangedRoleByRelationType.TryGetValue(relationType, out var cooked))
                        {
                            changeSet.DiffCookedWithCooked(this.Strategy, relationType, cooked, previousCooked);
                        }
                        else
                        {
                            var raw = this.Record.GetRole(roleType);
                            changeSet.DiffRawWithCooked(this.Strategy, relationType, raw, previousCooked);
                        }
                    }
                    else
                    {
                        var previousRaw = this.PreviousRecord?.GetRole(roleType);
                        if (hasCooked && this.ChangedRoleByRelationType.TryGetValue(relationType, out var cooked))
                        {
                            changeSet.DiffCookedWithRaw(this.Strategy, relationType, cooked, previousRaw);
                        }
                        else
                        {
                            var raw = this.Record.GetRole(roleType);
                            changeSet.DiffRawWithRaw(this.Strategy, relationType, raw, previousRaw);
                        }
                    }
                }
            }

            this.PreviousRecord = this.Record;
            this.PreviousChangedRoleByRelationType = this.ChangedRoleByRelationType;
        }

        public bool IsAssociationForRole(IRoleType roleType, long forRole)
        {
            if (roleType.ObjectType.IsUnit)
            {
                return false;
            }

            var role = this.GetRole(roleType);

            if (roleType.IsOne)
            {
                return (long?)role == forRole;
            }

            return this.Numbers.Contains(role, forRole);
        }

        private void SetChangedRole(IRoleType roleType, object role)
        {
            this.ChangedRoleByRelationType ??= new Dictionary<IRelationType, object>();
            this.ChangedRoleByRelationType[roleType.RelationType] = role;
            this.OnChange();
        }

        protected abstract void OnChange();
    }
}

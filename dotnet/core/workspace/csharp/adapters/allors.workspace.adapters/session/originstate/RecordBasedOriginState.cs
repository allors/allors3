// <copyright file="RecordBasedOriginState.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Numbers;

    public abstract class RecordBasedOriginState
    {
        public abstract Strategy Strategy { get; }

        protected bool HasChanges => this.Record == null || this.ChangedRoleByRelationType?.Count > 0;

        protected abstract IEnumerable<IRoleType> RoleTypes { get; }

        protected abstract IRecord Record { get; }

        protected IRecord PreviousRecord { get; set; }

        public Dictionary<IRelationType, object> ChangedRoleByRelationType { get; protected set; }

        private Dictionary<IRelationType, object> PreviousChangedRoleByRelationType { get; set; }

        public object GetRole(IRoleType roleType)
        {
            if (this.ChangedRoleByRelationType != null &&
                this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var role))
            {
                return role;
            }

            return this.Record?.GetRole(roleType);
        }

        public void SetUnitRole(IRoleType roleType, object role) => this.SetChangedRole(roleType, role);

        public void SetCompositeRole(IRoleType roleType, long? role)
        {
            var previousRole = (long?)this.GetRole(roleType);

            if (previousRole == role)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            if (associationType.IsOne && role.HasValue)
            {
                var previousAssociationObject =
                    this.Session.GetAssociation<IObject>(role.Value, associationType).FirstOrDefault();

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

        public void AddCompositeRole(IRoleType roleType, long roleToAdd)
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
            var previousAssociationObject =
                this.Session.GetAssociation<IObject>(roleToAdd, associationType).FirstOrDefault();
            previousAssociationObject?.Strategy.Set(roleType, null);
        }

        public void RemoveCompositeRole(IRoleType roleType, long roleToRemove)
        {
            var previousRole = this.GetRole(roleType);

            if (!this.Numbers.Contains(previousRole, roleToRemove))
            {
                return;
            }

            var role = this.Numbers.Remove(previousRole, roleToRemove);

            this.SetChangedRole(roleType, role);
        }

        public void SetCompositesRole(IRoleType roleType, object role)
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
                var previousAssociationObject =
                    this.Session.GetAssociation<IObject>(addedRole, associationType).FirstOrDefault();
                previousAssociationObject?.Strategy.Set(roleType, null);
            }
        }

        public void Checkpoint(ChangeSet changeSet)
        {
            // Same record
            if (this.PreviousRecord == null || this.Record == null ||
                this.Record.Version == this.PreviousRecord.Version)
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
                            var current = kvp.Value;
                            var previous = this.Record?.GetRole(relationType.RoleType);

                            changeSet.Diff(this.Strategy, relationType, current, previous);
                        }
                    }
                }
                // Previous changed roles
                else
                {
                    foreach (var kvp in this.ChangedRoleByRelationType)
                    {
                        var relationType = kvp.Key;
                        var current = kvp.Value;

                        _ = this.PreviousChangedRoleByRelationType.TryGetValue(relationType, out var previous);
                        changeSet.Diff(this.Strategy, relationType, current, previous);
                    }
                }
            }
            // Different record
            else
            {
                foreach (var roleType in this.RoleTypes)
                {
                    var relationType = roleType.RelationType;

                    object previous = null;
                    object current = null;

                    if (this.PreviousChangedRoleByRelationType?.TryGetValue(relationType, out previous) == true)
                    {
                        if (this.ChangedRoleByRelationType?.TryGetValue(relationType, out current) == true)
                        {
                            changeSet.Diff(this.Strategy, relationType, current, previous);
                        }
                        else
                        {
                            current = this.Record.GetRole(roleType);
                            changeSet.Diff(this.Strategy, relationType, current, previous);
                        }
                    }
                    else
                    {
                        previous = this.PreviousRecord?.GetRole(roleType);
                        if (this.ChangedRoleByRelationType?.TryGetValue(relationType, out current) == true)
                        {
                            changeSet.Diff(this.Strategy, relationType, current, previous);
                        }
                        else
                        {
                            current = this.Record.GetRole(roleType);
                            changeSet.Diff(this.Strategy, relationType, current, previous);
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

        protected abstract void OnChange();

        private void SetChangedRole(IRoleType roleType, object role)
        {
            this.ChangedRoleByRelationType ??= new Dictionary<IRelationType, object>();
            this.ChangedRoleByRelationType[roleType.RelationType] = role;
            this.OnChange();
        }

        #region Proxy Properties

        protected long Id => this.Strategy.Id;

        protected IClass Class => this.Strategy.Class;

        protected Session Session => this.Strategy.Session;

        protected Workspace Workspace => this.Session.Workspace;

        private INumbers Numbers => this.Strategy.Session.Workspace.Numbers;

        #endregion
    }
}

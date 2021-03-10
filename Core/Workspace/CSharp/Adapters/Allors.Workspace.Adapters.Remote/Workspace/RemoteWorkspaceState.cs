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
    using Meta;

    internal sealed class RemoteWorkspaceState
    {
        private readonly RemoteStrategy strategy;

        private RemoteWorkspaceObject workspaceObject;
        private Dictionary<IRelationType, object> changedRoleByRoleType;

        private RemoteWorkspaceObject previousWorkspaceObject;
        private Dictionary<IRelationType, object> previousChangedRoleByRoleType;

        internal RemoteWorkspaceState(RemoteStrategy strategy)
        {
            this.strategy = strategy;
            this.workspaceObject = this.Workspace.Get(this.Identity);
            this.previousWorkspaceObject = this.workspaceObject;
        }

        internal bool HasWorkspaceChanges => this.changedRoleByRoleType != null;

        private long Identity => this.strategy.Identity;

        private IClass Class => this.strategy.Class;

        private RemoteSession Session => this.strategy.Session;

        private RemoteWorkspace Workspace => this.Session.Workspace;

        internal object GetRole(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {

                if (this.changedRoleByRoleType == null || !this.changedRoleByRoleType.TryGetValue(roleType.RelationType, out var unit))
                {
                    unit = this.workspaceObject?.GetRole(roleType);
                }

                return unit;
            }

            if (roleType.IsOne)
            {
                if (this.changedRoleByRoleType != null &&
                    this.changedRoleByRoleType.TryGet<RemoteStrategy>(roleType.RelationType, out var workspaceRole))
                {
                    return workspaceRole?.Object;
                }

                var identity = (long?)this.workspaceObject?.GetRole(roleType);
                workspaceRole = this.Session.Get(identity);

                return workspaceRole?.Object;
            }

            if (this.changedRoleByRoleType != null &&
                this.changedRoleByRoleType.TryGet<RemoteStrategy[]>(roleType.RelationType, out var workspaceRoles))
            {
                return workspaceRoles != null ? workspaceRoles.Select(v => v.Object).ToArray() : Array.Empty<IObject>();
            }

            var identities = (long[])this.workspaceObject?.GetRole(roleType);
            return identities == null ? Array.Empty<IObject>() : identities.Select(v => this.Session.Instantiate<IObject>(v)).ToArray();
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

        internal void Push()
        {
            if (this.HasWorkspaceChanges)
            {
                this.Workspace.Push(this.Identity, this.Class, this.workspaceObject?.Version ?? 0, this.changedRoleByRoleType);
            }

            this.workspaceObject = this.Workspace.Get(this.Identity);
            this.changedRoleByRoleType = null;
        }

        internal void Reset()
        {
            this.workspaceObject = this.Workspace.Get(this.Identity);
            this.changedRoleByRoleType = null;
        }

        internal void Merge() => this.workspaceObject = this.Workspace.Get(this.Identity);

        internal void Checkpoint(RemoteChangeSet changeSet)
        {
            // Same workspace object
            if (this.workspaceObject.Identity == this.previousWorkspaceObject.Identity)
            {
                // No previous changed roles
                if (this.previousChangedRoleByRoleType == null)
                {
                    if (this.changedRoleByRoleType != null)
                    {
                        // Changed roles
                        foreach (var kvp in this.changedRoleByRoleType)
                        {
                            var relationType = kvp.Key;
                            var roleType = relationType.RoleType;

                            if (roleType.IsOne)
                            {
                                var currentRole = (RemoteStrategy)kvp.Value;
                                changeSet.AddRole(relationType, currentRole);

                                var previousRole = (RemoteStrategy)this.workspaceObject.GetRole(roleType);
                                if (previousRole != null)
                                {
                                    changeSet.AddRole(relationType, previousRole);
                                }
                            }
                            else
                            {
                                var currentRole = (RemoteStrategy[])kvp.Value;
                                var previousRole = (RemoteStrategy[])this.workspaceObject.GetRole(roleType);

                                var addedRoles = currentRole.Except(previousRole).ToArray();
                                var removedRoles = previousRole.Except(currentRole).ToArray();

                                foreach (var role in addedRoles.Union(removedRoles))
                                {
                                    changeSet.AddRole(relationType, role);
                                }
                            }

                            changeSet.AddAssociation(relationType, this.strategy);
                        }
                    }
                }
                // Previous changed roles
                else
                {
                    foreach (var kvp in this.changedRoleByRoleType)
                    {
                        var relationType = kvp.Key;
                        var roleType = relationType.RoleType;

                        this.previousChangedRoleByRoleType.TryGetValue(relationType, out var previousRoleValue);

                        if (roleType.IsOne)
                        {
                            var currentRole = (RemoteStrategy)kvp.Value;
                            var previousRole = (RemoteStrategy)previousRoleValue;

                            if (!Equals(currentRole, previousRole))
                            {
                                if (currentRole != null)
                                {
                                    changeSet.AddRole(relationType, currentRole);
                                }

                                if (previousRole != null)
                                {
                                    changeSet.AddRole(relationType, previousRole);
                                }

                                changeSet.AddAssociation(relationType, this.strategy);
                            }
                        }
                        else
                        {
                            var currentRole = (RemoteStrategy[])kvp.Value;
                            var previousRole = (RemoteStrategy[])previousRoleValue;

                            if (currentRole?.Length > 0 && previousRole?.Length > 0)
                            {
                                var addedRoles = currentRole.Except(previousRole).ToArray();
                                var removedRoles = previousRole.Except(currentRole).ToArray();

                                if (addedRoles.Length > 0 && removedRoles.Length > 0)
                                {
                                    foreach (var role in addedRoles.Union(removedRoles))
                                    {
                                        changeSet.AddRole(relationType, role);
                                    }

                                    changeSet.AddAssociation(relationType, this.strategy);
                                }
                            }
                            else if (currentRole?.Length > 0)
                            {
                                foreach (var role in currentRole)
                                {
                                    changeSet.AddRole(relationType, role);
                                }

                                changeSet.AddAssociation(relationType, this.strategy);
                            }
                            else if (previousRole?.Length > 0)
                            {
                                foreach (var role in previousRole)
                                {
                                    changeSet.AddRole(relationType, role);
                                }

                                changeSet.AddAssociation(relationType, this.strategy);
                            }
                        }

                        changeSet.AddAssociation(relationType, this.strategy);
                    }
                }
            }
            // Different workspace objects
            else
            {
                var diff = this.previousWorkspaceObject.Diff(this.workspaceObject);
            }

            this.previousWorkspaceObject = this.workspaceObject;
            this.previousChangedRoleByRoleType = this.changedRoleByRoleType;
        }

        private void SetUnitRole(IRoleType roleType, object role)
        {
            var previousRole = this.GetRole(roleType);
            if (Equals(previousRole, role))
            {
                return;
            }

            this.changedRoleByRoleType ??= new Dictionary<IRelationType, object>();
            this.changedRoleByRoleType[roleType.RelationType] = role;

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
                    var previousAssociationObject = this.Session.GetAssociation(previousRole, associationType).FirstOrDefault();
                    previousAssociationObject?.Strategy.Set(roleType, null);
                }
            }

            this.changedRoleByRoleType ??= new Dictionary<IRelationType, object>();
            this.changedRoleByRoleType[roleType.RelationType] = role?.Strategy;

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
                    var addedObjects = this.Session.Instantiate<IObject>(addedRoles);
                    foreach (var addedObject in addedObjects)
                    {
                        var previousAssociationObject = this.Session.GetAssociation(addedObject, associationType).FirstOrDefault();
                        previousAssociationObject?.Strategy.Remove(roleType, addedObject);
                    }
                }
            }

            this.changedRoleByRoleType ??= new Dictionary<IRelationType, object>();
            this.changedRoleByRoleType[roleType.RelationType] = role;

            this.Session.OnChange(this);
        }
    }
}

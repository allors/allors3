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

        private Identity Identity => this.strategy.Identity;

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
                if (this.changedRoleByRoleType == null || !this.changedRoleByRoleType.TryGetValue(roleType.RelationType, out var workspaceRole))
                {
                    workspaceRole = (Identity)this.workspaceObject?.GetRole(roleType);
                }

                return this.Session.Instantiate<IObject>((Identity)workspaceRole);
            }

            if (this.changedRoleByRoleType == null || !this.changedRoleByRoleType.TryGetValue(roleType.RelationType, out var identities))
            {
                identities = (Identity[])this.workspaceObject?.GetRole(roleType);
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
                                var currentRole = (Identity)kvp.Value;
                                changeSet.AddRole(relationType, currentRole);

                                var previousRole = (Identity)this.workspaceObject.GetRole(roleType);
                                if (previousRole != null)
                                {
                                    changeSet.AddRole(relationType, previousRole);
                                }
                            }
                            else
                            {
                                var currentRole = (Identity[])kvp.Value;
                                var previousRole = (Identity[])this.workspaceObject.GetRole(roleType);

                                var addedRoles = currentRole.Except(previousRole).ToArray();
                                var removedRoles = previousRole.Except(currentRole).ToArray();

                                foreach (var role in addedRoles.Union(removedRoles))
                                {
                                    changeSet.AddRole(relationType, role);
                                }
                            }

                            changeSet.AddAssociation(relationType, this.Identity);
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
                            var currentRole = (Identity)kvp.Value;
                            var previousRole = (Identity)previousRoleValue;

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

                                changeSet.AddAssociation(relationType, this.Identity);
                            }
                        }
                        else
                        {
                            var currentRole = (Identity[])kvp.Value;
                            var previousRole = (Identity[])previousRoleValue;

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

                                    changeSet.AddAssociation(relationType, this.Identity);
                                }
                            }
                            else if (currentRole?.Length > 0)
                            {
                                foreach (var role in currentRole)
                                {
                                    changeSet.AddRole(relationType, role);
                                }

                                changeSet.AddAssociation(relationType, this.Identity);
                            }
                            else if (previousRole?.Length > 0)
                            {
                                foreach (var role in previousRole)
                                {
                                    changeSet.AddRole(relationType, role);
                                }

                                changeSet.AddAssociation(relationType, this.Identity);
                            }
                        }

                        changeSet.AddAssociation(relationType, this.Identity);
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
            var role = ((IObject)value)?.Identity;
            var previousRole = ((IObject)this.GetRole(roleType))?.Identity;
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
                    var previousRoleObject = this.Session.Instantiate<IObject>(previousRole);
                    var previousAssociationObject = this.Session.GetAssociation(previousRoleObject, associationType).FirstOrDefault();
                    previousAssociationObject?.Strategy.Set(roleType, null);
                }
            }

            this.changedRoleByRoleType ??= new Dictionary<IRelationType, object>();
            this.changedRoleByRoleType[roleType.RelationType] = role;

            this.Session.OnChange(this);
        }

        private void SetCompositesRole(IRoleType roleType, object value)
        {
            var previousRole = ((IObject[])this.GetRole(roleType)).Select(v => v.Identity).ToArray();

            var role = Array.Empty<Identity>();
            if (value != null)
            {
                role = ((IEnumerable<IObject>)value).Select(v => v.Identity).ToArray();
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

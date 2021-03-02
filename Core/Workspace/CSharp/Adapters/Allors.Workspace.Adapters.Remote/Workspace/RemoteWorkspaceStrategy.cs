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

    public sealed class RemoteWorkspaceStrategy : RemoteStrategy
    {
        private IObject @object;

        private Dictionary<Guid, object> changedRoleByRoleType;

        internal RemoteWorkspaceStrategy(RemoteSession session, Identity identity, IClass @class)
        {
            this.Session = session;
            this.Identity = identity;
            this.Class = @class;

            this.WorkspaceRoles = this.Session.Workspace.Get(this.Identity);
        }

        ISession IStrategy.Session => this.Session;

        public RemoteSession Session { get; }

        public IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        public IClass Class { get; }

        public Identity Identity { get; }

        private RemoteWorkspaceRoles WorkspaceRoles { get; set; }

        internal bool HasWorkspaceChanges => this.changedRoleByRoleType != null;

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
            var objectType = roleType.ObjectType;

            if (objectType.IsUnit)
            {
                switch (roleType.Origin)
                {
                    case Origin.Session:
                        this.Session.State.GetRole(this.Identity, roleType, out var role);
                        return this.Session.Instantiate<IObject>((Identity)role);
                    case Origin.Workspace:
                        if (!this.changedRoleByRoleType.TryGetValue(roleType.RelationType.Id, out var unit))
                        {
                            unit = this.WorkspaceRoles?.GetRole(roleType);
                        }

                        return unit;
                    default:
                        throw new ArgumentException("Origin Database not supported");
                }
            }

            if (roleType.IsOne)
            {
                switch (roleType.Origin)
                {
                    case Origin.Session:
                        this.Session.State.GetRole(this.Identity, roleType, out var sessionRole);
                        return sessionRole;
                    case Origin.Workspace:
                        if (this.changedRoleByRoleType == null || !this.changedRoleByRoleType.TryGetValue(roleType.RelationType.Id, out var workspaceRole))
                        {
                            workspaceRole = (Identity)this.WorkspaceRoles?.GetRole(roleType);
                        }

                        return this.Session.Instantiate<IObject>((Identity)workspaceRole);
                    default:
                        throw new ArgumentException("Origin Database not supported");
                }
            }

            object identities;
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.State.GetRole(this.Identity, roleType, out var sessionRoleIdentities);
                    identities = (Identity[])sessionRoleIdentities;
                    break;
                case Origin.Workspace:
                    if (this.changedRoleByRoleType == null || !this.changedRoleByRoleType.TryGetValue(roleType.RelationType.Id, out identities))
                    {
                        identities = (Identity[])this.WorkspaceRoles?.GetRole(roleType);
                    }

                    break;
                default:
                    throw new ArgumentException("Origin Database not supported");
            }

            var ids = (IEnumerable<Identity>)identities;
            return ids?.Select(v => this.Session.Instantiate<IObject>(v)).ToArray() ?? this.Session.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
        }

        public void Set(IRoleType roleType, object value)
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

            this.changedRoleByRoleType ??= new Dictionary<Guid, object>();

            if (roleType.ObjectType.IsUnit)
            {
                this.changedRoleByRoleType[roleType.RelationType.Id] = value;
            }
            else
            {
                if (roleType.IsOne)
                {
                    this.changedRoleByRoleType[roleType.RelationType.Id] = ((IObject)value)?.Identity;
                }
                else
                {
                    this.changedRoleByRoleType[roleType.RelationType.Id] = ((IEnumerable<object>)value).Select(v => ((IObject)v).Identity).ToArray();
                }
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

        public IObject GetAssociation(IAssociationType associationType)
        {
            if (associationType.Origin == Origin.Session)
            {
                var population = this.Session.State;
                population.GetAssociation(this.Identity, associationType, out var association);
                var id = (Identity)association;
                return id != null ? this.Session.Instantiate<IObject>(id) : null;
            }

            // TODO:

            return null;
        }

        public IEnumerable<IObject> GetAssociations(IAssociationType associationType)
        {
            if (associationType.Origin == Origin.Session)
            {
                var population = this.Session.State;
                population.GetAssociation(this.Identity, associationType, out var association);
                var ids = (IEnumerable<Identity>)association;
                return ids?.Select(v => this.Session.Instantiate<IObject>(v)).ToArray() ?? Array.Empty<IObject>();
            }

            // TODO:
            return null;
        }

        public bool CanRead(IRoleType roleType) => true;

        public bool CanWrite(IRoleType roleType) => true;

        public bool CanExecute(IMethodType methodType) => false;

        internal void Reset()
        {
            this.WorkspaceRoles = this.Session.Workspace.Get(this.Identity);

            this.changedRoleByRoleType = null;
        }

        internal void Save()
        {
            if (this.HasWorkspaceChanges)
            {
                this.Session.Workspace.Push(this.Identity, this.Class, this.WorkspaceRoles?.Version ?? 0, this.changedRoleByRoleType);
            }

            this.Reset();
        }
    }
}

// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Remote
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public sealed class RemoteWorkspaceStrategy : RemoteStrategy
    {
        private readonly RemoteWorkspaceState workspaceState;

        private IObject @object;

        internal RemoteWorkspaceStrategy(RemoteSession session, Identity identity, IClass @class)
        {
            this.Session = session;
            this.Identity = identity;
            this.Class = @class;

            this.workspaceState = new RemoteWorkspaceState(this);
        }

        public override RemoteSession Session { get; }

        public override IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        public override IClass Class { get; }

        public override Identity Identity { get; }

        public override bool Exist(IRoleType roleType)
        {
            var value = this.Get(roleType);

            if (roleType.ObjectType.IsComposite && roleType.IsMany)
            {
                return ((IEnumerable<IObject>)value).Any();
            }

            return value != null;
        }

        public override object Get(IRoleType roleType) =>
            roleType.Origin switch
            {
                Origin.Session => this.Session.GetRole(this.Identity, roleType),
                Origin.Workspace => this.workspaceState.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public override void Set(IRoleType roleType, object value)
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SetRole(this.Identity, roleType, value);
                    break;

                case Origin.Workspace:
                    this.workspaceState.SetRole(roleType, value);

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
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

            // TODO:

            return null;
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

            // TODO:
            return null;
        }

        public override bool CanRead(IRoleType roleType) => true;

        public override bool CanWrite(IRoleType roleType) => true;

        public override bool CanExecute(IMethodType methodType) => false;

        internal void Save() => this.workspaceState.Push();
    }
}

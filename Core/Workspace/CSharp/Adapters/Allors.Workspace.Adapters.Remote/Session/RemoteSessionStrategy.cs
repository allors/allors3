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

    public sealed class RemoteSessionStrategy : RemoteStrategy
    {
        private IObject @object;

        private RemoteSessionStrategy(RemoteSession session, Identity identity, IClass @class)
        {
            this.Session = session;
            this.Identity = identity;
            this.Class = @class;
        }

        public RemoteSessionStrategy(RemoteSession session, IClass @class, Identity identity) : this(session, identity, @class) { }

        ISession IStrategy.Session => this.Session;

        public RemoteSession Session { get; }

        public IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        public IClass Class { get; }

        public Identity Identity { get; }

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

            return null;
        }

        public void Set(IRoleType roleType, object value)
        {
            if (roleType.Origin == Origin.Session)
            {
                var state = this.Session.State;
                state.SetRole(this.Identity, roleType, value);
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

            return Array.Empty<IObject>();
        }

        public bool CanRead(IRoleType roleType) => true;

        public bool CanWrite(IRoleType roleType) => true;

        public bool CanExecute(IMethodType methodType) => false;
    }
}

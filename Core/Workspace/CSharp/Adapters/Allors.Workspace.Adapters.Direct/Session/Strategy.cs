// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Direct
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public abstract class Strategy : IStrategy
    {
        private IObject @object;

        protected Strategy(Session session, long workspaceId, IClass @class)
        {
            this.Session = session;
            this.WorkspaceId = workspaceId;
            this.Class = @class;
        }

        ISession IStrategy.Session => this.Session;

        public Session Session { get; }

        public IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        public IClass Class { get; }

        public long WorkspaceId { get; }
        
        public virtual bool Exist(IRoleType roleType)
        {
            var value = this.Get(roleType);

            if (roleType.ObjectType.IsComposite && roleType.IsMany)
            {
                return ((IEnumerable<IObject>)value).Any();
            }

            return value != null;
        }

        public virtual object Get(IRoleType roleType)
        {
            var population = this.GetPopulation(roleType.Origin);
            population.GetRole(this.WorkspaceId, roleType, out var role);
            if (roleType.ObjectType.IsUnit)
            {
                return role;
            }

            if (roleType.IsOne)
            {
                var id = (long?)role;
                return id.HasValue ? this.Session.Instantiate(id.Value) : null;
            }

            var ids = (IEnumerable<long>)role;
            return ids?.Select(v => this.Session.Instantiate(v)).ToArray() ?? this.Session.Workspace.ObjectFactory.EmptyArray(roleType.ObjectType);
        }

        public virtual void Set(IRoleType roleType, object value)
        {
            var population = this.GetPopulation(roleType.Origin);
            population.SetRole(this.WorkspaceId, roleType, value);
        }

        public virtual void Add(IRoleType roleType, IObject value)
        {
            var roles = (IObject[])this.Get(roleType);
            if (!roles.Contains(value))
            {
                roles = new List<IObject>(roles) { value }.ToArray();
            }

            this.Set(roleType, roles);
        }

        public virtual void Remove(IRoleType roleType, IObject value)
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

        public virtual IObject GetAssociation(IAssociationType associationType)
        {
            var population = this.GetPopulation(associationType.Origin);
            population.GetAssociation(this.WorkspaceId, associationType, out var association);
            var id = (long?)association;
            return id.HasValue ? this.Session.Instantiate(id.Value) : null;
        }

        public virtual IEnumerable<IObject> GetAssociations(IAssociationType associationType)
        {
            var population = this.GetPopulation(associationType.Origin);
            population.GetAssociation(this.WorkspaceId, associationType, out var association);
            var ids = (IEnumerable<long>)association;
            return ids?.Select(v => this.Session.Instantiate(v)).ToArray() ?? Array.Empty<IObject>();
        }
        
        internal abstract State GetPopulation(Origin origin);
    }
}

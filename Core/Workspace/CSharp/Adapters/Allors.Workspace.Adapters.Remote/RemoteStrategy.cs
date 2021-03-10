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
    using Meta;

    public sealed class RemoteStrategy : IStrategy
    {
        private IObject @object;

        private readonly RemoteWorkspaceState workspaceState;
        private readonly RemoteDatabaseState databaseState;

        internal RemoteStrategy(RemoteSession session, IClass @class, long identity)
        {
            this.Session = session;
            this.Identity = identity;
            this.Class = @class;

            if (!this.Class.HasSessionOrigin)
            {
                this.workspaceState = new RemoteWorkspaceState(this);
            }

            if (this.Class.HasDatabaseOrigin)
            {
                this.databaseState = new RemoteDatabaseState(this);
            }
        }

        internal RemoteStrategy(RemoteSession session, RemoteDatabaseObject databaseObject)
        {
            this.Session = session;
            this.Identity = databaseObject.Identity;
            this.Class = databaseObject.Class;

            this.workspaceState = new RemoteWorkspaceState(this);
            this.databaseState = new RemoteDatabaseState(this, databaseObject);
        }

        ISession IStrategy.Session => this.Session;
        internal RemoteSession Session { get; }

        public IClass Class { get; }

        public long Identity { get; private set; }

        public IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        internal bool HasDatabaseChanges => this.databaseState.HasDatabaseChanges;

        internal long DatabaseVersion => this.databaseState.Version;

        public bool Exist(IRoleType roleType)
        {
            var value = this.Get(roleType);

            if (roleType.ObjectType.IsComposite && roleType.IsMany)
            {
                return ((IEnumerable<IObject>)value).Any();
            }

            return value != null;
        }

        public object Get(IRoleType roleType) =>
            roleType.Origin switch
            {
                Origin.Session => this.Session.GetRole(this, roleType),
                Origin.Workspace => this.workspaceState?.GetRole(roleType),
                Origin.Database => this.databaseState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public void Set(IRoleType roleType, object value)
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SetRole(this, roleType, value);
                    break;

                case Origin.Workspace:
                    this.workspaceState?.SetRole(roleType, value);

                    break;

                case Origin.Database:
                    this.databaseState?.SetRole(roleType, value);

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
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
                _ = newRoles.Remove(value.Strategy);
                roles = newRoles.ToArray();
            }

            this.Set(roleType, roles);
        }

        public IObject GetAssociation(IAssociationType associationType)
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetAssociation(this.Object, associationType).FirstOrDefault();
            }

            this.Session.SessionState.GetAssociation(this, associationType, out var association);
            var id = (long?)association;
            return id != null ? this.Session.Instantiate<IObject>(id) : null;
        }

        public IEnumerable<IObject> GetAssociations(IAssociationType associationType)
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetAssociation(this.Object, associationType);
            }

            this.Session.SessionState.GetAssociation(this, associationType, out var association);
            var ids = (IEnumerable<long>)association;
            return ids?.Select(v => this.Session.Instantiate<IObject>(v)).ToArray() ?? Array.Empty<IObject>();
        }

        public bool CanRead(IRoleType roleType) => this.databaseState?.CanRead(roleType) ?? true;

        public bool CanWrite(IRoleType roleType) => this.databaseState?.CanWrite(roleType) ?? true;

        public bool CanExecute(IMethodType methodType) => this.databaseState?.CanExecute(methodType) ?? false;

        internal void Reset()
        {
            this.workspaceState?.Reset();
            this.databaseState?.Reset();
        }

        internal void Merge()
        {
            this.workspaceState?.Merge();
            this.databaseState?.Merge();
        }

        internal PushRequestNewObject DatabaseSaveNew() => this.databaseState.SaveNew();

        internal PushRequestObject DatabaseSaveExisting() => this.databaseState.SaveExisting();

        internal void DatabasePushResponse(RemoteDatabaseObject databaseObject)
        {
            this.Identity = databaseObject.Identity;
            this.databaseState.PushResponse(databaseObject);
        }

        internal void WorkspaceSave() => this.workspaceState.Push();
    }
}

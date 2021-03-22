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
            this.Id = identity;
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
            this.Id = databaseObject.Identity;
            this.Class = databaseObject.Class;

            this.workspaceState = new RemoteWorkspaceState(this);
            this.databaseState = new RemoteDatabaseState(this, databaseObject);
        }

        ISession IStrategy.Session => this.Session;
        internal RemoteSession Session { get; }

        public IClass Class { get; }

        public long Id { get; private set; }

        public IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        internal bool HasDatabaseChanges => this.databaseState.HasDatabaseChanges;

        internal long DatabaseVersion => this.databaseState.Version;

        public bool Exist(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                return this.GetUnitRole(roleType) != null;
            }

            if (roleType.IsOne)
            {
                return this.GetCompositeRole<IObject>(roleType) != null;
            }

            return this.GetCompositesRole<IObject>(roleType).Any();
        }

        public object GetRole(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                return this.GetUnitRole(roleType);
            }

            if (roleType.IsOne)
            {
                return this.GetCompositeRole<IObject>(roleType);
            }

            return this.GetCompositesRole<IObject>(roleType);
        }

        public object GetUnitRole(IRoleType roleType) =>
            roleType.Origin switch
            {
                Origin.Session => this.Session.GetRole(this, roleType),
                Origin.Workspace => this.workspaceState?.GetRole(roleType),
                Origin.Database => this.databaseState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public T GetCompositeRole<T>(IRoleType roleType) where T : IObject =>
            roleType.Origin switch
            {
                Origin.Session => (T)this.Session.GetRole(this, roleType),
                Origin.Workspace => (T)this.workspaceState?.GetRole(roleType),
                Origin.Database => (T)this.databaseState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public IEnumerable<T> GetCompositesRole<T>(IRoleType roleType) where T : IObject
        {
            var roles = roleType.Origin switch
            {
                Origin.Session => this.Session.GetRole(this, roleType),
                Origin.Workspace => this.workspaceState?.GetRole(roleType),
                Origin.Database => this.databaseState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

            if (roles != null)
            {
                foreach (var role in (IObject[])roles)
                {
                    yield return (T)role;
                }
            }
        }

        public void SetRole(IRoleType roleType, object value)
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

        public void SetUnitRole(IRoleType roleType, object value)
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionState.SetUnitRole(this, roleType, value);
                    break;

                case Origin.Workspace:
                    this.workspaceState?.SetUnitRole(roleType, value);

                    break;

                case Origin.Database:
                    this.databaseState?.SetUnitRole(roleType, value);

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void SetCompositeRole(IRoleType roleType, object value)
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionState.SetCompositeRole(this, roleType, value);
                    break;

                case Origin.Workspace:
                    this.workspaceState?.SetCompositeRole(roleType, value);

                    break;

                case Origin.Database:
                    this.databaseState?.SetCompositeRole(roleType, value);

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void SetCompositesRole(IRoleType roleType, object value)
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionState.SetCompositesRole(this, roleType, value);
                    break;

                case Origin.Workspace:
                    this.workspaceState?.SetCompositesRole(roleType, value);

                    break;

                case Origin.Database:
                    this.databaseState?.SetCompositesRole(roleType, value);

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void AddRole(IRoleType roleType, IObject value)
        {
            if (!this.GetCompositesRole<IObject>(roleType).Contains(value))
            {
                var roles = this.GetCompositesRole<IObject>(roleType).Append(value).ToArray();
                this.SetRole(roleType, roles);
            }
        }

        public void RemoveRole(IRoleType roleType, IObject value)
        {
            if (!this.GetCompositesRole<IObject>(roleType).Contains(value))
            {
                return;
            }

            var roles = this.GetCompositesRole<IObject>(roleType).Where(v => !v.Equals(value)).ToArray();
            this.SetRole(roleType, roles);
        }

        public IObject GetCompositeAssociation(IAssociationType associationType)
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetAssociation(this, associationType).FirstOrDefault();
            }

            this.Session.SessionState.GetAssociation(this, associationType, out var association);
            var id = (long?)association;
            return id != null ? this.Session.Get<IObject>(id) : null;
        }

        public IEnumerable<IObject> GetCompositesAssociation(IAssociationType associationType)
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetAssociation(this, associationType);
            }

            this.Session.SessionState.GetAssociation(this, associationType, out var association);
            var ids = (IEnumerable<long>)association;
            return ids?.Select(v => this.Session.Get<IObject>(v)).ToArray() ?? Array.Empty<IObject>();
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
            this.Id = databaseObject.Identity;
            this.databaseState.PushResponse(databaseObject);
        }

        internal void WorkspaceSave() => this.workspaceState.Push();

        public bool IsAssociationForRole(IRoleType roleType, RemoteStrategy role)
            =>
                roleType.Origin switch
                {
                    Origin.Session => this.Session.SessionState.IsAssociationForRole(this, roleType, role),
                    Origin.Workspace => this.workspaceState?.IsAssociationForRole(roleType, role) ?? false,
                    Origin.Database => this.databaseState?.IsAssociationForRole(roleType, role) ?? false,
                    _ => throw new ArgumentException("Unsupported Origin")
                };
    }
}

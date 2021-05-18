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

    public sealed class Strategy : IStrategy
    {
        private IObject @object;

        private readonly WorkspaceOriginState workspaceState;
        private readonly DatabaseOriginState databaseState;

        internal Strategy(Session session, IClass @class, long identity)
        {
            this.Session = session;
            this.Id = identity;
            this.Class = @class;

            if (!this.Class.HasSessionOrigin)
            {
                this.workspaceState = new WorkspaceOriginState(this);
            }

            if (this.Class.HasDatabaseOrigin)
            {
                this.databaseState = new DatabaseOriginState(this);
            }
        }

        internal Strategy(Session session, DatabaseObject databaseObject)
        {
            this.Session = session;
            this.Id = databaseObject.Identity;
            this.Class = databaseObject.Class;

            this.workspaceState = new WorkspaceOriginState(this);
            this.databaseState = new DatabaseOriginState(this, databaseObject);
        }

        ISession IStrategy.Session => this.Session;

        internal Session Session { get; }

        public IClass Class { get; }

        public long Id { get; private set; }

        public IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        internal bool HasDatabaseChanges => this.databaseState.HasDatabaseChanges;

        internal long DatabaseVersion => this.databaseState.Version;

        public bool Exist(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                return this.GetUnit(roleType) != null;
            }

            if (roleType.IsOne)
            {
                return this.GetComposite<IObject>(roleType) != null;
            }

            return this.GetComposites<IObject>(roleType).Any();
        }

        public object Get(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                return this.GetUnit(roleType);
            }

            if (roleType.IsOne)
            {
                return this.GetComposite<IObject>(roleType);
            }

            return this.GetComposites<IObject>(roleType);
        }

        public object GetUnit(IRoleType roleType) =>
            roleType.Origin switch
            {
                Origin.Session => this.Session.GetRole(this, roleType),
                Origin.Workspace => this.workspaceState?.GetRole(roleType),
                Origin.Database => this.databaseState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public T GetComposite<T>(IRoleType roleType) where T : IObject =>
            roleType.Origin switch
            {
                Origin.Session => (T)this.Session.GetRole(this, roleType),
                Origin.Workspace => (T)this.workspaceState?.GetRole(roleType),
                Origin.Database => (T)this.databaseState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public IEnumerable<T> GetComposites<T>(IRoleType roleType) where T : IObject
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

        public void Set(IRoleType roleType, object value)
        {
            if (roleType.ObjectType.IsUnit)
            {
                this.SetUnit(roleType, value);
            }
            else
            {
                if (roleType.IsOne)
                {
                    this.SetComposite(roleType, (IObject)value);
                }
                else
                {
                    this.SetComposites(roleType, (IEnumerable<IObject>)value);
                }
            }
        }

        public void SetUnit(IRoleType roleType, object value)
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

        public void SetComposite<T>(IRoleType roleType, T value) where T : IObject
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

        public void SetComposites<T>(IRoleType roleType, in IEnumerable<T> role) where T : IObject
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionState.SetCompositesRole(this, roleType, role);
                    break;

                case Origin.Workspace:
                    this.workspaceState?.SetCompositesRole(roleType, role);

                    break;

                case Origin.Database:
                    this.databaseState?.SetCompositesRole(roleType, role);

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void Add<T>(IRoleType roleType, T value) where T : IObject
        {
            if (!this.GetComposites<IObject>(roleType).Contains(value))
            {
                var roles = this.GetComposites<IObject>(roleType).Append(value).ToArray();
                this.Set(roleType, roles);
            }
        }

        public void Remove<T>(IRoleType roleType, T value) where T : IObject
        {
            if (!this.GetComposites<IObject>(roleType).Contains(value))
            {
                return;
            }

            var roles = this.GetComposites<IObject>(roleType).Where(v => !v.Equals(value)).ToArray();
            this.Set(roleType, roles);
        }

        public void Remove(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                this.SetUnit(roleType, null);
            }
            else
            {
                if (roleType.IsOne)
                {
                    this.SetComposite(roleType, (IObject)null);
                }
                else
                {
                    this.SetComposites(roleType, (IEnumerable<IObject>)null);
                }
            }
        }

        public T GetComposite<T>(IAssociationType associationType) where T : IObject
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetAssociation<T>(this, associationType).FirstOrDefault();
            }

            this.Session.SessionState.GetAssociation(this, associationType, out var association);
            var id = (long?)association;
            return id != null ? this.Session.Get<T>(id) : default;
        }

        public IEnumerable<T> GetComposites<T>(IAssociationType associationType) where T : IObject
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetAssociation<T>(this, associationType);
            }

            this.Session.SessionState.GetAssociation(this, associationType, out var association);
            var ids = (IEnumerable<long>)association;
            return ids?.Select(v => this.Session.Get<T>(v)).ToArray() ?? Array.Empty<T>();
        }

        public bool CanRead(IRoleType roleType) => this.databaseState?.CanRead(roleType) ?? true;

        public bool CanWrite(IRoleType roleType) => this.databaseState?.CanWrite(roleType) ?? true;

        public bool CanExecute(IMethodType methodType) => this.databaseState?.CanExecute(methodType) ?? false;

        public void Reset()
        {
            this.workspaceState?.Reset();
            this.databaseState?.Reset();
        }

        internal PushRequestNewObject DatabasePushNew() => this.databaseState.PushNew();

        internal PushRequestObject DatabasePushExisting() => this.databaseState.PushExisting();

        internal void DatabasePushResponse(DatabaseObject databaseObject)
        {
            this.Id = databaseObject.Identity;
            this.databaseState.PushResponse(databaseObject);
        }

        internal void WorkspacePush() => this.workspaceState.Push();

        public bool IsAssociationForRole(IRoleType roleType, Strategy role)
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

// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters.Local
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public sealed class Strategy : IStrategy
    {
        private IObject @object;

        private readonly WorkspaceState workspaceState;

        internal DatabaseState DatabaseState { get; }

        internal Strategy(Session session, IClass @class, long identity)
        {
            this.Session = session;
            this.Id = identity;
            this.Class = @class;

            if (!this.Class.HasSessionOrigin)
            {
                this.workspaceState = new WorkspaceState(this);
            }

            if (this.Class.HasDatabaseOrigin)
            {
                this.DatabaseState = new DatabaseState(this);
            }
        }

        internal Strategy(Session session, DatabaseObject databaseObject)
        {
            this.Session = session;
            this.Id = databaseObject.Identity;
            this.Class = databaseObject.Class;

            this.workspaceState = new WorkspaceState(this);
            this.DatabaseState = new DatabaseState(this, databaseObject);
        }

        ISession IStrategy.Session => this.Session;
        internal Session Session { get; }

        public IClass Class { get; }

        public long Id { get; private set; }

        public IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        internal bool HasDatabaseChanges => this.DatabaseState.HasDatabaseChanges;

        internal long DatabaseVersion => this.DatabaseState.Version;

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
                Origin.Database => this.DatabaseState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public T GetComposite<T>(IRoleType roleType) where T : IObject =>
            roleType.Origin switch
            {
                Origin.Session => (T)this.Session.GetRole(this, roleType),
                Origin.Workspace => (T)this.workspaceState?.GetRole(roleType),
                Origin.Database => (T)this.DatabaseState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public IEnumerable<T> GetComposites<T>(IRoleType roleType) where T : IObject
        {
            var roles = roleType.Origin switch
            {
                Origin.Session => this.Session.GetRole(this, roleType),
                Origin.Workspace => this.workspaceState?.GetRole(roleType),
                Origin.Database => this.DatabaseState?.GetRole(roleType),
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
                    this.DatabaseState?.SetUnitRole(roleType, value);

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
                    this.DatabaseState?.SetCompositeRole(roleType, value);

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void SetComposites<T>(IRoleType roleType, in IEnumerable<T> value) where T : IObject
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
                    this.DatabaseState?.SetCompositesRole(roleType, value);

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

        public bool CanRead(IRoleType roleType) => this.DatabaseState?.CanRead(roleType) ?? true;

        public bool CanWrite(IRoleType roleType) => this.DatabaseState?.CanWrite(roleType) ?? true;

        public bool CanExecute(IMethodType methodType) => this.DatabaseState?.CanExecute(methodType) ?? false;

        public void Reset()
        {
            this.workspaceState?.Reset();
            this.DatabaseState?.Reset();
        }

        public IEnumerable<IRelationType> Diff()
        {
            if (this.workspaceState != null)
            {
                foreach (var diff in this.workspaceState.Diff())
                {
                    yield return diff;
                }
            }

            if (this.DatabaseState == null)
            {
                yield break;
            }

            foreach (var diff in this.DatabaseState.Diff())
            {
                yield return diff;
            }
        }
        
        internal bool IsAssociationForRole(IRoleType roleType, Strategy role)
            =>
                roleType.Origin switch
                {
                    Origin.Session => this.Session.SessionState.IsAssociationForRole(this, roleType, role),
                    Origin.Workspace => this.workspaceState?.IsAssociationForRole(roleType, role) ?? false,
                    Origin.Database => this.DatabaseState?.IsAssociationForRole(roleType, role) ?? false,
                    _ => throw new ArgumentException("Unsupported Origin")
                };

        internal void DatabasePushResponse(DatabaseObject databaseObject)
        {
            this.Id = databaseObject.Identity;
            this.DatabaseState.PushResponse(databaseObject);
        }

        internal void WorkspacePush() => this.workspaceState.Push();
    }
}

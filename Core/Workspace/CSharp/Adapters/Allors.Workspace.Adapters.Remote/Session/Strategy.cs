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
    using Numbers;

    public sealed class Strategy : IStrategy
    {
        private IObject @object;

        internal Strategy(Session session, IClass @class, long identity)
        {
            this.Session = session;
            this.Id = identity;
            this.Class = @class;

            if (!this.Class.HasSessionOrigin)
            {
                this.WorkspaceOriginState = new WorkspaceOriginState(this, this.Session.Workspace.GetRecord(this.Id));
            }

            if (this.Class.HasDatabaseOrigin)
            {
                this.DatabaseOriginState = new DatabaseOriginState(this);
            }
        }

        internal Strategy(Session session, DatabaseRecord databaseRecord)
        {
            this.Session = session;
            this.Id = databaseRecord.Id;
            this.Class = databaseRecord.Class;

            this.WorkspaceOriginState = new WorkspaceOriginState(this, this.Session.Workspace.GetRecord(this.Id));
            this.DatabaseOriginState = new DatabaseOriginState(this, databaseRecord);
        }

        ISession IStrategy.Session => this.Session;

        internal Session Session { get; }

        public IClass Class { get; }

        public long Id { get; private set; }

        public IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        internal bool HasDatabaseChanges => this.DatabaseOriginState.HasDatabaseChanges;

        internal long DatabaseVersion => this.DatabaseOriginState.Version;

        internal WorkspaceOriginState WorkspaceOriginState { get; }

        internal DatabaseOriginState DatabaseOriginState { get; }

        internal INumbers Numbers => this.Session.Numbers;

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
                Origin.Workspace => this.WorkspaceOriginState?.GetRole(roleType),
                Origin.Database => this.DatabaseOriginState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public T GetComposite<T>(IRoleType roleType) where T : IObject =>
            roleType.Origin switch
            {
                Origin.Session => this.Session.Get<T>((long?)this.Session.GetRole(this, roleType)),
                Origin.Workspace => this.Session.Get<T>((long?)this.WorkspaceOriginState?.GetRole(roleType)),
                Origin.Database => (T)this.DatabaseOriginState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public IEnumerable<T> GetComposites<T>(IRoleType roleType) where T : IObject
        {
            var roles = roleType.Origin switch
            {
                Origin.Session => this.Session.GetRole(this, roleType),
                Origin.Workspace => this.WorkspaceOriginState?.GetRole(roleType),
                Origin.Database => this.DatabaseOriginState?.GetRole(roleType),
                _ => throw new ArgumentException("Unsupported Origin")
            };


            if (roleType.Origin == Origin.Database)
            {
                if (roles != null)
                {
                    foreach (var role in (IObject[])roles)
                    {
                        yield return (T)role;
                    }
                }

            }
            else
            {
                foreach (var role in this.Numbers.Enumerate(roles))
                {
                    yield return this.Session.Get<T>(role);
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
                    this.Session.SessionOriginState.SetUnitRole(this.Id, roleType, value);
                    break;

                case Origin.Workspace:
                    this.WorkspaceOriginState?.SetUnitRole(roleType, value);

                    break;

                case Origin.Database:
                    this.DatabaseOriginState?.SetUnitRole(roleType, value);

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
                    this.Session.SessionOriginState.SetCompositeRole(this.Id, roleType, value?.Id);
                    break;

                case Origin.Workspace:
                    this.WorkspaceOriginState?.SetCompositeRole(roleType, value?.Id);

                    break;

                case Origin.Database:
                    this.DatabaseOriginState?.SetCompositeRole(roleType, value);

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
                    this.Session.SessionOriginState.SetCompositesRole(this.Id, roleType, role);
                    break;

                case Origin.Workspace:
                    this.WorkspaceOriginState?.SetCompositesRole(roleType, role);

                    break;

                case Origin.Database:
                    this.DatabaseOriginState?.SetCompositesRole(roleType, role);

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

            var association = this.Session.SessionOriginState.Get(this.Id, associationType);
            var id = (long?)association;
            return id != null ? this.Session.Get<T>(id) : default;
        }

        public IEnumerable<T> GetComposites<T>(IAssociationType associationType) where T : IObject
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetAssociation<T>(this, associationType);
            }

            var association = this.Session.SessionOriginState.Get(this.Id, associationType);
            var ids = (IEnumerable<long>)association;
            return ids?.Select(v => this.Session.Get<T>(v)).ToArray() ?? Array.Empty<T>();
        }

        public bool CanRead(IRoleType roleType) => this.DatabaseOriginState?.CanRead(roleType) ?? true;

        public bool CanWrite(IRoleType roleType) => this.DatabaseOriginState?.CanWrite(roleType) ?? true;

        public bool CanExecute(IMethodType methodType) => this.DatabaseOriginState?.CanExecute(methodType) ?? false;

        public void Reset()
        {
            this.DatabaseOriginState?.Reset();
        }

        internal PushRequestNewObject DatabasePushNew() => this.DatabaseOriginState.PushNew();

        internal PushRequestObject DatabasePushExisting() => this.DatabaseOriginState.PushExisting();

        internal void DatabasePushResponse(DatabaseRecord databaseRecord)
        {
            this.Id = databaseRecord.Id;
            this.DatabaseOriginState.PushResponse(databaseRecord);
        }

        internal void WorkspacePush() => this.WorkspaceOriginState.Push();

        internal bool IsAssociationForRole(IRoleType roleType, long forRoleId)
        {
            var role = this.Session.SessionOriginState.Get(this.Id, roleType);
            return roleType.Origin switch
            {
                Origin.Session => Equals(role, forRoleId),
                Origin.Workspace => this.WorkspaceOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false,
                _ => throw new ArgumentException("Unsupported Origin")
            };
        }

        public bool IsAssociationForRole(IRoleType roleType, Strategy forRole)
        {
            var role = this.Session.SessionOriginState.Get(this.Id, roleType);

            return roleType.Origin switch
            {
                Origin.Session => Equals(role, forRole.Id),
                Origin.Workspace => this.WorkspaceOriginState?.IsAssociationForRole(roleType, forRole.Id) ?? false,
                Origin.Database => this.DatabaseOriginState?.IsAssociationForRole(roleType, forRole) ?? false,
                _ => throw new ArgumentException("Unsupported Origin")
            };
        }
    }
}

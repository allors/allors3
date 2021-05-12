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
    using Numbers;

    public sealed class Strategy : IStrategy
    {
        private IObject @object;

        private readonly WorkspaceOriginState workspaceOriginState;

        internal DatabaseOriginState DatabaseOriginState { get; }

        internal Strategy(Session session, IClass @class, long id)
        {
            this.Session = session;
            this.Id = id;
            this.Class = @class;

            if (!this.Class.HasSessionOrigin)
            {
                this.workspaceOriginState = new WorkspaceOriginState(this, this.Session.Workspace.GetRecord(this.Id));
            }

            if (this.Class.HasDatabaseOrigin)
            {
                this.DatabaseOriginState = new DatabaseOriginState(this, this.Session.DatabaseAdapter.GetRecord(this.Id));
            }
        }

        internal Strategy(Session session, DatabaseRecord databaseRecord)
        {
            this.Session = session;
            this.Id = databaseRecord.Id;
            this.Class = databaseRecord.Class;

            this.workspaceOriginState = new WorkspaceOriginState(this, this.Session.Workspace.GetRecord(this.Id));
            this.DatabaseOriginState = new DatabaseOriginState(this, databaseRecord);
        }

        ISession IStrategy.Session => this.Session;
        internal Session Session { get; }

        public IClass Class { get; }

        public long Id { get; private set; }

        public IObject Object => this.@object ??= this.Session.Workspace.ObjectFactory.Create(this);

        internal INumbers Numbers => this.Session.Numbers;

        internal bool HasDatabaseChanges => this.DatabaseOriginState.HasChanges;

        internal long DatabaseVersion => this.DatabaseOriginState.Version;

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
                Origin.Workspace => this.workspaceOriginState?.GetRole(roleType),
                Origin.Database => this.CanRead(roleType) ? this.DatabaseOriginState?.GetRole(roleType) : null,
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public T GetComposite<T>(IRoleType roleType) where T : IObject =>
            this.Session.Get<T>(roleType.Origin switch
            {
                Origin.Session => (long?)this.Session.GetRole(this, roleType),
                Origin.Workspace => (long?)this.workspaceOriginState?.GetRole(roleType),
                Origin.Database => this.CanRead(roleType) ? (long?)this.DatabaseOriginState?.GetRole(roleType) : null,
                _ => throw new ArgumentException("Unsupported Origin")
            });


        public IEnumerable<T> GetComposites<T>(IRoleType roleType) where T : IObject
        {
            var roles = roleType.Origin switch
            {
                Origin.Session => this.Session.GetRole(this, roleType),
                Origin.Workspace => this.workspaceOriginState?.GetRole(roleType),
                Origin.Database => this.CanRead(roleType) ? this.DatabaseOriginState?.GetRole(roleType) : null,
                _ => throw new ArgumentException("Unsupported Origin")
            };

            foreach (var role in this.Numbers.Enumerate(roles))
            {
                yield return this.Session.Get<T>(role);
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
                    this.workspaceOriginState?.SetUnitRole(roleType, value);

                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState?.SetUnitRole(roleType, value);
                    }

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
                    this.workspaceOriginState?.SetCompositeRole(roleType, value?.Id);

                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState?.SetCompositeRole(roleType, value?.Id);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void SetComposites<T>(IRoleType roleType, in IEnumerable<T> role) where T : IObject
        {
            var roleNumbers = this.Numbers.From(role?.Select(v => v.Id));

            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.SetCompositesRole(this.Id, roleType, roleNumbers);
                    break;

                case Origin.Workspace:
                    this.workspaceOriginState?.SetCompositesRole(roleType, roleNumbers);

                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState?.SetCompositesRole(roleType, roleNumbers);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void Add<T>(IRoleType roleType, T value) where T : IObject
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.AddRole(this.Id, roleType, value.Id);
                    break;

                case Origin.Workspace:
                    this.workspaceOriginState.AddCompositeRole(roleType, value.Id);

                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState.AddCompositeRole(roleType, value.Id);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void Remove<T>(IRoleType roleType, T value) where T : IObject
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.AddRole(this.Id, roleType, value.Id);
                    break;

                case Origin.Workspace:
                    this.workspaceOriginState.RemoveCompositeRole(roleType, value.Id);

                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState.RemoveCompositeRole(roleType, value.Id);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
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
                return this.Session.GetAssociation<T>(this.Id, associationType).FirstOrDefault();
            }

            var association = (long?)this.Session.SessionOriginState.Get(this.Id, associationType);
            return association != null ? this.Session.Get<T>(association) : default;
        }

        public IEnumerable<T> GetComposites<T>(IAssociationType associationType) where T : IObject
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetAssociation<T>(this.Id, associationType);
            }

            var association = this.Session.SessionOriginState.Get(this.Id, associationType);

            return association switch
            {
                long id => new[] { this.Session.Get<T>(id) },
                long[] ids => ids.Select(v => this.Session.Get<T>(v)).ToArray(),
                _ => Array.Empty<T>()
            };
        }

        public bool CanRead(IRoleType roleType) => this.DatabaseOriginState?.CanRead(roleType) ?? true;

        public bool CanWrite(IRoleType roleType) => this.DatabaseOriginState?.CanWrite(roleType) ?? true;

        public bool CanExecute(IMethodType methodType) => this.DatabaseOriginState?.CanExecute(methodType) ?? false;

        public void Reset()
        {
            this.workspaceOriginState?.Reset();
            this.DatabaseOriginState?.Reset();
        }

        internal bool IsAssociationForRole(IRoleType roleType, long forRoleId)
        {
            var role = this.Session.SessionOriginState.Get(this.Id, roleType);
            return roleType.Origin switch
            {
                Origin.Session => Equals(role, forRoleId),
                Origin.Workspace => this.workspaceOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false,
                Origin.Database => this.DatabaseOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false,
                _ => throw new ArgumentException("Unsupported Origin")
            };
        }

        internal void DatabasePushResponse(DatabaseRecord databaseRecord)
        {
            this.Id = databaseRecord.Id;
            this.DatabaseOriginState.PushResponse(databaseRecord);
        }

        internal void WorkspacePush() => this.workspaceOriginState.Push();
    }
}

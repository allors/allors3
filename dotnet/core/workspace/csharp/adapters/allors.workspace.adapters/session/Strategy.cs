// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;

    public abstract class Strategy : IStrategy
    {
        private IObject @object;

        protected Strategy(Session session, IClass @class, long id)
        {
            this.Session = session;
            this.Id = id;
            this.Class = @class;

            if (!this.Class.HasSessionOrigin)
            {
                this.WorkspaceOriginState = new WorkspaceOriginState(this, this.Session.Workspace.GetRecord(this.Id));
            }
        }

        protected Strategy(Session session, DatabaseRecord databaseRecord)
        {
            this.Session = session;
            this.Id = databaseRecord.Id;
            this.Class = databaseRecord.Class;

            this.WorkspaceOriginState = new WorkspaceOriginState(this, this.Session.Workspace.GetRecord(this.Id));
        }

        public long Version =>
            this.Class.Origin switch
            {
                Origin.Session => Allors.Version.Initial.Value,
                Origin.Workspace => this.WorkspaceOriginState.Version,
                Origin.Database => this.DatabaseOriginState.Version,
                _ => throw new Exception()
            };

        public Session Session { get; }

        public DatabaseOriginState DatabaseOriginState { get; protected set; }

        public WorkspaceOriginState WorkspaceOriginState { get; }

        ISession IStrategy.Session => this.Session;

        public IClass Class { get; }

        public long Id { get; private set; }

        public bool IsNew => Session.IsNewId(this.Id);

        public IObject Object => this.@object ??= this.Session.Workspace.DatabaseConnection.Configuration.ObjectFactory.Create(this);

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
                Origin.Database => this.DatabaseOriginState?.IsVersionUnknown == true ? throw new Exception() : this.CanRead(roleType) ? this.DatabaseOriginState?.GetRole(roleType) : null,
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public T GetComposite<T>(IRoleType roleType) where T : IObject =>
            this.Session.GetOne<T>(roleType.Origin switch
            {
                Origin.Session => (long?)this.Session.GetRole(this, roleType),
                Origin.Workspace => (long?)this.WorkspaceOriginState?.GetRole(roleType),
                Origin.Database => this.DatabaseOriginState?.IsVersionUnknown == true ? throw new Exception() : this.CanRead(roleType) ? (long?)this.DatabaseOriginState?.GetRole(roleType) : null,
                _ => throw new ArgumentException("Unsupported Origin")
            });

        public IEnumerable<T> GetComposites<T>(IRoleType roleType) where T : IObject
        {
            var roles = roleType.Origin switch
            {
                Origin.Session => this.Session.GetRole(this, roleType),
                Origin.Workspace => this.WorkspaceOriginState?.GetRole(roleType),
                Origin.Database => this.DatabaseOriginState?.IsVersionUnknown == true ? throw new Exception() : this.CanRead(roleType) ? this.DatabaseOriginState?.GetRole(roleType) : null,
                _ => throw new ArgumentException("Unsupported Origin")
            };

            if (roles == null)
            {
                yield break;
            }

            foreach (var role in (IEnumerable<IObject>)roles)
            {
                yield return (T)role;
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
                    this.WorkspaceOriginState?.SetCompositeRole(roleType, value?.Id);

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
            var roleNumbers = this.Session.Workspace.Numbers.From(role?.Select(v => v.Id));

            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.SetCompositesRole(this.Id, roleType, roleNumbers);
                    break;

                case Origin.Workspace:
                    this.WorkspaceOriginState?.SetCompositesRole(roleType, roleNumbers);

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
                    this.WorkspaceOriginState.AddCompositeRole(roleType, value.Id);

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
                    this.WorkspaceOriginState.RemoveCompositeRole(roleType, value.Id);

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

        public void RemoveAll(IRoleType roleType)
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

        public T GetCompositeAssociation<T>(IAssociationType associationType) where T : IObject
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetCompositeAssociation<T>(this.Id, associationType);
            }

            var association = (long?)this.Session.SessionOriginState.Get(this.Id, associationType);
            return association != null ? this.Session.GetOne<T>(association) : default;
        }

        public IEnumerable<T> GetCompositesAssociation<T>(IAssociationType associationType) where T : IObject
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetCompositesAssociation<T>(this.Id, associationType);
            }

            var association = this.Session.SessionOriginState.Get(this.Id, associationType);

            return association switch
            {
                long id => new[] { this.Session.GetOne<T>(id) },
                long[] ids => ids.Select(v => this.Session.GetOne<T>(v)).ToArray(),
                _ => Array.Empty<T>()
            };
        }

        public bool CanRead(IRoleType roleType) => this.DatabaseOriginState?.CanRead(roleType) ?? true;

        public bool CanWrite(IRoleType roleType) => this.DatabaseOriginState?.CanWrite(roleType) ?? true;

        public bool CanExecute(IMethodType methodType) => this.DatabaseOriginState?.CanExecute(methodType) ?? false;

        public bool IsAssociationForRole(IRoleType roleType, long forRoleId)
        {
            var role = this.Session.SessionOriginState.Get(this.Id, roleType);
            return roleType.Origin switch
            {
                Origin.Session => Equals(role, forRoleId),
                Origin.Workspace => this.WorkspaceOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false,
                Origin.Database => this.DatabaseOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false,
                _ => throw new ArgumentException("Unsupported Origin")
            };
        }

        public void OnDatabasePushNewId(long newId) => this.Id = newId;

        public void OnDatabasePushResponse(DatabaseRecord databaseRecord) => this.DatabaseOriginState.PushResponse(databaseRecord);
    }
}

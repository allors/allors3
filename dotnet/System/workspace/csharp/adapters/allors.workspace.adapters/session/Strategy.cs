// <copyright file="Object.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
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

            if (this.Class.Origin != Origin.Session)
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
                Origin.Session => Allors.Version.WorkspaceInitial.Value,
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

        public bool ExistRole(IRoleType roleType)
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
                Origin.Session => this.Session.GetUnitRole(this, roleType),
                Origin.Workspace => this.WorkspaceOriginState?.GetUnitRole(roleType),
                Origin.Database => this.CanRead(roleType) ? this.DatabaseOriginState?.GetUnitRole(roleType) : null,
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public T GetCompositeRole<T>(IRoleType roleType) where T : IObject =>
            this.Session.GetOne<T>(roleType.Origin switch
            {
                Origin.Session => this.Session.GetCompositeRole(this, roleType),
                Origin.Workspace => this.WorkspaceOriginState?.GetCompositeRole(roleType),
                Origin.Database => this.CanRead(roleType) ? this.DatabaseOriginState.GetCompositeRole(roleType) : null,
                _ => throw new ArgumentException("Unsupported Origin")
            });

        public IEnumerable<T> GetCompositesRole<T>(IRoleType roleType) where T : IObject
        {
            var roles = roleType.Origin switch
            {
                Origin.Session => this.Session.GetCompositesRole(this, roleType),
                Origin.Workspace => this.WorkspaceOriginState?.GetCompositesRole(roleType),
                Origin.Database => this.CanRead(roleType) ? this.DatabaseOriginState?.GetCompositesRole(roleType) : null,
                _ => throw new ArgumentException("Unsupported Origin")
            };

            return roles == null ? Array.Empty<T>() : this.Session.GetMany<T>(roles);
        }

        public void SetRole(IRoleType roleType, object value)
        {
            if (roleType.ObjectType.IsUnit)
            {
                this.SetUnitRole(roleType, value);
            }
            else if (roleType.IsOne)
            {
                this.SetCompositeRole(roleType, (IObject)value);
            }
            else
            {
                this.SetCompositesRole(roleType, (IEnumerable<IObject>)value);
            }
        }

        public void SetUnitRole(IRoleType roleType, object value)
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

        public void SetCompositeRole<T>(IRoleType roleType, T value) where T : IObject
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.SetCompositeRole(this.Id, roleType, value?.Id);
                    break;

                case Origin.Workspace:
                    this.WorkspaceOriginState.SetCompositeRole(roleType, value?.Id);

                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState.SetCompositeRole(roleType, value?.Id);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void SetCompositesRole<T>(IRoleType roleType, in IEnumerable<T> role) where T : IObject
        {
            var ranges = this.Session.Workspace.Ranges;
            var roleIds = ranges.ImportFrom(role?.Select(v => v.Id));

            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.SetCompositesRole(this.Id, roleType, roleIds);
                    break;

                case Origin.Workspace:
                    this.WorkspaceOriginState?.SetCompositesRole(roleType, roleIds);

                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState?.SetCompositesRole(roleType, roleIds);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void AddCompositesRole<T>(IRoleType roleType, T value) where T : IObject
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.AddCompositesRole(this.Id, roleType, value.Id);
                    break;

                case Origin.Workspace:
                    this.WorkspaceOriginState.AddCompositesRole(roleType, value.Id);

                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState.AddCompositesRole(roleType, value.Id);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void RemoveCompositesRole<T>(IRoleType roleType, T value) where T : IObject
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.RemoveCompositesRole(this.Id, roleType, value.Id);
                    break;

                case Origin.Workspace:
                    this.WorkspaceOriginState.RemoveCompositesRole(roleType, value.Id);

                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState.RemoveCompositesRole(roleType, value.Id);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void RemoveRole(IRoleType roleType)
        {
            if (roleType.ObjectType.IsUnit)
            {
                this.SetUnitRole(roleType, null);
            }
            else if (roleType.IsOne)
            {
                this.SetCompositeRole(roleType, (IObject)null);
            }
            else
            {
                this.SetCompositesRole(roleType, (IEnumerable<IObject>)null);
            }
        }

        public T GetCompositeAssociation<T>(IAssociationType associationType) where T : IObject
        {
            if (associationType.Origin != Origin.Session)
            {
                return (T)this.Session.GetCompositeAssociation(this.Id, associationType)?.Object;
            }

            var association = this.Session.SessionOriginState.GetCompositeRole(this.Id, associationType);
            return association != null ? this.Session.GetOne<T>(association) : default;
        }

        public IEnumerable<T> GetCompositesAssociation<T>(IAssociationType associationType) where T : IObject
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetCompositesAssociation(this.Id, associationType).Select(v => v.@object).Cast<T>();
            }

            var association = this.Session.SessionOriginState.GetCompositesRole(this.Id, associationType);
            return association.IsEmpty ? Array.Empty<T>() : association.Select(v => this.Session.GetOne<T>(v)).ToArray();
        }

        public bool CanRead(IRoleType roleType) => this.DatabaseOriginState?.CanRead(roleType) ?? true;

        public bool CanWrite(IRoleType roleType) => this.DatabaseOriginState?.CanWrite(roleType) ?? true;

        public bool CanExecute(IMethodType methodType) => this.DatabaseOriginState?.CanExecute(methodType) ?? false;

        public bool IsCompositeAssociationForRole(IRoleType roleType, long forRoleId) =>
            roleType.Origin switch
            {
                Origin.Session => Equals(this.Session.SessionOriginState.GetCompositeRole(this.Id, roleType), forRoleId),
                Origin.Workspace => this.WorkspaceOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false,
                Origin.Database => this.DatabaseOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false,
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public bool IsCompositesAssociationForRole(IRoleType roleType, long forRoleId) =>
            roleType.Origin switch
            {
                Origin.Session => this.Session.Workspace.Ranges.Ensure(this.Session.SessionOriginState.GetCompositesRole(this.Id, roleType)).Contains(forRoleId),
                Origin.Workspace => this.WorkspaceOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false,
                Origin.Database => this.DatabaseOriginState?.IsAssociationForRole(roleType, forRoleId) ?? false,
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public void OnDatabasePushNewId(long newId) => this.Id = newId;

        public void OnDatabasePushResponse(DatabaseRecord databaseRecord) => this.DatabaseOriginState.PushResponse(databaseRecord);
    }
}

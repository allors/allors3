// <copyright file="Object.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Ranges;

    public abstract class Strategy : IStrategy, IComparable<Strategy>
    {
        private readonly long rangeId;

        private IObject @object;

        protected Strategy(Session session, IClass @class, long id)
        {
            this.Session = session;
            this.Id = id;
            this.rangeId = this.Id;
            this.Class = @class;
            this.Ranges = this.Session.Workspace.StrategyRanges;
        }

        protected Strategy(Session session, DatabaseRecord databaseRecord)
        {
            this.Session = session;
            this.Id = databaseRecord.Id;
            this.rangeId = this.Id;
            this.Class = databaseRecord.Class;
            this.Ranges = this.Session.Workspace.StrategyRanges;
        }

        public long Version => this.DatabaseOriginState.Version;

        public Session Session { get; }

        public DatabaseOriginState DatabaseOriginState { get; protected set; }

        public IRanges<Strategy> Ranges { get; }

        ISession IStrategy.Session => this.Session;

        public IClass Class { get; }

        public long Id { get; private set; }

        public bool IsNew => Session.IsNewId(this.Id);

        public IObject Object => this.@object ??= this.Session.Workspace.DatabaseConnection.Configuration.ObjectFactory.Create(this);
        public IReadOnlyList<IDiff> Diff()
        {
            var diffs = new List<IDiff>();
            this.DatabaseOriginState.Diff(diffs);
            return diffs.ToArray();
        }

        public bool HasChanges => this.DatabaseOriginState.HashChanges();

        public void Reset() => this.DatabaseOriginState.Reset();

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

        public bool HasChanged(IRoleType roleType) =>
            roleType.Origin switch
            {
                Origin.Session => false,
                Origin.Database => this.CanRead(roleType) && this.DatabaseOriginState.HasChanged(roleType),
                _ => throw new ArgumentException("Unknown origin")
            };

        public void RestoreRole(IRoleType roleType)
        {
            switch (roleType.Origin)
            {
                case Origin.Session:
                    return;
                case Origin.Database:
                    if (this.CanRead(roleType))
                    {
                        this.DatabaseOriginState.RestoreRole(roleType);
                    }

                    return;
                default:
                    throw new ArgumentException("Unknown origin");
            }
        }

        public object GetRole(IRoleType roleType)
        {
            if (roleType == null)
            {
                throw new ArgumentNullException(nameof(roleType));
            }

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

        public object GetUnitRole(IRoleType roleType)
        {
            if (roleType.RelationType.IsDerived)
            {
                var rule = this.Session.Resolve(this, roleType);
                if (rule != null)
                {
                    return rule.Derive(this.Object);
                }
            }

            return roleType.Origin switch
            {
                Origin.Session => this.Session.SessionOriginState.GetUnitRole(this, roleType),
                Origin.Database => this.CanRead(roleType) ? this.DatabaseOriginState.GetUnitRole(roleType) : null,
                _ => throw new ArgumentException("Unsupported Origin")
            };
        }

        public T GetCompositeRole<T>(IRoleType roleType) where T : class, IObject
        {
            if (roleType.RelationType.IsDerived)
            {
                var rule = this.Session.Resolve(this, roleType);
                if (rule != null)
                {
                    return (T)rule.Derive(this.Object);
                }
            }

            return roleType.Origin switch
            {
                Origin.Session => (T)this.Session.SessionOriginState.GetCompositeRole(this, roleType)?.Object,
                Origin.Database => this.CanRead(roleType)
                    ? (T)this.DatabaseOriginState.GetCompositeRole(roleType)?.Object
                    : null,
                _ => throw new ArgumentException("Unsupported Origin")
            };
        }

        public IEnumerable<T> GetCompositesRole<T>(IRoleType roleType) where T : class, IObject
        {
            if (roleType.RelationType.IsDerived)
            {
                var rule = this.Session.Resolve(this, roleType);
                if (rule != null)
                {
                    return (IEnumerable<T>)rule.Derive(this.Object);
                }
            }

            return roleType.Origin switch
            {
                Origin.Session => this.Session.SessionOriginState.GetCompositesRole(this, roleType)
                    .Select(v => (T)v.Object),
                Origin.Database => this.CanRead(roleType)
                    ? this.DatabaseOriginState.GetCompositesRole(roleType).Select(v => (T)v.Object)
                    : Array.Empty<T>(),
                _ => throw new ArgumentException("Unsupported Origin")
            };
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
            AssertUnit(roleType, value);

            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.SetUnitRole(this, roleType, value);
                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState.SetUnitRole(roleType, value);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void SetCompositeRole<T>(IRoleType roleType, T value) where T : class, IObject
        {
            this.AssertComposite(value);

            if (value != null)
            {
                this.AssertSameType(roleType, value);
                this.AssertSameSession(value);
            }

            if (roleType.IsMany)
            {
                throw new ArgumentException($"Given {nameof(roleType)} is the wrong multiplicity");
            }

            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.SetCompositeRole(this, roleType, (Strategy)value?.Strategy);
                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState.SetCompositeRole(roleType, (Strategy)value?.Strategy);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void SetCompositesRole<T>(IRoleType roleType, in IEnumerable<T> role) where T : class, IObject
        {
            this.AssertComposites(role);

            var roleStrategies = this.Ranges.Load(role?.Select(v => (Strategy)v.Strategy));

            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.SetCompositesRole(this, roleType, roleStrategies);
                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState.SetCompositesRole(roleType, roleStrategies);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void AddCompositesRole<T>(IRoleType roleType, T value) where T : class, IObject
        {
            if (value == null)
            {
                return;
            }

            this.AssertComposite(value);

            this.AssertSameType(roleType, value);

            if (roleType.IsOne)
            {
                throw new ArgumentException($"Given {nameof(roleType)} is the wrong multiplicity");
            }

            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.AddCompositesRole(this, roleType, (Strategy)value.Strategy);
                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState.AddCompositesRole(roleType, (Strategy)value.Strategy);
                    }

                    break;
                default:
                    throw new ArgumentException("Unsupported Origin");
            }
        }

        public void RemoveCompositesRole<T>(IRoleType roleType, T value) where T : class, IObject
        {
            if (value == null)
            {
                return;
            }

            this.AssertComposite(value);

            switch (roleType.Origin)
            {
                case Origin.Session:
                    this.Session.SessionOriginState.RemoveCompositesRole(this, roleType, (Strategy)value.Strategy);
                    break;

                case Origin.Database:
                    if (this.CanWrite(roleType))
                    {
                        this.DatabaseOriginState.RemoveCompositesRole(roleType, (Strategy)value.Strategy);
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

        public T GetCompositeAssociation<T>(IAssociationType associationType) where T : class, IObject
        {
            if (associationType.Origin != Origin.Session)
            {
                return (T)this.Session.GetCompositeAssociation(this, associationType)?.Object;
            }

            var association = this.Session.SessionOriginState.GetCompositeAssociation(this, associationType);
            return (T)association?.Object;
        }

        public IEnumerable<T> GetCompositesAssociation<T>(IAssociationType associationType) where T : class, IObject
        {
            if (associationType.Origin != Origin.Session)
            {
                return this.Session.GetCompositesAssociation(this, associationType).Select(v => v.Object).Cast<T>();
            }

            var association = this.Session.SessionOriginState.GetCompositesAssociation(this, associationType);
            return association?.Select(v => (T)v.Object) ?? Array.Empty<T>();
        }

        public bool CanRead(IRoleType roleType) => roleType.RelationType.Origin != Origin.Database || this.DatabaseOriginState.CanRead(roleType);

        public bool CanWrite(IRoleType roleType) => roleType.RelationType.Origin != Origin.Database || this.DatabaseOriginState.CanWrite(roleType);

        public bool CanExecute(IMethodType methodType) => this.DatabaseOriginState.CanExecute(methodType);

        public bool IsCompositeAssociationForRole(IRoleType roleType, Strategy forRole) =>
            roleType.Origin switch
            {
                Origin.Session => Equals(this.Session.SessionOriginState.GetCompositeRole(this, roleType), forRole),
                Origin.Database => this.DatabaseOriginState.IsAssociationForRole(roleType, forRole),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public bool IsCompositesAssociationForRole(IRoleType roleType, Strategy forRoleId) =>
            roleType.Origin switch
            {
                Origin.Session => this.Session.SessionOriginState.GetCompositesRole(this, roleType).Contains(forRoleId),
                Origin.Database => this.DatabaseOriginState.IsAssociationForRole(roleType, forRoleId),
                _ => throw new ArgumentException("Unsupported Origin")
            };

        public void OnDatabasePushNewId(long newId) => this.Id = newId;

        public void OnDatabasePushed() => this.DatabaseOriginState.OnPushed();

        private void AssertSameType<T>(IRoleType roleType, T value) where T : class, IObject
        {
            if (!((IComposite)roleType.ObjectType).IsAssignableFrom(value.Strategy.Class))
            {
                throw new ArgumentException($"Types do not match: {nameof(roleType)}: {roleType.ObjectType.ClrType} and {nameof(value)}: {value.GetType()}");
            }
        }

        private void AssertSameSession(IObject value)
        {
            if (this.Session != value.Strategy.Session)
            {
                throw new ArgumentException($"Session do not match");
            }
        }

        private static void AssertUnit(IRoleType roleType, object value)
        {
            if (value == null)
            {
                return;
            }

            switch (roleType.ObjectType.Tag)
            {
                case UnitTags.Binary:
                    if (!(value is byte[]))
                    {
                        throw new ArgumentException($"{nameof(value)} is not a Binary");
                    }
                    break;
                case UnitTags.Boolean:
                    if (!(value is bool))
                    {
                        throw new ArgumentException($"{nameof(value)} is not an Bool");
                    }
                    break;
                case UnitTags.DateTime:
                    if (!(value is DateTime))
                    {
                        throw new ArgumentException($"{nameof(value)} is not an DateTime");
                    }
                    break;
                case UnitTags.Decimal:
                    if (!(value is decimal))
                    {
                        throw new ArgumentException($"{nameof(value)} is not an Decimal");
                    }
                    break;
                case UnitTags.Float:
                    if (!(value is double))
                    {
                        throw new ArgumentException($"{nameof(value)} is not an Float");
                    }
                    break;
                case UnitTags.Integer:
                    if (!(value is int))
                    {
                        throw new ArgumentException($"{nameof(value)} is not an Integer");
                    }
                    break;
                case UnitTags.String:
                    if (!(value is string))
                    {
                        throw new ArgumentException($"{nameof(value)} is not an String");
                    }
                    break;
                case UnitTags.Unique:
                    if (!(value is Guid))
                    {
                        throw new ArgumentException($"{nameof(value)} is not an Unique");
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(roleType));
            }
        }

        private void AssertComposite(IObject value)
        {
            if (value == null)
            {
                return;
            }

            if (this.Session != value.Strategy.Session)
            {
                throw new ArgumentException("Strategy is from a different session");
            }
        }

        private void AssertComposites(IEnumerable<IObject> inputs)
        {
            if (inputs == null)
            {
                return;
            }

            foreach (var input in inputs)
            {
                this.AssertComposite(input);
            }
        }

        int IComparable<Strategy>.CompareTo(Strategy other)
        {
            if (ReferenceEquals(this, other))
            {
                return 0;
            }

            return other is null ? 1 : this.rangeId.CompareTo(other.rangeId);
        }
    }
}

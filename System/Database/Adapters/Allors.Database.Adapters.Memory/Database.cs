// <copyright file="Database.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Xml;

    using Meta;
    using Derivations;

    public class Database : IDatabase
    {
        private readonly Dictionary<IObjectType, object> concreteClassesByObjectType;
        private Transaction transaction;

        public Database(IDatabaseLifecycle state, Configuration configuration)
        {
            this.Lifecycle = state;
            if (this.Lifecycle == null)
            {
                throw new Exception("Services is missing");
            }

            this.ObjectFactory = configuration.ObjectFactory;
            if (this.ObjectFactory == null)
            {
                throw new Exception("Configuration.ObjectFactory is missing");
            }

            this.concreteClassesByObjectType = new Dictionary<IObjectType, object>();

            this.Id = string.IsNullOrWhiteSpace(configuration.Id) ? Guid.NewGuid().ToString("N").ToLowerInvariant() : configuration.Id;

            this.Derivations = Array.Empty<IDomainDerivation>();
            this.Procedures = new DefaultProcedures(this.ObjectFactory.Assembly);

            this.Lifecycle.OnInit(this);
        }

        public event ObjectNotLoadedEventHandler ObjectNotLoaded;

        public event RelationNotLoadedEventHandler RelationNotLoaded;

        public IDomainDerivation[] Derivations { get; private set; }

        public IProcedures Procedures { get; set; }

        public string Id { get; }

        public bool IsShared => false;

        public IObjectFactory ObjectFactory { get; }

        public IMetaPopulation MetaPopulation => this.ObjectFactory.MetaPopulation;

        public IDatabaseLifecycle Lifecycle { get; }

        internal bool IsLoading { get; private set; }

        protected virtual Transaction Transaction => this.transaction ??= new Transaction(this, this.Lifecycle.CreateTransactionInstance());

        public ITransaction CreateTransaction() => this.CreateDatabaseTransaction();

        ITransaction IDatabase.CreateTransaction() => this.CreateDatabaseTransaction();

        public ITransaction CreateDatabaseTransaction() => this.Transaction;

        public void AddDerivation(IDomainDerivation derivation) => this.Derivations = new List<IDomainDerivation>(this.Derivations) { derivation }.ToArray();

        public void Load(XmlReader reader)
        {
            this.Init();

            try
            {
                this.IsLoading = true;

                var load = new Load(this.Transaction, reader);
                load.Execute();

                this.Transaction.Commit();
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public void Save(XmlWriter writer) => this.Transaction.Save(writer);

        public bool ContainsClass(IComposite objectType, IObjectType concreteClass)
        {
            if (!this.concreteClassesByObjectType.TryGetValue(objectType, out var concreteClassOrClasses))
            {
                if (objectType.ExistExclusiveDatabaseClass)
                {
                    concreteClassOrClasses = objectType.ExclusiveDatabaseClass;
                    this.concreteClassesByObjectType[objectType] = concreteClassOrClasses;
                }
                else
                {
                    concreteClassOrClasses = new HashSet<IObjectType>(objectType.DatabaseClasses);
                    this.concreteClassesByObjectType[objectType] = concreteClassOrClasses;
                }
            }

            if (concreteClassOrClasses is IObjectType)
            {
                return concreteClass.Equals(concreteClassOrClasses);
            }

            var concreteClasses = (HashSet<IObjectType>)concreteClassOrClasses;
            return concreteClasses.Contains(concreteClass);
        }

        public void UnitRoleChecks(IStrategy strategy, IRoleType roleType)
        {
            if (!this.ContainsClass(roleType.AssociationType.ObjectType, strategy.Class))
            {
                throw new ArgumentException(strategy.Class + " is not a valid association object type for " + roleType + ".");
            }

            if (roleType.ObjectType is IComposite)
            {
                throw new ArgumentException(roleType.ObjectType + " on roleType " + roleType + " is not a unit type.");
            }
        }

        public void CompositeRoleChecks(IStrategy strategy, IRoleType roleType) => this.CompositeSharedChecks(strategy, roleType, null);

        public void CompositeRoleChecks(IStrategy strategy, IRoleType roleType, Strategy roleStrategy)
        {
            this.CompositeSharedChecks(strategy, roleType, roleStrategy);
            if (!roleType.IsOne)
            {
                throw new ArgumentException("RelationType " + roleType + " has multiplicity many.");
            }
        }

        public Strategy CompositeRolesChecks(IStrategy strategy, IRoleType roleType, Strategy roleStrategy)
        {
            this.CompositeSharedChecks(strategy, roleType, roleStrategy);
            if (!roleType.IsMany)
            {
                throw new ArgumentException("RelationType " + roleType + " has multiplicity one.");
            }

            return roleStrategy;
        }

        public virtual void Init()
        {
            this.Transaction.Init();

            this.transaction = null;

            this.Lifecycle.OnInit(this);
        }

        internal void OnObjectNotLoaded(Guid metaTypeId, long allorsObjectId)
        {
            var args = new ObjectNotLoadedEventArgs(metaTypeId, allorsObjectId);
            if (this.ObjectNotLoaded != null)
            {
                this.ObjectNotLoaded(this, args);
            }
            else
            {
                throw new Exception("Object not loaded: " + args);
            }
        }

        internal void OnRelationNotLoaded(Guid relationTypeId, long associationObjectId, string roleContents)
        {
            var args = new RelationNotLoadedEventArgs(relationTypeId, associationObjectId, roleContents);
            if (this.RelationNotLoaded != null)
            {
                this.RelationNotLoaded(this, args);
            }
            else
            {
                throw new Exception("RelationType not loaded: " + args);
            }
        }

        private void CompositeSharedChecks(IStrategy strategy, IRoleType roleType, Strategy roleStrategy)
        {
            if (!this.ContainsClass(roleType.AssociationType.ObjectType, strategy.Class))
            {
                throw new ArgumentException(strategy.Class + " has no roleType with role " + roleType + ".");
            }

            if (roleStrategy != null)
            {
                if (!strategy.Transaction.Equals(roleStrategy.Transaction))
                {
                    throw new ArgumentException(roleStrategy + " is from different transaction");
                }

                if (roleStrategy.IsDeleted)
                {
                    throw new ArgumentException(roleType + " on object " + strategy + " is removed.");
                }

                if (!(roleType.ObjectType is IComposite compositeType))
                {
                    throw new ArgumentException(roleStrategy + " has no CompositeType");
                }

                if (!compositeType.IsAssignableFrom(roleStrategy.Class))
                {
                    throw new ArgumentException(roleStrategy.Class + " is not compatible with type " + roleType.ObjectType + " of role " + roleType + ".");
                }
            }
        }
    }
}

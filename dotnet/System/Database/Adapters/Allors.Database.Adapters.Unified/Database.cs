// <copyright file="Database.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Unified
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Allors.Database.Tracing;
    using Meta;
    using Version = Allors.Version;

    public class Database : IDatabase, ITransaction, IDisposable
    {
        private static readonly HashSet<Strategy> EmptyStrategies = new HashSet<Strategy>();

        private readonly Dictionary<IObjectType, object> concreteClassesByObjectType;
        private readonly Dictionary<IObjectType, IObjectType[]> concreteClassesByObjectTypeForExtent;
        private readonly IDatabaseServices databaseServices;

        private ITransactionServices transactionServices;
        private bool busyCommittingOrRollingBack;
        private Dictionary<long, Strategy> strategyByObjectId;
        private Dictionary<IObjectType, HashSet<Strategy>> strategiesByObjectType;
        private long currentId;

        public Database(IDatabaseServices services, Configuration configuration)
        {
            this.databaseServices = services;
            if (this.databaseServices == null)
            {
                throw new Exception("Services is missing");
            }

            this.ObjectFactory = configuration.ObjectFactory;
            if (this.ObjectFactory == null)
            {
                throw new Exception("Configuration.ObjectFactory is missing");
            }

            this.MetaPopulation = this.ObjectFactory.MetaPopulation;

            this.concreteClassesByObjectType = new Dictionary<IObjectType, object>();
            this.concreteClassesByObjectTypeForExtent = new Dictionary<IObjectType, IObjectType[]>();

            this.Id = string.IsNullOrWhiteSpace(configuration.Id) ? Guid.NewGuid().ToString("N").ToLowerInvariant() : configuration.Id;

            this.busyCommittingOrRollingBack = false;
            this.ChangeLog = new ChangeLog();
            this.Reset();

            this.databaseServices.OnInit(this);
            this.transactionServices = this.databaseServices.CreateTransactionServices();
            this.transactionServices.OnInit(this);
        }

        public event ObjectNotLoadedEventHandler ObjectNotLoaded;

        public event RelationNotLoadedEventHandler RelationNotLoaded;

        public string Id { get; }

        public bool IsShared => false;

        public IObjectFactory ObjectFactory { get; }

        public IMetaPopulation MetaPopulation { get; }

        IDatabaseServices IDatabase.Services => this.databaseServices;

        ITransactionServices ITransaction.Services => this.transactionServices;

        IDatabase ITransaction.Database => this;

        public ISink Sink { get; set; }

        internal bool IsLoading { get; private set; }

        internal ChangeLog ChangeLog { get; private set; }

        public ITransaction CreateTransaction() => this;

        ITransaction IDatabase.CreateTransaction() => this;

        public void Load(XmlReader reader)
        {
            this.Init();

            try
            {
                this.IsLoading = true;

                var load = new Load(this, reader);
                load.Execute();

                this.Commit();
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public void Save(XmlWriter writer)
        {
            var sortedNonDeletedStrategiesByObjectType = new Dictionary<IObjectType, List<Strategy>>();
            foreach (var dictionaryEntry in this.strategyByObjectId)
            {
                var strategy = dictionaryEntry.Value;
                if (!strategy.IsDeleted)
                {
                    var objectType = strategy.UncheckedObjectType;

                    if (!sortedNonDeletedStrategiesByObjectType.TryGetValue(objectType, out var sortedNonDeletedStrategies))
                    {
                        sortedNonDeletedStrategies = new List<Strategy>();
                        sortedNonDeletedStrategiesByObjectType[objectType] = sortedNonDeletedStrategies;
                    }

                    sortedNonDeletedStrategies.Add(strategy);
                }
            }

            foreach (var dictionaryEntry in sortedNonDeletedStrategiesByObjectType)
            {
                var sortedNonDeletedStrategies = dictionaryEntry.Value;
                sortedNonDeletedStrategies.Sort(new Strategy.ObjectIdComparer());
            }

            var save = new Save(this, writer, sortedNonDeletedStrategiesByObjectType);
            save.Execute();
        }

        public virtual void Init()
        {
            this.Reset();
            this.ChangeLog = new ChangeLog();

            this.transactionServices = this.databaseServices.CreateTransactionServices();
            this.transactionServices.OnInit(this);

            this.databaseServices.OnInit(this);
        }

        public void Commit()
        {
            if (!this.busyCommittingOrRollingBack)
            {
                try
                {
                    this.busyCommittingOrRollingBack = true;

                    IList<Strategy> strategiesToDelete = null;
                    foreach (var dictionaryEntry in this.strategyByObjectId)
                    {
                        var strategy = dictionaryEntry.Value;

                        strategy.Commit();

                        if (strategy.IsDeleted)
                        {
                            strategiesToDelete ??= new List<Strategy>();
                            strategiesToDelete.Add(strategy);
                        }
                    }

                    if (strategiesToDelete != null)
                    {
                        foreach (var strategy in strategiesToDelete)
                        {
                            this.strategyByObjectId.Remove(strategy.ObjectId);

                            if (this.strategiesByObjectType.TryGetValue(strategy.UncheckedObjectType, out var strategies))
                            {
                                strategies.Remove(strategy);
                            }
                        }
                    }

                    this.ChangeLog = new ChangeLog();
                }
                finally
                {
                    this.busyCommittingOrRollingBack = false;
                }
            }
        }

        public void Rollback()
        {
            if (!this.busyCommittingOrRollingBack)
            {
                try
                {
                    this.busyCommittingOrRollingBack = true;

                    foreach (var strategy in new List<Strategy>(this.strategyByObjectId.Values))
                    {
                        strategy.Rollback();
                        if (strategy.IsDeleted)
                        {
                            this.strategyByObjectId.Remove(strategy.ObjectId);

                            if (this.strategiesByObjectType.TryGetValue(strategy.UncheckedObjectType, out var strategies))
                            {
                                strategies.Remove(strategy);
                            }
                        }
                    }

                    this.ChangeLog = new ChangeLog();
                }
                finally
                {
                    this.busyCommittingOrRollingBack = false;
                }
            }
        }

        public void Dispose() => this.Rollback();

        public T Create<T>() where T : IObject
        {
            var objectType = this.ObjectFactory.GetObjectType(typeof(T));

            if (!(objectType is IClass @class))
            {
                throw new Exception("IObjectType should be a class");
            }

            return (T)this.Create(@class);
        }

        public T[] Create<T>(int count) where T : IObject
        {
            var objects = new T[count];

            for (var i = 0; i < count; i++)
            {
                objects[i] = this.Create<T>();
            }

            return objects;
        }

        public IObject[] Create(IClass objectType, int count)
        {
            var arrayType = this.ObjectFactory.GetType(objectType);
            var allorsObjects = (IObject[])Array.CreateInstance(arrayType, count);
            for (var i = 0; i < count; i++)
            {
                allorsObjects[i] = this.Create(objectType);
            }

            return allorsObjects;
        }

        public virtual IObject Create(IClass objectType)
        {
            var strategy = new Strategy(this, objectType, ++this.currentId, Version.DatabaseInitial);
            this.AddStrategy(strategy);

            this.ChangeLog.OnCreated(strategy);

            return strategy.GetObject();
        }

        public IObject Instantiate(string objectIdString) => long.TryParse(objectIdString, out var id) ? this.Instantiate(id) : null;

        public IObject Instantiate(IObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            return this.Instantiate(obj.Strategy.ObjectId);
        }

        public IObject Instantiate(long objectId)
        {
            var strategy = this.InstantiateMemoryStrategy(objectId);
            return strategy?.GetObject();
        }

        public IStrategy InstantiateStrategy(long objectId) => this.InstantiateMemoryStrategy(objectId);

        public IObject[] Instantiate(IEnumerable<string> objectIdStrings) => objectIdStrings != null ? this.Instantiate(objectIdStrings.Select(long.Parse)) : Array.Empty<IObject>();

        public IObject[] Instantiate(IEnumerable<IObject> objects) => objects != null ? this.Instantiate(objects.Select(v => v.Id)) : Array.Empty<IObject>();

        public IObject[] Instantiate(IEnumerable<long> objectIds) => objectIds != null ? objectIds.Select(v => this.InstantiateMemoryStrategy(v)?.GetObject()).Where(v => v != null).ToArray() : Array.Empty<IObject>();

        public void Prefetch<T>(PrefetchPolicy prefetchPolicy, params T[] objects) where T : IObject
        {
            // nop
        }

        public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<string> objectIds)
        {
            // nop
        }

        public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<long> objectIds)
        {
            // nop
        }

        public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<IStrategy> strategies)
        {
            // nop
        }

        public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<IObject> objects)
        {
            // nop
        }

        public IChangeSet Checkpoint()
        {
            try
            {
                return this.ChangeLog.Checkpoint();
            }
            finally
            {
                this.ChangeLog = new ChangeLog();
            }
        }

        public Extent<T> Extent<T>() where T : IObject
        {
            if (!(this.ObjectFactory.GetObjectType(typeof(T)) is IComposite compositeType))
            {
                throw new Exception("type should be a CompositeType");
            }

            return this.Extent(compositeType);
        }

        public virtual Allors.Database.Extent Extent(IComposite objectType) => new ExtentFiltered(this, objectType);

        public virtual Allors.Database.Extent Union(Allors.Database.Extent firstOperand, Allors.Database.Extent secondOperand) => new ExtentOperation(this, (Extent)firstOperand, (Extent)secondOperand, ExtentOperationType.Union);

        public virtual Allors.Database.Extent Intersect(Allors.Database.Extent firstOperand, Allors.Database.Extent secondOperand) => new ExtentOperation(this, (Extent)firstOperand, (Extent)secondOperand, ExtentOperationType.Intersect);

        public virtual Allors.Database.Extent Except(Allors.Database.Extent firstOperand, Allors.Database.Extent secondOperand)
        {
            var firstExtent = (Extent)firstOperand;
            var secondExtent = (Extent)secondOperand;

            return new ExtentOperation(this, firstExtent, secondExtent, ExtentOperationType.Except);
        }

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

        internal Type GetTypeForObjectType(IObjectType objectType) => this.ObjectFactory.GetType(objectType);

        internal virtual Strategy InsertStrategy(IClass objectType, long objectId, long objectVersion)
        {
            var strategy = this.GetStrategy(objectId);
            if (strategy != null)
            {
                throw new Exception("Duplicate id error");
            }

            if (this.currentId < objectId)
            {
                this.currentId = objectId;
            }

            strategy = new Strategy(this, objectType, objectId, objectVersion);
            this.AddStrategy(strategy);

            this.ChangeLog.OnCreated(strategy);

            return strategy;
        }

        internal virtual Strategy InstantiateMemoryStrategy(long objectId) => this.GetStrategy(objectId);

        internal Strategy GetStrategy(IObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            return this.GetStrategy(obj.Id);
        }

        internal Strategy GetStrategy(long objectId)
        {
            if (!this.strategyByObjectId.TryGetValue(objectId, out var strategy))
            {
                return null;
            }

            return strategy.IsDeleted ? null : strategy;
        }

        internal void AddStrategy(Strategy strategy)
        {
            this.strategyByObjectId.Add(strategy.ObjectId, strategy);

            if (!this.strategiesByObjectType.TryGetValue(strategy.UncheckedObjectType, out var strategies))
            {
                strategies = new HashSet<Strategy>();
                this.strategiesByObjectType.Add(strategy.UncheckedObjectType, strategies);
            }

            strategies.Add(strategy);
        }

        internal virtual HashSet<Strategy> GetStrategiesForExtentIncludingDeleted(IObjectType type)
        {
            if (!this.concreteClassesByObjectTypeForExtent.TryGetValue(type, out var concreteClasses))
            {
                var sortedClassAndSubclassList = new List<IObjectType>();

                if (type is IClass)
                {
                    sortedClassAndSubclassList.Add(type);
                }

                if (type is IInterface)
                {
                    foreach (var subClass in ((IInterface)type).DatabaseClasses)
                    {
                        sortedClassAndSubclassList.Add(subClass);
                    }
                }

                concreteClasses = sortedClassAndSubclassList.ToArray();

                this.concreteClassesByObjectTypeForExtent[type] = concreteClasses;
            }

            switch (concreteClasses.Length)
            {
                case 0:
                    return EmptyStrategies;

                case 1:
                {
                    var objectType = concreteClasses[0];
                    if (this.strategiesByObjectType.TryGetValue(objectType, out var strategies))
                    {
                        return strategies;
                    }

                    return EmptyStrategies;
                }

                default:
                {
                    var strategies = new HashSet<Strategy>();

                    foreach (var objectType in concreteClasses)
                    {
                        if (this.strategiesByObjectType.TryGetValue(objectType, out var objectTypeStrategies))
                        {
                            strategies.UnionWith(objectTypeStrategies);
                        }
                    }

                    return strategies;
                }
            }
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
                if (!ReferenceEquals(strategy.Transaction, roleStrategy.Database))
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

        private void Reset()
        {
            this.strategyByObjectId = new Dictionary<long, Strategy>();
            this.strategiesByObjectType = new Dictionary<IObjectType, HashSet<Strategy>>();
            this.currentId = 0;
        }
    }
}

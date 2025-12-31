// <copyright file="Transaction.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    using Meta;
    using Version = Allors.Version;

    public class Transaction : ITransaction
    {
        private static readonly HashSet<Strategy> EmptyStrategies = new HashSet<Strategy>();

        private readonly Dictionary<IObjectType, IObjectType[]> concreteClassesByObjectType;
        private bool busyCommittingOrRollingBack;

        private Dictionary<long, Strategy> instantiatedStrategies;
        private HashSet<long> newObjectIds;
        private HashSet<long> deletedObjectIds;
        private HashSet<long> modifiedObjectIds;
        private Dictionary<long, long> snapshotVersionByObjectId;

        internal Transaction(Database database, ITransactionServices scope)
        {
            this.Database = database;
            this.Services = scope;

            this.busyCommittingOrRollingBack = false;

            this.concreteClassesByObjectType = new Dictionary<IObjectType, IObjectType[]>();

            this.ChangeLog = new ChangeLog();

            this.Reset();

            this.Services.OnInit(this);
        }

        public IDatabase Population => this.Database;

        IDatabase ITransaction.Database => this.Database;

        public ITransactionServices Services { get; }

        internal ChangeLog ChangeLog { get; private set; }

        internal Database Database { get; }

        public void Commit()
        {
            if (!this.busyCommittingOrRollingBack)
            {
                try
                {
                    this.busyCommittingOrRollingBack = true;

                    var changes = new TransactionChanges();

                    foreach (var objectId in this.deletedObjectIds)
                    {
                        changes.DeletedObjectIds.Add(objectId);
                    }

                    foreach (var kvp in this.instantiatedStrategies)
                    {
                        var strategy = kvp.Value;
                        var objectId = kvp.Key;

                        if (strategy.IsDeleted)
                        {
                            continue;
                        }

                        strategy.Commit();

                        if (this.newObjectIds.Contains(objectId))
                        {
                            var committedObject = strategy.BuildCommittedObject();
                            changes.NewObjects.Add(committedObject);
                        }
                        else if (this.modifiedObjectIds.Contains(objectId))
                        {
                            var committedObject = strategy.BuildCommittedObject();
                            committedObject.OriginalVersion = this.snapshotVersionByObjectId[objectId];
                            changes.ModifiedObjects.Add(committedObject);
                        }
                    }

                    this.Database.CommitTransaction(changes);

                    // Remove deleted strategies
                    foreach (var deletedId in this.deletedObjectIds)
                    {
                        this.instantiatedStrategies.Remove(deletedId);
                    }

                    // Update snapshot versions for all surviving instantiated strategies
                    // This enables the rolling transaction model where objects can be modified
                    // again after a commit
                    this.snapshotVersionByObjectId.Clear();
                    foreach (var kvp in this.instantiatedStrategies)
                    {
                        if (!kvp.Value.IsDeleted)
                        {
                            this.snapshotVersionByObjectId[kvp.Key] = kvp.Value.ObjectVersion;
                        }
                    }

                    this.newObjectIds.Clear();
                    this.deletedObjectIds.Clear();
                    this.modifiedObjectIds.Clear();

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

                    foreach (var strategy in new List<Strategy>(this.instantiatedStrategies.Values))
                    {
                        strategy.Rollback();

                        if (this.newObjectIds.Contains(strategy.ObjectId))
                        {
                            this.instantiatedStrategies.Remove(strategy.ObjectId);
                        }
                        else
                        {
                            // Refresh from committed store to get latest state
                            // (other transactions may have committed changes)
                            strategy.Refresh();
                        }
                    }

                    // Update snapshot versions from refreshed strategies
                    this.snapshotVersionByObjectId.Clear();
                    foreach (var kvp in this.instantiatedStrategies)
                    {
                        if (!kvp.Value.IsDeleted)
                        {
                            this.snapshotVersionByObjectId[kvp.Key] = kvp.Value.ObjectVersion;
                        }
                    }

                    this.newObjectIds.Clear();
                    this.deletedObjectIds.Clear();
                    this.modifiedObjectIds.Clear();

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
            var objectType = this.Database.ObjectFactory.GetObjectType(typeof(T));

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
            var arrayType = this.Database.ObjectFactory.GetType(objectType);
            var allorsObjects = (IObject[])Array.CreateInstance(arrayType, count);
            for (var i = 0; i < count; i++)
            {
                allorsObjects[i] = this.Create(objectType);
            }

            return allorsObjects;
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
            if (!(this.Database.ObjectFactory.GetObjectType(typeof(T)) is IComposite compositeType))
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

        public virtual IObject Create(IClass objectType)
        {
            var objectId = this.Database.NextId();
            var strategy = new Strategy(this, objectType, objectId, Version.DatabaseInitial);

            this.instantiatedStrategies[objectId] = strategy;
            this.newObjectIds.Add(objectId);

            this.ChangeLog.OnCreated(strategy);

            return strategy.GetObject();
        }

        internal void Init() => this.Reset();

        internal Type GetTypeForObjectType(IObjectType objectType) => this.Database.ObjectFactory.GetType(objectType);

        internal virtual Strategy InsertStrategy(IClass objectType, long objectId, long objectVersion)
        {
            if (this.instantiatedStrategies.TryGetValue(objectId, out var existing) && !existing.IsDeleted)
            {
                throw new Exception("Duplicate id error");
            }

            this.Database.UpdateCurrentId(objectId);

            var strategy = new Strategy(this, objectType, objectId, objectVersion);

            this.instantiatedStrategies[objectId] = strategy;
            this.newObjectIds.Add(objectId);

            this.ChangeLog.OnCreated(strategy);

            return strategy;
        }

        internal virtual Strategy InstantiateMemoryStrategy(long objectId)
        {
            if (this.deletedObjectIds.Contains(objectId))
            {
                return null;
            }

            if (this.instantiatedStrategies.TryGetValue(objectId, out var strategy))
            {
                return strategy.IsDeleted ? null : strategy;
            }

            var snapshot = this.Database.CommittedStore.GetSnapshot(objectId);
            if (snapshot == null)
            {
                return null;
            }

            strategy = new Strategy(this, snapshot);
            this.instantiatedStrategies[objectId] = strategy;
            this.snapshotVersionByObjectId[objectId] = snapshot.Version;

            return strategy;
        }

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
            if (this.deletedObjectIds.Contains(objectId))
            {
                return null;
            }

            if (this.instantiatedStrategies.TryGetValue(objectId, out var strategy))
            {
                return strategy.IsDeleted ? null : strategy;
            }

            return null;
        }

        internal void MarkModified(long objectId)
        {
            if (!this.newObjectIds.Contains(objectId))
            {
                this.modifiedObjectIds.Add(objectId);
            }
        }

        internal void MarkDeleted(long objectId)
        {
            this.deletedObjectIds.Add(objectId);
            this.modifiedObjectIds.Remove(objectId);
        }

        internal virtual HashSet<Strategy> GetStrategiesForExtentIncludingDeleted(IObjectType type)
        {
            var concreteClasses = this.GetConcreteClasses(type);

            if (concreteClasses.Length == 0)
            {
                return EmptyStrategies;
            }

            var strategies = new HashSet<Strategy>();

            foreach (var concreteClass in concreteClasses)
            {
                var committedIds = this.Database.CommittedStore.GetObjectIdsForType(concreteClass);
                foreach (var objectId in committedIds)
                {
                    var strategy = this.InstantiateMemoryStrategy(objectId);
                    if (strategy != null)
                    {
                        strategies.Add(strategy);
                    }
                }
            }

            foreach (var newId in this.newObjectIds)
            {
                if (this.instantiatedStrategies.TryGetValue(newId, out var strategy))
                {
                    foreach (var concreteClass in concreteClasses)
                    {
                        if (strategy.UncheckedObjectType.Equals(concreteClass))
                        {
                            strategies.Add(strategy);
                            break;
                        }
                    }
                }
            }

            return strategies;
        }

        /// <summary>
        /// Gets strategies for new objects (not yet committed) that match the given type.
        /// Used by index-based queries to include uncommitted objects.
        /// </summary>
        internal IEnumerable<Strategy> GetNewStrategiesForType(IObjectType type)
        {
            var concreteClasses = this.GetConcreteClasses(type);

            if (concreteClasses.Length == 0)
            {
                yield break;
            }

            foreach (var newId in this.newObjectIds)
            {
                if (this.instantiatedStrategies.TryGetValue(newId, out var strategy))
                {
                    foreach (var concreteClass in concreteClasses)
                    {
                        if (strategy.UncheckedObjectType.Equals(concreteClass))
                        {
                            yield return strategy;
                            break;
                        }
                    }
                }
            }
        }

        internal void Save(XmlWriter writer)
        {
            var sortedNonDeletedStrategiesByObjectType = new Dictionary<IObjectType, List<Strategy>>();

            var committedObjects = this.Database.CommittedStore.GetAllObjects();
            foreach (var committed in committedObjects)
            {
                var strategy = this.InstantiateMemoryStrategy(committed.ObjectId);
                if (strategy != null && !strategy.IsDeleted)
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

        private IObjectType[] GetConcreteClasses(IObjectType type)
        {
            if (!this.concreteClassesByObjectType.TryGetValue(type, out var concreteClasses))
            {
                var sortedClassAndSubclassList = new List<IObjectType>();

                if (type is IClass)
                {
                    sortedClassAndSubclassList.Add(type);
                }

                if (type is IInterface @interface)
                {
                    foreach (var subClass in @interface.DatabaseClasses)
                    {
                        sortedClassAndSubclassList.Add(subClass);
                    }
                }

                concreteClasses = sortedClassAndSubclassList.ToArray();

                this.concreteClassesByObjectType[type] = concreteClasses;
            }

            return concreteClasses;
        }

        private void Reset()
        {
            this.instantiatedStrategies = new Dictionary<long, Strategy>();
            this.newObjectIds = new HashSet<long>();
            this.deletedObjectIds = new HashSet<long>();
            this.modifiedObjectIds = new HashSet<long>();
            this.snapshotVersionByObjectId = new Dictionary<long, long>();
        }
    }
}

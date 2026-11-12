// <copyright file="Transaction.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Version = Allors.Version;

    /// <summary>
    /// A transaction reads the snapshot that was current when it started, or when it last committed or
    /// rolled back, and writes to a private overlay of cloned records. A transaction is used by one
    /// thread at a time.
    /// </summary>
    public sealed class Transaction : ITransaction
    {
        private readonly Dictionary<long, Strategy> strategyByObjectId = new Dictionary<long, Strategy>();

        private Snapshot baseSnapshot;
        private Dictionary<long, Record> overlay = new Dictionary<long, Record>();
        private HashSet<long> created = new HashSet<long>();
        private HashSet<long> roleChanged = new HashSet<long>();
        private HashSet<long> deleted = new HashSet<long>();

        // The created objects in the order of their creation and, for every class, the positions of its
        // objects in that order: an extent finds the created objects of its classes without going through
        // all of them.
        private List<long> createdInOrder = new List<long>();
        private Dictionary<IClass, List<int>> createdPositionsByClass = new Dictionary<IClass, List<int>>();

        internal Transaction(Database database, Snapshot snapshot)
        {
            this.Database = database;
            this.baseSnapshot = snapshot;

            this.ChangeLog = new ChangeLog();

            this.Services = this.Database.Services.CreateTransactionServices();
            this.Services.OnInit(this);
        }

        IDatabase ITransaction.Database => this.Database;

        public ITransactionServices Services { get; }

        internal Database Database { get; }

        internal ChangeLog ChangeLog { get; private set; }

        public void Commit()
        {
            var snapshot = this.overlay.Count > 0 || this.deleted.Count > 0
                ? this.Database.Commit(this.baseSnapshot, this.overlay, this.created, this.roleChanged, this.deleted)
                : this.Database.CurrentSnapshot;

            foreach (var objectId in this.deleted)
            {
                if (this.strategyByObjectId.Remove(objectId, out var strategy))
                {
                    strategy.Invalidate();
                }
            }

            this.Reset(snapshot);
        }

        public void Rollback()
        {
            var snapshot = this.Database.CurrentSnapshot;

            foreach (var objectId in this.created)
            {
                if (this.strategyByObjectId.Remove(objectId, out var strategy))
                {
                    strategy.Invalidate();
                }
            }

            this.Reset(snapshot);
        }

        public void Dispose() => this.Rollback();

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

        public IObject Create(IClass objectType)
        {
            var layout = this.Database.GetLayout(objectType);
            var objectId = this.Database.AllocateId();

            var record = new Record(layout, objectId, Version.DatabaseInitial);
            this.overlay[objectId] = record;
            this.AddCreated(objectType, objectId);

            var strategy = new Strategy(this, objectId);
            this.strategyByObjectId[objectId] = strategy;

            this.ChangeLog.OnCreated(strategy);

            return strategy.GetObject();
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

        public IObject Instantiate(long objectId) => this.GetStrategy(objectId)?.GetObject();

        public IStrategy InstantiateStrategy(long objectId) => this.GetStrategy(objectId);

        public IObject[] Instantiate(IEnumerable<string> objectIdStrings) => objectIdStrings != null ? this.Instantiate(objectIdStrings.Select(long.Parse)) : Array.Empty<IObject>();

        public IObject[] Instantiate(IEnumerable<IObject> objects) => objects != null ? this.Instantiate(objects.Select(v => v.Id)) : Array.Empty<IObject>();

        public IObject[] Instantiate(IEnumerable<long> objectIds) => objectIds != null ? objectIds.Select(v => this.GetStrategy(v)?.GetObject()).Where(v => v != null).ToArray() : Array.Empty<IObject>();

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

        public Extent<T> Extent<T>() where T : IObject
        {
            if (!(this.Database.ObjectFactory.GetObjectType(typeof(T)) is IComposite compositeType))
            {
                throw new Exception("type should be a CompositeType");
            }

            return this.Extent(compositeType);
        }

        public Allors.Database.Extent Extent(IComposite objectType) => new ExtentFiltered(this, objectType);

        public Allors.Database.Extent Union(Allors.Database.Extent firstOperand, Allors.Database.Extent secondOperand) => new ExtentOperation(this, (Extent)firstOperand, (Extent)secondOperand, ExtentOperationType.Union);

        public Allors.Database.Extent Intersect(Allors.Database.Extent firstOperand, Allors.Database.Extent secondOperand) => new ExtentOperation(this, (Extent)firstOperand, (Extent)secondOperand, ExtentOperationType.Intersect);

        public Allors.Database.Extent Except(Allors.Database.Extent firstOperand, Allors.Database.Extent secondOperand) => new ExtentOperation(this, (Extent)firstOperand, (Extent)secondOperand, ExtentOperationType.Except);

        internal Type GetTypeForObjectType(IObjectType objectType) => this.Database.ObjectFactory.GetType(objectType);

        internal bool IsCreated(long objectId) => this.created.Contains(objectId);

        internal void MarkDeleted(long objectId) => this.deleted.Add(objectId);

        internal void MarkRoleChanged(long objectId) => this.roleChanged.Add(objectId);

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
            if (this.deleted.Contains(objectId))
            {
                return null;
            }

            if (this.strategyByObjectId.TryGetValue(objectId, out var strategy))
            {
                return strategy.IsDeleted ? null : strategy;
            }

            if (!this.overlay.ContainsKey(objectId) && !this.baseSnapshot.RecordById.ContainsKey(objectId))
            {
                return null;
            }

            strategy = new Strategy(this, objectId);
            this.strategyByObjectId[objectId] = strategy;
            return strategy;
        }

        /// <summary>
        /// The record of an object as this transaction sees it, or null when the object does not exist
        /// or is deleted in this transaction.
        /// </summary>
        internal Record ResolveRecord(long objectId)
        {
            if (this.deleted.Contains(objectId))
            {
                return null;
            }

            if (this.overlay.TryGetValue(objectId, out var record))
            {
                return record;
            }

            return this.baseSnapshot.RecordById.TryGetValue(objectId, out record) ? record : null;
        }

        /// <summary>
        /// The record of an object in the overlay of this transaction, cloned from the snapshot on the
        /// first write.
        /// </summary>
        internal Record EnsureInOverlay(long objectId)
        {
            if (this.overlay.TryGetValue(objectId, out var record))
            {
                return record;
            }

            if (!this.baseSnapshot.RecordById.TryGetValue(objectId, out var committed))
            {
                throw new InvalidOperationException($"Object {objectId} does not exist in this transaction.");
            }

            record = committed.Clone();
            this.overlay[objectId] = record;
            return record;
        }

        internal Strategy InsertStrategy(IClass objectType, long objectId, long objectVersion)
        {
            if (this.overlay.ContainsKey(objectId) || this.baseSnapshot.RecordById.ContainsKey(objectId))
            {
                throw new Exception("Duplicate id error");
            }

            this.Database.AdjustId(objectId);

            var record = new Record(this.Database.GetLayout(objectType), objectId, objectVersion);
            this.overlay[objectId] = record;
            this.AddCreated(objectType, objectId);

            var strategy = new Strategy(this, objectId);
            this.strategyByObjectId[objectId] = strategy;

            this.ChangeLog.OnCreated(strategy);

            return strategy;
        }

        internal IEnumerable<Strategy> GetStrategiesForExtent(IComposite objectType)
        {
            var concreteClasses = this.Database.GetConcreteClasses(objectType);

            foreach (var concreteClass in concreteClasses)
            {
                if (this.baseSnapshot.ObjectIdsByClass.TryGetValue(concreteClass, out var objectIds))
                {
                    foreach (var objectId in objectIds)
                    {
                        var strategy = this.GetStrategy(objectId);
                        if (strategy != null)
                        {
                            yield return strategy;
                        }
                    }
                }
            }

            if (this.created.Count > 0)
            {
                foreach (var objectId in this.CreatedObjectIds(concreteClasses))
                {
                    var strategy = this.GetStrategy(objectId);
                    if (strategy != null)
                    {
                        yield return strategy;
                    }
                }
            }
        }

        private void AddCreated(IClass @class, long objectId)
        {
            this.created.Add(objectId);

            if (!this.createdPositionsByClass.TryGetValue(@class, out var positions))
            {
                positions = new List<int>();
                this.createdPositionsByClass.Add(@class, positions);
            }

            positions.Add(this.createdInOrder.Count);
            this.createdInOrder.Add(objectId);
        }

        /// <summary>
        /// The ids of the objects of these classes that this transaction created, in the order of their
        /// creation.
        /// </summary>
        private long[] CreatedObjectIds(IClass[] classes)
        {
            List<int> positions = null;
            var merged = false;

            foreach (var @class in classes)
            {
                if (this.createdPositionsByClass.TryGetValue(@class, out var positionsOfClass))
                {
                    if (positions == null)
                    {
                        positions = positionsOfClass;
                    }
                    else
                    {
                        if (!merged)
                        {
                            positions = new List<int>(positions);
                            merged = true;
                        }

                        positions.AddRange(positionsOfClass);
                    }
                }
            }

            if (positions == null)
            {
                return Array.Empty<long>();
            }

            if (merged)
            {
                positions.Sort();
            }

            var objectIds = new long[positions.Count];
            for (var i = 0; i < objectIds.Length; i++)
            {
                objectIds[i] = this.createdInOrder[positions[i]];
            }

            return objectIds;
        }

        private void Reset(Snapshot snapshot)
        {
            if (this.baseSnapshot.Epoch != snapshot.Epoch)
            {
                foreach (var kvp in this.strategyByObjectId.ToArray())
                {
                    if (!snapshot.RecordById.TryGetValue(kvp.Key, out var record) || kvp.Value.UncheckedObjectType != record.Class)
                    {
                        kvp.Value.Invalidate();
                        this.strategyByObjectId.Remove(kvp.Key);
                    }
                }
            }

            this.baseSnapshot = snapshot;
            this.overlay = new Dictionary<long, Record>();
            this.created = new HashSet<long>();
            this.roleChanged = new HashSet<long>();
            this.deleted = new HashSet<long>();
            this.createdInOrder = new List<long>();
            this.createdPositionsByClass = new Dictionary<IClass, List<int>>();
            this.ChangeLog = new ChangeLog();
        }
    }
}

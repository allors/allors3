// <copyright file="Transaction.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the Transaction type.</summary>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Caching;
    using Meta;

    public sealed class Transaction : ITransaction
    {
        private bool busyCommittingOrRollingBack;

        private Dictionary<string, object> properties;

        private readonly ReadGuard readGuard;

        private readonly WriteScope writeScope;

        private readonly State state;

        internal Transaction(Database database, IConnection connection, ITransactionServices scope, ReadGuard readGuard = null, WriteScope writeScope = null)
        {
            this.Database = database;
            this.readGuard = readGuard;
            this.writeScope = writeScope;
            // A writer may hold mutable values from its shared cache. A protected
            // attempt only caches data loaded from its own physical snapshot.
            this.Cache = readGuard == null ? database.Cache : new DefaultCache(null);
            this.Connection = readGuard != null ? new ReadConnection(readGuard) : writeScope != null ? new WriteConnection(writeScope) : connection;
            this.CommandConnection = readGuard == null ? connection : new ReadCommandConnection(connection, readGuard);
            this.Services = readGuard == null ? scope : new ReadServices(scope, readGuard);

            this.state = new State(this, true);


            if (this.Database.Sink != null)
            {
                this.Prefetcher = new TraceablePrefetcher(this);
                this.Commands = new TraceableCommands(this, this.CommandConnection);
            }
            else
            {
                this.Prefetcher = new UntraceablePrefetcher(this);
                this.Commands = new UntraceableCommands(this, this.CommandConnection);
            }

            scope.OnInit(this);
        }

        public IConnection Connection { get; }

        public Commands Commands { get; }

        public State State
        {
            get
            {
                this.AssertWritable("Direct transaction-state access");
                return this.state;
            }
        }

        internal State AdapterState
        {
            get
            {
                this.EnsureReadable();
                return this.state;
            }
        }

        internal IConnection CommandConnection { get; }

        internal ICache Cache { get; }

        internal bool IsProtectedRead => this.readGuard != null;

        internal void EnsureReadable()
        {
            this.readGuard?.EnsureValid();
            this.writeScope?.EnsureReadable();
        }

        internal void AssertWritable(string operation)
        {
            this.readGuard?.Reject(operation);
            this.writeScope?.AssertWritable(operation);
        }

        internal void CaptureAffected(Strategy strategy, AffectedObjectKind kind) => this.writeScope?.Capture(strategy, kind);

        IDatabase ITransaction.Database => this.Database;

        public ITransactionServices Services { get; }

        public Database Database { get; }

        private Prefetcher Prefetcher { get; }

        public object this[string name]
        {
            get
            {
                this.EnsureReadable();
                if (this.properties == null)
                {
                    return null;
                }

                this.properties.TryGetValue(name, out var value);
                return value;
            }

            set
            {
                this.EnsureReadable();
                if (this.properties == null)
                {
                    this.properties = new Dictionary<string, object>();
                }

                if (value == null)
                {
                    this.properties.Remove(name);
                }
                else
                {
                    this.properties[name] = value;
                }
            }
        }

        public IObject Create(IClass objectType)
        {
            this.AssertWritable("Object creation");
            if (!objectType.IsClass)
            {
                throw new ArgumentException("Can not create non concrete composite type " + objectType);
            }

            var reference = this.Commands.CreateObject(objectType);
            this.AdapterState.ReferenceByObjectId[reference.ObjectId] = reference;

            this.Cache.SetObjectType(reference.ObjectId, objectType);

            this.AdapterState.ChangeLog.OnCreated(reference.Strategy);

            return reference.Strategy.GetObject();
        }

        public IObject[] Create(IClass objectType, int count)
        {
            this.AssertWritable("Object creation");
            if (!objectType.IsClass)
            {
                throw new ArgumentException("Can not create non concrete composite type " + objectType);
            }

            var references = this.Commands.CreateObjects(objectType, count);

            var arrayType = this.Database.ObjectFactory.GetType(objectType);
            var domainObjects = (IObject[])Array.CreateInstance(arrayType, count);

            for (var i = 0; i < references.Count; i++)
            {
                var reference = references[i];
                this.AdapterState.ReferenceByObjectId[reference.ObjectId] = reference;

                domainObjects[i] = reference.Strategy.GetObject();

                this.AdapterState.ChangeLog.OnCreated(reference.Strategy);
            }

            return domainObjects;
        }

        public IObject Instantiate(IObject obj) => obj != null ? this.Instantiate(obj.Strategy.ObjectId) : null;

        public IObject Instantiate(string objectId) => long.TryParse(objectId, out var id) ? this.Instantiate(id) : null;

        public IObject Instantiate(long objectId)
        {
            var strategy = this.InstantiateStrategy(objectId);
            return strategy?.GetObject();
        }

        public IStrategy InstantiateStrategy(long objectId)
        {
            this.EnsureReadable();
            if (!this.AdapterState.ReferenceByObjectId.TryGetValue(objectId, out var reference))
            {
                reference = this.Commands.InstantiateObject(objectId);
                if (reference != null)
                {
                    this.AdapterState.ReferenceByObjectId[objectId] = reference;
                }
            }

            if (reference == null || !reference.Exists)
            {
                return null;
            }

            return reference.Strategy;
        }

        public IObject[] Instantiate(IEnumerable<string> objectIdStrings) => objectIdStrings != null ? this.Instantiate(objectIdStrings.Select(long.Parse)) : Array.Empty<IObject>();

        public IObject[] Instantiate(IEnumerable<IObject> objects) => objects != null ? this.Instantiate(objects.Select(v => v.Id)) : Array.Empty<IObject>();

        public IObject[] Instantiate(IEnumerable<long> objectIds)
        {
            this.EnsureReadable();
            IObject[] emptyObjects = Array.Empty<IObject>();
            if (objectIds == null)
            {
                return emptyObjects;
            }

            var objectIdArray = objectIds.ToArray();

            if (objectIdArray.Length == 0)
            {
                return emptyObjects;
            }

            var references = new List<Reference>(objectIdArray.Length);

            List<long> nonCachedObjectIds = null;
            foreach (var objectId in objectIdArray)
            {
                if (!this.AdapterState.ReferenceByObjectId.TryGetValue(objectId, out var reference))
                {
                    if (nonCachedObjectIds == null)
                    {
                        nonCachedObjectIds = new List<long>();
                    }

                    nonCachedObjectIds.Add(objectId);
                }
                else if (!reference.Strategy.IsDeleted)
                {
                    references.Add(reference);
                }
            }

            if (nonCachedObjectIds != null)
            {
                var nonCachedReferences = this.Commands.InstantiateReferences(nonCachedObjectIds);
                references.AddRange(nonCachedReferences);

                // Return objects in the same order as objectIds
                var referenceById = new Dictionary<long, Reference>();
                foreach (var reference in references)
                {
                    referenceById[reference.ObjectId] = reference;
                }
                references = new List<Reference>();
                foreach (var objectId in objectIdArray)
                {
                    if (referenceById.TryGetValue(objectId, out var reference))
                    {
                        references.Add(reference);
                    }
                }
            }

            var allorsObjects = new IObject[references.Count];
            for (var i = 0; i < allorsObjects.Length; i++)
            {
                allorsObjects[i] = references[i].Strategy.GetObject();
            }

            return allorsObjects;
        }

        public void Prefetch<T>(PrefetchPolicy prefetchPolicy, params T[] objects) where T : IObject => this.Prefetch(prefetchPolicy, objects.Select(x => x.Strategy.ObjectId));

        public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<IObject> objects) => this.Prefetch(prefetchPolicy, objects.Select(x => x.Strategy.ObjectId));

        public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<IStrategy> strategies) => this.Prefetch(prefetchPolicy, strategies.Select(v => v.ObjectId));

        public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<string> objectIdStrings) => this.Prefetch(prefetchPolicy, objectIdStrings.Select(long.Parse));

        public void Prefetch(PrefetchPolicy prefetchPolicy, IEnumerable<long> objectIds)
        {
            this.EnsureReadable();
            var references = this.Prefetcher.GetReferencesForPrefetching(objectIds);

            if (references.Count != 0)
            {
                this.AdapterState.Flush();

                var prefetcher = new Prefetch(this.Prefetcher, prefetchPolicy, references);
                prefetcher.Execute();
            }
        }

        public IChangeSet Checkpoint()
        {
            this.EnsureReadable();
            this.writeScope?.AssertWritable("Transaction checkpoint");
            try
            {
                return this.AdapterState.ChangeLog.Checkpoint();
            }
            finally
            {
                this.AdapterState.ChangeLog.Reset();
            }
        }

        public Extent<T> Extent<T>() where T : IObject => this.Extent((IComposite)this.Database.ObjectFactory.GetObjectType(typeof(T)));

        public Allors.Database.Extent Extent(IComposite type)
        {
            this.EnsureReadable();
            return new ExtentFiltered(this, type);
        }

        public Allors.Database.Extent Union(Allors.Database.Extent firstOperand, Allors.Database.Extent secondOperand) => this.CreateExtentOperation(firstOperand, secondOperand, ExtentOperations.Union);

        public Allors.Database.Extent Intersect(Allors.Database.Extent firstOperand, Allors.Database.Extent secondOperand) => this.CreateExtentOperation(firstOperand, secondOperand, ExtentOperations.Intersect);

        public Allors.Database.Extent Except(Allors.Database.Extent firstOperand, Allors.Database.Extent secondOperand) => this.CreateExtentOperation(firstOperand, secondOperand, ExtentOperations.Except);

        public void Commit()
        {
            this.AssertWritable("Transaction commit");
            this.writeScope?.AssertTransactionCommit();
            if (!this.busyCommittingOrRollingBack)
            {
                try
                {
                    this.busyCommittingOrRollingBack = true;

                    var accessed = new List<long>(this.AdapterState.ReferenceByObjectId.Keys);
                    this.AdapterState.Flush();

                    var changed = this.AdapterState.ModifiedRolesByReference?.Select(dictionaryEntry => dictionaryEntry.Key.ObjectId).ToArray() ?? Array.Empty<long>();
                    if (changed.Length > 0)
                    {
                        this.Commands.UpdateVersion(changed);
                    }

                    if (this.writeScope == null)
                    {
                        this.Connection.Commit();
                    }
                    else
                    {
                        this.writeScope.CommitConnection();
                    }

                    this.AdapterState.ModifiedRolesByReference = null;

                    var referencesWithStrategy = new HashSet<Reference>();
                    foreach (var reference in this.AdapterState.ReferenceByObjectId.Values)
                    {
                        reference.Commit(referencesWithStrategy);
                    }

                    this.AdapterState.ExistingObjectIdsWithoutReference = new HashSet<long>();
                    this.AdapterState.ReferencesWithoutVersions = referencesWithStrategy;

                    this.AdapterState.ReferenceByObjectId = new Dictionary<long, Reference>();
                    foreach (var reference in referencesWithStrategy)
                    {
                        this.AdapterState.ReferenceByObjectId[reference.ObjectId] = reference;
                    }

                    this.AdapterState.AssociationByRoleByAssociationType = new Dictionary<IAssociationType, Dictionary<Reference, Reference>>();
                    this.AdapterState.AssociationsByRoleByAssociationType = new Dictionary<IAssociationType, Dictionary<Reference, long[]>>();

                    this.AdapterState.ChangeLog.Reset();

                    this.Cache.OnCommit(accessed, changed);

                    this.Prefetcher.ResetCommands();
                    this.Commands.ResetCommands();
                }
                finally
                {
                    this.busyCommittingOrRollingBack = false;
                }
            }
        }

        public void Rollback()
        {
            this.readGuard?.Reject("Transaction rollback or restart");
            this.writeScope?.AssertTransactionRollback();
            if (!this.busyCommittingOrRollingBack)
            {
                try
                {
                    this.busyCommittingOrRollingBack = true;

                    var accessed = new List<long>(this.AdapterState.ReferenceByObjectId.Keys);

                    this.CommandConnection.Rollback();

                    var referencesWithStrategy = new HashSet<Reference>();
                    foreach (var reference in this.AdapterState.ReferenceByObjectId.Values)
                    {
                        reference.Rollback(referencesWithStrategy);
                    }

                    this.AdapterState.ExistingObjectIdsWithoutReference = new HashSet<long>();
                    this.AdapterState.ReferencesWithoutVersions = referencesWithStrategy;

                    this.AdapterState.ReferenceByObjectId = new Dictionary<long, Reference>();
                    foreach (var reference in referencesWithStrategy)
                    {
                        this.AdapterState.ReferenceByObjectId[reference.ObjectId] = reference;
                    }

                    this.AdapterState.UnflushedRolesByReference = null;
                    this.AdapterState.ModifiedRolesByReference = null;
                    this.AdapterState.TriggersFlushRolesByAssociationType = null;

                    this.AdapterState.AssociationByRoleByAssociationType = new Dictionary<IAssociationType, Dictionary<Reference, Reference>>();
                    this.AdapterState.AssociationsByRoleByAssociationType = new Dictionary<IAssociationType, Dictionary<Reference, long[]>>();

                    this.AdapterState.ChangeLog.Reset();

                    this.Cache.OnRollback(accessed);

                    this.Prefetcher.ResetCommands();
                    this.Commands.ResetCommands();
                }
                finally
                {
                    this.busyCommittingOrRollingBack = false;
                }
            }
        }

        public void Dispose() => this.Rollback();

        public T[] Create<T>(int count) where T : IObject
        {
            var objectType = (IClass)this.Database.ObjectFactory.GetObjectType(typeof(T));
            var objects = this.Create(objectType, count);

            var result = new T[objects.Length];
            for (var i = 0; i < objects.Length; i++)
            {
                result[i] = (T)objects[i];
            }

            return result;
        }

        public T Create<T>() where T : IObject
        {
            var objectType = (IClass)this.Database.ObjectFactory.GetObjectType(typeof(T));
            return (T)this.Create(objectType);
        }

        public override string ToString() => "Transaction[id=" + this.GetHashCode() + "] " + this.Database;

        private Allors.Database.Extent CreateExtentOperation(Allors.Database.Extent firstOperand, Allors.Database.Extent secondOperand, ExtentOperations operation)
        {
            this.EnsureReadable();
            if (this.IsProtectedRead &&
                (firstOperand is not Extent first || secondOperand is not Extent second ||
                 !ReferenceEquals(first.ContainedInExtent.Transaction, this) || !ReferenceEquals(second.ContainedInExtent.Transaction, this)))
            {
                this.AssertWritable("Combining extents from another transaction");
            }

            return new ExtentOperation(((Extent)firstOperand).ContainedInExtent, ((Extent)secondOperand).ContainedInExtent, operation);
        }

        internal Reference[] GetOrCreateReferencesForExistingObjects(IEnumerable<long> objectIds) => this.AdapterState.GetOrCreateReferencesForExistingObjects(objectIds, this);

        internal long[] GetAssociations(Strategy roleStrategy, IAssociationType associationType)
        {
            var associationsByRole = this.AdapterState.GetAssociationsByRole(associationType);

            if (!associationsByRole.TryGetValue(roleStrategy.Reference, out var associations))
            {
                this.AdapterState.FlushConditionally(roleStrategy.ObjectId, associationType);
                associations = this.Commands.GetCompositesAssociation(roleStrategy, associationType);
                associationsByRole[roleStrategy.Reference] = associations;
            }

            return associations;
        }

        internal void RemoveAssociation(Reference association, Reference role, IAssociationType associationType)
        {
            var associationsByRole = this.AdapterState.GetAssociationsByRole(associationType);

            if (associationsByRole.TryGetValue(role, out var associations))
            {
                associationsByRole[role] = associations.Remove(association.ObjectId);
            }
        }

        internal void AddReferenceWithoutVersionOrExistsKnown(Reference reference) => this.AdapterState.ReferencesWithoutVersions.Add(reference);

        internal void GetVersionAndExists()
        {
            if (this.AdapterState.ReferencesWithoutVersions.Count > 0)
            {
                var versionByObjectId = this.Commands.GetVersions(this.AdapterState.ReferencesWithoutVersions);
                foreach (var association in this.AdapterState.ReferencesWithoutVersions)
                {
                    if (versionByObjectId.TryGetValue(association.ObjectId, out var version))
                    {
                        association.Version = version;
                        association.Exists = true;
                    }
                    else
                    {
                        association.Exists = false;
                    }
                }

                this.AdapterState.ReferencesWithoutVersions = new HashSet<Reference>();
            }
        }

        internal void InstantiateReferences(IEnumerable<long> objectIds)
        {
            var forceEvaluation = this.Commands.InstantiateReferences(objectIds).ToArray();
        }
    }
}

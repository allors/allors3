// <copyright file="Database.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;
    using System.Xml;
    using Allors.Database.Tracing;
    using Meta;

    /// <summary>
    /// An in-memory database that many transactions, also on different threads, share. The committed
    /// state is an immutable <see cref="Snapshot"/>: a transaction reads the snapshot that was current
    /// when it started or when it last committed or rolled back, writes to a private overlay, and
    /// publishes a new snapshot when it commits. Commits are serialised by one lock, and when two
    /// transactions change the same object the last one to commit wins.
    /// </summary>
    public class Database : IDatabase
    {
        private readonly object commitLock = new object();
        private readonly Dictionary<IClass, ClassLayout> layoutByClass;
        private readonly Dictionary<IObjectType, IClass[]> concreteClassesByObjectType;
        private readonly Dictionary<IObjectType, HashSet<IClass>> concreteClassSetByObjectType;

        private Snapshot snapshot;
        private long nextId;
        private long epoch;

        public Database(IDatabaseServices services, Configuration configuration)
        {
            this.Services = services ?? throw new Exception("Services is missing");
            this.ObjectFactory = configuration?.ObjectFactory ?? throw new Exception("Configuration.ObjectFactory is missing");
            this.MetaPopulation = this.ObjectFactory.MetaPopulation;

            this.layoutByClass = new Dictionary<IClass, ClassLayout>();
            foreach (var @class in this.MetaPopulation.DatabaseClasses)
            {
                this.layoutByClass[@class] = new ClassLayout(@class);
            }

            this.concreteClassesByObjectType = new Dictionary<IObjectType, IClass[]>();
            this.concreteClassSetByObjectType = new Dictionary<IObjectType, HashSet<IClass>>();
            foreach (var composite in this.MetaPopulation.DatabaseComposites)
            {
                var concreteClasses = new List<IClass>();
                if (composite is IClass @class)
                {
                    concreteClasses.Add(@class);
                }
                else if (composite is IInterface @interface)
                {
                    concreteClasses.AddRange(@interface.DatabaseClasses);
                }

                this.concreteClassesByObjectType[composite] = concreteClasses.ToArray();
                this.concreteClassSetByObjectType[composite] = new HashSet<IClass>(concreteClasses);
            }

            this.Id = string.IsNullOrWhiteSpace(configuration.Id) ? Guid.NewGuid().ToString("N").ToLowerInvariant() : configuration.Id;

            this.snapshot = Snapshot.Empty(this.epoch);

            this.Services.OnInit(this);
        }

        public event ObjectNotLoadedEventHandler ObjectNotLoaded;

        public event RelationNotLoadedEventHandler RelationNotLoaded;

        public string Id { get; }

        public bool IsShared => true;

        public IObjectFactory ObjectFactory { get; }

        public IMetaPopulation MetaPopulation { get; }

        public IDatabaseServices Services { get; }

        public ISink Sink { get; set; }

        internal bool IsLoading { get; private set; }

        internal Snapshot CurrentSnapshot => Volatile.Read(ref this.snapshot);

        public ITransaction CreateTransaction() => new Transaction(this, this.CurrentSnapshot);

        public void Init()
        {
            this.Replace(ImmutableDictionary<long, Record>.Empty, ImmutableDictionary<IClass, ImmutableSortedSet<long>>.Empty, 0);

            this.Services.OnInit(this);
        }

        public void Load(XmlReader reader)
        {
            this.Init();

            try
            {
                this.IsLoading = true;

                using (var transaction = new Transaction(this, this.CurrentSnapshot))
                {
                    var load = new Load(transaction, reader);
                    load.Execute();

                    transaction.Commit();
                }
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public void Save(XmlWriter writer)
        {
            var save = new Save(this, writer, this.CurrentSnapshot);
            save.Execute();
        }

        /// <summary>
        /// Takes a restore point: the committed state of this database and the position of its id sequence.
        /// Taking one costs no more than a reference, because the committed state is immutable.
        /// </summary>
        public RestorePoint TakeRestorePoint()
        {
            lock (this.commitLock)
            {
                return new RestorePoint(this, this.snapshot, Volatile.Read(ref this.nextId));
            }
        }

        /// <summary>
        /// Brings the database back to a restore point taken from it, in constant time. Like <see cref="Init"/>
        /// this starts a new epoch, so a transaction that started earlier can not commit any more, and objects
        /// created afterwards take the ids that were free when the restore point was taken, so every restore
        /// leads to the same ids.
        /// </summary>
        public void Restore(RestorePoint restorePoint)
        {
            if (restorePoint == null)
            {
                throw new ArgumentNullException(nameof(restorePoint));
            }

            if (restorePoint.Database != this)
            {
                throw new ArgumentException("The restore point was taken from another database. Take one from this database, or use CreateClone to start a database from the state of another one.", nameof(restorePoint));
            }

            this.Replace(restorePoint.Snapshot.RecordById, restorePoint.Snapshot.ObjectIdsByClass, restorePoint.NextId);

            this.Services.OnInit(this);
        }

        /// <summary>
        /// Creates a database that starts from the committed state of this database. The two databases
        /// share that immutable state and go their own way from there; the clone continues the id
        /// sequence of this database.
        /// </summary>
        public Database CreateClone(IDatabaseServices services)
        {
            var clone = new Database(services, new Configuration { ObjectFactory = this.ObjectFactory })
            {
                Sink = this.Sink,
            };

            Snapshot current;
            long currentNextId;
            lock (this.commitLock)
            {
                current = this.snapshot;
                currentNextId = Volatile.Read(ref this.nextId);
            }

            clone.Replace(current.RecordById, current.ObjectIdsByClass, currentNextId);

            return clone;
        }

        internal long AllocateId() => Interlocked.Increment(ref this.nextId);

        internal void AdjustId(long objectId)
        {
            while (true)
            {
                var current = Volatile.Read(ref this.nextId);
                if (current >= objectId || Interlocked.CompareExchange(ref this.nextId, objectId, current) == current)
                {
                    return;
                }
            }
        }

        internal ClassLayout GetLayout(IClass @class) =>
            this.layoutByClass.TryGetValue(@class, out var layout)
                ? layout
                : throw new ArgumentException($"{@class} is not a database class of this database. Only classes of the meta population that are stored in the database can have objects.");

        internal IClass[] GetConcreteClasses(IObjectType objectType) =>
            this.concreteClassesByObjectType.TryGetValue(objectType, out var concreteClasses) ? concreteClasses : Array.Empty<IClass>();

        internal bool ContainsClass(IComposite objectType, IClass concreteClass) =>
            this.concreteClassSetByObjectType.TryGetValue(objectType, out var concreteClasses) && concreteClasses.Contains(concreteClass);

        internal Snapshot Commit(Snapshot baseSnapshot, Dictionary<long, Record> overlay, HashSet<long> created, HashSet<long> roleChanged, HashSet<long> deleted)
        {
            lock (this.commitLock)
            {
                var current = this.snapshot;
                if (current.Epoch != baseSnapshot.Epoch)
                {
                    throw new InvalidOperationException("The database was initialised, loaded or restored after this transaction started, so its changes can not be committed. Roll back and redo the work in a new transaction.");
                }

                var recordById = current.RecordById.ToBuilder();
                var objectIdsByClass = current.ObjectIdsByClass.ToBuilder();

                foreach (var objectId in deleted)
                {
                    if (recordById.TryGetValue(objectId, out var record))
                    {
                        recordById.Remove(objectId);
                        RemoveObjectId(objectIdsByClass, record.Class, objectId);
                    }
                }

                var bumpVersions = !this.IsLoading;
                foreach (var kvp in overlay)
                {
                    var objectId = kvp.Key;
                    if (deleted.Contains(objectId))
                    {
                        continue;
                    }

                    var record = kvp.Value;
                    var isCreated = created.Contains(objectId);

                    if (!isCreated)
                    {
                        if (!recordById.TryGetValue(objectId, out var committed))
                        {
                            // Another transaction deleted the object since this one started; the deletion stands.
                            continue;
                        }

                        // When the later of two transactions that changed the same object wins, its version still
                        // moves past the version the earlier one committed.
                        if (committed.Version > record.Version)
                        {
                            record.Version = committed.Version;
                        }
                    }

                    if (bumpVersions && roleChanged.Contains(objectId))
                    {
                        record.Version++;
                    }

                    recordById[objectId] = record;

                    if (isCreated)
                    {
                        AddObjectId(objectIdsByClass, record.Class, objectId);
                    }
                }

                var next = new Snapshot(current.Epoch, recordById.ToImmutable(), objectIdsByClass.ToImmutable());
                Volatile.Write(ref this.snapshot, next);
                return next;
            }
        }

        internal void UnitRoleChecks(IStrategy strategy, IRoleType roleType)
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

        internal void CompositeRoleChecks(Strategy strategy, IRoleType roleType) => this.CompositeSharedChecks(strategy, roleType, null);

        internal void CompositeRoleChecks(Strategy strategy, IRoleType roleType, Strategy roleStrategy)
        {
            this.CompositeSharedChecks(strategy, roleType, roleStrategy);
            if (!roleType.IsOne)
            {
                throw new ArgumentException("RelationType " + roleType + " has multiplicity many.");
            }
        }

        internal Strategy CompositeRolesChecks(Strategy strategy, IRoleType roleType, Strategy roleStrategy)
        {
            this.CompositeSharedChecks(strategy, roleType, roleStrategy);
            if (!roleType.IsMany)
            {
                throw new ArgumentException("RelationType " + roleType + " has multiplicity one.");
            }

            return roleStrategy;
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

        private static void AddObjectId(ImmutableDictionary<IClass, ImmutableSortedSet<long>>.Builder objectIdsByClass, IClass @class, long objectId) =>
            objectIdsByClass[@class] = objectIdsByClass.TryGetValue(@class, out var objectIds) ? objectIds.Add(objectId) : ImmutableSortedSet.Create(objectId);

        private static void RemoveObjectId(ImmutableDictionary<IClass, ImmutableSortedSet<long>>.Builder objectIdsByClass, IClass @class, long objectId)
        {
            if (objectIdsByClass.TryGetValue(@class, out var objectIds))
            {
                var remaining = objectIds.Remove(objectId);
                if (remaining.IsEmpty)
                {
                    objectIdsByClass.Remove(@class);
                }
                else
                {
                    objectIdsByClass[@class] = remaining;
                }
            }
        }

        private void Replace(ImmutableDictionary<long, Record> recordById, ImmutableDictionary<IClass, ImmutableSortedSet<long>> objectIdsByClass, long newNextId)
        {
            lock (this.commitLock)
            {
                this.epoch++;
                Volatile.Write(ref this.snapshot, new Snapshot(this.epoch, recordById, objectIdsByClass));
                Interlocked.Exchange(ref this.nextId, newNextId);
            }
        }

        private void CompositeSharedChecks(Strategy strategy, IRoleType roleType, Strategy roleStrategy)
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

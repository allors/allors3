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
    /// publishes a new snapshot when it commits. Commits are serialised by one lock. A transaction that
    /// changes or deletes an object which another transaction changed or deleted after this transaction
    /// read it fails on commit with a <see cref="ConcurrencyException"/>, as it would on a database server;
    /// a change to the other end of a relation is reconciled instead.
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

            // Discard services populated by the import transaction before another transaction reads the loaded objects.
            this.Services.OnInit(this);
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

                AssertNoWriteConflicts(baseSnapshot, current, created, roleChanged, deleted);

                var recordById = current.RecordById.ToBuilder();
                var objectIdsByClass = current.ObjectIdsByClass.ToBuilder();
                var writable = new Dictionary<long, Record>();
                var changedRoles = new HashSet<long>();

                Record ForWrite(long objectId)
                {
                    if (!writable.TryGetValue(objectId, out var record))
                    {
                        record = recordById[objectId].Clone();
                        writable[objectId] = record;
                        recordById[objectId] = record;
                    }

                    return record;
                }

                void RemoveRelation(IRoleType roleType, long associationId, long roleId)
                {
                    if (recordById.TryGetValue(associationId, out var association))
                    {
                        if (roleType.IsOne)
                        {
                            if (association.GetCompositeRole(roleType) == roleId)
                            {
                                ForWrite(associationId).SetCompositeRole(roleType, 0);
                                changedRoles.Add(associationId);
                            }
                        }
                        else if (association.GetCompositesRole(roleType).Contains(roleId))
                        {
                            ForWrite(associationId).RemoveCompositesRole(roleType, roleId);
                            changedRoles.Add(associationId);
                        }
                    }

                    if (recordById.TryGetValue(roleId, out var role))
                    {
                        var associationType = roleType.AssociationType;
                        if (associationType.IsOne)
                        {
                            if (role.GetCompositeAssociation(associationType) == associationId)
                            {
                                ForWrite(roleId).SetCompositeAssociation(associationType, 0);
                            }
                        }
                        else if (role.GetCompositesAssociation(associationType).Contains(associationId))
                        {
                            ForWrite(roleId).RemoveCompositesAssociation(associationType, associationId);
                        }
                    }
                }

                void AddRelation(IRoleType roleType, long associationId, long roleId)
                {
                    // A deletion committed since this transaction started still wins.
                    if (!recordById.TryGetValue(associationId, out var association) ||
                        !recordById.TryGetValue(roleId, out var role))
                    {
                        return;
                    }

                    var associationType = roleType.AssociationType;
                    if (associationType.IsOne)
                    {
                        var previousAssociationId = role.GetCompositeAssociation(associationType);
                        if (previousAssociationId != 0 && previousAssociationId != associationId)
                        {
                            RemoveRelation(roleType, previousAssociationId, roleId);
                        }
                    }

                    if (roleType.IsOne)
                    {
                        var previousRoleId = association.GetCompositeRole(roleType);
                        if (previousRoleId != roleId)
                        {
                            if (previousRoleId != 0)
                            {
                                RemoveRelation(roleType, associationId, previousRoleId);
                            }

                            ForWrite(associationId).SetCompositeRole(roleType, roleId);
                            changedRoles.Add(associationId);
                        }
                    }
                    else if (!association.GetCompositesRole(roleType).Contains(roleId))
                    {
                        ForWrite(associationId).AddCompositesRole(roleType, roleId);
                        changedRoles.Add(associationId);
                    }

                    if (associationType.IsOne)
                    {
                        if (role.GetCompositeAssociation(associationType) != associationId)
                        {
                            ForWrite(roleId).SetCompositeAssociation(associationType, associationId);
                        }
                    }
                    else if (!role.GetCompositesAssociation(associationType).Contains(associationId))
                    {
                        ForWrite(roleId).AddCompositesAssociation(associationType, associationId);
                    }
                }

                foreach (var objectId in deleted)
                {
                    if (recordById.TryGetValue(objectId, out var record))
                    {
                        // Use the current relations, including those committed after the deleter started.
                        foreach (var relation in record.CompositeRoles())
                        {
                            RemoveRelation(relation.Key, objectId, relation.Value);
                        }

                        foreach (var relation in record.CompositesRoles())
                        {
                            foreach (var roleId in relation.Value)
                            {
                                RemoveRelation(relation.Key, objectId, roleId);
                            }
                        }

                        foreach (var associationType in record.Layout.CompositeAssociationTypes)
                        {
                            var associationId = record.GetCompositeAssociation(associationType);
                            if (associationId != 0)
                            {
                                RemoveRelation(associationType.RoleType, associationId, objectId);
                            }
                        }

                        foreach (var associationType in record.Layout.CompositesAssociationTypes)
                        {
                            foreach (var associationId in record.GetCompositesAssociation(associationType))
                            {
                                RemoveRelation(associationType.RoleType, associationId, objectId);
                            }
                        }

                        recordById.Remove(objectId);
                        writable.Remove(objectId);
                        RemoveObjectId(objectIdsByClass, record.Class, objectId);
                    }
                }

                var replacements = new List<Record>();
                foreach (var kvp in overlay)
                {
                    var objectId = kvp.Key;
                    var isCreated = created.Contains(objectId);
                    if (deleted.Contains(objectId) || (!isCreated && !roleChanged.Contains(objectId)))
                    {
                        continue;
                    }

                    var record = kvp.Value;
                    if (isCreated)
                    {
                        // Install every new object before resolving relations between new objects.
                        var added = new Record(record.Layout, objectId, record.Version);
                        recordById[objectId] = added;
                        writable[objectId] = added;
                        AddObjectId(objectIdsByClass, record.Class, objectId);
                    }
                    else
                    {
                        // The conflict check guarantees that the object still exists at the version this
                        // transaction read.
                        ForWrite(objectId);
                    }

                    replacements.Add(record);
                    if (roleChanged.Contains(objectId))
                    {
                        changedRoles.Add(objectId);
                    }
                }

                // Forward roles are authoritative. Keep current inverse associations and reconcile both
                // ends, rather than publishing the inverse arrays captured by a stale transaction.
                foreach (var replacement in replacements)
                {
                    var objectId = replacement.ObjectId;
                    var record = ForWrite(objectId);
                    record.CopyUnitRolesFrom(replacement);

                    foreach (var relation in record.CompositeRoles())
                    {
                        if (replacement.GetCompositeRole(relation.Key) != relation.Value || !recordById.ContainsKey(relation.Value))
                        {
                            RemoveRelation(relation.Key, objectId, relation.Value);
                        }
                    }

                    foreach (var relation in record.CompositesRoles())
                    {
                        var desired = replacement.GetCompositesRole(relation.Key);
                        foreach (var roleId in relation.Value)
                        {
                            if (!desired.Contains(roleId) || !recordById.ContainsKey(roleId))
                            {
                                RemoveRelation(relation.Key, objectId, roleId);
                            }
                        }
                    }
                }

                foreach (var replacement in replacements)
                {
                    foreach (var relation in replacement.CompositeRoles())
                    {
                        AddRelation(relation.Key, replacement.ObjectId, relation.Value);
                    }

                    foreach (var relation in replacement.CompositesRoles())
                    {
                        foreach (var roleId in relation.Value)
                        {
                            AddRelation(relation.Key, replacement.ObjectId, roleId);
                        }
                    }
                }

                if (!this.IsLoading)
                {
                    foreach (var objectId in changedRoles)
                    {
                        if (recordById.ContainsKey(objectId))
                        {
                            ForWrite(objectId).Version++;
                        }
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

        /// <summary>
        /// A transaction may commit a change to or the deletion of an object only when the object still has
        /// the version this transaction read. Changes to the other end of a relation are not writes to that
        /// end; they are reconciled against the current state.
        /// </summary>
        private static void AssertNoWriteConflicts(Snapshot baseSnapshot, Snapshot current, HashSet<long> created, HashSet<long> roleChanged, HashSet<long> deleted)
        {
            void Check(long objectId)
            {
                if (created.Contains(objectId) || !baseSnapshot.RecordById.TryGetValue(objectId, out var read))
                {
                    return;
                }

                if (!current.RecordById.TryGetValue(objectId, out var committed))
                {
                    throw new ConcurrencyException($"Object {objectId} of class {read.Class.Name} was deleted by another transaction after this transaction read it. Roll back and redo the work.");
                }

                if (committed.Version != read.Version)
                {
                    throw new ConcurrencyException($"Object {objectId} of class {read.Class.Name} was changed by another transaction after this transaction read it (version {read.Version} then, {committed.Version} now). Roll back and redo the work.");
                }
            }

            foreach (var objectId in roleChanged)
            {
                Check(objectId);
            }

            foreach (var objectId in deleted)
            {
                Check(objectId);
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

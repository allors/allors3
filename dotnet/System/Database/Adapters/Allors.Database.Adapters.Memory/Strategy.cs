// <copyright file="Strategy.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;
    using Adapters;
    using Meta;

    public sealed class Strategy : IStrategy
    {
        private CommittedObject snapshot;

        private Dictionary<IRoleType, object> unitRoleByRoleType;
        private Dictionary<IRoleType, long?> compositeRoleByRoleType;
        private Dictionary<IRoleType, HashSet<long>> compositesRoleByRoleType;
        private Dictionary<IAssociationType, long?> compositeAssociationByAssociationType;
        private Dictionary<IAssociationType, HashSet<long>> compositesAssociationByAssociationType;

        private Dictionary<IRoleType, object> rollbackUnitRoleByRoleType;
        private Dictionary<IRoleType, long?> rollbackCompositeRoleByRoleType;
        private HashSet<IRoleType> modifiedCompositesRoleTypes;
        private Dictionary<IAssociationType, long?> rollbackCompositeAssociationByAssociationType;
        private HashSet<IAssociationType> modifiedCompositesAssociationTypes;
        private bool isDeletedOnRollback;
        private WeakReference allorizedObjectWeakReference;

        internal Strategy(Transaction transaction, IClass objectType, long objectId, long version)
        {
            this.Transaction = transaction;
            this.UncheckedObjectType = objectType;
            this.ObjectId = objectId;

            this.snapshot = null;

            this.IsDeleted = false;
            this.isDeletedOnRollback = true;
            this.IsNewInTransaction = true;

            this.ObjectVersion = version;

            this.unitRoleByRoleType = new Dictionary<IRoleType, object>();
            this.compositeRoleByRoleType = new Dictionary<IRoleType, long?>();
            this.compositesRoleByRoleType = new Dictionary<IRoleType, HashSet<long>>();
            this.compositeAssociationByAssociationType = new Dictionary<IAssociationType, long?>();
            this.compositesAssociationByAssociationType = new Dictionary<IAssociationType, HashSet<long>>();

            this.rollbackUnitRoleByRoleType = null;
            this.rollbackCompositeRoleByRoleType = null;
            this.modifiedCompositesRoleTypes = null;
            this.rollbackCompositeAssociationByAssociationType = null;
            this.modifiedCompositesAssociationTypes = null;
        }

        internal Strategy(Transaction transaction, CommittedObject snapshot)
        {
            this.Transaction = transaction;
            this.UncheckedObjectType = snapshot.ObjectType;
            this.ObjectId = snapshot.ObjectId;

            this.snapshot = snapshot;

            this.IsDeleted = false;
            this.isDeletedOnRollback = false;
            this.IsNewInTransaction = false;

            this.ObjectVersion = snapshot.Version;

            this.unitRoleByRoleType = null;
            this.compositeRoleByRoleType = null;
            this.compositesRoleByRoleType = null;
            this.compositeAssociationByAssociationType = null;
            this.compositesAssociationByAssociationType = null;

            this.rollbackUnitRoleByRoleType = null;
            this.rollbackCompositeRoleByRoleType = null;
            this.modifiedCompositesRoleTypes = null;
            this.rollbackCompositeAssociationByAssociationType = null;
            this.modifiedCompositesAssociationTypes = null;
        }

        public bool IsDeleted { get; private set; }

        public bool IsNewInTransaction { get; private set; }

        public long ObjectId { get; }

        public long ObjectVersion { get; private set; }

        public IClass Class
        {
            get
            {
                this.AssertNotDeleted();
                return this.UncheckedObjectType;
            }
        }

        ITransaction IStrategy.Transaction => this.Transaction;

        internal IClass UncheckedObjectType { get; }

        internal Transaction Transaction { get; }

        private ChangeLog ChangeLog => this.Transaction.ChangeLog;

        private Dictionary<IRoleType, object> RollbackUnitRoleByRoleType => this.rollbackUnitRoleByRoleType ??= new Dictionary<IRoleType, object>();

        private Dictionary<IRoleType, long?> RollbackCompositeRoleByRoleType => this.rollbackCompositeRoleByRoleType ??= new Dictionary<IRoleType, long?>();

        private HashSet<IRoleType> ModifiedCompositesRoleTypes => this.modifiedCompositesRoleTypes ??= new HashSet<IRoleType>();

        private Dictionary<IAssociationType, long?> RollbackCompositeAssociationByAssociationType => this.rollbackCompositeAssociationByAssociationType ??= new Dictionary<IAssociationType, long?>();

        private HashSet<IAssociationType> ModifiedCompositesAssociationTypes => this.modifiedCompositesAssociationTypes ??= new HashSet<IAssociationType>();

        public override string ToString() => this.UncheckedObjectType.Name + " " + this.ObjectId;

        public object GetRole(IRoleType roleType) =>
            roleType switch
            {
                { } unitRole when unitRole.ObjectType.IsUnit => this.GetUnitRole(roleType),
                { } compositeRole when compositeRole.IsOne => this.GetCompositeRole(roleType),
                _ => this.GetCompositesRole<IObject>(roleType)
            };

        public void SetRole(IRoleType roleType, object value)
        {
            switch (roleType)
            {
                case { } unitRole when unitRole.ObjectType.IsUnit:
                    this.SetUnitRole(roleType, value);
                    break;
                case { } compositeRole when compositeRole.IsOne:
                    this.SetCompositeRole(roleType, (IObject)value);
                    break;
                default:
                    this.SetCompositesRole(roleType, (IEnumerable<IObject>)value);
                    break;
            }
        }

        public void RemoveRole(IRoleType roleType)
        {
            switch (roleType)
            {
                case { } unitRole when unitRole.ObjectType.IsUnit:
                    this.RemoveUnitRole(roleType);
                    break;
                case { } compositeRole when compositeRole.IsOne:
                    this.RemoveCompositeRole(roleType);
                    break;
                default:
                    this.RemoveCompositesRole(roleType);
                    break;
            }
        }

        public bool ExistRole(IRoleType roleType) =>
            roleType switch
            {
                { } unitRole when unitRole.ObjectType.IsUnit => this.ExistUnitRole(roleType),
                { } compositeRole when compositeRole.IsOne => this.ExistCompositeRole(roleType),
                _ => this.ExistCompositesRole(roleType)
            };

        public object GetUnitRole(IRoleType roleType)
        {
            this.AssertNotDeleted();
            return this.GetInternalizedUnitRole(roleType);
        }

        public void SetUnitRole(IRoleType roleType, object role)
        {
            this.AssertNotDeleted();
            this.Transaction.Database.UnitRoleChecks(this, roleType);

            var previousRole = this.GetInternalizedUnitRole(roleType);
            role = roleType.Normalize(role);

            if (Equals(role, previousRole))
            {
                return;
            }

            this.unitRoleByRoleType ??= new Dictionary<IRoleType, object>();

            if (!this.RollbackUnitRoleByRoleType.ContainsKey(roleType))
            {
                this.RollbackUnitRoleByRoleType[roleType] = previousRole;
            }

            this.ChangeLog.OnChangingUnitRole(this, roleType, previousRole);

            switch (role)
            {
                case null:
                    this.unitRoleByRoleType.Remove(roleType);
                    break;
                default:
                    this.unitRoleByRoleType[roleType] = role;
                    break;
            }

            this.Transaction.MarkModified(this.ObjectId);
        }

        public void RemoveUnitRole(IRoleType roleType) => this.SetUnitRole(roleType, null);

        public bool ExistUnitRole(IRoleType roleType)
        {
            this.AssertNotDeleted();
            return this.GetInternalizedUnitRole(roleType) != null;
        }

        public IObject GetCompositeRole(IRoleType roleType)
        {
            this.AssertNotDeleted();
            var roleId = this.GetCompositeRoleId(roleType);
            if (roleId == null)
            {
                return null;
            }

            var strategy = this.Transaction.InstantiateMemoryStrategy(roleId.Value);
            return strategy?.GetObject();
        }

        public void SetCompositeRole(IRoleType roleType, IObject newRole)
        {
            if (newRole == null)
            {
                this.RemoveCompositeRole(roleType);
            }
            else if (roleType.AssociationType.IsOne)
            {
                this.SetCompositeRoleOne2One(roleType, (Strategy)newRole.Strategy);
            }
            else
            {
                this.SetCompositeRoleMany2One(roleType, (Strategy)newRole.Strategy);
            }
        }

        public void RemoveCompositeRole(IRoleType roleType)
        {
            if (roleType.AssociationType.IsOne)
            {
                this.RemoveCompositeRoleOne2One(roleType);
            }
            else
            {
                this.RemoveCompositeRoleMany2One(roleType);
            }
        }

        public bool ExistCompositeRole(IRoleType roleType)
        {
            this.AssertNotDeleted();
            return this.GetCompositeRoleId(roleType) != null;
        }

        public IEnumerable<T> GetCompositesRole<T>(IRoleType roleType) where T : IObject
        {
            this.AssertNotDeleted();

            var roleIds = this.GetCompositesRoleIds(roleType);
            if (roleIds == null)
            {
                yield break;
            }

            foreach (var roleId in roleIds.ToArray())
            {
                var strategy = this.Transaction.InstantiateMemoryStrategy(roleId);
                if (strategy != null)
                {
                    yield return (T)strategy.GetObject();
                }
            }
        }

        public void SetCompositesRole(IRoleType roleType, IEnumerable<IObject> roles)
        {
            if (roles == null || (roles is ICollection<IObject> collection && collection.Count == 0))
            {
                this.RemoveCompositesRole(roleType);
            }
            else
            {
                var strategies = roles
                    .Where(v => v != null)
                    .Select(v => this.Transaction.Database.CompositeRolesChecks(this, roleType, (Strategy)v.Strategy))
                    .Distinct();

                if (roleType.AssociationType.IsMany)
                {
                    this.SetCompositesRolesMany2Many(roleType, strategies);
                }
                else
                {
                    this.SetCompositesRolesOne2Many(roleType, strategies);
                }
            }
        }

        public void AddCompositesRole(IRoleType roleType, IObject role)
        {
            this.AssertNotDeleted();
            if (role == null)
            {
                return;
            }

            var roleStrategy = this.Transaction.Database.CompositeRolesChecks(this, roleType, (Strategy)role.Strategy);

            if (roleType.AssociationType.IsMany)
            {
                this.AddCompositeRoleMany2Many(roleType, roleStrategy);
            }
            else
            {
                this.AddCompositeRoleOne2Many(roleType, roleStrategy);
            }
        }

        public void RemoveCompositesRole(IRoleType roleType, IObject role)
        {
            this.AssertNotDeleted();

            if (role == null)
            {
                return;
            }

            var roleStrategy = this.Transaction.Database.CompositeRolesChecks(this, roleType, (Strategy)role.Strategy);

            if (roleType.AssociationType.IsMany)
            {
                this.RemoveCompositeRoleMany2Many(roleType, roleStrategy);
            }
            else
            {
                this.RemoveCompositeRoleOne2Many(roleType, roleStrategy);
            }
        }

        public void RemoveCompositesRole(IRoleType roleType)
        {
            this.AssertNotDeleted();

            if (roleType.AssociationType.IsMany)
            {
                this.RemoveCompositeRolesMany2Many(roleType);
            }
            else
            {
                this.RemoveCompositeRolesOne2Many(roleType);
            }
        }

        public bool ExistCompositesRole(IRoleType roleType)
        {
            this.AssertNotDeleted();
            var roleIds = this.GetCompositesRoleIds(roleType);
            return roleIds != null && roleIds.Count > 0;
        }

        public object GetAssociation(IAssociationType associationType) => associationType.IsMany ? this.GetCompositesAssociation<IObject>(associationType) : (object)this.GetCompositeAssociation(associationType);

        public bool ExistAssociation(IAssociationType associationType) => associationType.IsMany ? this.ExistCompositesAssociation(associationType) : this.ExistCompositeAssociation(associationType);

        public IObject GetCompositeAssociation(IAssociationType associationType)
        {
            this.AssertNotDeleted();
            var associationId = this.GetCompositeAssociationId(associationType);
            if (associationId == null)
            {
                return null;
            }

            var strategy = this.Transaction.InstantiateMemoryStrategy(associationId.Value);
            return strategy?.GetObject();
        }

        public bool ExistCompositeAssociation(IAssociationType associationType) => this.GetCompositeAssociation(associationType) != null;

        public IEnumerable<T> GetCompositesAssociation<T>(IAssociationType associationType) where T : IObject
        {
            this.AssertNotDeleted();

            var associationIds = this.GetCompositesAssociationIds(associationType);
            if (associationIds == null)
            {
                yield break;
            }

            foreach (var associationId in associationIds.ToArray())
            {
                var strategy = this.Transaction.InstantiateMemoryStrategy(associationId);
                if (strategy != null)
                {
                    yield return (T)strategy.GetObject();
                }
            }
        }

        public bool ExistCompositesAssociation(IAssociationType associationType)
        {
            this.AssertNotDeleted();
            var associationIds = this.GetCompositesAssociationIds(associationType);
            return associationIds != null && associationIds.Count > 0;
        }

        public void Delete()
        {
            this.AssertNotDeleted();

            foreach (var roleType in this.UncheckedObjectType.DatabaseRoleTypes)
            {
                if (this.ExistRole(roleType))
                {
                    if (roleType.ObjectType is IUnit)
                    {
                        this.RemoveUnitRole(roleType);
                    }
                    else
                    {
                        var associationType = roleType.AssociationType;
                        if (associationType.IsMany)
                        {
                            if (roleType.IsMany)
                            {
                                this.RemoveCompositeRolesMany2Many(roleType);
                            }
                            else
                            {
                                this.RemoveCompositeRoleMany2One(roleType);
                            }
                        }
                        else if (roleType.IsMany)
                        {
                            this.RemoveCompositeRolesOne2Many(roleType);
                        }
                        else
                        {
                            this.RemoveCompositeRoleOne2One(roleType);
                        }
                    }
                }
            }

            foreach (var associationType in this.UncheckedObjectType.DatabaseAssociationTypes)
            {
                var roleType = associationType.RoleType;

                if (this.ExistAssociation(associationType))
                {
                    if (associationType.IsMany)
                    {
                        var associationIds = this.GetCompositesAssociationIds(associationType);

                        if (associationIds != null)
                        {
                            foreach (var associationId in new HashSet<long>(associationIds))
                            {
                                var associationStrategy = this.Transaction.InstantiateMemoryStrategy(associationId);
                                if (associationStrategy != null)
                                {
                                    if (roleType.IsMany)
                                    {
                                        associationStrategy.RemoveCompositeRoleMany2Many(roleType, this);
                                    }
                                    else
                                    {
                                        associationStrategy.RemoveCompositeRoleMany2One(roleType);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var associationId = this.GetCompositeAssociationId(associationType);

                        if (associationId != null)
                        {
                            var associationStrategy = this.Transaction.InstantiateMemoryStrategy(associationId.Value);
                            if (associationStrategy != null)
                            {
                                if (roleType.IsMany)
                                {
                                    associationStrategy.RemoveCompositeRoleOne2Many(roleType, this);
                                }
                                else
                                {
                                    associationStrategy.RemoveCompositeRoleOne2One(roleType);
                                }
                            }
                        }
                    }
                }
            }

            this.IsDeleted = true;
            this.Transaction.MarkDeleted(this.ObjectId);

            this.ChangeLog.OnDeleted(this);
        }

        public IObject GetObject()
        {
            IObject allorsObject;
            if (this.allorizedObjectWeakReference == null)
            {
                allorsObject = this.Transaction.Database.ObjectFactory.Create(this);
                this.allorizedObjectWeakReference = new WeakReference(allorsObject);
            }
            else
            {
                allorsObject = (IObject)this.allorizedObjectWeakReference.Target;
                if (allorsObject == null)
                {
                    allorsObject = this.Transaction.Database.ObjectFactory.Create(this);
                    this.allorizedObjectWeakReference.Target = allorsObject;
                }
            }

            return allorsObject;
        }

        internal void Commit()
        {
            if (!this.IsDeleted && !this.Transaction.Database.IsLoading)
            {
                if (this.rollbackUnitRoleByRoleType != null ||
                    this.rollbackCompositeRoleByRoleType != null ||
                    this.modifiedCompositesRoleTypes != null)
                {
                    ++this.ObjectVersion;
                }
            }

            this.rollbackUnitRoleByRoleType = null;
            this.rollbackCompositeRoleByRoleType = null;
            this.modifiedCompositesRoleTypes = null;
            this.rollbackCompositeAssociationByAssociationType = null;
            this.modifiedCompositesAssociationTypes = null;

            this.isDeletedOnRollback = this.IsDeleted;
            this.IsNewInTransaction = false;
        }

        internal void Rollback()
        {
            if (this.rollbackUnitRoleByRoleType != null)
            {
                foreach (var dictionaryItem in this.rollbackUnitRoleByRoleType)
                {
                    var roleType = dictionaryItem.Key;
                    var role = dictionaryItem.Value;

                    this.unitRoleByRoleType ??= new Dictionary<IRoleType, object>();

                    if (role != null)
                    {
                        this.unitRoleByRoleType[roleType] = role;
                    }
                    else
                    {
                        this.unitRoleByRoleType.Remove(roleType);
                    }
                }
            }

            if (this.rollbackCompositeRoleByRoleType != null)
            {
                foreach (var dictionaryItem in this.rollbackCompositeRoleByRoleType)
                {
                    var roleType = dictionaryItem.Key;
                    var role = dictionaryItem.Value;

                    this.compositeRoleByRoleType ??= new Dictionary<IRoleType, long?>();

                    if (role != null)
                    {
                        this.compositeRoleByRoleType[roleType] = role;
                    }
                    else
                    {
                        this.compositeRoleByRoleType.Remove(roleType);
                    }
                }
            }

            if (this.modifiedCompositesRoleTypes != null)
            {
                foreach (var roleType in this.modifiedCompositesRoleTypes)
                {
                    // Restore from immutable snapshot - copy only happens during rare rollback
                    if (this.snapshot != null && this.snapshot.CompositesRoleByRoleType.TryGetValue(roleType, out var snapshotRoleIds))
                    {
                        // Snapshot had this role - restore it
                        this.compositesRoleByRoleType ??= new Dictionary<IRoleType, HashSet<long>>();
                        this.compositesRoleByRoleType[roleType] = new HashSet<long>(snapshotRoleIds);
                    }
                    else
                    {
                        // Snapshot didn't have this role (was empty/null) - remove local modification
                        this.compositesRoleByRoleType?.Remove(roleType);
                    }
                }
            }

            if (this.rollbackCompositeAssociationByAssociationType != null)
            {
                foreach (var dictionaryItem in this.rollbackCompositeAssociationByAssociationType)
                {
                    var associationType = dictionaryItem.Key;
                    var association = dictionaryItem.Value;

                    this.compositeAssociationByAssociationType ??= new Dictionary<IAssociationType, long?>();

                    if (association != null)
                    {
                        this.compositeAssociationByAssociationType[associationType] = association;
                    }
                    else
                    {
                        this.compositeAssociationByAssociationType.Remove(associationType);
                    }
                }
            }

            if (this.modifiedCompositesAssociationTypes != null)
            {
                foreach (var associationType in this.modifiedCompositesAssociationTypes)
                {
                    // Restore from immutable snapshot - copy only happens during rare rollback
                    if (this.snapshot != null && this.snapshot.CompositesAssociationByAssociationType.TryGetValue(associationType, out var snapshotAssociationIds))
                    {
                        // Snapshot had this association - restore it
                        this.compositesAssociationByAssociationType ??= new Dictionary<IAssociationType, HashSet<long>>();
                        this.compositesAssociationByAssociationType[associationType] = new HashSet<long>(snapshotAssociationIds);
                    }
                    else
                    {
                        // Snapshot didn't have this association (was empty/null) - remove local modification
                        this.compositesAssociationByAssociationType?.Remove(associationType);
                    }
                }
            }

            this.rollbackUnitRoleByRoleType = null;
            this.rollbackCompositeRoleByRoleType = null;
            this.modifiedCompositesRoleTypes = null;
            this.rollbackCompositeAssociationByAssociationType = null;
            this.modifiedCompositesAssociationTypes = null;

            this.IsDeleted = this.isDeletedOnRollback;
            this.IsNewInTransaction = false;
        }

        internal void Refresh()
        {
            // Get a fresh snapshot from the committed store
            var freshSnapshot = this.Transaction.Database.CommittedStore.GetSnapshot(this.ObjectId);
            if (freshSnapshot != null)
            {
                this.snapshot = freshSnapshot;
                this.ObjectVersion = freshSnapshot.Version;

                // Clear local modifications to use snapshot values
                this.unitRoleByRoleType = null;
                this.compositeRoleByRoleType = null;
                this.compositesRoleByRoleType = null;
                this.compositeAssociationByAssociationType = null;
                this.compositesAssociationByAssociationType = null;

                this.rollbackUnitRoleByRoleType = null;
                this.rollbackCompositeRoleByRoleType = null;
                this.modifiedCompositesRoleTypes = null;
                this.rollbackCompositeAssociationByAssociationType = null;
                this.modifiedCompositesAssociationTypes = null;

                this.isDeletedOnRollback = false;
            }
        }

        internal CommittedObject BuildCommittedObject()
        {
            var committed = new CommittedObject(this.ObjectId, this.UncheckedObjectType, this.ObjectVersion);

            if (this.snapshot != null)
            {
                foreach (var kvp in this.snapshot.UnitRoleByRoleType)
                {
                    committed.UnitRoleByRoleType[kvp.Key] = kvp.Value;
                }

                foreach (var kvp in this.snapshot.CompositeRoleByRoleType)
                {
                    committed.CompositeRoleByRoleType[kvp.Key] = kvp.Value;
                }

                foreach (var kvp in this.snapshot.CompositesRoleByRoleType)
                {
                    // FrozenSet is immutable - reference directly
                    committed.CompositesRoleByRoleType[kvp.Key] = kvp.Value;
                }

                foreach (var kvp in this.snapshot.CompositeAssociationByAssociationType)
                {
                    committed.CompositeAssociationByAssociationType[kvp.Key] = kvp.Value;
                }

                foreach (var kvp in this.snapshot.CompositesAssociationByAssociationType)
                {
                    // FrozenSet is immutable - reference directly
                    committed.CompositesAssociationByAssociationType[kvp.Key] = kvp.Value;
                }
            }

            if (this.unitRoleByRoleType != null)
            {
                foreach (var kvp in this.unitRoleByRoleType)
                {
                    committed.SetUnitRole(kvp.Key, kvp.Value);
                }
            }

            if (this.compositeRoleByRoleType != null)
            {
                foreach (var kvp in this.compositeRoleByRoleType)
                {
                    committed.SetCompositeRole(kvp.Key, kvp.Value);
                }
            }

            if (this.compositesRoleByRoleType != null)
            {
                foreach (var kvp in this.compositesRoleByRoleType)
                {
                    committed.SetCompositesRole(kvp.Key, kvp.Value);
                }
            }

            if (this.compositeAssociationByAssociationType != null)
            {
                foreach (var kvp in this.compositeAssociationByAssociationType)
                {
                    committed.SetCompositeAssociation(kvp.Key, kvp.Value);
                }
            }

            if (this.compositesAssociationByAssociationType != null)
            {
                foreach (var kvp in this.compositesAssociationByAssociationType)
                {
                    committed.SetCompositesAssociation(kvp.Key, kvp.Value);
                }
            }

            return committed;
        }

        internal object GetInternalizedUnitRole(IRoleType roleType)
        {
            if (this.unitRoleByRoleType != null && this.unitRoleByRoleType.TryGetValue(roleType, out var role))
            {
                return role;
            }

            if (this.snapshot != null && this.snapshot.UnitRoleByRoleType.TryGetValue(roleType, out role))
            {
                return role;
            }

            return null;
        }

        internal void SetCompositeRoleOne2One(IRoleType roleType, Strategy @new)
        {
            this.AssertNotDeleted();
            this.Transaction.Database.CompositeRoleChecks(this, roleType, @new);

            var previousRoleId = this.GetCompositeRoleId(roleType);
            var newRoleId = @new.ObjectId;

            if (newRoleId == previousRoleId)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            if (previousRoleId != null)
            {
                var previousRole = this.Transaction.InstantiateMemoryStrategy(previousRoleId.Value);
                if (previousRole != null)
                {
                    var previousRoleAssociationId = previousRole.GetCompositeAssociationId(associationType);
                    this.ChangeLog.OnChangingCompositeAssociation(previousRole, associationType, previousRoleAssociationId.HasValue ? previousRole : null);

                    previousRole.BackupCompositeAssociation(associationType);
                    previousRole.SetCompositeAssociationId(associationType, null);
                    this.Transaction.MarkModified(previousRole.ObjectId);
                }
            }

            var newPreviousAssociationId = @new.GetCompositeAssociationId(associationType);
            this.ChangeLog.OnChangingCompositeAssociation(@new, associationType, newPreviousAssociationId.HasValue ? this.Transaction.InstantiateMemoryStrategy(newPreviousAssociationId.Value) : null);

            if (newPreviousAssociationId != null && newPreviousAssociationId != this.ObjectId)
            {
                var newPreviousAssociation = this.Transaction.InstantiateMemoryStrategy(newPreviousAssociationId.Value);
                if (newPreviousAssociation != null)
                {
                    this.ChangeLog.OnChangingCompositeRole(newPreviousAssociation, roleType, null, previousRoleId.HasValue ? @new : null);

                    newPreviousAssociation.BackupCompositeRole(roleType);
                    newPreviousAssociation.SetCompositeRoleId(roleType, null);
                    this.Transaction.MarkModified(newPreviousAssociation.ObjectId);
                }
            }

            this.ChangeLog.OnChangingCompositeRole(this, roleType, @new, previousRoleId.HasValue ? this.Transaction.InstantiateMemoryStrategy(previousRoleId.Value) : null);

            this.BackupCompositeRole(roleType);
            this.SetCompositeRoleId(roleType, newRoleId);
            this.Transaction.MarkModified(this.ObjectId);

            @new.BackupCompositeAssociation(associationType);
            @new.SetCompositeAssociationId(associationType, this.ObjectId);
            this.Transaction.MarkModified(@new.ObjectId);
        }

        internal void SetCompositeRoleMany2One(IRoleType roleType, Strategy @new)
        {
            this.AssertNotDeleted();
            this.Transaction.Database.CompositeRoleChecks(this, roleType, @new);

            var previousRoleId = this.GetCompositeRoleId(roleType);
            var newRoleId = @new.ObjectId;

            if (newRoleId == previousRoleId)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            if (previousRoleId != null)
            {
                var previousRole = this.Transaction.InstantiateMemoryStrategy(previousRoleId.Value);
                if (previousRole != null)
                {
                    var previousRoleAssociations = previousRole.GetCompositesAssociationIds(associationType);
                    this.ChangeLog.OnChangingCompositesAssociation(previousRole, associationType, previousRoleAssociations != null ? previousRoleAssociations.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray() : null);

                    previousRole.BackupCompositesAssociation(associationType);
                    previousRole.RemoveCompositesAssociationId(associationType, this.ObjectId);
                    this.Transaction.MarkModified(previousRole.ObjectId);
                }
            }

            this.ChangeLog.OnChangingCompositeRole(this, roleType, @new, previousRoleId.HasValue ? this.Transaction.InstantiateMemoryStrategy(previousRoleId.Value) : null);

            this.BackupCompositeRole(roleType);
            this.SetCompositeRoleId(roleType, newRoleId);
            this.Transaction.MarkModified(this.ObjectId);

            var newAssociations = @new.GetCompositesAssociationIds(associationType);
            this.ChangeLog.OnChangingCompositesAssociation(@new, associationType, newAssociations != null ? newAssociations.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray() : null);

            @new.BackupCompositesAssociation(associationType);
            @new.AddCompositesAssociationId(associationType, this.ObjectId);
            this.Transaction.MarkModified(@new.ObjectId);
        }

        internal void SetCompositesRolesOne2Many(IRoleType roleType, IEnumerable<Strategy> roles)
        {
            this.AssertNotDeleted();

            var originalRoleIds = this.GetCompositesRoleIds(roleType);

            if (originalRoleIds == null || originalRoleIds.Count == 0)
            {
                foreach (var role in roles)
                {
                    this.AddCompositeRoleOne2Many(roleType, role);
                }
            }
            else
            {
                var toRemove = new HashSet<long>(originalRoleIds);

                foreach (var role in roles)
                {
                    if (toRemove.Contains(role.ObjectId))
                    {
                        toRemove.Remove(role.ObjectId);
                    }
                    else
                    {
                        this.AddCompositeRoleOne2Many(roleType, role);
                    }
                }

                foreach (var roleId in toRemove)
                {
                    var roleStrategy = this.Transaction.InstantiateMemoryStrategy(roleId);
                    if (roleStrategy != null)
                    {
                        this.RemoveCompositeRoleOne2Many(roleType, roleStrategy);
                    }
                }
            }
        }

        internal void SetCompositesRolesMany2Many(IRoleType roleType, IEnumerable<Strategy> roles)
        {
            this.AssertNotDeleted();

            var originalRoleIds = this.GetCompositesRoleIds(roleType);

            if (originalRoleIds == null || originalRoleIds.Count == 0)
            {
                foreach (var role in roles)
                {
                    this.AddCompositeRoleMany2Many(roleType, role);
                }
            }
            else
            {
                var toRemove = new HashSet<long>(originalRoleIds);

                foreach (var role in roles)
                {
                    if (toRemove.Contains(role.ObjectId))
                    {
                        toRemove.Remove(role.ObjectId);
                    }
                    else
                    {
                        this.AddCompositeRoleMany2Many(roleType, role);
                    }
                }

                foreach (var roleId in toRemove)
                {
                    var roleStrategy = this.Transaction.InstantiateMemoryStrategy(roleId);
                    if (roleStrategy != null)
                    {
                        this.RemoveCompositeRoleMany2Many(roleType, roleStrategy);
                    }
                }
            }
        }

        internal void FillRoleForSave(Dictionary<IRoleType, List<Strategy>> strategiesByRoleType)
        {
            if (this.IsDeleted)
            {
                return;
            }

            foreach (var roleType in this.UncheckedObjectType.DatabaseRoleTypes)
            {
                if (roleType.ObjectType is IUnit)
                {
                    if (this.GetInternalizedUnitRole(roleType) != null)
                    {
                        if (!strategiesByRoleType.TryGetValue(roleType, out var strategies))
                        {
                            strategies = new List<Strategy>();
                            strategiesByRoleType.Add(roleType, strategies);
                        }

                        strategies.Add(this);
                    }
                }
                else if (roleType.IsOne)
                {
                    if (this.GetCompositeRoleId(roleType) != null)
                    {
                        if (!strategiesByRoleType.TryGetValue(roleType, out var strategies))
                        {
                            strategies = new List<Strategy>();
                            strategiesByRoleType.Add(roleType, strategies);
                        }

                        strategies.Add(this);
                    }
                }
                else
                {
                    var roleIds = this.GetCompositesRoleIds(roleType);
                    if (roleIds != null && roleIds.Count > 0)
                    {
                        if (!strategiesByRoleType.TryGetValue(roleType, out var strategies))
                        {
                            strategies = new List<Strategy>();
                            strategiesByRoleType.Add(roleType, strategies);
                        }

                        strategies.Add(this);
                    }
                }
            }
        }

        internal void SaveUnit(XmlWriter writer, IRoleType roleType)
        {
            var unitType = (IUnit)roleType.ObjectType;
            var value = Serialization.WriteString(unitType.Tag, this.GetInternalizedUnitRole(roleType));

            writer.WriteStartElement(Serialization.Relation);
            writer.WriteAttributeString(Serialization.Association, this.ObjectId.ToString());
            writer.WriteString(value);
            writer.WriteEndElement();
        }

        internal void SaveComposites(XmlWriter writer, IRoleType roleType)
        {
            writer.WriteStartElement(Serialization.Relation);
            writer.WriteAttributeString(Serialization.Association, this.ObjectId.ToString());

            var roleIds = this.GetCompositesRoleIds(roleType);
            var i = 0;
            foreach (var roleId in roleIds)
            {
                if (i > 0)
                {
                    writer.WriteString(Serialization.ObjectsSplitter);
                }

                writer.WriteString(roleId.ToString());
                ++i;
            }

            writer.WriteEndElement();
        }

        internal void SaveComposite(XmlWriter writer, IRoleType roleType)
        {
            writer.WriteStartElement(Serialization.Relation);
            writer.WriteAttributeString(Serialization.Association, this.ObjectId.ToString());

            var roleId = this.GetCompositeRoleId(roleType);
            writer.WriteString(roleId.ToString());

            writer.WriteEndElement();
        }

        internal bool ShouldTrim(IRoleType roleType, object originalRole)
        {
            var role = this.GetInternalizedUnitRole(roleType);
            return Equals(role, originalRole);
        }

        internal bool ShouldTrim(IRoleType roleType, Strategy originalRole)
        {
            var roleId = this.GetCompositeRoleId(roleType);
            var originalRoleId = originalRole?.ObjectId;
            return roleId == originalRoleId;
        }

        internal bool ShouldTrim(IRoleType roleType, Strategy[] originalRoles)
        {
            var roleIds = this.GetCompositesRoleIds(roleType);

            if (roleIds == null || roleIds.Count == 0)
            {
                return originalRoles == null || originalRoles.Length == 0;
            }

            if (originalRoles == null)
            {
                return roleIds.Count == 0;
            }

            var originalRoleIds = new HashSet<long>(originalRoles.Select(r => r.ObjectId));
            return roleIds.SetEquals(originalRoleIds);
        }

        internal bool ShouldTrim(IAssociationType associationType, Strategy originalAssociation)
        {
            var associationId = this.GetCompositeAssociationId(associationType);
            var originalAssociationId = originalAssociation?.ObjectId;
            return associationId == originalAssociationId;
        }

        internal bool ShouldTrim(IAssociationType associationType, Strategy[] originalAssociations)
        {
            var associationIds = this.GetCompositesAssociationIds(associationType);

            if (associationIds == null || associationIds.Count == 0)
            {
                return originalAssociations == null || originalAssociations.Length == 0;
            }

            if (originalAssociations == null)
            {
                return associationIds.Count == 0;
            }

            var originalAssociationIds = new HashSet<long>(originalAssociations.Select(a => a.ObjectId));
            return associationIds.SetEquals(originalAssociationIds);
        }

        private long? GetCompositeRoleId(IRoleType roleType)
        {
            if (this.compositeRoleByRoleType != null && this.compositeRoleByRoleType.TryGetValue(roleType, out var roleId))
            {
                return roleId;
            }

            if (this.snapshot != null && this.snapshot.CompositeRoleByRoleType.TryGetValue(roleType, out var snapshotRoleId))
            {
                return snapshotRoleId;
            }

            return null;
        }

        private void SetCompositeRoleId(IRoleType roleType, long? roleId)
        {
            this.compositeRoleByRoleType ??= new Dictionary<IRoleType, long?>();
            if (roleId == null)
            {
                this.compositeRoleByRoleType.Remove(roleType);
            }
            else
            {
                this.compositeRoleByRoleType[roleType] = roleId;
            }
        }

        private IReadOnlySet<long> GetCompositesRoleIds(IRoleType roleType)
        {
            if (this.compositesRoleByRoleType != null && this.compositesRoleByRoleType.TryGetValue(roleType, out var roleIds))
            {
                return roleIds;
            }

            if (this.snapshot != null && this.snapshot.CompositesRoleByRoleType.TryGetValue(roleType, out var snapshotRoleIds))
            {
                // FrozenSet is immutable - return directly without copying
                return snapshotRoleIds;
            }

            return null;
        }

        private void EnsureCompositesRoleIds(IRoleType roleType)
        {
            this.compositesRoleByRoleType ??= new Dictionary<IRoleType, HashSet<long>>();
            if (!this.compositesRoleByRoleType.ContainsKey(roleType))
            {
                if (this.snapshot != null && this.snapshot.CompositesRoleByRoleType.TryGetValue(roleType, out var snapshotRoleIds))
                {
                    this.compositesRoleByRoleType[roleType] = new HashSet<long>(snapshotRoleIds);
                }
                else
                {
                    this.compositesRoleByRoleType[roleType] = new HashSet<long>();
                }
            }
        }

        private long? GetCompositeAssociationId(IAssociationType associationType)
        {
            if (this.compositeAssociationByAssociationType != null && this.compositeAssociationByAssociationType.TryGetValue(associationType, out var associationId))
            {
                return associationId;
            }

            if (this.snapshot != null && this.snapshot.CompositeAssociationByAssociationType.TryGetValue(associationType, out var snapshotAssociationId))
            {
                return snapshotAssociationId;
            }

            return null;
        }

        private void SetCompositeAssociationId(IAssociationType associationType, long? associationId)
        {
            this.compositeAssociationByAssociationType ??= new Dictionary<IAssociationType, long?>();
            if (associationId == null)
            {
                this.compositeAssociationByAssociationType.Remove(associationType);
            }
            else
            {
                this.compositeAssociationByAssociationType[associationType] = associationId;
            }
        }

        private IReadOnlySet<long> GetCompositesAssociationIds(IAssociationType associationType)
        {
            if (this.compositesAssociationByAssociationType != null && this.compositesAssociationByAssociationType.TryGetValue(associationType, out var associationIds))
            {
                return associationIds;
            }

            if (this.snapshot != null && this.snapshot.CompositesAssociationByAssociationType.TryGetValue(associationType, out var snapshotAssociationIds))
            {
                // FrozenSet is immutable - return directly without copying
                return snapshotAssociationIds;
            }

            return null;
        }

        private void EnsureCompositesAssociationIds(IAssociationType associationType)
        {
            this.compositesAssociationByAssociationType ??= new Dictionary<IAssociationType, HashSet<long>>();
            if (!this.compositesAssociationByAssociationType.ContainsKey(associationType))
            {
                if (this.snapshot != null && this.snapshot.CompositesAssociationByAssociationType.TryGetValue(associationType, out var snapshotAssociationIds))
                {
                    this.compositesAssociationByAssociationType[associationType] = new HashSet<long>(snapshotAssociationIds);
                }
                else
                {
                    this.compositesAssociationByAssociationType[associationType] = new HashSet<long>();
                }
            }
        }

        private void AddCompositesAssociationId(IAssociationType associationType, long associationId)
        {
            this.EnsureCompositesAssociationIds(associationType);
            this.compositesAssociationByAssociationType[associationType].Add(associationId);
        }

        private void RemoveCompositesAssociationId(IAssociationType associationType, long associationId)
        {
            this.EnsureCompositesAssociationIds(associationType);
            this.compositesAssociationByAssociationType[associationType].Remove(associationId);
            if (this.compositesAssociationByAssociationType[associationType].Count == 0)
            {
                this.compositesAssociationByAssociationType.Remove(associationType);
            }
        }

        private void BackupCompositeRole(IRoleType roleType)
        {
            if (!this.RollbackCompositeRoleByRoleType.ContainsKey(roleType))
            {
                this.RollbackCompositeRoleByRoleType[roleType] = this.GetCompositeRoleId(roleType);
            }
        }

        private void BackupCompositesRole(IRoleType roleType)
        {
            // Just mark as modified - on rollback we restore from immutable snapshot
            // No copy needed! This eliminates O(n) allocation per modification
            this.ModifiedCompositesRoleTypes.Add(roleType);
        }

        private void BackupCompositeAssociation(IAssociationType associationType)
        {
            if (!this.RollbackCompositeAssociationByAssociationType.ContainsKey(associationType))
            {
                this.RollbackCompositeAssociationByAssociationType[associationType] = this.GetCompositeAssociationId(associationType);
            }
        }

        private void BackupCompositesAssociation(IAssociationType associationType)
        {
            // Just mark as modified - on rollback we restore from immutable snapshot
            // No copy needed! This eliminates O(n) allocation per modification
            this.ModifiedCompositesAssociationTypes.Add(associationType);
        }

        private void RemoveCompositeRoleOne2One(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Transaction.Database.CompositeRoleChecks(this, roleType);

            var previousRoleId = this.GetCompositeRoleId(roleType);
            if (previousRoleId == null)
            {
                return;
            }

            var previousRole = this.Transaction.InstantiateMemoryStrategy(previousRoleId.Value);
            if (previousRole == null)
            {
                return;
            }

            var associationType = roleType.AssociationType;

            this.ChangeLog.OnChangingCompositeRole(this, roleType, null, previousRole);

            var previousRoleAssociationId = previousRole.GetCompositeAssociationId(associationType);
            this.ChangeLog.OnChangingCompositeAssociation(previousRole, associationType, previousRoleAssociationId.HasValue ? this : null);

            previousRole.BackupCompositeAssociation(associationType);
            previousRole.SetCompositeAssociationId(associationType, null);
            this.Transaction.MarkModified(previousRole.ObjectId);

            this.BackupCompositeRole(roleType);
            this.SetCompositeRoleId(roleType, null);
            this.Transaction.MarkModified(this.ObjectId);
        }

        private void RemoveCompositeRoleMany2One(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Transaction.Database.CompositeRoleChecks(this, roleType);

            var previousRoleId = this.GetCompositeRoleId(roleType);
            if (previousRoleId == null)
            {
                return;
            }

            var previousRole = this.Transaction.InstantiateMemoryStrategy(previousRoleId.Value);
            if (previousRole == null)
            {
                return;
            }

            this.ChangeLog.OnChangingCompositeRole(this, roleType, null, previousRole);

            var associationType = roleType.AssociationType;

            var previousRoleAssociations = previousRole.GetCompositesAssociationIds(associationType);
            this.ChangeLog.OnChangingCompositesAssociation(previousRole, associationType, previousRoleAssociations?.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray());

            previousRole.BackupCompositesAssociation(associationType);
            previousRole.RemoveCompositesAssociationId(associationType, this.ObjectId);
            this.Transaction.MarkModified(previousRole.ObjectId);

            this.BackupCompositeRole(roleType);
            this.SetCompositeRoleId(roleType, null);
            this.Transaction.MarkModified(this.ObjectId);
        }

        private void AddCompositeRoleMany2Many(IRoleType roleType, Strategy add)
        {
            var previousRoleIds = this.GetCompositesRoleIds(roleType);
            if (previousRoleIds?.Contains(add.ObjectId) == true)
            {
                return;
            }

            this.ChangeLog.OnChangingCompositesRole(this, roleType, add, previousRoleIds?.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray());

            this.BackupCompositesRole(roleType);
            this.EnsureCompositesRoleIds(roleType);
            this.compositesRoleByRoleType[roleType].Add(add.ObjectId);
            this.Transaction.MarkModified(this.ObjectId);

            var associationType = roleType.AssociationType;

            var addAssociationIds = add.GetCompositesAssociationIds(associationType);
            this.ChangeLog.OnChangingCompositesAssociation(add, associationType, addAssociationIds?.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray());

            add.BackupCompositesAssociation(associationType);
            add.AddCompositesAssociationId(associationType, this.ObjectId);
            this.Transaction.MarkModified(add.ObjectId);
        }

        private void AddCompositeRoleOne2Many(IRoleType roleType, Strategy add)
        {
            var previousRoleIds = this.GetCompositesRoleIds(roleType);
            if (previousRoleIds?.Contains(add.ObjectId) == true)
            {
                return;
            }

            this.ChangeLog.OnChangingCompositesRole(this, roleType, add, previousRoleIds?.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray());

            var associationType = roleType.AssociationType;

            var addPreviousAssociationId = add.GetCompositeAssociationId(associationType);

            this.ChangeLog.OnChangingCompositeAssociation(add, associationType, addPreviousAssociationId.HasValue ? this.Transaction.InstantiateMemoryStrategy(addPreviousAssociationId.Value) : null);

            if (addPreviousAssociationId != null && addPreviousAssociationId != this.ObjectId)
            {
                var addPreviousAssociation = this.Transaction.InstantiateMemoryStrategy(addPreviousAssociationId.Value);
                if (addPreviousAssociation != null)
                {
                    var addPreviousAssociationRoleIds = addPreviousAssociation.GetCompositesRoleIds(roleType);
                    this.ChangeLog.OnChangingCompositesRole(addPreviousAssociation, roleType, null, addPreviousAssociationRoleIds?.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray());

                    addPreviousAssociation.BackupCompositesRole(roleType);
                    addPreviousAssociation.EnsureCompositesRoleIds(roleType);
                    addPreviousAssociation.compositesRoleByRoleType[roleType].Remove(add.ObjectId);
                    if (addPreviousAssociation.compositesRoleByRoleType[roleType].Count == 0)
                    {
                        addPreviousAssociation.compositesRoleByRoleType.Remove(roleType);
                    }

                    this.Transaction.MarkModified(addPreviousAssociation.ObjectId);
                }
            }

            this.BackupCompositesRole(roleType);
            this.EnsureCompositesRoleIds(roleType);
            this.compositesRoleByRoleType[roleType].Add(add.ObjectId);
            this.Transaction.MarkModified(this.ObjectId);

            add.BackupCompositeAssociation(associationType);
            add.SetCompositeAssociationId(associationType, this.ObjectId);
            this.Transaction.MarkModified(add.ObjectId);
        }

        private void RemoveCompositeRoleMany2Many(IRoleType roleType, Strategy remove)
        {
            var roleIds = this.GetCompositesRoleIds(roleType);
            if (roleIds?.Contains(remove.ObjectId) != true)
            {
                return;
            }

            this.ChangeLog.OnChangingCompositesRole(this, roleType, remove, roleIds.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray());

            this.BackupCompositesRole(roleType);
            this.EnsureCompositesRoleIds(roleType);
            this.compositesRoleByRoleType[roleType].Remove(remove.ObjectId);
            if (this.compositesRoleByRoleType[roleType].Count == 0)
            {
                this.compositesRoleByRoleType.Remove(roleType);
            }

            this.Transaction.MarkModified(this.ObjectId);

            var associationType = roleType.AssociationType;

            var removeAssociationIds = remove.GetCompositesAssociationIds(associationType);
            this.ChangeLog.OnChangingCompositesAssociation(remove, associationType, removeAssociationIds?.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray());

            remove.BackupCompositesAssociation(associationType);
            remove.RemoveCompositesAssociationId(associationType, this.ObjectId);
            this.Transaction.MarkModified(remove.ObjectId);
        }

        private void RemoveCompositeRoleOne2Many(IRoleType roleType, Strategy roleToRemove)
        {
            var roleIds = this.GetCompositesRoleIds(roleType);
            if (roleIds?.Contains(roleToRemove.ObjectId) != true)
            {
                return;
            }

            this.ChangeLog.OnChangingCompositesRole(this, roleType, roleToRemove, roleIds.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray());

            this.BackupCompositesRole(roleType);
            this.EnsureCompositesRoleIds(roleType);
            this.compositesRoleByRoleType[roleType].Remove(roleToRemove.ObjectId);
            if (this.compositesRoleByRoleType[roleType].Count == 0)
            {
                this.compositesRoleByRoleType.Remove(roleType);
            }

            this.Transaction.MarkModified(this.ObjectId);

            var associationType = roleType.AssociationType;

            var previousAssociationId = roleToRemove.GetCompositeAssociationId(associationType);
            this.ChangeLog.OnChangingCompositeAssociation(roleToRemove, associationType, previousAssociationId.HasValue ? this : null);

            roleToRemove.BackupCompositeAssociation(associationType);
            roleToRemove.SetCompositeAssociationId(associationType, null);
            this.Transaction.MarkModified(roleToRemove.ObjectId);
        }

        private void RemoveCompositeRolesMany2Many(IRoleType roleType)
        {
            var previousRoleIds = this.GetCompositesRoleIds(roleType);
            if (previousRoleIds == null)
            {
                return;
            }

            foreach (var previousRoleId in previousRoleIds.ToList())
            {
                var previousRole = this.Transaction.InstantiateMemoryStrategy(previousRoleId);
                if (previousRole != null)
                {
                    this.ChangeLog.OnChangingCompositesRole(this, roleType, previousRole, previousRoleIds.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray());
                    this.RemoveCompositeRoleMany2Many(roleType, previousRole);
                }
            }
        }

        private void RemoveCompositeRolesOne2Many(IRoleType roleType)
        {
            var previousRoleIds = this.GetCompositesRoleIds(roleType);
            if (previousRoleIds == null)
            {
                return;
            }

            foreach (var previousRoleId in previousRoleIds.ToList())
            {
                var previousRole = this.Transaction.InstantiateMemoryStrategy(previousRoleId);
                if (previousRole != null)
                {
                    this.RemoveCompositeRoleOne2Many(roleType, previousRole);
                }
            }
        }

        private void AssertNotDeleted()
        {
            if (this.IsDeleted)
            {
                throw new Exception($"Object of class {this.UncheckedObjectType.Name} with id {this.ObjectId} has been deleted");
            }
        }

        public class ObjectIdComparer : IComparer<Strategy>
        {
            public int Compare(Strategy x, Strategy y) => x.ObjectId.CompareTo(y.ObjectId);
        }
    }
}

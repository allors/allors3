// <copyright file="Strategy.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using Adapters;
using Meta;

/// <summary>
/// Strategy implementation with slot-based storage for O(1) role and association access.
/// Uses flat arrays indexed by pre-computed slot indices instead of dictionaries.
/// </summary>
public sealed class Strategy : IStrategy
{
    private readonly SlotLayout slotLayout;

    // Committed snapshot (immutable reference)
    private SlotSnapshot snapshot;

    // Local modifications (slot-indexed arrays, lazy allocated on write)
    private object[] unitRoles;
    private long[] compositeRoles;              // 0 = no reference
    private HashSet<long>[] compositesRoles;    // null = unmodified, use snapshot
    private long[] compositeAssociations;       // 0 = no reference
    private HashSet<long>[] compositesAssociations;

    // Bitmask change tracking for rollback (supports up to 64 slots per category)
    private ulong unitRoleModifiedMask;
    private ulong compositeRoleModifiedMask;
    private ulong compositesRoleModifiedMask;
    private ulong compositeAssociationModifiedMask;
    private ulong compositesAssociationModifiedMask;

    // For roles/associations with slot index >= 64, use overflow sets
    private HashSet<int> unitRoleModifiedOverflow;
    private HashSet<int> compositeRoleModifiedOverflow;
    private HashSet<int> compositesRoleModifiedOverflow;
    private HashSet<int> compositeAssociationModifiedOverflow;
    private HashSet<int> compositesAssociationModifiedOverflow;

    // Rollback values for unit and composite roles (need to store original value)
    private object[] rollbackUnitRoles;
    private long[] rollbackCompositeRoles;
    private long[] rollbackCompositeAssociations;

    private bool isDeletedOnRollback;
    private WeakReference allorizedObjectWeakReference;

    /// <summary>
    /// Constructor for new objects (not yet committed).
    /// </summary>
    internal Strategy(Transaction transaction, IClass objectType, long objectId, long version)
    {
        this.Transaction = transaction;
        this.UncheckedObjectType = objectType;
        this.ObjectId = objectId;
        this.ObjectVersion = version;

        this.slotLayout = transaction.Database.SlotLayout;
        var counts = this.slotLayout.GetSlotCounts(objectType);

        this.snapshot = default; // No committed state yet

        this.IsDeleted = false;
        this.isDeletedOnRollback = true;
        this.IsNewInTransaction = true;

        // Allocate arrays for new object
        this.unitRoles = counts.UnitRoleCount > 0 ? new object[counts.UnitRoleCount] : null;
        this.compositeRoles = counts.CompositeRoleCount > 0 ? new long[counts.CompositeRoleCount] : null;
        this.compositesRoles = counts.CompositesRoleCount > 0 ? new HashSet<long>[counts.CompositesRoleCount] : null;
        this.compositeAssociations = counts.CompositeAssociationCount > 0 ? new long[counts.CompositeAssociationCount] : null;
        this.compositesAssociations = counts.CompositesAssociationCount > 0 ? new HashSet<long>[counts.CompositesAssociationCount] : null;
    }

    /// <summary>
    /// Constructor for existing objects (instantiated from committed state).
    /// </summary>
    internal Strategy(Transaction transaction, SlotSnapshot snapshot)
    {
        this.Transaction = transaction;
        this.UncheckedObjectType = snapshot.ObjectType;
        this.ObjectId = snapshot.ObjectId;
        this.ObjectVersion = snapshot.Version;

        this.slotLayout = transaction.Database.SlotLayout;

        this.snapshot = snapshot;

        this.IsDeleted = false;
        this.isDeletedOnRollback = false;
        this.IsNewInTransaction = false;

        // Local arrays start null - allocated on first write
        this.unitRoles = null;
        this.compositeRoles = null;
        this.compositesRoles = null;
        this.compositeAssociations = null;
        this.compositesAssociations = null;
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

    public override string ToString() => this.UncheckedObjectType.Name + " " + this.ObjectId;

    #region Role Dispatch

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

    #endregion

    #region Unit Roles

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public object GetUnitRole(IRoleType roleType)
    {
        this.AssertNotDeleted();
        return this.GetUnitRoleInternal(roleType);
    }

    /// <summary>
    /// Gets the unit role value for internal use (predicates, sorting).
    /// Returns the internalized (normalized) value.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal object GetInternalizedUnitRole(IRoleType roleType) => this.GetUnitRoleInternal(roleType);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private object GetUnitRoleInternal(IRoleType roleType)
    {
        var slot = this.slotLayout.GetUnitSlotIndex(roleType);

        // Check local modification first
        if (this.unitRoles != null)
        {
            var localValue = this.unitRoles[slot];
            if (localValue != null || this.IsSlotModified(slot, ref this.unitRoleModifiedMask, this.unitRoleModifiedOverflow))
            {
                return localValue;
            }
        }

        // Fall back to snapshot
        return this.snapshot.UnitRoles?[slot];
    }

    public void SetUnitRole(IRoleType roleType, object role)
    {
        this.AssertNotDeleted();
        this.Transaction.Database.UnitRoleChecks(this, roleType);

        var slot = this.slotLayout.GetUnitSlotIndex(roleType);
        var previousRole = this.GetUnitRoleInternal(roleType);
        role = roleType.Normalize(role);

        if (Equals(role, previousRole))
        {
            return;
        }

        // Ensure local array exists
        this.EnsureUnitRolesArray();

        // Backup for rollback (only first modification)
        if (!this.IsSlotModified(slot, ref this.unitRoleModifiedMask, this.unitRoleModifiedOverflow))
        {
            this.EnsureRollbackUnitRolesArray();
            this.rollbackUnitRoles[slot] = previousRole;
            this.MarkSlotModified(slot, ref this.unitRoleModifiedMask, ref this.unitRoleModifiedOverflow);
        }

        this.ChangeLog.OnChangingUnitRole(this, roleType, previousRole);

        this.unitRoles[slot] = role;
        this.Transaction.MarkModified(this.ObjectId);
    }

    public void RemoveUnitRole(IRoleType roleType) => this.SetUnitRole(roleType, null);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ExistUnitRole(IRoleType roleType)
    {
        this.AssertNotDeleted();
        return this.GetUnitRoleInternal(roleType) != null;
    }

    #endregion

    #region Composite Roles (One)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IObject GetCompositeRole(IRoleType roleType)
    {
        this.AssertNotDeleted();
        var roleId = this.GetCompositeRoleId(roleType);
        if (roleId == 0)
        {
            return null;
        }

        var strategy = this.Transaction.InstantiateMemoryStrategy(roleId);
        return strategy?.GetObject();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal long GetCompositeRoleId(IRoleType roleType)
    {
        var slot = this.slotLayout.GetCompositeSlotIndex(roleType);

        // Check local modification first
        if (this.compositeRoles != null && this.IsSlotModified(slot, ref this.compositeRoleModifiedMask, this.compositeRoleModifiedOverflow))
        {
            return this.compositeRoles[slot];
        }

        // Fall back to snapshot
        return this.snapshot.CompositeRoles?[slot] ?? 0;
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ExistCompositeRole(IRoleType roleType)
    {
        this.AssertNotDeleted();
        return this.GetCompositeRoleId(roleType) != 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetCompositeRoleId(IRoleType roleType, long roleId)
    {
        var slot = this.slotLayout.GetCompositeSlotIndex(roleType);
        this.EnsureCompositeRolesArray();
        this.compositeRoles[slot] = roleId;
    }

    private void BackupCompositeRole(IRoleType roleType)
    {
        var slot = this.slotLayout.GetCompositeSlotIndex(roleType);
        if (!this.IsSlotModified(slot, ref this.compositeRoleModifiedMask, this.compositeRoleModifiedOverflow))
        {
            this.EnsureRollbackCompositeRolesArray();
            this.rollbackCompositeRoles[slot] = this.GetCompositeRoleId(roleType);
            this.MarkSlotModified(slot, ref this.compositeRoleModifiedMask, ref this.compositeRoleModifiedOverflow);
        }
    }

    #endregion

    #region Composites Roles (Many)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IReadOnlySet<long> GetCompositesRoleIds(IRoleType roleType)
    {
        var slot = this.slotLayout.GetCompositesSlotIndex(roleType);

        // Check local modification first
        if (this.compositesRoles?[slot] != null)
        {
            return this.compositesRoles[slot];
        }

        // Fall back to snapshot (FrozenSet - zero copy)
        return this.snapshot.CompositesRoles?[slot];
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ExistCompositesRole(IRoleType roleType)
    {
        this.AssertNotDeleted();
        var roleIds = this.GetCompositesRoleIds(roleType);
        return roleIds != null && roleIds.Count > 0;
    }

    private void BackupCompositesRole(IRoleType roleType)
    {
        var slot = this.slotLayout.GetCompositesSlotIndex(roleType);
        // Just mark as modified - on rollback we restore from immutable snapshot
        this.MarkSlotModified(slot, ref this.compositesRoleModifiedMask, ref this.compositesRoleModifiedOverflow);
    }

    private void EnsureCompositesRoleIds(IRoleType roleType)
    {
        var slot = this.slotLayout.GetCompositesSlotIndex(roleType);
        this.EnsureCompositesRolesArray();

        if (this.compositesRoles[slot] == null)
        {
            var snapshotRoles = this.snapshot.CompositesRoles?[slot];
            this.compositesRoles[slot] = snapshotRoles != null
                ? new HashSet<long>(snapshotRoles)
                : new HashSet<long>();
        }
    }

    #endregion

    #region Associations

    public object GetAssociation(IAssociationType associationType) =>
        associationType.IsMany
            ? this.GetCompositesAssociation<IObject>(associationType)
            : (object)this.GetCompositeAssociation(associationType);

    public bool ExistAssociation(IAssociationType associationType) =>
        associationType.IsMany
            ? this.ExistCompositesAssociation(associationType)
            : this.ExistCompositeAssociation(associationType);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IObject GetCompositeAssociation(IAssociationType associationType)
    {
        this.AssertNotDeleted();
        var associationId = this.GetCompositeAssociationId(associationType);
        if (associationId == 0)
        {
            return null;
        }

        var strategy = this.Transaction.InstantiateMemoryStrategy(associationId);
        return strategy?.GetObject();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal long GetCompositeAssociationId(IAssociationType associationType)
    {
        var slot = this.slotLayout.GetCompositeAssociationSlotIndex(associationType);

        // Check local modification first
        if (this.compositeAssociations != null && this.IsSlotModified(slot, ref this.compositeAssociationModifiedMask, this.compositeAssociationModifiedOverflow))
        {
            return this.compositeAssociations[slot];
        }

        // Fall back to snapshot
        return this.snapshot.CompositeAssociations?[slot] ?? 0;
    }

    public bool ExistCompositeAssociation(IAssociationType associationType) =>
        this.GetCompositeAssociation(associationType) != null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal IReadOnlySet<long> GetCompositesAssociationIds(IAssociationType associationType)
    {
        var slot = this.slotLayout.GetCompositesAssociationSlotIndex(associationType);

        // Check local modification first
        if (this.compositesAssociations?[slot] != null)
        {
            return this.compositesAssociations[slot];
        }

        // Fall back to snapshot (FrozenSet - zero copy)
        return this.snapshot.CompositesAssociations?[slot];
    }

    public bool ExistCompositesAssociation(IAssociationType associationType)
    {
        this.AssertNotDeleted();
        var associationIds = this.GetCompositesAssociationIds(associationType);
        return associationIds != null && associationIds.Count > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void SetCompositeAssociationId(IAssociationType associationType, long associationId)
    {
        var slot = this.slotLayout.GetCompositeAssociationSlotIndex(associationType);
        this.EnsureCompositeAssociationsArray();
        this.compositeAssociations[slot] = associationId;
    }

    private void BackupCompositeAssociation(IAssociationType associationType)
    {
        var slot = this.slotLayout.GetCompositeAssociationSlotIndex(associationType);
        if (!this.IsSlotModified(slot, ref this.compositeAssociationModifiedMask, this.compositeAssociationModifiedOverflow))
        {
            this.EnsureRollbackCompositeAssociationsArray();
            this.rollbackCompositeAssociations[slot] = this.GetCompositeAssociationId(associationType);
            this.MarkSlotModified(slot, ref this.compositeAssociationModifiedMask, ref this.compositeAssociationModifiedOverflow);
        }
    }

    private void BackupCompositesAssociation(IAssociationType associationType)
    {
        var slot = this.slotLayout.GetCompositesAssociationSlotIndex(associationType);
        // Just mark as modified - on rollback we restore from immutable snapshot
        this.MarkSlotModified(slot, ref this.compositesAssociationModifiedMask, ref this.compositesAssociationModifiedOverflow);
    }

    internal void AddCompositesAssociationId(IAssociationType associationType, long associationId)
    {
        var slot = this.slotLayout.GetCompositesAssociationSlotIndex(associationType);
        this.EnsureCompositesAssociationsArray();

        if (this.compositesAssociations[slot] == null)
        {
            var snapshotAssociations = this.snapshot.CompositesAssociations?[slot];
            this.compositesAssociations[slot] = snapshotAssociations != null
                ? new HashSet<long>(snapshotAssociations)
                : new HashSet<long>();
        }

        this.compositesAssociations[slot].Add(associationId);
    }

    internal void RemoveCompositesAssociationId(IAssociationType associationType, long associationId)
    {
        var slot = this.slotLayout.GetCompositesAssociationSlotIndex(associationType);
        this.EnsureCompositesAssociationsArray();

        if (this.compositesAssociations[slot] == null)
        {
            var snapshotAssociations = this.snapshot.CompositesAssociations?[slot];
            this.compositesAssociations[slot] = snapshotAssociations != null
                ? new HashSet<long>(snapshotAssociations)
                : new HashSet<long>();
        }

        this.compositesAssociations[slot].Remove(associationId);
    }

    #endregion

    #region Delete

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

                    if (associationId != 0)
                    {
                        var associationStrategy = this.Transaction.InstantiateMemoryStrategy(associationId);
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

    #endregion

    #region Object

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

    #endregion

    #region Transaction Lifecycle

    /// <summary>
    /// Increments the object version if any roles were modified.
    /// Must be called BEFORE BuildCommittedSnapshot.
    /// </summary>
    internal void IncrementVersionIfModified()
    {
        if (!this.IsDeleted && !this.Transaction.Database.IsLoading)
        {
            // Check if any roles were modified (version increment)
            if (this.unitRoleModifiedMask != 0 ||
                this.compositeRoleModifiedMask != 0 ||
                this.compositesRoleModifiedMask != 0 ||
                this.unitRoleModifiedOverflow != null ||
                this.compositeRoleModifiedOverflow != null ||
                this.compositesRoleModifiedOverflow != null)
            {
                ++this.ObjectVersion;
            }
        }
    }

    /// <summary>
    /// Applies the committed snapshot and clears local modifications.
    /// Must be called AFTER BuildCommittedSnapshot.
    /// </summary>
    internal void ApplyCommittedSnapshot(SlotSnapshot committedSnapshot)
    {
        // Update the internal snapshot reference
        this.snapshot = committedSnapshot;

        // Clear local modification arrays
        this.unitRoles = null;
        this.compositeRoles = null;
        this.compositesRoles = null;
        this.compositeAssociations = null;
        this.compositesAssociations = null;

        // Clear rollback arrays
        this.rollbackUnitRoles = null;
        this.rollbackCompositeRoles = null;
        this.rollbackCompositeAssociations = null;

        // Clear modification tracking
        this.ClearModificationTracking();

        this.isDeletedOnRollback = this.IsDeleted;
        this.IsNewInTransaction = false;
    }

    /// <summary>
    /// Legacy commit method - just clears modification state without updating snapshot.
    /// Used for strategies that are not new or modified.
    /// </summary>
    internal void Commit()
    {
        this.IncrementVersionIfModified();

        // Clear rollback state
        this.ClearModificationTracking();

        this.isDeletedOnRollback = this.IsDeleted;
        this.IsNewInTransaction = false;
    }

    internal void Rollback()
    {
        // Simply clear all local modifications and let reads fall back to snapshot
        // The snapshot contains the correct committed state

        // Clear local modification arrays
        this.unitRoles = null;
        this.compositeRoles = null;
        this.compositesRoles = null;
        this.compositeAssociations = null;
        this.compositesAssociations = null;

        // Clear rollback arrays
        this.rollbackUnitRoles = null;
        this.rollbackCompositeRoles = null;
        this.rollbackCompositeAssociations = null;

        // Clear modification tracking
        this.ClearModificationTracking();

        this.IsDeleted = this.isDeletedOnRollback;
        this.IsNewInTransaction = false;
    }

    private void ClearModificationTracking()
    {
        this.unitRoleModifiedMask = 0;
        this.compositeRoleModifiedMask = 0;
        this.compositesRoleModifiedMask = 0;
        this.compositeAssociationModifiedMask = 0;
        this.compositesAssociationModifiedMask = 0;

        this.unitRoleModifiedOverflow = null;
        this.compositeRoleModifiedOverflow = null;
        this.compositesRoleModifiedOverflow = null;
        this.compositeAssociationModifiedOverflow = null;
        this.compositesAssociationModifiedOverflow = null;

        this.rollbackUnitRoles = null;
        this.rollbackCompositeRoles = null;
        this.rollbackCompositeAssociations = null;
    }

    internal void Refresh()
    {
        // Get a fresh snapshot from the committed store
        var freshSnapshot = this.Transaction.Database.CommittedStore.GetSnapshot(this.ObjectId);
        if (freshSnapshot.IsValid)
        {
            this.snapshot = freshSnapshot;
            this.ObjectVersion = freshSnapshot.Version;

            // Clear local modifications to use snapshot values
            this.unitRoles = null;
            this.compositeRoles = null;
            this.compositesRoles = null;
            this.compositeAssociations = null;
            this.compositesAssociations = null;

            this.ClearModificationTracking();

            this.isDeletedOnRollback = false;
        }
    }

    internal SlotSnapshot BuildCommittedSnapshot()
    {
        var counts = this.slotLayout.GetSlotCounts(this.UncheckedObjectType);

        // Build unit roles array
        object[] unitRolesArray = null;
        if (counts.UnitRoleCount > 0)
        {
            unitRolesArray = new object[counts.UnitRoleCount];
            for (var i = 0; i < counts.UnitRoleCount; i++)
            {
                if (this.unitRoles != null)
                {
                    var localValue = this.unitRoles[i];
                    if (localValue != null || this.IsSlotModified(i, ref this.unitRoleModifiedMask, this.unitRoleModifiedOverflow))
                    {
                        unitRolesArray[i] = localValue;
                        continue;
                    }
                }

                unitRolesArray[i] = this.snapshot.UnitRoles?[i];
            }
        }

        // Build composite roles array
        long[] compositeRolesArray = null;
        if (counts.CompositeRoleCount > 0)
        {
            compositeRolesArray = new long[counts.CompositeRoleCount];
            for (var i = 0; i < counts.CompositeRoleCount; i++)
            {
                if (this.compositeRoles != null && this.IsSlotModified(i, ref this.compositeRoleModifiedMask, this.compositeRoleModifiedOverflow))
                {
                    compositeRolesArray[i] = this.compositeRoles[i];
                }
                else
                {
                    compositeRolesArray[i] = this.snapshot.CompositeRoles?[i] ?? 0;
                }
            }
        }

        // Build composites roles array
        FrozenSet<long>[] compositesRolesArray = null;
        if (counts.CompositesRoleCount > 0)
        {
            compositesRolesArray = new FrozenSet<long>[counts.CompositesRoleCount];
            for (var i = 0; i < counts.CompositesRoleCount; i++)
            {
                if (this.compositesRoles?[i] != null)
                {
                    compositesRolesArray[i] = this.compositesRoles[i].Count > 0
                        ? this.compositesRoles[i].ToFrozenSet()
                        : null;
                }
                else
                {
                    compositesRolesArray[i] = this.snapshot.CompositesRoles?[i];
                }
            }
        }

        // Build composite associations array
        long[] compositeAssociationsArray = null;
        if (counts.CompositeAssociationCount > 0)
        {
            compositeAssociationsArray = new long[counts.CompositeAssociationCount];
            for (var i = 0; i < counts.CompositeAssociationCount; i++)
            {
                if (this.compositeAssociations != null && this.IsSlotModified(i, ref this.compositeAssociationModifiedMask, this.compositeAssociationModifiedOverflow))
                {
                    compositeAssociationsArray[i] = this.compositeAssociations[i];
                }
                else
                {
                    compositeAssociationsArray[i] = this.snapshot.CompositeAssociations?[i] ?? 0;
                }
            }
        }

        // Build composites associations array
        FrozenSet<long>[] compositesAssociationsArray = null;
        if (counts.CompositesAssociationCount > 0)
        {
            compositesAssociationsArray = new FrozenSet<long>[counts.CompositesAssociationCount];
            for (var i = 0; i < counts.CompositesAssociationCount; i++)
            {
                if (this.compositesAssociations?[i] != null)
                {
                    compositesAssociationsArray[i] = this.compositesAssociations[i].Count > 0
                        ? this.compositesAssociations[i].ToFrozenSet()
                        : null;
                }
                else
                {
                    compositesAssociationsArray[i] = this.snapshot.CompositesAssociations?[i];
                }
            }
        }

        return new SlotSnapshot(
            this.ObjectId,
            this.UncheckedObjectType,
            this.ObjectVersion,
            unitRolesArray,
            compositeRolesArray,
            compositesRolesArray,
            compositeAssociationsArray,
            compositesAssociationsArray);
    }

    #endregion

    #region Relationship Implementations

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

        if (previousRoleId != 0)
        {
            var previousRole = this.Transaction.InstantiateMemoryStrategy(previousRoleId);
            if (previousRole != null)
            {
                var previousRoleAssociationId = previousRole.GetCompositeAssociationId(associationType);
                this.ChangeLog.OnChangingCompositeAssociation(previousRole, associationType, previousRoleAssociationId != 0 ? previousRole : null);

                previousRole.BackupCompositeAssociation(associationType);
                previousRole.SetCompositeAssociationId(associationType, 0);
                this.Transaction.MarkModified(previousRole.ObjectId);
            }
        }

        var newPreviousAssociationId = @new.GetCompositeAssociationId(associationType);
        this.ChangeLog.OnChangingCompositeAssociation(@new, associationType, newPreviousAssociationId != 0 ? this.Transaction.InstantiateMemoryStrategy(newPreviousAssociationId) : null);

        if (newPreviousAssociationId != 0 && newPreviousAssociationId != this.ObjectId)
        {
            var newPreviousAssociation = this.Transaction.InstantiateMemoryStrategy(newPreviousAssociationId);
            if (newPreviousAssociation != null)
            {
                this.ChangeLog.OnChangingCompositeRole(newPreviousAssociation, roleType, null, previousRoleId != 0 ? @new : null);

                newPreviousAssociation.BackupCompositeRole(roleType);
                newPreviousAssociation.SetCompositeRoleId(roleType, 0);
                this.Transaction.MarkModified(newPreviousAssociation.ObjectId);
            }
        }

        this.ChangeLog.OnChangingCompositeRole(this, roleType, @new, previousRoleId != 0 ? this.Transaction.InstantiateMemoryStrategy(previousRoleId) : null);

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

        if (previousRoleId != 0)
        {
            var previousRole = this.Transaction.InstantiateMemoryStrategy(previousRoleId);
            if (previousRole != null)
            {
                var previousRoleAssociations = previousRole.GetCompositesAssociationIds(associationType);
                this.ChangeLog.OnChangingCompositesAssociation(previousRole, associationType, previousRoleAssociations != null ? previousRoleAssociations.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray() : null);

                previousRole.BackupCompositesAssociation(associationType);
                previousRole.RemoveCompositesAssociationId(associationType, this.ObjectId);
                this.Transaction.MarkModified(previousRole.ObjectId);
            }
        }

        this.ChangeLog.OnChangingCompositeRole(this, roleType, @new, previousRoleId != 0 ? this.Transaction.InstantiateMemoryStrategy(previousRoleId) : null);

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

    private void RemoveCompositeRoleOne2One(IRoleType roleType)
    {
        this.AssertNotDeleted();
        this.Transaction.Database.CompositeRoleChecks(this, roleType);

        var previousRoleId = this.GetCompositeRoleId(roleType);
        if (previousRoleId == 0)
        {
            return;
        }

        var previousRole = this.Transaction.InstantiateMemoryStrategy(previousRoleId);
        if (previousRole == null)
        {
            return;
        }

        var associationType = roleType.AssociationType;

        this.ChangeLog.OnChangingCompositeRole(this, roleType, null, previousRole);

        var previousRoleAssociationId = previousRole.GetCompositeAssociationId(associationType);
        this.ChangeLog.OnChangingCompositeAssociation(previousRole, associationType, previousRoleAssociationId != 0 ? this : null);

        previousRole.BackupCompositeAssociation(associationType);
        previousRole.SetCompositeAssociationId(associationType, 0);
        this.Transaction.MarkModified(previousRole.ObjectId);

        this.BackupCompositeRole(roleType);
        this.SetCompositeRoleId(roleType, 0);
        this.Transaction.MarkModified(this.ObjectId);
    }

    private void RemoveCompositeRoleMany2One(IRoleType roleType)
    {
        this.AssertNotDeleted();
        this.Transaction.Database.CompositeRoleChecks(this, roleType);

        var previousRoleId = this.GetCompositeRoleId(roleType);
        if (previousRoleId == 0)
        {
            return;
        }

        var previousRole = this.Transaction.InstantiateMemoryStrategy(previousRoleId);
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
        this.SetCompositeRoleId(roleType, 0);
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
        var slot = this.slotLayout.GetCompositesSlotIndex(roleType);
        this.compositesRoles[slot].Add(add.ObjectId);
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

        this.ChangeLog.OnChangingCompositeAssociation(add, associationType, addPreviousAssociationId != 0 ? this.Transaction.InstantiateMemoryStrategy(addPreviousAssociationId) : null);

        if (addPreviousAssociationId != 0 && addPreviousAssociationId != this.ObjectId)
        {
            var addPreviousAssociation = this.Transaction.InstantiateMemoryStrategy(addPreviousAssociationId);
            if (addPreviousAssociation != null)
            {
                var addPreviousAssociationRoleIds = addPreviousAssociation.GetCompositesRoleIds(roleType);
                this.ChangeLog.OnChangingCompositesRole(addPreviousAssociation, roleType, null, addPreviousAssociationRoleIds?.Select(id => this.Transaction.InstantiateMemoryStrategy(id)).Where(s => s != null).ToArray());

                addPreviousAssociation.BackupCompositesRole(roleType);
                addPreviousAssociation.EnsureCompositesRoleIds(roleType);
                var prevSlot = this.slotLayout.GetCompositesSlotIndex(roleType);
                addPreviousAssociation.compositesRoles[prevSlot].Remove(add.ObjectId);
                this.Transaction.MarkModified(addPreviousAssociation.ObjectId);
            }
        }

        this.BackupCompositesRole(roleType);
        this.EnsureCompositesRoleIds(roleType);
        var slot = this.slotLayout.GetCompositesSlotIndex(roleType);
        this.compositesRoles[slot].Add(add.ObjectId);
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
        var slot = this.slotLayout.GetCompositesSlotIndex(roleType);
        this.compositesRoles[slot].Remove(remove.ObjectId);
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
        var slot = this.slotLayout.GetCompositesSlotIndex(roleType);
        this.compositesRoles[slot].Remove(roleToRemove.ObjectId);
        this.Transaction.MarkModified(this.ObjectId);

        var associationType = roleType.AssociationType;

        var previousAssociationId = roleToRemove.GetCompositeAssociationId(associationType);
        this.ChangeLog.OnChangingCompositeAssociation(roleToRemove, associationType, previousAssociationId != 0 ? this : null);

        roleToRemove.BackupCompositeAssociation(associationType);
        roleToRemove.SetCompositeAssociationId(associationType, 0);
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

    #endregion

    #region Serialization

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
                if (this.GetUnitRoleInternal(roleType) != null)
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
                if (this.GetCompositeRoleId(roleType) != 0)
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
        var value = Serialization.WriteString(unitType.Tag, this.GetUnitRoleInternal(roleType));

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
        var role = this.GetUnitRoleInternal(roleType);
        return Equals(role, originalRole);
    }

    internal bool ShouldTrim(IRoleType roleType, Strategy originalRole)
    {
        var roleId = this.GetCompositeRoleId(roleType);
        var originalRoleId = originalRole?.ObjectId ?? 0;
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
        var originalAssociationId = originalAssociation?.ObjectId ?? 0;
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

    #endregion

    #region Array Management

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureUnitRolesArray()
    {
        if (this.unitRoles == null)
        {
            var counts = this.slotLayout.GetSlotCounts(this.UncheckedObjectType);
            this.unitRoles = new object[counts.UnitRoleCount];

            // Copy from snapshot if exists
            if (this.snapshot.UnitRoles != null)
            {
                Array.Copy(this.snapshot.UnitRoles, this.unitRoles, counts.UnitRoleCount);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureRollbackUnitRolesArray()
    {
        if (this.rollbackUnitRoles == null)
        {
            var counts = this.slotLayout.GetSlotCounts(this.UncheckedObjectType);
            this.rollbackUnitRoles = new object[counts.UnitRoleCount];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCompositeRolesArray()
    {
        if (this.compositeRoles == null)
        {
            var counts = this.slotLayout.GetSlotCounts(this.UncheckedObjectType);
            this.compositeRoles = new long[counts.CompositeRoleCount];

            // Copy from snapshot if exists
            if (this.snapshot.CompositeRoles != null)
            {
                Array.Copy(this.snapshot.CompositeRoles, this.compositeRoles, counts.CompositeRoleCount);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureRollbackCompositeRolesArray()
    {
        if (this.rollbackCompositeRoles == null)
        {
            var counts = this.slotLayout.GetSlotCounts(this.UncheckedObjectType);
            this.rollbackCompositeRoles = new long[counts.CompositeRoleCount];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCompositesRolesArray()
    {
        if (this.compositesRoles == null)
        {
            var counts = this.slotLayout.GetSlotCounts(this.UncheckedObjectType);
            this.compositesRoles = new HashSet<long>[counts.CompositesRoleCount];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCompositeAssociationsArray()
    {
        if (this.compositeAssociations == null)
        {
            var counts = this.slotLayout.GetSlotCounts(this.UncheckedObjectType);
            this.compositeAssociations = new long[counts.CompositeAssociationCount];

            // Copy from snapshot if exists
            if (this.snapshot.CompositeAssociations != null)
            {
                Array.Copy(this.snapshot.CompositeAssociations, this.compositeAssociations, counts.CompositeAssociationCount);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureRollbackCompositeAssociationsArray()
    {
        if (this.rollbackCompositeAssociations == null)
        {
            var counts = this.slotLayout.GetSlotCounts(this.UncheckedObjectType);
            this.rollbackCompositeAssociations = new long[counts.CompositeAssociationCount];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EnsureCompositesAssociationsArray()
    {
        if (this.compositesAssociations == null)
        {
            var counts = this.slotLayout.GetSlotCounts(this.UncheckedObjectType);
            this.compositesAssociations = new HashSet<long>[counts.CompositesAssociationCount];
        }
    }

    #endregion

    #region Bitmask Operations

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsSlotModified(int slot, ref ulong mask, HashSet<int> overflow)
    {
        if (slot < 64)
        {
            return (mask & (1UL << slot)) != 0;
        }

        return overflow?.Contains(slot) == true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MarkSlotModified(int slot, ref ulong mask, ref HashSet<int> overflow)
    {
        if (slot < 64)
        {
            mask |= 1UL << slot;
        }
        else
        {
            overflow ??= new HashSet<int>();
            overflow.Add(slot);
        }
    }

    #endregion

    #region Validation

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AssertNotDeleted()
    {
        if (this.IsDeleted)
        {
            throw new Exception($"Object of class {this.UncheckedObjectType.Name} with id {this.ObjectId} has been deleted");
        }
    }

    #endregion

    public class ObjectIdComparer : IComparer<Strategy>
    {
        public int Compare(Strategy x, Strategy y) => x.ObjectId.CompareTo(y.ObjectId);
    }
}

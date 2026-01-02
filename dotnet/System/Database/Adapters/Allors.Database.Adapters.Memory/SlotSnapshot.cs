// <copyright file="SlotSnapshot.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System.Collections.Frozen;
using Meta;

/// <summary>
/// Immutable snapshot of an object's committed state.
/// Uses slot-indexed arrays for O(1) role and association access.
/// FrozenSet used for many-to-many relationships to enable zero-copy reads.
/// </summary>
internal readonly struct SlotSnapshot
{
    public readonly long ObjectId;
    public readonly IClass ObjectType;
    public readonly long Version;

    // Roles (slot-indexed)
    public readonly object[] UnitRoles;
    public readonly long[] CompositeRoles;              // 0 = no reference
    public readonly FrozenSet<long>[] CompositesRoles;

    // Associations (slot-indexed)
    public readonly long[] CompositeAssociations;       // 0 = no reference
    public readonly FrozenSet<long>[] CompositesAssociations;

    public SlotSnapshot(
        long objectId,
        IClass objectType,
        long version,
        object[] unitRoles,
        long[] compositeRoles,
        FrozenSet<long>[] compositesRoles,
        long[] compositeAssociations,
        FrozenSet<long>[] compositesAssociations)
    {
        this.ObjectId = objectId;
        this.ObjectType = objectType;
        this.Version = version;
        this.UnitRoles = unitRoles;
        this.CompositeRoles = compositeRoles;
        this.CompositesRoles = compositesRoles;
        this.CompositeAssociations = compositeAssociations;
        this.CompositesAssociations = compositesAssociations;
    }

    /// <summary>
    /// Creates an empty snapshot for a new object.
    /// </summary>
    public static SlotSnapshot CreateNew(long objectId, IClass objectType, SlotCounts counts)
    {
        return new SlotSnapshot(
            objectId,
            objectType,
            version: 0,
            unitRoles: counts.UnitRoleCount > 0 ? new object[counts.UnitRoleCount] : null,
            compositeRoles: counts.CompositeRoleCount > 0 ? new long[counts.CompositeRoleCount] : null,
            compositesRoles: counts.CompositesRoleCount > 0 ? new FrozenSet<long>[counts.CompositesRoleCount] : null,
            compositeAssociations: counts.CompositeAssociationCount > 0 ? new long[counts.CompositeAssociationCount] : null,
            compositesAssociations: counts.CompositesAssociationCount > 0 ? new FrozenSet<long>[counts.CompositesAssociationCount] : null);
    }

    /// <summary>
    /// Checks if this snapshot is valid (has an object type).
    /// Default struct will have ObjectType = null.
    /// </summary>
    public bool IsValid => this.ObjectType != null;
}

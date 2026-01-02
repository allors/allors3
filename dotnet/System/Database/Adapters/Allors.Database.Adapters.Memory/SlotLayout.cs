// <copyright file="SlotLayout.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory;

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Meta;

/// <summary>
/// Computes and stores slot indices for role types and association types.
/// Slot indices enable O(1) array-based access instead of dictionary lookups.
/// Internal to the Memory adapter - no changes to meta-model interfaces.
/// </summary>
internal sealed class SlotLayout
{
    // Slot index lookups
    private readonly FrozenDictionary<IRoleType, int> unitSlotIndices;
    private readonly FrozenDictionary<IRoleType, int> compositeSlotIndices;
    private readonly FrozenDictionary<IRoleType, int> compositesSlotIndices;
    private readonly FrozenDictionary<IAssociationType, int> compositeAssociationSlotIndices;
    private readonly FrozenDictionary<IAssociationType, int> compositesAssociationSlotIndices;

    // Per-class slot counts
    private readonly FrozenDictionary<IClass, SlotCounts> slotCountsByClass;

    public SlotLayout(IMetaPopulation metaPopulation)
    {
        var unitSlots = new Dictionary<IRoleType, int>();
        var compositeSlots = new Dictionary<IRoleType, int>();
        var compositesSlots = new Dictionary<IRoleType, int>();
        var compositeAssocSlots = new Dictionary<IAssociationType, int>();
        var compositesAssocSlots = new Dictionary<IAssociationType, int>();

        // First pass: assign globally unique slot indices for all role types and association types
        var nextUnitSlot = 0;
        var nextCompositeSlot = 0;
        var nextCompositesSlot = 0;
        var nextCompositeAssocSlot = 0;
        var nextCompositesAssocSlot = 0;

        foreach (var @class in metaPopulation.DatabaseClasses)
        {
            foreach (var roleType in @class.DatabaseRoleTypes)
            {
                if (roleType.ObjectType is IUnit)
                {
                    if (!unitSlots.ContainsKey(roleType))
                    {
                        unitSlots[roleType] = nextUnitSlot++;
                    }
                }
                else if (roleType.IsOne)
                {
                    if (!compositeSlots.ContainsKey(roleType))
                    {
                        compositeSlots[roleType] = nextCompositeSlot++;
                    }
                }
                else
                {
                    if (!compositesSlots.ContainsKey(roleType))
                    {
                        compositesSlots[roleType] = nextCompositesSlot++;
                    }
                }
            }

            foreach (var associationType in @class.DatabaseAssociationTypes)
            {
                if (associationType.IsOne)
                {
                    if (!compositeAssocSlots.ContainsKey(associationType))
                    {
                        compositeAssocSlots[associationType] = nextCompositeAssocSlot++;
                    }
                }
                else
                {
                    if (!compositesAssocSlots.ContainsKey(associationType))
                    {
                        compositesAssocSlots[associationType] = nextCompositesAssocSlot++;
                    }
                }
            }
        }

        // Second pass: compute max slot index + 1 per class to get array sizes
        var slotCounts = new Dictionary<IClass, SlotCounts>();

        foreach (var @class in metaPopulation.DatabaseClasses)
        {
            var maxUnitSlot = -1;
            var maxCompositeSlot = -1;
            var maxCompositesSlot = -1;
            var maxCompositeAssocSlot = -1;
            var maxCompositesAssocSlot = -1;

            foreach (var roleType in @class.DatabaseRoleTypes)
            {
                if (roleType.ObjectType is IUnit)
                {
                    var slot = unitSlots[roleType];
                    if (slot > maxUnitSlot)
                    {
                        maxUnitSlot = slot;
                    }
                }
                else if (roleType.IsOne)
                {
                    var slot = compositeSlots[roleType];
                    if (slot > maxCompositeSlot)
                    {
                        maxCompositeSlot = slot;
                    }
                }
                else
                {
                    var slot = compositesSlots[roleType];
                    if (slot > maxCompositesSlot)
                    {
                        maxCompositesSlot = slot;
                    }
                }
            }

            foreach (var associationType in @class.DatabaseAssociationTypes)
            {
                if (associationType.IsOne)
                {
                    var slot = compositeAssocSlots[associationType];
                    if (slot > maxCompositeAssocSlot)
                    {
                        maxCompositeAssocSlot = slot;
                    }
                }
                else
                {
                    var slot = compositesAssocSlots[associationType];
                    if (slot > maxCompositesAssocSlot)
                    {
                        maxCompositesAssocSlot = slot;
                    }
                }
            }

            slotCounts[@class] = new SlotCounts(
                maxUnitSlot + 1,
                maxCompositeSlot + 1,
                maxCompositesSlot + 1,
                maxCompositeAssocSlot + 1,
                maxCompositesAssocSlot + 1);
        }

        // Freeze dictionaries for optimal read performance
        this.unitSlotIndices = unitSlots.ToFrozenDictionary();
        this.compositeSlotIndices = compositeSlots.ToFrozenDictionary();
        this.compositesSlotIndices = compositesSlots.ToFrozenDictionary();
        this.compositeAssociationSlotIndices = compositeAssocSlots.ToFrozenDictionary();
        this.compositesAssociationSlotIndices = compositesAssocSlots.ToFrozenDictionary();
        this.slotCountsByClass = slotCounts.ToFrozenDictionary();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetUnitSlotIndex(IRoleType roleType) => this.unitSlotIndices[roleType];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetCompositeSlotIndex(IRoleType roleType) => this.compositeSlotIndices[roleType];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetCompositesSlotIndex(IRoleType roleType) => this.compositesSlotIndices[roleType];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetCompositeAssociationSlotIndex(IAssociationType associationType) =>
        this.compositeAssociationSlotIndices[associationType];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetCompositesAssociationSlotIndex(IAssociationType associationType) =>
        this.compositesAssociationSlotIndices[associationType];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public SlotCounts GetSlotCounts(IClass @class) => this.slotCountsByClass[@class];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetUnitSlotIndex(IRoleType roleType, out int index) =>
        this.unitSlotIndices.TryGetValue(roleType, out index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetCompositeSlotIndex(IRoleType roleType, out int index) =>
        this.compositeSlotIndices.TryGetValue(roleType, out index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetCompositesSlotIndex(IRoleType roleType, out int index) =>
        this.compositesSlotIndices.TryGetValue(roleType, out index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetCompositeAssociationSlotIndex(IAssociationType associationType, out int index) =>
        this.compositeAssociationSlotIndices.TryGetValue(associationType, out index);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetCompositesAssociationSlotIndex(IAssociationType associationType, out int index) =>
        this.compositesAssociationSlotIndices.TryGetValue(associationType, out index);
}

/// <summary>
/// Slot counts for a specific class.
/// </summary>
internal readonly struct SlotCounts
{
    public readonly int UnitRoleCount;
    public readonly int CompositeRoleCount;
    public readonly int CompositesRoleCount;
    public readonly int CompositeAssociationCount;
    public readonly int CompositesAssociationCount;

    public SlotCounts(
        int unitRoleCount,
        int compositeRoleCount,
        int compositesRoleCount,
        int compositeAssociationCount,
        int compositesAssociationCount)
    {
        this.UnitRoleCount = unitRoleCount;
        this.CompositeRoleCount = compositeRoleCount;
        this.CompositesRoleCount = compositesRoleCount;
        this.CompositeAssociationCount = compositeAssociationCount;
        this.CompositesAssociationCount = compositesAssociationCount;
    }
}

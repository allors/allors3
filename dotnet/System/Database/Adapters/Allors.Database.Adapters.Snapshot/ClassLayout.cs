// <copyright file="ClassLayout.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// The slots of the records of one class. Every database role type and association type of the
    /// class has an ordinal within its kind, so that a record keeps its roles and associations in
    /// dense arrays. A layout is computed once per database and never changes.
    /// </summary>
    internal sealed class ClassLayout
    {
        private readonly Dictionary<IRoleType, int> unitRoleOrdinalByRoleType = new Dictionary<IRoleType, int>();
        private readonly Dictionary<IRoleType, int> compositeRoleOrdinalByRoleType = new Dictionary<IRoleType, int>();
        private readonly Dictionary<IRoleType, int> compositesRoleOrdinalByRoleType = new Dictionary<IRoleType, int>();
        private readonly Dictionary<IAssociationType, int> compositeAssociationOrdinalByAssociationType = new Dictionary<IAssociationType, int>();
        private readonly Dictionary<IAssociationType, int> compositesAssociationOrdinalByAssociationType = new Dictionary<IAssociationType, int>();

        internal ClassLayout(IClass @class)
        {
            this.Class = @class;

            var unitRoleTypes = new List<IRoleType>();
            var compositeRoleTypes = new List<IRoleType>();
            var compositesRoleTypes = new List<IRoleType>();

            foreach (var roleType in @class.DatabaseRoleTypes)
            {
                if (roleType.ObjectType.IsUnit)
                {
                    this.unitRoleOrdinalByRoleType[roleType] = unitRoleTypes.Count;
                    unitRoleTypes.Add(roleType);
                }
                else if (roleType.IsOne)
                {
                    this.compositeRoleOrdinalByRoleType[roleType] = compositeRoleTypes.Count;
                    compositeRoleTypes.Add(roleType);
                }
                else
                {
                    this.compositesRoleOrdinalByRoleType[roleType] = compositesRoleTypes.Count;
                    compositesRoleTypes.Add(roleType);
                }
            }

            var compositeAssociationTypes = new List<IAssociationType>();
            var compositesAssociationTypes = new List<IAssociationType>();

            foreach (var associationType in @class.DatabaseAssociationTypes)
            {
                if (associationType.IsOne)
                {
                    this.compositeAssociationOrdinalByAssociationType[associationType] = compositeAssociationTypes.Count;
                    compositeAssociationTypes.Add(associationType);
                }
                else
                {
                    this.compositesAssociationOrdinalByAssociationType[associationType] = compositesAssociationTypes.Count;
                    compositesAssociationTypes.Add(associationType);
                }
            }

            this.UnitRoleTypes = unitRoleTypes.ToArray();
            this.CompositeRoleTypes = compositeRoleTypes.ToArray();
            this.CompositesRoleTypes = compositesRoleTypes.ToArray();
            this.CompositeAssociationTypes = compositeAssociationTypes.ToArray();
            this.CompositesAssociationTypes = compositesAssociationTypes.ToArray();
        }

        internal IClass Class { get; }

        internal IRoleType[] UnitRoleTypes { get; }

        internal IRoleType[] CompositeRoleTypes { get; }

        internal IRoleType[] CompositesRoleTypes { get; }

        internal IAssociationType[] CompositeAssociationTypes { get; }

        internal IAssociationType[] CompositesAssociationTypes { get; }

        internal bool TryGetUnitRoleOrdinal(IRoleType roleType, out int ordinal) => this.unitRoleOrdinalByRoleType.TryGetValue(roleType, out ordinal);

        internal bool TryGetCompositeRoleOrdinal(IRoleType roleType, out int ordinal) => this.compositeRoleOrdinalByRoleType.TryGetValue(roleType, out ordinal);

        internal bool TryGetCompositesRoleOrdinal(IRoleType roleType, out int ordinal) => this.compositesRoleOrdinalByRoleType.TryGetValue(roleType, out ordinal);

        internal bool TryGetCompositeAssociationOrdinal(IAssociationType associationType, out int ordinal) => this.compositeAssociationOrdinalByAssociationType.TryGetValue(associationType, out ordinal);

        internal bool TryGetCompositesAssociationOrdinal(IAssociationType associationType, out int ordinal) => this.compositesAssociationOrdinalByAssociationType.TryGetValue(associationType, out ordinal);

        internal int UnitRoleOrdinal(IRoleType roleType) => this.TryGetUnitRoleOrdinal(roleType, out var ordinal) ? ordinal : throw this.UnknownRoleType(roleType, "unit");

        internal int CompositeRoleOrdinal(IRoleType roleType) => this.TryGetCompositeRoleOrdinal(roleType, out var ordinal) ? ordinal : throw this.UnknownRoleType(roleType, "composite");

        internal int CompositesRoleOrdinal(IRoleType roleType) => this.TryGetCompositesRoleOrdinal(roleType, out var ordinal) ? ordinal : throw this.UnknownRoleType(roleType, "composites");

        internal int CompositeAssociationOrdinal(IAssociationType associationType) => this.TryGetCompositeAssociationOrdinal(associationType, out var ordinal) ? ordinal : throw this.UnknownAssociationType(associationType, "composite");

        internal int CompositesAssociationOrdinal(IAssociationType associationType) => this.TryGetCompositesAssociationOrdinal(associationType, out var ordinal) ? ordinal : throw this.UnknownAssociationType(associationType, "composites");

        private ArgumentException UnknownRoleType(IRoleType roleType, string kind) =>
            new ArgumentException($"{this.Class.Name} has no {kind} database role {roleType}. Check that the role belongs to the class and is stored in the database.");

        private ArgumentException UnknownAssociationType(IAssociationType associationType, string kind) =>
            new ArgumentException($"{this.Class.Name} has no {kind} database association {associationType}. Check that the association belongs to the class and is stored in the database.");
    }
}

// <copyright file="Record.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using Meta;

    /// <summary>
    /// The state of one object: its class, version, roles and associations, with related objects held
    /// by id. A record that belongs to a published snapshot is never changed; a transaction clones it
    /// into its overlay before it writes.
    /// </summary>
    internal sealed class Record
    {
        private object[] unitRoles;
        private long[] compositeRoles;
        private ImmutableSortedSet<long>[] compositesRoles;
        private long[] compositeAssociations;
        private ImmutableSortedSet<long>[] compositesAssociations;

        internal Record(ClassLayout layout, long objectId, long version)
        {
            this.Layout = layout;
            this.ObjectId = objectId;
            this.Version = version;
        }

        private Record(Record source)
        {
            this.Layout = source.Layout;
            this.ObjectId = source.ObjectId;
            this.Version = source.Version;

            this.unitRoles = (object[])source.unitRoles?.Clone();
            this.compositeRoles = (long[])source.compositeRoles?.Clone();
            this.compositesRoles = (ImmutableSortedSet<long>[])source.compositesRoles?.Clone();
            this.compositeAssociations = (long[])source.compositeAssociations?.Clone();
            this.compositesAssociations = (ImmutableSortedSet<long>[])source.compositesAssociations?.Clone();
        }

        internal ClassLayout Layout { get; }

        internal IClass Class => this.Layout.Class;

        internal long ObjectId { get; }

        internal long Version { get; set; }

        internal Record Clone() => new Record(this);

        internal void CopyUnitRolesFrom(Record source) => this.unitRoles = (object[])source.unitRoles?.Clone();

        internal object GetUnitRole(IRoleType roleType) =>
            this.unitRoles != null && this.Layout.TryGetUnitRoleOrdinal(roleType, out var ordinal) ? this.unitRoles[ordinal] : null;

        internal void SetUnitRole(IRoleType roleType, object unit)
        {
            var ordinal = this.Layout.UnitRoleOrdinal(roleType);
            if (unit == null)
            {
                if (this.unitRoles != null)
                {
                    this.unitRoles[ordinal] = null;
                }

                return;
            }

            this.unitRoles ??= new object[this.Layout.UnitRoleTypes.Length];
            this.unitRoles[ordinal] = unit;
        }

        internal long GetCompositeRole(IRoleType roleType) =>
            this.compositeRoles != null && this.Layout.TryGetCompositeRoleOrdinal(roleType, out var ordinal) ? this.compositeRoles[ordinal] : 0;

        internal void SetCompositeRole(IRoleType roleType, long roleId)
        {
            var ordinal = this.Layout.CompositeRoleOrdinal(roleType);
            if (roleId == 0)
            {
                if (this.compositeRoles != null)
                {
                    this.compositeRoles[ordinal] = 0;
                }

                return;
            }

            this.compositeRoles ??= new long[this.Layout.CompositeRoleTypes.Length];
            this.compositeRoles[ordinal] = roleId;
        }

        internal ImmutableSortedSet<long> GetCompositesRole(IRoleType roleType) =>
            (this.compositesRoles != null && this.Layout.TryGetCompositesRoleOrdinal(roleType, out var ordinal) ? this.compositesRoles[ordinal] : null)
            ?? ImmutableSortedSet<long>.Empty;

        internal void SetCompositesRole(IRoleType roleType, ImmutableSortedSet<long> roleIds)
        {
            var ordinal = this.Layout.CompositesRoleOrdinal(roleType);
            if (roleIds == null || roleIds.IsEmpty)
            {
                if (this.compositesRoles != null)
                {
                    this.compositesRoles[ordinal] = null;
                }

                return;
            }

            this.compositesRoles ??= new ImmutableSortedSet<long>[this.Layout.CompositesRoleTypes.Length];
            this.compositesRoles[ordinal] = roleIds;
        }

        internal void AddCompositesRole(IRoleType roleType, long roleId) =>
            this.SetCompositesRole(roleType, this.GetCompositesRole(roleType).Add(roleId));

        internal void RemoveCompositesRole(IRoleType roleType, long roleId) =>
            this.SetCompositesRole(roleType, this.GetCompositesRole(roleType).Remove(roleId));

        internal long GetCompositeAssociation(IAssociationType associationType) =>
            this.compositeAssociations != null && this.Layout.TryGetCompositeAssociationOrdinal(associationType, out var ordinal) ? this.compositeAssociations[ordinal] : 0;

        internal void SetCompositeAssociation(IAssociationType associationType, long associationId)
        {
            var ordinal = this.Layout.CompositeAssociationOrdinal(associationType);
            if (associationId == 0)
            {
                if (this.compositeAssociations != null)
                {
                    this.compositeAssociations[ordinal] = 0;
                }

                return;
            }

            this.compositeAssociations ??= new long[this.Layout.CompositeAssociationTypes.Length];
            this.compositeAssociations[ordinal] = associationId;
        }

        internal ImmutableSortedSet<long> GetCompositesAssociation(IAssociationType associationType) =>
            (this.compositesAssociations != null && this.Layout.TryGetCompositesAssociationOrdinal(associationType, out var ordinal) ? this.compositesAssociations[ordinal] : null)
            ?? ImmutableSortedSet<long>.Empty;

        internal void SetCompositesAssociation(IAssociationType associationType, ImmutableSortedSet<long> associationIds)
        {
            var ordinal = this.Layout.CompositesAssociationOrdinal(associationType);
            if (associationIds == null || associationIds.IsEmpty)
            {
                if (this.compositesAssociations != null)
                {
                    this.compositesAssociations[ordinal] = null;
                }

                return;
            }

            this.compositesAssociations ??= new ImmutableSortedSet<long>[this.Layout.CompositesAssociationTypes.Length];
            this.compositesAssociations[ordinal] = associationIds;
        }

        internal void AddCompositesAssociation(IAssociationType associationType, long associationId) =>
            this.SetCompositesAssociation(associationType, this.GetCompositesAssociation(associationType).Add(associationId));

        internal void RemoveCompositesAssociation(IAssociationType associationType, long associationId) =>
            this.SetCompositesAssociation(associationType, this.GetCompositesAssociation(associationType).Remove(associationId));

        internal IEnumerable<KeyValuePair<IRoleType, object>> UnitRoles()
        {
            if (this.unitRoles == null)
            {
                yield break;
            }

            for (var i = 0; i < this.unitRoles.Length; i++)
            {
                var unit = this.unitRoles[i];
                if (unit != null)
                {
                    yield return new KeyValuePair<IRoleType, object>(this.Layout.UnitRoleTypes[i], unit);
                }
            }
        }

        internal IEnumerable<KeyValuePair<IRoleType, long>> CompositeRoles()
        {
            if (this.compositeRoles == null)
            {
                yield break;
            }

            for (var i = 0; i < this.compositeRoles.Length; i++)
            {
                var roleId = this.compositeRoles[i];
                if (roleId != 0)
                {
                    yield return new KeyValuePair<IRoleType, long>(this.Layout.CompositeRoleTypes[i], roleId);
                }
            }
        }

        internal IEnumerable<KeyValuePair<IRoleType, ImmutableSortedSet<long>>> CompositesRoles()
        {
            if (this.compositesRoles == null)
            {
                yield break;
            }

            for (var i = 0; i < this.compositesRoles.Length; i++)
            {
                var roleIds = this.compositesRoles[i];
                if (roleIds != null && !roleIds.IsEmpty)
                {
                    yield return new KeyValuePair<IRoleType, ImmutableSortedSet<long>>(this.Layout.CompositesRoleTypes[i], roleIds);
                }
            }
        }
    }
}

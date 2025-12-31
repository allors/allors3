// <copyright file="CommittedObject.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Memory
{
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// Represents the committed state of an object.
    /// All collections are stored by ID for isolation between transactions.
    /// </summary>
    internal sealed class CommittedObject
    {
        internal CommittedObject(long objectId, IClass objectType, long version)
        {
            this.ObjectId = objectId;
            this.ObjectType = objectType;
            this.Version = version;
            this.UnitRoleByRoleType = new Dictionary<IRoleType, object>();
            this.CompositeRoleByRoleType = new Dictionary<IRoleType, long>();
            this.CompositesRoleByRoleType = new Dictionary<IRoleType, FrozenSet<long>>();
            this.CompositeAssociationByAssociationType = new Dictionary<IAssociationType, long>();
            this.CompositesAssociationByAssociationType = new Dictionary<IAssociationType, FrozenSet<long>>();
        }

        internal long ObjectId { get; }

        internal IClass ObjectType { get; }

        internal long Version { get; set; }

        internal long OriginalVersion { get; set; }

        internal Dictionary<IRoleType, object> UnitRoleByRoleType { get; }

        internal Dictionary<IRoleType, long> CompositeRoleByRoleType { get; }

        internal Dictionary<IRoleType, FrozenSet<long>> CompositesRoleByRoleType { get; }

        internal Dictionary<IAssociationType, long> CompositeAssociationByAssociationType { get; }

        internal Dictionary<IAssociationType, FrozenSet<long>> CompositesAssociationByAssociationType { get; }

        internal CommittedObject Clone()
        {
            var clone = new CommittedObject(this.ObjectId, this.ObjectType, this.Version)
            {
                OriginalVersion = this.OriginalVersion,
            };

            foreach (var kvp in this.UnitRoleByRoleType)
            {
                clone.UnitRoleByRoleType[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in this.CompositeRoleByRoleType)
            {
                clone.CompositeRoleByRoleType[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in this.CompositesRoleByRoleType)
            {
                // FrozenSet is immutable, no need to clone
                clone.CompositesRoleByRoleType[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in this.CompositeAssociationByAssociationType)
            {
                clone.CompositeAssociationByAssociationType[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in this.CompositesAssociationByAssociationType)
            {
                // FrozenSet is immutable, no need to clone
                clone.CompositesAssociationByAssociationType[kvp.Key] = kvp.Value;
            }

            return clone;
        }

        internal void SetUnitRole(IRoleType roleType, object value)
        {
            if (value == null)
            {
                this.UnitRoleByRoleType.Remove(roleType);
            }
            else
            {
                this.UnitRoleByRoleType[roleType] = value;
            }
        }

        internal void SetCompositeRole(IRoleType roleType, long? value)
        {
            if (value == null)
            {
                this.CompositeRoleByRoleType.Remove(roleType);
            }
            else
            {
                this.CompositeRoleByRoleType[roleType] = value.Value;
            }
        }

        internal void SetCompositesRole(IRoleType roleType, HashSet<long> value)
        {
            if (value == null || value.Count == 0)
            {
                this.CompositesRoleByRoleType.Remove(roleType);
            }
            else
            {
                this.CompositesRoleByRoleType[roleType] = value.ToFrozenSet();
            }
        }

        internal void SetCompositeAssociation(IAssociationType associationType, long? value)
        {
            if (value == null)
            {
                this.CompositeAssociationByAssociationType.Remove(associationType);
            }
            else
            {
                this.CompositeAssociationByAssociationType[associationType] = value.Value;
            }
        }

        internal void SetCompositesAssociation(IAssociationType associationType, HashSet<long> value)
        {
            if (value == null || value.Count == 0)
            {
                this.CompositesAssociationByAssociationType.Remove(associationType);
            }
            else
            {
                this.CompositesAssociationByAssociationType[associationType] = value.ToFrozenSet();
            }
        }
    }
}

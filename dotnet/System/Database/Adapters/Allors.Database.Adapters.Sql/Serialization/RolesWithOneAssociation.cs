// <copyright file="RolesWithOneAssociation.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql
{
    using System;
    using System.Collections.Generic;
    using Meta;

    /// <summary>
    /// Stops a load at a role that occurs a second time in a relation type whose roles have one association.
    /// </summary>
    public sealed class RolesWithOneAssociation
    {
        // One bit for an object id, as long as the highest id leaves that at a word of memory for an object or less.
        private readonly ulong[] bits;

        // The ids themselves, when the ids lie too far apart for the bits and for an id outside the bits.
        private readonly HashSet<long> ids;

        public RolesWithOneAssociation(ICollection<long> objectIds)
        {
            var highest = 0L;
            foreach (var objectId in objectIds)
            {
                if (objectId > highest)
                {
                    highest = objectId;
                }
            }

            var words = (highest >> 6) + 1;
            this.bits = words <= objectIds.Count ? new ulong[words] : null;
            this.ids = new HashSet<long>();
        }

        /// <summary>
        /// The relations of a relation type whose roles have one association, with a check on every role.
        /// </summary>
        /// <param name="relationType">The relation type.</param>
        /// <param name="relations">The relations.</param>
        /// <returns>The relations.</returns>
        /// <exception cref="NotSupportedException">A role occurs a second time.</exception>
        public IEnumerable<CompositeRelation> Check(IRelationType relationType, IEnumerable<CompositeRelation> relations)
        {
            if (this.bits != null)
            {
                Array.Clear(this.bits, 0, this.bits.Length);
            }

            this.ids.Clear();

            foreach (var relation in relations)
            {
                if (!this.Add(relation.Role))
                {
                    throw Serialization.RoleWithSeveralAssociations(relationType, relation.Association, relation.Role);
                }

                yield return relation;
            }
        }

        private bool Add(long objectId)
        {
            var bits = this.bits;
            if (bits != null)
            {
                var word = objectId >> 6;
                if ((ulong)word < (ulong)bits.Length)
                {
                    var bit = 1UL << (int)(objectId & 63);
                    if ((bits[word] & bit) != 0)
                    {
                        return false;
                    }

                    bits[word] |= bit;
                    return true;
                }
            }

            return this.ids.Add(objectId);
        }
    }
}

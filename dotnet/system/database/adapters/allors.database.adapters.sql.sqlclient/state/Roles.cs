// <copyright file="Roles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Sql.SqlClient
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Caching;

    public sealed class Roles
    {
        private ICachedObject cachedObject;

        private Dictionary<IRoleType, object> modifiedRoleByRoleType;
        private Dictionary<IRoleType, CompositesRole> modifiedCompositesRoleByRoleType;

        private HashSet<IRoleType> requireFlushRoles;

        internal Roles(Reference reference)
        {
            this.Reference = reference;
            this.Transaction = reference.Transaction;
            this.State = this.Transaction.State;
        }

        internal Reference Reference { get; }

        internal Transaction Transaction { get; }

        public State State { get; }

        internal ICachedObject CachedObject
        {
            get
            {
                if (this.cachedObject != null || this.Reference.IsNew)
                {
                    return this.cachedObject;
                }

                var cache = this.Transaction.Database.Cache;
                this.cachedObject = cache.GetOrCreateCachedObject(this.Reference.Class, this.Reference.ObjectId, this.Reference.Version);
                return this.cachedObject;
            }
        }

        internal Dictionary<IRoleType, object> ModifiedRoleByRoleType =>
            this.modifiedRoleByRoleType ??= new Dictionary<IRoleType, object>();

        private HashSet<IRoleType> RequireFlushRoles => this.requireFlushRoles ??= new HashSet<IRoleType>();

        private Dictionary<IRoleType, CompositesRole> ModifiedRolesByRoleType =>
            this.modifiedCompositesRoleByRoleType ??= new Dictionary<IRoleType, CompositesRole>();

        internal bool TryGetUnitRole(IRoleType roleType, out object role)
        {
            role = null;
            if (this.TryGetModifiedRole(roleType, ref role))
            {
                return true;
            }

            if (this.CachedObject != null && this.CachedObject.TryGetValue(roleType, out role))
            {
                return true;
            }

            return this.Reference.IsNew;
        }

        private bool TryGetModifiedRole(IRoleType roleType, ref object role) => this.modifiedRoleByRoleType != null && this.modifiedRoleByRoleType.TryGetValue(roleType, out role);

        internal object GetUnitRole(IRoleType roleType)
        {
            object role = null;
            if (this.TryGetModifiedRole(roleType, ref role))
            {
                return role;
            }

            if (this.CachedObject != null && this.CachedObject.TryGetValue(roleType, out role))
            {
                return role;
            }

            if (this.Reference.IsNew)
            {
                return role;
            }

            this.Transaction.Commands.GetUnitRoles(this);
            this.cachedObject.TryGetValue(roleType, out role);
            return role;
        }

        internal void SetUnitRole(IRoleType roleType, object role)
        {
            var previousRole = this.GetUnitRole(roleType);
            if (!Equals(previousRole, role))
            {
                this.State.ChangeLog.OnChangingUnitRole(this.Reference.Strategy, roleType, role, previousRole);

                this.ModifiedRoleByRoleType[roleType] = role;
                this.RequireFlush(roleType);
            }
        }

        internal bool TryGetCompositeRole(IRoleType roleType, out long? roleId)
        {
            roleId = null;

            object role = null;

            if (this.TryGetModifiedRole(roleType, ref role))
            {
                roleId = (long?)role;
                return true;
            }

            if (this.CachedObject == null || !this.CachedObject.TryGetValue(roleType, out role))
            {
                if (!this.Reference.IsNew)
                {
                    return false;
                }
            }

            roleId = (long?)role;
            return true;
        }

        internal long? GetCompositeRole(IRoleType roleType)
        {
            object role = null;
            if (this.TryGetModifiedRole(roleType, ref role))
            {
                return (long?)role;
            }

            if (this.CachedObject != null && this.CachedObject.TryGetValue(roleType, out role))
            {
                return (long?)role;
            }

            if (this.Reference.IsNew)
            {
                return (long?)role;
            }

            this.Transaction.Commands.GetCompositeRole(this, roleType);
            this.cachedObject.TryGetValue(roleType, out role);
            return (long?)role;
        }

        internal void SetCompositeRoleOne2One(IRoleType roleType, Strategy role)
        {
            /*  [if exist]        [then remove]        set
             *
             *  RA ----- R         RA --x-- R       RA    -- R       RA    -- R
             *                ->                +        -        =       -
             *   A ----- PR         A --x-- PR       A --    PR       A --    PR
             */
            var previousRoleId = this.GetCompositeRole(roleType);
            var roleId = role.Reference.ObjectId;

            // R = PR
            if (Equals(roleId, previousRoleId))
            {
                return;
            }

            // A --x-- PR
            if (previousRoleId != null)
            {
                var previousRole = this.State.GetOrCreateReferenceForExistingObject(previousRoleId.Value, this.Transaction).Strategy;
                this.RemoveCompositeRoleOne2One(roleType, previousRole);
            }

            var roleAssociation = (Strategy)role.GetCompositeAssociation(roleType.AssociationType)?.Strategy;

            // RA --x-- R
            roleAssociation?.Roles.RemoveCompositeRoleOne2One(roleType, role);

            // A <---- R
            var associationByRole = this.State.GetAssociationByRole(roleType.AssociationType);
            associationByRole[role.Reference] = this.Reference;

            // A ----> R
            this.ModifiedRoleByRoleType[roleType] = roleId;
            this.RequireFlush(roleType);
        }

        internal void SetCompositeRoleMany2One(IRoleType roleType, Strategy role)
        {
            /*  [if exist]        [then remove]        set
             *
             *  RA ----- R         RA       R       RA    -- R       RA ----- R
             *                ->                +        -        =       -
             *   A ----- PR         A --x-- PR       A --    PR       A --    PR
             */
            var previousRoleId = this.GetCompositeRole(roleType);
            var roleId = role.Reference.ObjectId;

            // R = PR
            if (Equals(roleId, previousRoleId))
            {
                return;
            }

            // A --x-- PR
            if (previousRoleId != null)
            {
                var previousRole = this.State.GetOrCreateReferenceForExistingObject(previousRoleId.Value, this.Transaction).Strategy;
                this.RemoveCompositeRoleMany2One(roleType, previousRole);
            }

            // A <---- R
            var associationsByRole = this.State.GetAssociationsByRole(roleType.AssociationType);
            if (associationsByRole.TryGetValue(role.Reference, out var associations))
            {
                associationsByRole[role.Reference] = associations.Add(this.Reference.ObjectId);
            }

            this.Transaction.TriggerFlush(roleId, roleType.AssociationType);

            // A ----> R
            this.ModifiedRoleByRoleType[roleType] = roleId;
            this.RequireFlush(roleType);
        }

        internal void RemoveCompositeRoleOne2One(IRoleType roleType)
        {
            var roleId = this.GetCompositeRole(roleType);
            if (roleId == null)
            {
                return;
            }

            var role = this.State.GetOrCreateReferenceForExistingObject(roleId.Value, this.Transaction).Strategy;
            this.RemoveCompositeRoleOne2One(roleType, role);
        }

        internal void RemoveCompositeRoleMany2One(IRoleType roleType)
        {
            var roleId = this.GetCompositeRole(roleType);
            if (roleId == null)
            {
                return;
            }

            var role = this.State.GetOrCreateReferenceForExistingObject(roleId.Value, this.Transaction).Strategy;
            this.RemoveCompositeRoleMany2One(roleType, role);
        }

        internal bool TryGetCompositesRole(IRoleType roleType, out IEnumerable<long> roleIds)
        {
            roleIds = null;

            if (this.ModifiedRolesByRoleType != null && this.ModifiedRolesByRoleType.TryGetValue(roleType, out var compositesRole))
            {
                roleIds = compositesRole.ObjectIds;
            }

            return false;
        }

        internal IEnumerable<long> GetCompositesRole(IRoleType roleType)
        {
            if (this.ModifiedRolesByRoleType != null && this.ModifiedRolesByRoleType.TryGetValue(roleType, out var compositesRole))
            {
                return compositesRole.ObjectIds;
            }

            return this.GetNonModifiedCompositeRoles(roleType);
        }

        internal void AddCompositeRoleOne2Many(IRoleType roleType, Strategy role)
        {
            /*  [if exist]        [then remove]        set
            *
            *  RA ----- R         RA       R       RA    -- R       RA ----- R
            *                ->                +        -        =       -
            *   A ----- PR         A --x-- PR       A --    PR       A --    PR
            */
            var previousRoleIds = this.GetCompositesRole(roleType);

            // R in PR 
            if (previousRoleIds.Contains(role.ObjectId))
            {
                return;
            }

            // A --x-- PR
            var previousAssociation = (Strategy)role.GetCompositeAssociation(roleType.AssociationType)?.Strategy;
            previousAssociation?.RemoveCompositeRole(roleType, role.GetObject());

            // A <---- R
            var associationByRole = this.State.GetAssociationByRole(roleType.AssociationType);
            associationByRole[role.Reference] = this.Reference;

            // A ----> R
            var compositesRole = this.GetOrCreateModifiedCompositeRoles(roleType);
            compositesRole.Add(role.ObjectId);
            this.RequireFlush(roleType);
        }

        internal void AddCompositeRoleMany2Many(IRoleType roleType, Strategy role)
        {
            /*  [if exist]        [no remove]         set
            *
            *  RA ----- R         RA       R       RA    -- R       RA ----- R
            *                ->                +        -        =       -
            *   A ----- PR         A       PR       A --    PR       A --    PR
            */
            var previousRoleIds = this.GetCompositesRole(roleType);

            // R in PR 
            if (previousRoleIds.Contains(role.ObjectId))
            {
                return;
            }

            // A <---- R
            var associationsByRole = this.State.GetAssociationsByRole(roleType.AssociationType);
            if (associationsByRole.TryGetValue(role.Reference, out var associations))
            {
                associationsByRole[role.Reference] = associations.Add(this.Reference.ObjectId);
            }

            this.Transaction.TriggerFlush(role.ObjectId, roleType.AssociationType);

            // A ----> R
            var compositesRole = this.GetOrCreateModifiedCompositeRoles(roleType);
            compositesRole.Add(role.ObjectId);
            this.RequireFlush(roleType);
        }

        internal void RemoveCompositeRoleOne2Many(IRoleType roleType, Strategy role)
        {
            var previousRoleIds = this.GetCompositesRole(roleType);

            // R not in PR 
            if (!previousRoleIds.Contains(role.ObjectId))
            {
                return;
            }

            // A <---- R
            var associationByRole = this.State.GetAssociationByRole(roleType.AssociationType);
            associationByRole[role.Reference] = null;

            // A ----> R
            var compositesRole = this.GetOrCreateModifiedCompositeRoles(roleType);
            compositesRole.Remove(role.ObjectId);
            this.RequireFlush(roleType);
        }

        internal void RemoveCompositeRoleMany2Many(IRoleType roleType, Strategy role)
        {
            var previousRoleIds = this.GetCompositesRole(roleType);

            // R not in PR 
            if (!previousRoleIds.Contains(role.ObjectId))
            {
                return;
            }

            // A <---- R
            this.Transaction.RemoveAssociation(this.Reference, role.Reference, roleType.AssociationType);
            this.Transaction.TriggerFlush(role.ObjectId, roleType.AssociationType);

            // A ----> R
            var compositesRole = this.GetOrCreateModifiedCompositeRoles(roleType);
            compositesRole.Remove(role.ObjectId);
            this.RequireFlush(roleType);
        }

        internal void Flush(Flush flush)
        {
            IRoleType unitRole = null;
            List<IRoleType> unitRoles = null;
            foreach (var flushRole in this.RequireFlushRoles)
            {
                if (flushRole.ObjectType.IsUnit)
                {
                    if (unitRole == null)
                    {
                        unitRole = flushRole;
                    }
                    else
                    {
                        if (unitRoles == null)
                        {
                            unitRoles = new List<IRoleType> { unitRole };
                        }

                        unitRoles.Add(flushRole);
                    }
                }
                else
                {
                    if (flushRole.IsOne)
                    {
                        var role = this.GetCompositeRole(flushRole);
                        if (role != null)
                        {
                            flush.SetCompositeRole(this.Reference, flushRole, role.Value);
                        }
                        else
                        {
                            flush.ClearCompositeAndCompositesRole(this.Reference, flushRole);
                        }
                    }
                    else
                    {
                        var roles = this.ModifiedRolesByRoleType[flushRole];
                        roles.Flush(flush, this, flushRole);
                    }
                }
            }

            if (unitRoles != null)
            {
                unitRoles.Sort();
                this.Transaction.Commands.SetUnitRoles(this, unitRoles);
            }
            else if (unitRole != null)
            {
                flush.SetUnitRole(this.Reference, unitRole, this.GetUnitRole(unitRole));
            }

            this.requireFlushRoles = null;
        }

        internal int ExtentCount(IRoleType roleType)
        {
            if (this.ModifiedRolesByRoleType != null && this.ModifiedRolesByRoleType.TryGetValue(roleType, out var compositesRole))
            {
                return compositesRole.Count;
            }

            return this.GetNonModifiedCompositeRoles(roleType).Length;
        }

        internal IObject ExtentFirst(Transaction transaction, IRoleType roleType)
        {
            if (this.ModifiedRolesByRoleType != null && this.ModifiedRolesByRoleType.TryGetValue(roleType, out var compositesRole))
            {
                var objectId = compositesRole.First;
                return objectId == null ? null : transaction.State.GetOrCreateReferenceForExistingObject(objectId.Value, transaction).Strategy.GetObject();
            }

            var nonModifiedCompositeRoles = this.GetNonModifiedCompositeRoles(roleType);
            if (nonModifiedCompositeRoles.Length > 0)
            {
                return transaction.State.GetOrCreateReferenceForExistingObject(nonModifiedCompositeRoles[0], transaction).Strategy.GetObject();
            }

            return null;
        }

        internal void ExtentCopyTo(Transaction transaction, IRoleType roleType, Array array, int index)
        {
            if (this.ModifiedRolesByRoleType != null && this.ModifiedRolesByRoleType.TryGetValue(roleType, out var compositesRole))
            {
                var i = 0;
                foreach (var objectId in compositesRole.ObjectIds)
                {
                    array.SetValue(transaction.State.GetOrCreateReferenceForExistingObject(objectId, transaction).Strategy.GetObject(), index + i);
                    ++i;
                }

                return;
            }

            var nonModifiedCompositeRoles = this.GetNonModifiedCompositeRoles(roleType);
            for (var i = 0; i < nonModifiedCompositeRoles.Length; i++)
            {
                var objectId = nonModifiedCompositeRoles[i];
                array.SetValue(transaction.State.GetOrCreateReferenceForExistingObject(objectId, transaction).Strategy.GetObject(), index + i);
            }
        }

        internal bool ExtentContains(IRoleType roleType, long objectId)
        {
            if (this.ModifiedRolesByRoleType != null && this.ModifiedRolesByRoleType.TryGetValue(roleType, out var compositesRole))
            {
                return compositesRole.Contains(objectId);
            }

            return Array.IndexOf(this.GetNonModifiedCompositeRoles(roleType), objectId) >= 0;
        }

        private long[] GetNonModifiedCompositeRoles(IRoleType roleType)
        {
            if (!this.Reference.IsNew)
            {
                if (this.CachedObject != null && this.CachedObject.TryGetValue(roleType, out var roleOut))
                {
                    return (long[])roleOut;
                }

                this.Transaction.Commands.GetCompositesRole(this, roleType);
                this.cachedObject.TryGetValue(roleType, out roleOut);
                var role = (long[])roleOut;
                return role;
            }

            return Array.Empty<long>();
        }

        private void RemoveCompositeRoleOne2One(IRoleType roleType, Strategy role)
        {
            var associationByRole = this.State.GetAssociationByRole(roleType.AssociationType);
            associationByRole[role.Reference] = null;

            this.ModifiedRoleByRoleType[roleType] = null;
            this.RequireFlush(roleType);
        }

        private void RemoveCompositeRoleMany2One(IRoleType roleType, Strategy role)
        {
            this.Transaction.RemoveAssociation(this.Reference, role.Reference, roleType.AssociationType);

            this.Transaction.TriggerFlush(role.ObjectId, roleType.AssociationType);

            this.ModifiedRoleByRoleType[roleType] = null;
            this.RequireFlush(roleType);
        }

        private CompositesRole GetOrCreateModifiedCompositeRoles(IRoleType roleType)
        {
            if (this.ModifiedRolesByRoleType == null || !this.ModifiedRolesByRoleType.TryGetValue(roleType, out var compositesRole))
            {
                compositesRole = new CompositesRole(this.GetCompositesRole(roleType));
                this.ModifiedRolesByRoleType[roleType] = compositesRole;
            }

            return compositesRole;
        }

        private void RequireFlush(IRoleType roleType)
        {
            this.RequireFlushRoles.Add(roleType);
            this.Transaction.RequireFlush(this.Reference, this);
        }
    }
}

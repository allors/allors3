// <copyright file="Strategy.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Adapters.Snapshot
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Adapters;
    using Meta;

    /// <summary>
    /// The handle of one object in one transaction. A strategy holds no state of its own: it reads the
    /// record that its transaction resolves for its object id and writes to the overlay of that transaction.
    /// </summary>
    public sealed class Strategy : IStrategy
    {
        private readonly long objectId;
        private IClass @class;
        private IObject allorsObject;
        private bool invalidated;

        internal Strategy(Transaction transaction, long objectId)
        {
            this.Transaction = transaction;
            this.objectId = objectId;
        }

        public bool IsDeleted => this.Record == null;

        public bool IsNewInTransaction => this.Transaction.IsCreated(this.objectId);

        public long ObjectId => this.objectId;

        public long ObjectVersion => this.Record?.Version ?? 0;

        public IClass Class
        {
            get
            {
                this.AssertNotDeleted();
                return this.UncheckedObjectType;
            }
        }

        ITransaction IStrategy.Transaction => this.Transaction;

        internal IClass UncheckedObjectType => this.@class ??= this.Record?.Class;

        internal Transaction Transaction { get; }

        private ChangeLog ChangeLog => this.Transaction.ChangeLog;

        private Record Record => this.invalidated ? null : this.Transaction.ResolveRecord(this.objectId);

        public override string ToString() => (this.UncheckedObjectType?.Name ?? "?") + " " + this.objectId;

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
            var value = this.GetInternalizedUnitRole(roleType);
            return value is byte[] binary ? (byte[])binary.Clone() : value;
        }

        public void SetUnitRole(IRoleType roleType, object role)
        {
            this.AssertNotDeleted();
            this.Transaction.Database.UnitRoleChecks(this, roleType);

            var previousRole = this.GetInternalizedUnitRole(roleType);
            role = roleType.RoundToScale(roleType.Normalize(role));

            if (UnitEquals(role, previousRole))
            {
                return;
            }

            this.ChangeLog.OnChangingUnitRole(this, roleType, previousRole);

            this.WritableRecordForRole().SetUnitRole(roleType, role is byte[] binary ? (byte[])binary.Clone() : role);
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
            return this.CompositeRoleStrategy(roleType)?.GetObject();
        }

        public void SetCompositeRole(IRoleType roleType, IObject newRole)
        {
            if (newRole == null)
            {
                this.RemoveCompositeRole(roleType);
            }
            else if (roleType.AssociationType.IsOne)
            {
                // 1-1
                this.SetCompositeRoleOne2One(roleType, (Strategy)newRole.Strategy);
            }
            else
            {
                // *-1
                this.SetCompositeRoleMany2One(roleType, (Strategy)newRole.Strategy);
            }
        }

        public void RemoveCompositeRole(IRoleType roleType)
        {
            if (roleType.AssociationType.IsOne)
            {
                // 1-1
                this.RemoveCompositeRoleOne2One(roleType);
            }
            else
            {
                // *-1
                this.RemoveCompositeRoleMany2One(roleType);
            }
        }

        public bool ExistCompositeRole(IRoleType roleType)
        {
            this.AssertNotDeleted();
            return this.CompositeRoleStrategy(roleType) != null;
        }

        public IEnumerable<T> GetCompositesRole<T>(IRoleType roleType) where T : IObject
        {
            this.AssertNotDeleted();

            var roleIds = this.Record?.GetCompositesRole(roleType);
            if (roleIds == null || roleIds.IsEmpty)
            {
                yield break;
            }

            foreach (var roleId in roleIds)
            {
                var strategy = this.Transaction.GetStrategy(roleId);
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
            return !this.Record.GetCompositesRole(roleType).IsEmpty;
        }

        public object GetAssociation(IAssociationType associationType) => associationType.IsMany ? this.GetCompositesAssociation<IObject>(associationType) : (object)this.GetCompositeAssociation(associationType);

        public bool ExistAssociation(IAssociationType associationType) => associationType.IsMany ? this.ExistCompositesAssociation(associationType) : this.ExistCompositeAssociation(associationType);

        public IObject GetCompositeAssociation(IAssociationType associationType)
        {
            this.AssertNotDeleted();
            return this.CompositeAssociationStrategy(associationType)?.GetObject();
        }

        public bool ExistCompositeAssociation(IAssociationType associationType) => this.GetCompositeAssociation(associationType) != null;

        public IEnumerable<T> GetCompositesAssociation<T>(IAssociationType associationType) where T : IObject
        {
            this.AssertNotDeleted();

            var associationIds = this.Record?.GetCompositesAssociation(associationType);
            if (associationIds == null || associationIds.IsEmpty)
            {
                yield break;
            }

            foreach (var associationId in associationIds)
            {
                var strategy = this.Transaction.GetStrategy(associationId);
                if (strategy != null)
                {
                    yield return (T)strategy.GetObject();
                }
            }
        }

        public bool ExistCompositesAssociation(IAssociationType associationType)
        {
            this.AssertNotDeleted();
            return !this.Record.GetCompositesAssociation(associationType).IsEmpty;
        }

        public void Delete()
        {
            this.AssertNotDeleted();

            var objectType = this.UncheckedObjectType;

            // Roles
            foreach (var roleType in objectType.DatabaseRoleTypes)
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

            // Associations
            foreach (var associationType in objectType.DatabaseAssociationTypes)
            {
                var roleType = associationType.RoleType;

                if (associationType.IsMany)
                {
                    var associationStrategies = this.CompositesAssociationStrategies(associationType);
                    if (associationStrategies != null)
                    {
                        foreach (var associationStrategy in associationStrategies)
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
                else
                {
                    var associationStrategy = this.CompositeAssociationStrategy(associationType);
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

            this.Transaction.MarkDeleted(this.objectId);

            this.ChangeLog.OnDeleted(this);
        }

        public IObject GetObject() => this.allorsObject ??= this.Transaction.Database.ObjectFactory.Create(this);

        internal object GetInternalizedUnitRole(IRoleType roleType) => this.Record?.GetUnitRole(roleType);

        internal void Invalidate() => this.invalidated = true;

        internal void SetCompositeRoleOne2One(IRoleType roleType, Strategy @new)
        {
            this.AssertNotDeleted();
            this.Transaction.Database.CompositeRoleChecks(this, roleType, @new);

            var previousRole = this.CompositeRoleStrategy(roleType);
            if (@new.Equals(previousRole))
            {
                return;
            }

            this.ChangeLog.OnChangingCompositeRole(this, roleType, @new, previousRole);

            var associationType = roleType.AssociationType;

            if (previousRole != null)
            {
                this.ChangeLog.OnChangingCompositeAssociation(previousRole, associationType, previousRole.CompositeAssociationStrategy(associationType));

                // previous role
                previousRole.WritableRecordForAssociation().SetCompositeAssociation(associationType, 0);
            }

            // previous association of newRole
            var newPreviousAssociation = @new.CompositeAssociationStrategy(associationType);

            this.ChangeLog.OnChangingCompositeAssociation(@new, associationType, newPreviousAssociation);

            if (newPreviousAssociation != null && !this.Equals(newPreviousAssociation))
            {
                this.ChangeLog.OnChangingCompositeRole(newPreviousAssociation, roleType, null, @new);

                newPreviousAssociation.WritableRecordForRole().SetCompositeRole(roleType, 0);
            }

            // Set new role
            this.WritableRecordForRole().SetCompositeRole(roleType, @new.objectId);

            // Set new role's association
            @new.WritableRecordForAssociation().SetCompositeAssociation(associationType, this.objectId);
        }

        internal void SetCompositeRoleMany2One(IRoleType roleType, Strategy @new)
        {
            this.AssertNotDeleted();
            this.Transaction.Database.CompositeRoleChecks(this, roleType, @new);

            var previousRole = this.CompositeRoleStrategy(roleType);
            if (@new.Equals(previousRole))
            {
                return;
            }

            this.ChangeLog.OnChangingCompositeRole(this, roleType, @new, previousRole);

            var associationType = roleType.AssociationType;

            // Update association of previous role
            if (previousRole != null)
            {
                this.ChangeLog.OnChangingCompositesAssociation(previousRole, associationType, previousRole.OriginalCompositesAssociation(associationType));

                previousRole.WritableRecordForAssociation().RemoveCompositesAssociation(associationType, this.objectId);
            }

            this.WritableRecordForRole().SetCompositeRole(roleType, @new.objectId);

            this.ChangeLog.OnChangingCompositesAssociation(@new, associationType, @new.OriginalCompositesAssociation(associationType));

            @new.WritableRecordForAssociation().AddCompositesAssociation(associationType, this.objectId);
        }

        internal void SetCompositesRolesOne2Many(IRoleType roleType, IEnumerable<Strategy> roles)
        {
            this.AssertNotDeleted();

            var originalRoles = this.CompositesRoleStrategies(roleType);

            if (originalRoles == null)
            {
                foreach (var role in roles)
                {
                    this.AddCompositeRoleOne2Many(roleType, role);
                }
            }
            else
            {
                ISet<Strategy> toRemove = new HashSet<Strategy>(originalRoles);

                foreach (var role in roles)
                {
                    if (!toRemove.Remove(role))
                    {
                        this.AddCompositeRoleOne2Many(roleType, role);
                    }
                }

                foreach (var strategy in toRemove)
                {
                    this.RemoveCompositeRoleOne2Many(roleType, strategy);
                }
            }
        }

        internal void SetCompositesRolesMany2Many(IRoleType roleType, IEnumerable<Strategy> roles)
        {
            this.AssertNotDeleted();

            var originalRoles = this.CompositesRoleStrategies(roleType);

            if (originalRoles == null)
            {
                foreach (var role in roles)
                {
                    this.AddCompositeRoleMany2Many(roleType, role);
                }
            }
            else
            {
                ISet<Strategy> toRemove = new HashSet<Strategy>(originalRoles);

                foreach (var role in roles)
                {
                    if (!toRemove.Remove(role))
                    {
                        this.AddCompositeRoleMany2Many(roleType, role);
                    }
                }

                foreach (var strategy in toRemove)
                {
                    this.RemoveCompositeRoleMany2Many(roleType, strategy);
                }
            }
        }

        internal bool ShouldTrim(IRoleType roleType, object originalRole) => UnitEquals(this.GetInternalizedUnitRole(roleType), originalRole);

        internal bool ShouldTrim(IRoleType roleType, Strategy originalRole) => (this.Record?.GetCompositeRole(roleType) ?? 0) == (originalRole?.objectId ?? 0);

        internal bool ShouldTrim(IRoleType roleType, Strategy[] originalRole) => SetEquals(this.Record?.GetCompositesRole(roleType), originalRole);

        internal bool ShouldTrim(IAssociationType associationType, Strategy originalAssociation) => (this.Record?.GetCompositeAssociation(associationType) ?? 0) == (originalAssociation?.objectId ?? 0);

        internal bool ShouldTrim(IAssociationType associationType, Strategy[] originalAssociation) => SetEquals(this.Record?.GetCompositesAssociation(associationType), originalAssociation);

        private static bool UnitEquals(object first, object second) =>
            first is byte[] firstBinary && second is byte[] secondBinary
                ? firstBinary.SequenceEqual(secondBinary)
                : Equals(first, second);

        private static bool SetEquals(ImmutableSortedSet<long> objectIds, Strategy[] strategies)
        {
            if (objectIds == null || objectIds.IsEmpty)
            {
                return strategies == null || strategies.Length == 0;
            }

            if (strategies == null)
            {
                return false;
            }

            return objectIds.SetEquals(strategies.Select(v => v.objectId));
        }

        private Record WritableRecordForRole()
        {
            this.Transaction.MarkRoleChanged(this.objectId);
            return this.Transaction.EnsureInOverlay(this.objectId);
        }

        private Record WritableRecordForAssociation() => this.Transaction.EnsureInOverlay(this.objectId);

        private Strategy CompositeRoleStrategy(IRoleType roleType)
        {
            var roleId = this.Record?.GetCompositeRole(roleType) ?? 0;
            return roleId == 0 ? null : this.Transaction.GetStrategy(roleId);
        }

        private Strategy[] CompositesRoleStrategies(IRoleType roleType) => this.Strategies(this.Record?.GetCompositesRole(roleType));

        private Strategy CompositeAssociationStrategy(IAssociationType associationType)
        {
            var associationId = this.Record?.GetCompositeAssociation(associationType) ?? 0;
            return associationId == 0 ? null : this.Transaction.GetStrategy(associationId);
        }

        private Strategy[] CompositesAssociationStrategies(IAssociationType associationType) => this.Strategies(this.Record?.GetCompositesAssociation(associationType));

        private Strategy[] Strategies(ImmutableSortedSet<long> objectIds)
        {
            if (objectIds == null || objectIds.IsEmpty)
            {
                return null;
            }

            var strategies = new List<Strategy>(objectIds.Count);
            foreach (var objectId in objectIds)
            {
                var strategy = this.Transaction.GetStrategy(objectId);
                if (strategy != null)
                {
                    strategies.Add(strategy);
                }
            }

            return strategies.Count > 0 ? strategies.ToArray() : null;
        }

        // The change log keeps the roles and associations from before the first change in a checkpoint and
        // enumerates them only then: a change to a large role does not read all of its objects again.
        private IEnumerable<Strategy> OriginalCompositesRole(IRoleType roleType) => this.OriginalStrategies(this.Record?.GetCompositesRole(roleType));

        private IEnumerable<Strategy> OriginalCompositesAssociation(IAssociationType associationType) => this.OriginalStrategies(this.Record?.GetCompositesAssociation(associationType));

        private IEnumerable<Strategy> OriginalStrategies(ImmutableSortedSet<long> objectIds) =>
            objectIds == null || objectIds.IsEmpty
                ? null
                : objectIds.Select(v => this.Transaction.GetStrategy(v)).Where(v => v != null);

        private void RemoveCompositeRoleOne2One(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Transaction.Database.CompositeRoleChecks(this, roleType);

            var previousRoleId = this.Record.GetCompositeRole(roleType);
            if (previousRoleId == 0)
            {
                return;
            }

            var previousRole = this.Transaction.GetStrategy(previousRoleId);
            var associationType = roleType.AssociationType;

            this.ChangeLog.OnChangingCompositeRole(this, roleType, null, previousRole);

            if (previousRole != null)
            {
                this.ChangeLog.OnChangingCompositeAssociation(previousRole, associationType, previousRole.CompositeAssociationStrategy(associationType));

                previousRole.WritableRecordForAssociation().SetCompositeAssociation(associationType, 0);
            }

            // remove role
            this.WritableRecordForRole().SetCompositeRole(roleType, 0);
        }

        private void RemoveCompositeRoleMany2One(IRoleType roleType)
        {
            this.AssertNotDeleted();
            this.Transaction.Database.CompositeRoleChecks(this, roleType);

            var previousRoleId = this.Record.GetCompositeRole(roleType);
            if (previousRoleId == 0)
            {
                return;
            }

            var previousRole = this.Transaction.GetStrategy(previousRoleId);
            var associationType = roleType.AssociationType;

            this.ChangeLog.OnChangingCompositeRole(this, roleType, null, previousRole);

            if (previousRole != null)
            {
                this.ChangeLog.OnChangingCompositesAssociation(previousRole, associationType, previousRole.OriginalCompositesAssociation(associationType));

                previousRole.WritableRecordForAssociation().RemoveCompositesAssociation(associationType, this.objectId);
            }

            // remove role
            this.WritableRecordForRole().SetCompositeRole(roleType, 0);
        }

        private void AddCompositeRoleMany2Many(IRoleType roleType, Strategy add)
        {
            var roleIds = this.Record.GetCompositesRole(roleType);
            if (roleIds.Contains(add.objectId))
            {
                return;
            }

            this.ChangeLog.OnChangingCompositesRole(this, roleType, add, this.OriginalStrategies(roleIds));

            // Add the new role
            this.WritableRecordForRole().AddCompositesRole(roleType, add.objectId);

            // Add the new association
            var associationType = roleType.AssociationType;

            this.ChangeLog.OnChangingCompositesAssociation(add, associationType, add.OriginalCompositesAssociation(associationType));

            add.WritableRecordForAssociation().AddCompositesAssociation(associationType, this.objectId);
        }

        private void AddCompositeRoleOne2Many(IRoleType roleType, Strategy add)
        {
            var roleIds = this.Record.GetCompositesRole(roleType);
            if (roleIds.Contains(add.objectId))
            {
                return;
            }

            this.ChangeLog.OnChangingCompositesRole(this, roleType, add, this.OriginalStrategies(roleIds));

            var associationType = roleType.AssociationType;

            // 1-...
            var addPreviousAssociation = add.CompositeAssociationStrategy(associationType);

            this.ChangeLog.OnChangingCompositeAssociation(add, associationType, addPreviousAssociation);

            if (addPreviousAssociation != null)
            {
                this.ChangeLog.OnChangingCompositesRole(addPreviousAssociation, roleType, null, addPreviousAssociation.OriginalCompositesRole(roleType));

                // Remove obsolete role
                addPreviousAssociation.WritableRecordForRole().RemoveCompositesRole(roleType, add.objectId);
            }

            // Add the new role
            this.WritableRecordForRole().AddCompositesRole(roleType, add.objectId);

            // Set new association
            add.WritableRecordForAssociation().SetCompositeAssociation(associationType, this.objectId);
        }

        private void RemoveCompositeRoleMany2Many(IRoleType roleType, Strategy remove)
        {
            var roleIds = this.Record.GetCompositesRole(roleType);
            if (!roleIds.Contains(remove.objectId))
            {
                return;
            }

            this.ChangeLog.OnChangingCompositesRole(this, roleType, remove, this.OriginalStrategies(roleIds));

            // Remove role
            this.WritableRecordForRole().RemoveCompositesRole(roleType, remove.objectId);

            // Remove association
            var associationType = roleType.AssociationType;

            this.ChangeLog.OnChangingCompositesAssociation(remove, associationType, remove.OriginalCompositesAssociation(associationType));

            remove.WritableRecordForAssociation().RemoveCompositesAssociation(associationType, this.objectId);
        }

        private void RemoveCompositeRoleOne2Many(IRoleType roleType, Strategy roleToRemove)
        {
            var roleIds = this.Record.GetCompositesRole(roleType);
            if (!roleIds.Contains(roleToRemove.objectId))
            {
                return;
            }

            this.ChangeLog.OnChangingCompositesRole(this, roleType, roleToRemove, this.OriginalStrategies(roleIds));

            // Remove role
            this.WritableRecordForRole().RemoveCompositesRole(roleType, roleToRemove.objectId);

            // Remove association
            var associationType = roleType.AssociationType;

            this.ChangeLog.OnChangingCompositeAssociation(roleToRemove, associationType, roleToRemove.CompositeAssociationStrategy(associationType));

            roleToRemove.WritableRecordForAssociation().SetCompositeAssociation(associationType, 0);
        }

        private void RemoveCompositeRolesMany2Many(IRoleType roleType)
        {
            var previousRoleStrategies = this.CompositesRoleStrategies(roleType);
            if (previousRoleStrategies != null)
            {
                foreach (var previousRoleStrategy in previousRoleStrategies)
                {
                    this.ChangeLog.OnChangingCompositesRole(this, roleType, previousRoleStrategy, previousRoleStrategies);
                }

                foreach (var strategy in previousRoleStrategies)
                {
                    this.RemoveCompositeRoleMany2Many(roleType, strategy);
                }
            }
        }

        private void RemoveCompositeRolesOne2Many(IRoleType roleType)
        {
            var previousRoleStrategies = this.CompositesRoleStrategies(roleType);
            if (previousRoleStrategies != null)
            {
                foreach (var strategy in previousRoleStrategies)
                {
                    this.RemoveCompositeRoleOne2Many(roleType, strategy);
                }
            }
        }

        private void AssertNotDeleted()
        {
            if (this.IsDeleted)
            {
                throw new Exception($"Object of class {this.UncheckedObjectType?.Name ?? "?"} with id {this.ObjectId} has been deleted");
            }
        }
    }
}

// <copyright file="RecordBasedOriginState.cs" company="Allors bv">
// Copyright (c) Allors bv. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Adapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Ranges;

    public abstract class RecordBasedOriginState
    {
        public abstract Strategy Strategy { get; }

        protected bool HasChanges => this.Record == null || this.ChangedRoleByRelationType?.Count > 0;

        protected abstract IEnumerable<IRoleType> RoleTypes { get; }

        protected abstract IRecord Record { get; }

        protected IRecord PreviousRecord { get; set; }

        public Dictionary<IRelationType, object> ChangedRoleByRelationType { get; private set; }

        private Dictionary<IRelationType, object> PreviousChangedRoleByRelationType { get; set; }

        public object GetUnitRole(IRoleType roleType)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var role))
            {
                return role;
            }

            return this.Record?.GetRole(roleType);
        }

        public void SetUnitRole(IRoleType roleType, object role) => this.SetChangedRole(roleType, role);

        public Strategy GetCompositeRole(IRoleType roleType)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var changedRole))
            {
                return (Strategy)changedRole;
            }

            var role = this.Record?.GetRole(roleType);

            if (role == null)
            {
                return null;
            }

            var strategy = this.Session.GetStrategy((long)role);
            this.AssertStrategy(strategy, (long)role, roleType);
            return strategy;
        }

        public long GetCompositeRoleId(IRoleType roleType)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var changedRole))
            {
                return ((Strategy)changedRole)?.Id ?? 0;
            }

            return (long?)this.Record?.GetRole(roleType) ?? 0;
        }

        public void SetCompositeRole(IRoleType roleType, Strategy role)
        {
            if (this.SameCompositeRole(roleType, role))
            {
                return;
            }

            var associationType = roleType.AssociationType;
            if (associationType.IsOne && role != null)
            {
                var previousAssociation = this.Session.GetCompositeAssociation(role, associationType);
                this.SetChangedRole(roleType, role);
                if (associationType.IsOne && previousAssociation != null)
                {
                    // OneToOne
                    previousAssociation.SetRole(roleType, null);
                }
            }
            else
            {
                this.SetChangedRole(roleType, role);
            }
        }

        public IRange<long> GetCompositesRoleIds(IRoleType roleType) => this.CompositesIds(this.GetCompositesDelta(roleType), this.Record, roleType);

        public IRange<Strategy> GetCompositesRole(IRoleType roleType)
        {
            var delta = this.GetCompositesDelta(roleType);
            var strategies = this.RecordCompositesRole(roleType, delta?.Removed);
            return delta == null ? strategies : this.Ranges.Union(strategies, delta.Added);
        }

        public void AddCompositesRole(IRoleType roleType, Strategy roleToAdd)
        {
            if (this.HasCompositesRole(roleType, roleToAdd))
            {
                return;
            }

            var associationType = roleType.AssociationType;
            var previousAssociation = associationType.IsOne ? this.Session.GetCompositeAssociation(roleToAdd, associationType) : null;

            var ranges = this.Session.Workspace.RecordRanges;
            var delta = this.GetCompositesDelta(roleType);
            var added = this.Ranges.Ensure(delta?.Added);
            var removed = ranges.Ensure(delta?.Removed);
            if (this.RecordCompositesRoleIds(roleType).Contains(roleToAdd.Id))
            {
                removed = ranges.Remove(removed, roleToAdd.Id);
            }
            else
            {
                added = this.Ranges.Add(added, roleToAdd);
            }

            this.SetChangedRole(roleType, new CompositesDelta(added, removed));

            if (associationType.IsMany)
            {
                return;
            }

            // OneToMany
            if (previousAssociation != null && previousAssociation != this.Strategy)
            {
                previousAssociation.RemoveCompositesRole(roleType, roleToAdd.Object);
            }
        }

        public void RemoveCompositesRole(IRoleType roleType, Strategy roleToRemove)
        {
            if (!this.HasCompositesRole(roleType, roleToRemove))
            {
                return;
            }

            var ranges = this.Session.Workspace.RecordRanges;
            var delta = this.GetCompositesDelta(roleType);
            var added = this.Ranges.Ensure(delta?.Added);
            var removed = ranges.Ensure(delta?.Removed);
            if (added.Contains(roleToRemove))
            {
                added = this.Ranges.Remove(added, roleToRemove);
            }
            else
            {
                removed = ranges.Add(removed, roleToRemove.Id);
            }

            this.SetChangedRole(roleType, new CompositesDelta(added, removed));
        }

        public void SetCompositesRole(IRoleType roleType, IRange<Strategy> role)
        {
            role = this.Ranges.Ensure(role);

            if (this.SameCompositesRole(roleType, role))
            {
                return;
            }

            var associationType = roleType.AssociationType;
            var previousIds = this.GetCompositesRoleIds(roleType);
            var addedRoles = role.Where(v => !previousIds.Contains(v.Id)).ToArray();
            var previousAssociations = associationType.IsOne
                ? addedRoles.Select(v => this.Session.GetCompositeAssociation(v, associationType)).ToArray()
                : Array.Empty<Strategy>();

            var ranges = this.Session.Workspace.RecordRanges;
            var recordIds = this.RecordCompositesRoleIds(roleType);
            var added = this.Ranges.Import(role.Where(v => !recordIds.Contains(v.Id)));
            var removed = ranges.Except(recordIds, ranges.Import(role.Select(v => v.Id)));
            this.SetChangedRole(roleType, new CompositesDelta(added, removed));

            if (associationType.IsMany)
            {
                return;
            }

            // OneToMany
            for (var i = 0; i < addedRoles.Length; i++)
            {
                var previousAssociation = previousAssociations[i];
                if (previousAssociation != null && previousAssociation != this.Strategy)
                {
                    previousAssociation.RemoveCompositesRole(roleType, addedRoles[i].Object);
                }
            }
        }

        public void Checkpoint(ChangeSet changeSet)
        {
            var sameRecord = this.PreviousRecord == null || this.Record == null || this.Record.Version == this.PreviousRecord.Version;
            var previousRecord = sameRecord ? this.Record : this.PreviousRecord;

            // Same record: only the changed roles; different record: every role
            var relationTypes = sameRecord
                ? this.ChangedRoleByRelationType?.Keys.ToArray() ?? Array.Empty<IRelationType>()
                : this.RoleTypes.Select(v => v.RelationType).ToArray();

            foreach (var relationType in relationTypes)
            {
                var roleType = relationType.RoleType;

                object changedRole = null;
                var hasCurrent = this.ChangedRoleByRelationType?.TryGetValue(relationType, out changedRole) == true;
                object previousChangedRole = null;
                var hasPrevious = this.PreviousChangedRoleByRelationType?.TryGetValue(relationType, out previousChangedRole) == true;

                var current = hasCurrent ? changedRole : this.Record?.GetRole(roleType);
                var previous = hasPrevious ? previousChangedRole : previousRecord?.GetRole(roleType);

                if (roleType.ObjectType.IsUnit)
                {
                    changeSet.DiffUnit(this.Strategy, relationType, current, previous);
                }
                else if (roleType.IsOne)
                {
                    changeSet.DiffComposite(
                        this.Strategy,
                        relationType,
                        hasCurrent ? ((Strategy)current)?.Id ?? 0 : (long?)current ?? 0,
                        hasPrevious ? ((Strategy)previous)?.Id ?? 0 : (long?)previous ?? 0);
                }
                else
                {
                    changeSet.DiffComposites(
                        this.Strategy,
                        relationType,
                        this.CompositesIds(hasCurrent ? (CompositesDelta)current : null, this.Record, roleType),
                        this.CompositesIds(hasPrevious ? (CompositesDelta)previous : null, previousRecord, roleType));
                }
            }

            this.PreviousRecord = this.Record;
            this.PreviousChangedRoleByRelationType = this.ChangedRoleByRelationType != null ? new Dictionary<IRelationType, object>(this.ChangedRoleByRelationType) : null;
        }

        public void Diff(IList<IDiff> diffs)
        {
            if (this.ChangedRoleByRelationType == null)
            {
                return;
            }

            var ranges = this.Session.Workspace.RecordRanges;

            foreach (var kvp in this.ChangedRoleByRelationType)
            {
                var relationType = kvp.Key;
                var roleType = relationType.RoleType;

                var changed = kvp.Value;
                var original = this.Record?.GetRole(roleType);

                if (roleType.ObjectType.IsUnit)
                {
                    diffs.Add(new UnitDiff(relationType, this.Strategy)
                    {
                        OriginalRole = original,
                        ChangedRole = changed,
                    });
                }
                else if (roleType.IsOne)
                {
                    var originalRoleId = (long?)original ?? 0;
                    var changedRole = (Strategy)changed;
                    diffs.Add(new CompositeDiff(relationType, this.Strategy)
                    {
                        OriginalRoleId = originalRoleId,
                        ChangedRoleId = changedRole?.Id ?? 0,
                        OriginalRole = originalRoleId != 0 ? this.Session.GetStrategy(originalRoleId) : null,
                        ChangedRole = changedRole,
                    });
                }
                else
                {
                    var originalRoleIds = ranges.Ensure(original);
                    var changedRoleIds = this.CompositesIds((CompositesDelta)changed, this.Record, roleType);
                    diffs.Add(new CompositesDiff(relationType, this.Strategy)
                    {
                        OriginalRoleIds = originalRoleIds.ToArray(),
                        ChangedRoleIds = changedRoleIds.ToArray(),
                        OriginalRoles = this.HeldStrategies(originalRoleIds),
                        ChangedRoles = this.HeldStrategies(changedRoleIds),
                    });
                }
            }
        }

        public bool CanMerge(IRecord newRecord)
        {
            if (this.ChangedRoleByRelationType == null)
            {
                return true;
            }

            var ranges = this.Session.Workspace.RecordRanges;

            foreach (var kvp in this.ChangedRoleByRelationType)
            {
                var relationType = kvp.Key;
                var roleType = relationType.RoleType;

                var original = this.Record?.GetRole(roleType);
                var newOriginal = newRecord?.GetRole(roleType);

                if (roleType.ObjectType.IsUnit)
                {
                    var same = original is byte[] originalBinary && newOriginal is byte[] newOriginalBinary
                        ? originalBinary.SequenceEqual(newOriginalBinary)
                        : Equals(original, newOriginal);
                    if (!same)
                    {
                        return false;
                    }
                }
                else if (roleType.IsOne)
                {
                    if (!Equals(original, newOriginal))
                    {
                        return false;
                    }
                }
                else if (!ranges.Ensure(original).Equals(ranges.Ensure(newOriginal)))
                {
                    return false;
                }
            }

            return true;
        }

        public bool HashChanges() => this.ChangedRoleByRelationType != null;

        public void Reset() => this.ChangedRoleByRelationType = null;

        public bool IsAssociationForRole(IRoleType roleType, Strategy forRole)
        {
            if (roleType.IsOne)
            {
                var roleId = this.GetCompositeRoleId(roleType);
                return roleId != 0 && roleId == forRole.Id;
            }

            return this.HasCompositesRole(roleType, forRole);
        }

        public bool HasChanged(IRoleType roleType) => this.ChangedRoleByRelationType?.ContainsKey(roleType.RelationType) ?? false;

        public void RestoreRole(IRoleType roleType) => this.ChangedRoleByRelationType?.Remove(roleType.RelationType);

        protected abstract void OnChange();

        private void SetChangedRole(IRoleType roleType, object role)
        {
            this.ChangedRoleByRelationType ??= new Dictionary<IRelationType, object>();
            this.ChangedRoleByRelationType[roleType.RelationType] = role;
            this.OnChange();
        }

        private CompositesDelta GetCompositesDelta(IRoleType roleType) =>
            this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var changedRole)
                ? (CompositesDelta)changedRole
                : null;

        private IRange<long> RecordCompositesRoleIds(IRoleType roleType) => this.Session.Workspace.RecordRanges.Ensure(this.Record?.GetRole(roleType));

        private IRange<Strategy> RecordCompositesRole(IRoleType roleType, IRange<long> removed)
        {
            var ids = this.Session.Workspace.RecordRanges.Except(this.RecordCompositesRoleIds(roleType), removed);
            return this.Ranges.Import(ids.Select(v =>
            {
                var strategy = this.Session.GetStrategy(v);
                this.AssertStrategy(strategy, v, roleType);
                return strategy;
            }));
        }

        private bool HasCompositesRole(IRoleType roleType, Strategy role)
        {
            var delta = this.GetCompositesDelta(roleType);
            if (delta != null)
            {
                if (delta.Added.Contains(role))
                {
                    return true;
                }

                if (delta.Removed.Contains(role.Id))
                {
                    return false;
                }
            }

            return this.RecordCompositesRoleIds(roleType).Contains(role.Id);
        }

        private IRange<long> CompositesIds(CompositesDelta delta, IRecord record, IRoleType roleType)
        {
            var ranges = this.Session.Workspace.RecordRanges;
            var ids = ranges.Ensure(record?.GetRole(roleType));
            if (delta == null)
            {
                return ids;
            }

            ids = ranges.Except(ids, delta.Removed);
            foreach (var added in delta.Added)
            {
                ids = ranges.Add(ids, added.Id);
            }

            return ids;
        }

        private Strategy[] HeldStrategies(IRange<long> ids) => ids.Select(v => this.Session.GetStrategy(v)).Where(v => v != null).ToArray();

        private bool SameCompositeRole(IRoleType roleType, Strategy role)
        {
            if (this.ChangedRoleByRelationType != null && this.ChangedRoleByRelationType.TryGetValue(roleType.RelationType, out var changedRole))
            {
                return role == changedRole;
            }

            var changedRoleId = this.Record?.GetRole(roleType);

            if (role == null)
            {
                return changedRoleId == null;
            }

            if (changedRoleId == null)
            {
                return false;
            }

            return role.Id == (long)changedRoleId;
        }

        private bool SameCompositesRole(IRoleType roleType, IRange<Strategy> role) =>
            this.GetCompositesRoleIds(roleType).Equals(this.Session.Workspace.RecordRanges.Import(role.Select(v => v.Id)));

        private void AssertStrategy(Strategy strategy, long id, IRoleType roleType)
        {
            if (strategy == null)
            {
                throw new Exception($"Object {id} in role {roleType.AssociationType.ObjectType.SingularName}.{roleType.Name} is not in this session. Include the role in the Pull, or pull the object, before reading it.");
            }
        }

        #region Proxy Properties

        protected long Id => this.Strategy.Id;

        protected IClass Class => this.Strategy.Class;

        protected Session Session => this.Strategy.Session;

        protected Workspace Workspace => this.Session.Workspace;

        private IRanges<Strategy> Ranges => this.Strategy.Session.Workspace.StrategyRanges;

        #endregion
    }
}
